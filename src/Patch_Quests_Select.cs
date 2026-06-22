using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
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
            if(!(questField.GetValue(__instance) is Quest currentQuest))
                return;

            if(currentQuest.QuestLookTargets.Count() != 1)
                return;

            GlobalTargetInfo target = currentQuest.QuestLookTargets.FirstOrDefault();
            if(!TryCreateRequest(target, out WorldTargetInfoRequest request))
                return;

            Find.WindowStack.Add(new Window_QuestTargetInfo(request));
        }

        private static void CloseOpenedWindows()
        {
            if(!Find.WindowStack.Windows.OfType<Window_QuestTargetInfo>().Any())
                return;

            var infoWindows = Find.WindowStack.Windows
                .OfType<Window_QuestTargetInfo>()
                .ToList();

            foreach(Window_QuestTargetInfo window in infoWindows)
                Find.WindowStack.TryRemove(window);
        }

        private static bool TryCreateRequest(
            GlobalTargetInfo target,
            out WorldTargetInfoRequest request)
        {
            request = null;

            if(!IsValid(target))
                return false;

            PlanetTile originTile = Find.CurrentMap?.Tile ?? PlanetTile.Invalid;
            if(!originTile.Valid)
                return false;

            WorldObject targetObject = target.HasWorldObject ? target.WorldObject : null;

            request = new WorldTargetInfoRequest(
                originTile,
                target.Tile,
                target.Label,
                targetObject,
                WorldTargetInfoSource.Quest);

            return true;
        }

        private static bool IsValid(GlobalTargetInfo target)
        {
            return target.IsValid
                && target.IsWorldTarget
                && target.Tile.Valid
                && target.Label != "Location".Translate();
        }
    }
}