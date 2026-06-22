using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace QuestTargetInfo
{
    [HarmonyPatch(typeof(WorldInspectPane), "get_CurTabs")]
    public static class Patch_WorldInspectPane_CurTabs
    {
        private static readonly WITab_WorldTargetInfo TargetInfoTab =
            new WITab_WorldTargetInfo();

        public static void Postfix(ref IEnumerable<InspectTabBase> __result)
        {
            if(!WorldTargetInfoSelectionUtility.TryCreateRequest(
                out WorldTargetInfoRequest _))
            {
                return;
            }

            List<InspectTabBase> tabs = __result?.ToList()
                ?? new List<InspectTabBase>();

            if(tabs.Any(tab => tab.GetType() == typeof(WITab_WorldTargetInfo)))
                return;

            tabs.Add(TargetInfoTab);
            __result = tabs;
        }
    }
}