using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using System.Linq;
using Verse;

namespace QuestTargetInfo
{
    [HarmonyPatch(typeof(MainTabWindow_Quests), "Select")]
    public static class Patch_Quests_Select
    {
        public static void Postfix(MainTabWindow_Quests __instance)
        {
            CloseOpenedWindows();

            var questField = AccessTools.Field(typeof(MainTabWindow_Quests), "selected");
            if (!(questField.GetValue(__instance) is Quest currentQuest))
                return;

            if (currentQuest.QuestLookTargets.Count() != 1)
                return;

            var target = currentQuest.QuestLookTargets.FirstOrDefault();
            if (!IsValid(target))
                return;

            Find.WindowStack.Add(new Window_QuestTargetInfo(target));
        }

        private static void CloseOpenedWindows()
        {
            if (Find.WindowStack.Windows.OfType<Window_QuestTargetInfo>().Any())
            {
                var infoWindows = Find.WindowStack.Windows
                    .OfType<Window_QuestTargetInfo>()
                    .ToList();
                foreach (var window in infoWindows)
                    Find.WindowStack.TryRemove(window);
            }
        }
        private static bool IsValid(GlobalTargetInfo target) =>
            target.IsValid && target.IsWorldTarget && target.Tile >= 0 && target.Tile < Find.WorldGrid.TilesCount && target.Label != "Location".Translate();
    }
}