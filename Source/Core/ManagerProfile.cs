using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace FluffyManager
{
    /// <summary>
    /// Map-independent representation of the manager job stack for Odyssey travel (SwapMap).
    /// </summary>
    public class ManagerProfile : IExposable
    {
        public const int CurrentSchemaVersion = 1;

        public int schemaVersion = CurrentSchemaVersion;
        public List<ManagerJobRecord> jobs = new List<ManagerJobRecord>();

        public void ExposeData()
        {
            Scribe_Values.Look( ref schemaVersion, "schemaVersion", CurrentSchemaVersion );
            Scribe_Collections.Look( ref jobs, "jobs", LookMode.Deep );
            if ( jobs == null )
                jobs = new List<ManagerJobRecord>();
        }
    }

    /// <summary>
    /// Portable job record: contains base job fields plus job-specific config.
    /// </summary>
    public class ManagerJobRecord : IExposable
    {
        public string jobType;

        // base ManagerJob fields
        public int  priority;
        public bool managed;
        public bool suspended;
        public int  lastAction;
        public int  updateIntervalTicks;
        public bool checkReachable;
        public bool pathBasedDistance;

        // job-specific config
        public IExposable config;

        public void ExposeData()
        {
            Scribe_Values.Look( ref jobType, "jobType" );
            Scribe_Values.Look( ref priority, "priority" );
            Scribe_Values.Look( ref managed, "managed" );
            Scribe_Values.Look( ref suspended, "suspended" );
            Scribe_Values.Look( ref lastAction, "lastAction" );
            Scribe_Values.Look( ref updateIntervalTicks, "updateIntervalTicks" );
            Scribe_Values.Look( ref checkReachable, "checkReachable", true );
            Scribe_Values.Look( ref pathBasedDistance, "pathBasedDistance" );
            Scribe_Deep.Look( ref config, "config" );
        }
    }

    /// <summary>
    /// Threshold trigger configuration without a Manager reference.
    /// </summary>
    public class ThresholdTriggerConfig : IExposable
    {
        public Trigger_Threshold.Ops op = Trigger_Threshold.Ops.LowerThan;
        public int targetCount = Trigger_Threshold.DefaultCount;
        public int maxUpperThreshold = Trigger_Threshold.DefaultMaxUpperThreshold;
        public bool countAllOnMap;
        public string stockpileLabel;
        public ThingFilter thresholdFilter;

        public void ExposeData()
        {
            Scribe_Values.Look( ref op, "op", Trigger_Threshold.Ops.LowerThan );
            Scribe_Values.Look( ref targetCount, "targetCount", Trigger_Threshold.DefaultCount );
            Scribe_Values.Look( ref maxUpperThreshold, "maxUpperThreshold", Trigger_Threshold.DefaultMaxUpperThreshold );
            Scribe_Values.Look( ref countAllOnMap, "countAllOnMap" );
            Scribe_Values.Look( ref stockpileLabel, "stockpileLabel" );
            Scribe_Deep.Look( ref thresholdFilter, "thresholdFilter" );
            if ( Scribe.mode == LoadSaveMode.PostLoadInit && thresholdFilter == null )
                thresholdFilter = new ThingFilter();
        }
    }

    public class HuntingJobConfig : IExposable
    {
        public ThresholdTriggerConfig trigger;
        public Dictionary<PawnKindDef, bool> allowedAnimals;
        public string huntingAreaLabel;
        public bool unforbidCorpses = true;
        public bool allowHumanLikeMeat;
        public bool allowInsectMeat;

        public void ExposeData()
        {
            Scribe_Deep.Look( ref trigger, "trigger" );
            Scribe_Collections.Look( ref allowedAnimals, "allowedAnimals", LookMode.Def, LookMode.Value );
            Scribe_Values.Look( ref huntingAreaLabel, "huntingAreaLabel" );
            Scribe_Values.Look( ref unforbidCorpses, "unforbidCorpses", true );
            Scribe_Values.Look( ref allowHumanLikeMeat, "allowHumanLikeMeat" );
            Scribe_Values.Look( ref allowInsectMeat, "allowInsectMeat" );
            if ( allowedAnimals == null )
                allowedAnimals = new Dictionary<PawnKindDef, bool>();
        }
    }

    public class ForestryJobConfig : IExposable
    {
        public ThresholdTriggerConfig trigger;
        public ManagerJob_Forestry.ForestryJobType type = ManagerJob_Forestry.ForestryJobType.Logging;
        public Dictionary<ThingDef, bool> allowedTrees;
        public bool allowSaplings;
        public bool clearWindCells;
        public Dictionary<string, bool> clearAreasByLabel;
        public string loggingAreaLabel;

        public void ExposeData()
        {
            Scribe_Deep.Look( ref trigger, "trigger" );
            Scribe_Values.Look( ref type, "type", ManagerJob_Forestry.ForestryJobType.Logging );
            Scribe_Collections.Look( ref allowedTrees, "allowedTrees", LookMode.Def, LookMode.Value );
            Scribe_Values.Look( ref allowSaplings, "allowSaplings" );
            Scribe_Values.Look( ref clearWindCells, "clearWindCells" );
            Scribe_Collections.Look( ref clearAreasByLabel, "clearAreasByLabel", LookMode.Value, LookMode.Value );
            Scribe_Values.Look( ref loggingAreaLabel, "loggingAreaLabel" );

            if ( allowedTrees == null )
                allowedTrees = new Dictionary<ThingDef, bool>();
            if ( clearAreasByLabel == null )
                clearAreasByLabel = new Dictionary<string, bool>();
        }
    }

    public class ForagingJobConfig : IExposable
    {
        public ThresholdTriggerConfig trigger;
        public Dictionary<ThingDef, bool> allowedPlants;
        public string foragingAreaLabel;
        public bool forceFullyMature;
        public Utilities.SyncDirection sync;
        public bool syncFilterAndAllowed = true;

        public void ExposeData()
        {
            Scribe_Deep.Look( ref trigger, "trigger" );
            Scribe_Collections.Look( ref allowedPlants, "allowedPlants", LookMode.Def, LookMode.Value );
            Scribe_Values.Look( ref foragingAreaLabel, "foragingAreaLabel" );
            Scribe_Values.Look( ref forceFullyMature, "forceFullyMature" );
            Scribe_Values.Look( ref sync, "sync", Utilities.SyncDirection.AllowedToFilter );
            Scribe_Values.Look( ref syncFilterAndAllowed, "syncFilterAndAllowed", true );
            if ( allowedPlants == null )
                allowedPlants = new Dictionary<ThingDef, bool>();
        }
    }

    public class MiningJobConfig : IExposable
    {
        public ThresholdTriggerConfig trigger;
        public Dictionary<ThingDef, bool> allowedMinerals;
        public Dictionary<ThingDef, bool> allowedBuildings;
        public string miningAreaLabel;
        public Utilities.SyncDirection sync;
        public bool syncFilterAndAllowed = true;
        public bool deconstructBuildings;
        public bool checkRoofSupport = true;
        public bool checkRoofSupportAdvanced;
        public bool checkRoomDivision = true;

        public void ExposeData()
        {
            Scribe_Deep.Look( ref trigger, "trigger" );
            Scribe_Collections.Look( ref allowedMinerals, "allowedMinerals", LookMode.Def, LookMode.Value );
            Scribe_Collections.Look( ref allowedBuildings, "allowedBuildings", LookMode.Def, LookMode.Value );
            Scribe_Values.Look( ref miningAreaLabel, "miningAreaLabel" );
            Scribe_Values.Look( ref sync, "sync", Utilities.SyncDirection.AllowedToFilter );
            Scribe_Values.Look( ref syncFilterAndAllowed, "syncFilterAndAllowed", true );
            Scribe_Values.Look( ref deconstructBuildings, "deconstructBuildings" );
            Scribe_Values.Look( ref checkRoofSupport, "checkRoofSupport", true );
            Scribe_Values.Look( ref checkRoofSupportAdvanced, "checkRoofSupportAdvanced" );
            Scribe_Values.Look( ref checkRoomDivision, "checkRoomDivision", true );

            if ( allowedMinerals == null )
                allowedMinerals = new Dictionary<ThingDef, bool>();
            if ( allowedBuildings == null )
                allowedBuildings = new Dictionary<ThingDef, bool>();
        }
    }

    public class LivestockJobConfig : IExposable
    {
        public PawnKindDef pawnKind;
        public Dictionary<AgeAndSex, int> countTargets;

        public bool butcherBonded;
        public bool butcherExcess = true;
        public bool butcherPregnant;
        public bool butcherTrained;
        public bool respectBonds = true;

        public bool tryTameMore;
        public string tameAreaLabel;

        public bool restrictToArea;
        public List<string> restrictAreaLabels;

        public bool sendToSlaughterArea;
        public string slaughterAreaLabel;
        public bool sendToMilkingArea;
        public string milkAreaLabel;
        public bool sendToShearingArea;
        public string shearAreaLabel;
        public bool sendToTrainingArea;
        public string trainingAreaLabel;

        public bool setFollow = true;
        public bool followDrafted = true;
        public bool followFieldwork = true;
        public bool followTraining;

        public MasterMode masters;
        public Pawn master;
        public MasterMode trainers;
        public Pawn trainer;

        public ManagerJob_Livestock.TrainingTracker training;

        public void ExposeData()
        {
            Scribe_Defs.Look( ref pawnKind, "pawnKind" );
            Scribe_Collections.Look( ref countTargets, "countTargets", LookMode.Value, LookMode.Value );

            Scribe_Values.Look( ref butcherBonded, "butcherBonded" );
            Scribe_Values.Look( ref butcherExcess, "butcherExcess", true );
            Scribe_Values.Look( ref butcherPregnant, "butcherPregnant" );
            Scribe_Values.Look( ref butcherTrained, "butcherTrained" );
            Scribe_Values.Look( ref respectBonds, "respectBonds", true );

            Scribe_Values.Look( ref tryTameMore, "tryTameMore" );
            Scribe_Values.Look( ref tameAreaLabel, "tameAreaLabel" );

            Scribe_Values.Look( ref restrictToArea, "restrictToArea" );
            Scribe_Collections.Look( ref restrictAreaLabels, "restrictAreaLabels", LookMode.Value );

            Scribe_Values.Look( ref sendToSlaughterArea, "sendToSlaughterArea" );
            Scribe_Values.Look( ref slaughterAreaLabel, "slaughterAreaLabel" );
            Scribe_Values.Look( ref sendToMilkingArea, "sendToMilkingArea" );
            Scribe_Values.Look( ref milkAreaLabel, "milkAreaLabel" );
            Scribe_Values.Look( ref sendToShearingArea, "sendToShearingArea" );
            Scribe_Values.Look( ref shearAreaLabel, "shearAreaLabel" );
            Scribe_Values.Look( ref sendToTrainingArea, "sendToTrainingArea" );
            Scribe_Values.Look( ref trainingAreaLabel, "trainingAreaLabel" );

            Scribe_Values.Look( ref setFollow, "setFollow", true );
            Scribe_Values.Look( ref followDrafted, "followDrafted", true );
            Scribe_Values.Look( ref followFieldwork, "followFieldwork", true );
            Scribe_Values.Look( ref followTraining, "followTraining" );

            Scribe_Values.Look( ref masters, "masters", MasterMode.Default );
            Scribe_References.Look( ref master, "master" );
            Scribe_Values.Look( ref trainers, "trainers", MasterMode.Default );
            Scribe_References.Look( ref trainer, "trainer" );

            Scribe_Deep.Look( ref training, "training" );
            if ( Scribe.mode == LoadSaveMode.PostLoadInit && training == null )
                training = new ManagerJob_Livestock.TrainingTracker();

            if ( countTargets == null )
                countTargets = Utilities_Livestock.AgeSexArray.ToDictionary( k => k, v => 5 );
            if ( restrictAreaLabels == null )
                restrictAreaLabels = new List<string>( Utilities_Livestock.AgeSexArray.Length );
        }
    }

    public static class ManagerProfileHelpers
    {
        public static void FillBaseFromJob( this ManagerJobRecord rec, ManagerJob job )
        {
            if ( rec == null || job == null )
                return;

            rec.jobType = job.GetType().FullName;
            rec.priority = job.priority;
            rec.managed = job.Managed;
            rec.suspended = job.Suspended;
            rec.lastAction = job.lastAction;
            rec.updateIntervalTicks = job.UpdateInterval?.ticks ?? Settings.DefaultUpdateInterval?.ticks ?? GenDate.TicksPerDay;
            rec.checkReachable = job.CheckReachable;
            rec.pathBasedDistance = job.PathBasedDistance;
        }

        public static void ApplyBaseToJob( this ManagerJobRecord rec, ManagerJob job )
        {
            if ( rec == null || job == null )
                return;

            job.priority = rec.priority;
            job.Managed = rec.managed;
            job.Suspended = rec.suspended;
            job.lastAction = rec.lastAction;
            job.CheckReachable = rec.checkReachable;
            job.PathBasedDistance = rec.pathBasedDistance;

            // Rehydrate update interval from ticks using known options if possible.
            var interval =
                Utilities.UpdateIntervalOptions?.Find( ui => ui != null && ui.ticks == rec.updateIntervalTicks );
            job.UpdateInterval = interval ?? Settings.DefaultUpdateInterval;
        }

        public static ThresholdTriggerConfig ToConfig( this Trigger_Threshold trigger )
        {
            if ( trigger == null )
                return null;

            return new ThresholdTriggerConfig
            {
                op = trigger.Op,
                targetCount = trigger.TargetCount,
                maxUpperThreshold = trigger.MaxUpperThreshold,
                countAllOnMap = trigger.countAllOnMap,
                stockpileLabel = trigger.stockpile?.label,
                thresholdFilter = trigger.ThresholdFilter
            };
        }

        public static void ApplyConfig( this Trigger_Threshold trigger, ThresholdTriggerConfig cfg, Map map )
        {
            if ( trigger == null || cfg == null )
                return;

            trigger.Op = cfg.op;
            trigger.TargetCount = cfg.targetCount;
            trigger.MaxUpperThreshold = cfg.maxUpperThreshold;
            trigger.countAllOnMap = cfg.countAllOnMap;
            trigger.ThresholdFilter = cfg.thresholdFilter ?? trigger.ThresholdFilter ?? new ThingFilter();

            if ( map != null && !cfg.stockpileLabel.NullOrEmpty() )
            {
                try
                {
                    trigger.stockpile = map.zoneManager.AllZones
                                       .FirstOrDefault( z => z is Zone_Stockpile && z.label == cfg.stockpileLabel ) as
                        Zone_Stockpile;
                }
                catch
                {
                    trigger.stockpile = null;
                }
            }
            else
            {
                trigger.stockpile = null;
            }
        }

        public static Area FindAreaByLabel( Map map, string label )
        {
            if ( map == null || label.NullOrEmpty() )
                return null;

            // Prefer exact label match.
            var match = map.areaManager.AllAreas.FirstOrDefault( a => a != null && a.Label == label );
            return match;
        }
    }
}


