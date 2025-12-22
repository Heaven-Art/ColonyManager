using System;
using HarmonyLib;
using Verse;

namespace FluffyManager
{
    internal static class Harmony_MapLifecycle
    {
        public static void Apply( Harmony harmony )
        {
            if ( harmony == null )
                return;

            // Odyssey 1.6: gravship can swap maps without deinit/remove; Things receive PreSwapMap/PostSwapMap callbacks.
            // We hook these to transfer manager profiles using the traveling manager-station buildings as carriers.
            var patchedPreSwapMap = PatchIfExists(
                harmony,
                "Verse.Thing",
                "PreSwapMap",
                prefix: new HarmonyMethod( typeof( Harmony_MapLifecycle ), nameof( Thing_PreSwapMap_Prefix ) ) );

            var patchedPostSwapMap = PatchIfExists(
                harmony,
                "Verse.Thing",
                "PostSwapMap",
                postfix: new HarmonyMethod( typeof( Harmony_MapLifecycle ), nameof( Thing_PostSwapMap_Postfix ) ) );
        }

        private static bool PatchIfExists( Harmony harmony, string typeName, string methodName,
                                           HarmonyMethod prefix = null, HarmonyMethod postfix = null )
        {
            try
            {
                var type = AccessTools.TypeByName( typeName );
                if ( type == null )
                    return false;

                var method = AccessTools.Method( type, methodName );
                if ( method == null )
                    return false;

                harmony.Patch( method, prefix, postfix );
                return true;
            }
            catch ( Exception err )
            {
                Logger.Warning( $"Failed patching {typeName}.{methodName}: {err}" );
                return false;
            }
        }

        public static void Thing_PreSwapMap_Prefix( Thing __instance )
        {
            try
            {
                MapSwapManagerPersistence.OnThingPreSwapMap( __instance );
            }
            catch ( Exception err )
            {
                Logger.Warning( $"SwapMap persistence snapshot failed: {err}" );
            }
        }

        public static void Thing_PostSwapMap_Postfix( Thing __instance )
        {
            try
            {
                MapSwapManagerPersistence.OnThingPostSwapMap( __instance );
            }
            catch ( Exception err )
            {
                Logger.Warning( $"SwapMap persistence rehydrate failed: {err}" );
            }
        }
    }
}




