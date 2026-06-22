using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal sealed class WorldTargetInfoDrawOptions
    {
        public static WorldTargetInfoDrawOptions ForWorldInspectTab()
        {
            return CreateDefault(
                drawTitle: true);
        }

        public static WorldTargetInfoDrawOptions ForQuestWindow()
        {
            return CreateDefault(
                drawTitle: false);
        }

        private static WorldTargetInfoDrawOptions CreateDefault(
            bool drawTitle)
        {
            return new WorldTargetInfoDrawOptions
            {
                DrawTitle = drawTitle,
                TitleFont = GameFont.Medium,
                BodyFont = GameFont.Small,
                TitleBottomGap = 5f,
                SectionGap = 12f,
                LineGap = 2f,

                // Muted RimWorld-friendly status colors.
                AvailableStatusColor = new Color(0.45f, 0.9f, 0.45f),
                WarningStatusColor = new Color(1f, 0.82f, 0.35f),
                ErrorStatusColor = new Color(1f, 0.45f, 0.35f),
                UnavailableStatusColor = new Color(0.6f, 0.6f, 0.6f)
            };
        }

        public bool DrawTitle { get; private set; }

        public GameFont TitleFont { get; private set; }

        public GameFont BodyFont { get; private set; }

        public float TitleBottomGap { get; private set; }

        public float SectionGap { get; private set; }

        public float LineGap { get; private set; }

        public Color AvailableStatusColor { get; private set; }

        public Color WarningStatusColor { get; private set; }

        public Color ErrorStatusColor { get; private set; }

        public Color UnavailableStatusColor { get; private set; }
    }
}