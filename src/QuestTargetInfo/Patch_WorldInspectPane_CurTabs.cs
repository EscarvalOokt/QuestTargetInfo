using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using QuestTargetInfo.Info;
using RimWorld.Planet;
using Verse;

namespace QuestTargetInfo
{
    [HarmonyPatch(typeof(WorldInspectPane), "get_CurTabs")]
    // ReSharper disable once InconsistentNaming
    public static class Patch_WorldInspectPane_CurTabs
    {
        private static readonly WITab_WorldTargetInfo _targetInfoTab = new WITab_WorldTargetInfo();

        // ReSharper disable once InconsistentNaming
        public static void Postfix(ref IEnumerable<InspectTabBase> __result)
        {
            if (!QuestTargetInfoMod.Settings.ShowWorldInspectTravelTab)
            {
                _targetInfoTab.Notify_RequestUnavailable();
                return;
            }

            if (!WorldTargetInfoSelectionUtility.TryCreateRequest(out WorldTargetInfoRequest _))
            {
                _targetInfoTab.Notify_RequestUnavailable();
                return;
            }

            List<InspectTabBase> tabs = __result?.ToList() ?? new List<InspectTabBase>();

            if (tabs.Any(tab => tab.GetType() == typeof(WITab_WorldTargetInfo)))
                return;

            tabs.Add(_targetInfoTab);
            __result = tabs;
        }
    }
}