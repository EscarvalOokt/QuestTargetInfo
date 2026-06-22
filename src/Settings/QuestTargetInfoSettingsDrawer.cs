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
                ref settings.ShowFuelAdjustedDistance,
                "QuestTargetInfo.SettingShowFuelAdjustedDistance");

            DrawCheckbox(
                listing,
                settings,
                ref settings.ShowUnavailableTransports,
                "QuestTargetInfo.SettingShowUnavailableTransports");

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