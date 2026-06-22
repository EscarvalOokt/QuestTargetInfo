using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoDrawer
    {
        private const float HeaderStatusWidthFactor = 0.45f;
        private const float HeaderColumnGap = 8f;

        public static float Draw(
            Rect rect,
            WorldTargetInfoModel model,
            WorldTargetInfoDrawOptions options)
        {
            if(model == null)
                return 0f;

            options = options ?? WorldTargetInfoDrawOptions.ForWorldInspectTab();

            GameFont oldFont = Text.Font;
            TextAnchor oldAnchor = Text.Anchor;
            Color oldColor = GUI.color;

            try
            {
                Text.Anchor = TextAnchor.UpperLeft;

                float curY = rect.y;
                bool hasDrawnContent = false;

                if(options.DrawTitle && !model.Title.NullOrEmpty())
                {
                    Text.Font = options.TitleFont;

                    curY = DrawLine(
                        rect,
                        curY,
                        model.Title,
                        options.LineGap);

                    curY += options.TitleBottomGap;
                    hasDrawnContent = true;
                }

                Text.Font = options.BodyFont;

                foreach(WorldTargetInfoSectionModel section in model.Sections)
                {
                    if(!HasDrawableContent(section))
                        continue;

                    if(hasDrawnContent && section.Kind != WorldTargetInfoSectionKind.Route)
                        curY += options.SectionGap;

                    curY = DrawSection(
                        rect,
                        curY,
                        section,
                        options);

                    hasDrawnContent = true;
                }

                return Mathf.Max(0f, curY - rect.y);
            }
            finally
            {
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
                GUI.color = oldColor;
            }
        }

        private static float DrawSection(
            Rect rect,
            float curY,
            WorldTargetInfoSectionModel section,
            WorldTargetInfoDrawOptions options)
        {
            curY = DrawSectionHeader(
                rect,
                curY,
                section,
                options.LineGap);

            foreach(WorldTargetInfoLineModel line in section.Lines)
            {
                string text = WorldTargetInfoLineFormatter.FormatLine(line);

                if(text.NullOrEmpty())
                    continue;

                curY = DrawLine(
                    rect,
                    curY,
                    text,
                    options.LineGap);
            }

            return curY;
        }

        private static float DrawSectionHeader(
            Rect rect,
            float curY,
            WorldTargetInfoSectionModel section,
            float lineGap)
        {
            if(section.Kind == WorldTargetInfoSectionKind.Route)
                return curY;

            if(section.Title.NullOrEmpty() && section.StatusText.NullOrEmpty())
                return curY;

            if(section.StatusText.NullOrEmpty())
            {
                return DrawLine(
                    rect,
                    curY,
                    section.Title,
                    lineGap);
            }

            float statusWidth = rect.width * HeaderStatusWidthFactor;
            float titleWidth = rect.width - statusWidth - HeaderColumnGap;

            var titleRect = new Rect(
                rect.x,
                curY,
                titleWidth,
                99999f);

            var statusRect = new Rect(
                rect.xMax - statusWidth,
                curY,
                statusWidth,
                99999f);

            float titleHeight = section.Title.NullOrEmpty()
                ? 0f
                : Text.CalcHeight(section.Title, titleRect.width);

            float statusHeight = Text.CalcHeight(
                section.StatusText,
                statusRect.width);

            float height = Mathf.Max(titleHeight, statusHeight);

            titleRect.height = height;
            statusRect.height = height;

            TextAnchor oldAnchor = Text.Anchor;

            if(!section.Title.NullOrEmpty())
            {
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(titleRect, section.Title);
            }

            Text.Anchor = TextAnchor.UpperRight;
            Widgets.Label(statusRect, section.StatusText);

            Text.Anchor = oldAnchor;

            return curY + height + lineGap;
        }

        private static float DrawLine(
            Rect rect,
            float curY,
            string text,
            float lineGap)
        {
            if(text.NullOrEmpty())
                return curY;

            float height = Text.CalcHeight(text, rect.width);

            var lineRect = new Rect(
                rect.x,
                curY,
                rect.width,
                height);

            Widgets.Label(lineRect, text);

            return curY + height + lineGap;
        }

        private static bool HasDrawableContent(
            WorldTargetInfoSectionModel section)
        {
            if(section == null)
                return false;

            if(!section.Title.NullOrEmpty())
                return true;

            if(!section.StatusText.NullOrEmpty())
                return true;

            if(section.Lines == null)
                return false;

            return section.Lines.Count > 0;
        }
    }
}