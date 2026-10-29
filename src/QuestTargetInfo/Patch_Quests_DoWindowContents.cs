using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace QuestTargetInfo
{
    [HarmonyPatch(typeof(MainTabWindow_Quests), nameof(MainTabWindow_Quests.DoWindowContents), new[] { typeof(Rect) })]
    // ReSharper disable once InconsistentNaming
    public static class Patch_Quests_DoWindowContents
    {
        // Category changes and dismissing a quest can assign selected without calling Select.
        // Harmony reads the field directly in the generated patch; no reflection lookup is needed here.
        // ReSharper disable once InconsistentNaming
        public static void Postfix(MainTabWindow_Quests __instance, Quest ___selected)
        {
            WorldTargetInfoWindowUtility.Synchronize(__instance, ___selected);
        }
    }
}