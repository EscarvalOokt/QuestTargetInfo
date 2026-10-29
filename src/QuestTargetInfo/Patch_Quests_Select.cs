using HarmonyLib;
using RimWorld;

namespace QuestTargetInfo
{
    [HarmonyPatch(typeof(MainTabWindow_Quests), nameof(MainTabWindow_Quests.Select), new[] { typeof(Quest) })]
    // ReSharper disable once InconsistentNaming
    public static class Patch_Quests_Select
    {
        // ReSharper disable once InconsistentNaming
        public static void Postfix(MainTabWindow_Quests __instance, Quest quest)
        {
            WorldTargetInfoWindowUtility.Synchronize(__instance, quest);
        }
    }
}