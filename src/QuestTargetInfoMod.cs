using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace QuestTargetInfo
{
    public class QuestTargetInfoMod : Mod
    {
        public QuestTargetInfoMod(ModContentPack content) : base(content)
        {
            try
            {
                var harmony = new Harmony("escarval.questtargetinfo");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Log.Message("[QuestTargetInfo] Initialized");
            }
            catch(Exception ex)
            {
                Log.Error("[QuestTargetInfo] Failed to initialize:\n" + ex);
                throw;
            }
        }
    }
}