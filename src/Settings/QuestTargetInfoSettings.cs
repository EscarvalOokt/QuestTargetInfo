using Verse;

namespace QuestTargetInfo
{
    internal sealed class QuestTargetInfoSettings : ModSettings
    {
        public bool ShowQuestTravelWindow = true;
        public bool ShowWorldInspectTravelTab = true;
        public bool ShowAncientPodCompatibilityLine = true;
        public bool ShowFuelAdjustedDistance = true;
        public bool ShowUnavailableTransports = true;

        public int Version { get; private set; }

        public override void ExposeData()
        {
            Scribe_Values.Look(
                ref ShowQuestTravelWindow,
                "showQuestTravelWindow",
                true);

            Scribe_Values.Look(
                ref ShowWorldInspectTravelTab,
                "showWorldInspectTravelTab",
                true);

            Scribe_Values.Look(
                ref ShowAncientPodCompatibilityLine,
                "showAncientPodCompatibilityLine",
                true);

            Scribe_Values.Look(
                ref ShowFuelAdjustedDistance,
                "showFuelAdjustedDistance",
                true);

            Scribe_Values.Look(
                ref ShowUnavailableTransports,
                "showUnavailableTransports",
                true);
        }

        public void NotifyChanged()
        {
            Version++;
        }
    }
}