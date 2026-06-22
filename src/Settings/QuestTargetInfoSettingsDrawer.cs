using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal static class QuestTargetInfoSettingsDrawer
    {
        public static void Draw(
            Rect rect,
            QuestTargetInfoSettings settings)
        {
            if(settings == null)
                return;

            var listing = new Listing_Standard();
            listing.Begin(rect);

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowQuestTravelWindow,
                "QuestTargetInfo.SettingShowQuestTravelWindow");

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowWorldInspectTravelTab,
                "QuestTargetInfo.SettingShowWorldInspectTravelTab");

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowAncientPodCompatibilityLine,
                "QuestTargetInfo.SettingShowAncientPodCompatibilityLine");

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowLayerAdjustedDistance,
                "QuestTargetInfo.SettingShowFuelAdjustedDistance");

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowUnavailableTransports,
                "QuestTargetInfo.SettingShowUnavailableTransports");

            DrawCheckbox(
                listing,
                settings,
                ref settings.CompactMode,
                "QuestTargetInfo.SettingCompactMode");

            listing.GapLine();
            listing.Label(
                "QuestTargetInfo.SettingTransportVisibility".Translate().ToString());
            listing.Gap();

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowTransportPod,
                "QuestTargetInfo.SettingShowTransportPod");

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowShuttle,
                "QuestTargetInfo.SettingShowShuttle");

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowGravship,
                "QuestTargetInfo.SettingShowGravship");

            listing.End();
        }

        private static void DrawCheckbox(
            Listing_Standard listing,
            QuestTargetInfoSettings settings,
            ref bool value,
            string labelKey)
        {
            bool oldValue = value;

            listing.CheckboxLabeled(
                labelKey.Translate().ToString(),
                ref value);

            if(value != oldValue)
                settings.NotifyChanged();
        }
    }
}