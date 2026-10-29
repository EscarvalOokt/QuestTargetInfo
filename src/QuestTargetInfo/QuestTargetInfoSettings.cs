using QuestTargetInfo.Info;
using Verse;

namespace QuestTargetInfo
{
    internal sealed class QuestTargetInfoSettings : ModSettings
    {
        // Keep the default explicit alongside the other settings.
        // ReSharper disable once RedundantDefaultMemberInitializer
        public bool CompactMode = false;
        public bool ShowAncientPodCompatibilityLine = true;
        public bool ShowGravship = true;
        public bool ShowLayerAdjustedDistance = true;
        public bool ShowQuestTravelWindow = true;
        public bool ShowShuttle = true;
        public bool ShowTransportPod = true;
        public bool ShowUnavailableTransports = true;
        public bool ShowWorldInspectTravelTab = true;

        public int Version { get; private set; }

        public bool ShouldShowAncientPodCompatibilityLine => !CompactMode && ShowAncientPodCompatibilityLine;

        public bool ShouldShowLayerAdjustedDistance => !CompactMode && ShowLayerAdjustedDistance;

        public bool ShouldShowTransportDetails => !CompactMode;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ShowQuestTravelWindow, "showQuestTravelWindow", true);

            Scribe_Values.Look(ref ShowWorldInspectTravelTab, "showWorldInspectTravelTab", true);

            Scribe_Values.Look(ref ShowAncientPodCompatibilityLine, "showAncientPodCompatibilityLine", true);

            Scribe_Values.Look(ref ShowLayerAdjustedDistance, "showLayerAdjustedDistance", true);

            Scribe_Values.Look(ref ShowUnavailableTransports, "showUnavailableTransports", true);

            Scribe_Values.Look(ref CompactMode, "compactMode");

            Scribe_Values.Look(ref ShowTransportPod, "showTransportPod", true);

            Scribe_Values.Look(ref ShowShuttle, "showShuttle", true);

            Scribe_Values.Look(ref ShowGravship, "showGravship", true);
        }

        public bool IsTransportVisible(WorldTargetTransportKind kind)
        {
            switch (kind)
            {
                case WorldTargetTransportKind.TransportPod:
                    return ShowTransportPod;

                case WorldTargetTransportKind.Shuttle:
                    return ShowShuttle;

                case WorldTargetTransportKind.Gravship:
                    return ShowGravship;

                default:
                    return true;
            }
        }

        public void NotifyChanged()
        {
            Version++;
        }
    }
}