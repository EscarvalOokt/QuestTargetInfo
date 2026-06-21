using Verse;

namespace QuestTargetInfo
{
    internal class QuestTargetInfoMod : Mod
    {
        public QuestTargetInfoMod(ModContentPack content) : base(content)
        {
            var harmony = new HarmonyLib.Harmony("questtargetinfo.patch");
            harmony.PatchAll();
            Log.Message("[QuestTargetInfo] Initialized");
        }
    }
}