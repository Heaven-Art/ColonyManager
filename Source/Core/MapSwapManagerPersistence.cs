using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace FluffyManager
{
    /// <summary>
    /// Odyssey travel (SwapMap) persistence glue.
    /// When a manager station Thing moves via PreSwapMap/PostSwapMap, it carries the current Manager job stack.
    /// </summary>
    internal static class MapSwapManagerPersistence
    {
        // Carrier path (manager stations moving between maps): cache snapshot per map per tick,
        // and ensure rehydrate applies at most once per map per tick (multiple stations are allowed/redundant).
        private static int _lastCarrierTick = -1;
        private static Dictionary<string, ManagerProfile> _carrierSnapshotByMap;
        private static HashSet<string> _carrierAppliedMaps;

        public static void OnThingPreSwapMap( Thing thing )
        {
            // Odyssey moves Things between maps; use the moving manager-station building as the persistence carrier.
            // This is the most robust solution: if the station travels, the settings travel.
            try
            {
                if ( thing == null )
                    return;

                if ( !( thing is Building_ManagerStation ) )
                    return;

                var map = thing.Map;
                if ( map == null )
                    return;

                var carrier = thing.TryGetComp<Comp_MapSwapManagerProfileCarrier>();
                if ( carrier == null )
                    return;

                var manager = Manager.For( map );
                if ( manager == null )
                    return;

                ThrottleCarrierSets();
                var mapId = map.GetUniqueLoadID();
                if ( mapId.NullOrEmpty() )
                    return;

                // Reuse the same snapshot across multiple stations on the same map.
                ManagerProfile snap;
                if ( !_carrierSnapshotByMap.TryGetValue( mapId, out snap ) )
                {
                    var jobs = manager.JobStack.FullStack();
                    if ( jobs == null || jobs.Count == 0 )
                    {
                        snap = null;
                        _carrierSnapshotByMap[mapId] = null;
                    }
                    else
                    {
                        snap = BuildProfile( manager );
                        _carrierSnapshotByMap[mapId] = snap;
                    }
                }

                carrier.profile = snap;
                Logger.Message(
                    $"MapSwap persistence: Carrier snapshot {(carrier.profile == null ? "skipped (no jobs)" : "OK")} | thing={thing.def?.defName ?? "null"} | thingId={thing.GetUniqueLoadID()} | map={map.GetUniqueLoadID()} | jobs={carrier.profile?.jobs?.Count ?? 0}" );
            }
            catch ( Exception err )
            {
                Logger.Warning( $"MapSwap persistence carrier snapshot failed: {err}" );
            }
        }

        public static void OnThingPostSwapMap( Thing thing )
        {
            try
            {
                if ( thing == null )
                    return;

                if ( !( thing is Building_ManagerStation ) )
                    return;

                var map = thing.Map;
                if ( map == null )
                    return;

                var carrier = thing.TryGetComp<Comp_MapSwapManagerProfileCarrier>();
                if ( carrier == null || carrier.profile == null )
                    return;

                var manager = Manager.For( map );
                if ( manager == null )
                {
                    carrier.profile = null;
                    return;
                }

                ThrottleCarrierSets();
                var mapId = map.GetUniqueLoadID();
                if ( mapId.NullOrEmpty() )
                {
                    carrier.profile = null;
                    return;
                }

                // If another station already applied on this map tick, just clear and bail.
                if ( _carrierAppliedMaps.Contains( mapId ) )
                {
                    carrier.profile = null;
                    return;
                }

                // Prevent multiple stations from applying the same profile:
                // if the manager already has jobs, do nothing and clear the cached profile.
                if ( manager.JobStack.FullStack().Any() )
                {
                    Logger.Message(
                        $"MapSwap persistence: Carrier rehydrate skipped (map already has jobs) | thing={thing.def?.defName ?? "null"} | thingId={thing.GetUniqueLoadID()} | map={map.GetUniqueLoadID()} | storedJobs={carrier.profile.jobs?.Count ?? 0}" );
                    carrier.profile = null;
                    return;
                }

                Logger.Message(
                    $"MapSwap persistence: Carrier rehydrate APPLY | thing={thing.def?.defName ?? "null"} | thingId={thing.GetUniqueLoadID()} | map={map.GetUniqueLoadID()} | jobs={carrier.profile.jobs?.Count ?? 0}" );
                ApplyProfile( manager, carrier.profile );
                _carrierAppliedMaps.Add( mapId );
                carrier.profile = null;
            }
            catch ( Exception err )
            {
                Logger.Warning( $"MapSwap persistence carrier rehydrate failed: {err}" );
            }
        }

        private static void ThrottleCarrierSets()
        {
            var tick = Find.TickManager?.TicksGame ?? -1;
            if ( tick == _lastCarrierTick && _carrierSnapshotByMap != null && _carrierAppliedMaps != null )
                return;

            _lastCarrierTick = tick;
            if ( _carrierSnapshotByMap == null )
                _carrierSnapshotByMap = new Dictionary<string, ManagerProfile>();
            if ( _carrierAppliedMaps == null )
                _carrierAppliedMaps = new HashSet<string>();

            _carrierSnapshotByMap.Clear();
            _carrierAppliedMaps.Clear();
        }

        private static ManagerProfile BuildProfile( Manager manager )
        {
            var profile = new ManagerProfile();

            foreach ( var job in manager.JobStack.FullStack() )
            {
                var rec = job?.ToMapSwapRecord();
                if ( rec != null )
                    profile.jobs.Add( rec );
            }

            return profile;
        }

        private static void ApplyProfile( Manager manager, ManagerProfile profile )
        {
            if ( manager == null || profile == null )
                return;

            var records = profile.jobs?.OrderBy( r => r.priority ).ToList() ?? new List<ManagerJobRecord>();
            var createdJobs = new List<ManagerJob>();

            // Create jobs first
            foreach ( var rec in records )
            {
                var job = CreateJobFromRecord( manager, rec );
                if ( job != null )
                    createdJobs.Add( job );
            }

            // Build a new stack
            var stack = new JobStack( manager );
            foreach ( var job in createdJobs )
                stack.Add( job );

            // Replace manager stack (this will Touch() jobs)
            manager.NewJobStack( stack );

            // Restore lastAction / priority after NewJobStack Touch()
            for ( var i = 0; i < createdJobs.Count && i < records.Count; i++ )
            {
                createdJobs[i].priority = records[i].priority;
                createdJobs[i].lastAction = records[i].lastAction;
            }
        }

        private static ManagerJob CreateJobFromRecord( Manager manager, ManagerJobRecord rec )
        {
            if ( manager == null || rec == null )
                return null;

            try
            {
                if ( rec.config is HuntingJobConfig )
                {
                    var job = new ManagerJob_Hunting( manager );
                    job.ApplyMapSwapRecord( rec );
                    return job;
                }

                if ( rec.config is ForestryJobConfig )
                {
                    var job = new ManagerJob_Forestry( manager );
                    job.ApplyMapSwapRecord( rec );
                    return job;
                }

                if ( rec.config is ForagingJobConfig )
                {
                    var job = new ManagerJob_Foraging( manager );
                    job.ApplyMapSwapRecord( rec );
                    return job;
                }

                if ( rec.config is MiningJobConfig )
                {
                    var job = new ManagerJob_Mining( manager );
                    job.ApplyMapSwapRecord( rec );
                    return job;
                }

                if ( rec.config is LivestockJobConfig )
                {
                    var job = new ManagerJob_Livestock( manager );
                    job.ApplyMapSwapRecord( rec );
                    return job;
                }
            }
            catch ( Exception err )
            {
                Logger.Warning( $"Failed to rehydrate transferred manager job '{rec.jobType}': {err}" );
            }

            return null;
        }
    }
}


