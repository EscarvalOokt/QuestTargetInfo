using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    public class QuestTargetInfoMod : Mod
    {
        internal static QuestTargetInfoSettings Settings { get; private set; } =
            new QuestTargetInfoSettings();

        public QuestTargetInfoMod(ModContentPack content) : base(content)
        {
            try
            {
                Settings = GetSettings<QuestTargetInfoSettings>();

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

        public override string SettingsCategory()
        {
            return "QuestTargetInfo.SettingsCategory".Translate().ToString();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            QuestTargetInfoSettingsDrawer.Draw(inRect, Settings);
        }
    }
}