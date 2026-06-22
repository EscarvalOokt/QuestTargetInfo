using Verse;

namespace QuestTargetInfo
{
    internal sealed class WorldTargetInfoDrawOptions
    {
        public static WorldTargetInfoDrawOptions ForWorldInspectTab()
        {
            return new WorldTargetInfoDrawOptions
            {
                DrawTitle = true,
                TitleFont = GameFont.Medium,
                BodyFont = GameFont.Small,
                TitleBottomGap = 5f,
                SectionGap = 12f,
                LineGap = 2f
            };
        }

        public static WorldTargetInfoDrawOptions ForQuestWindow()
        {
            return new WorldTargetInfoDrawOptions
            {
                DrawTitle = false,
                TitleFont = GameFont.Medium,
                BodyFont = GameFont.Small,
                TitleBottomGap = 5f,
                SectionGap = 12f,
                LineGap = 2f
            };
        }

        public bool DrawTitle { get; private set; }

        public GameFont TitleFont { get; private set; }

        public GameFont BodyFont { get; private set; }

        public float TitleBottomGap { get; private set; }

        public float SectionGap { get; private set; }

        public float LineGap { get; private set; }
    }
}