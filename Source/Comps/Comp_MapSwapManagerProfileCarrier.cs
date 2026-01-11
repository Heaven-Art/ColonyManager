// Comp_MapSwapManagerProfileCarrier.cs
// Stores a serializable ManagerProfile on a moving manager station Thing so it can be restored after Odyssey SwapMap.

using Verse;

namespace FluffyManager
{
    public class Comp_MapSwapManagerProfileCarrier : ThingComp
    {
        public ManagerProfile profile;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look( ref profile, "FM_MapSwapManagerProfile" );
        }
    }
}


