using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal class Window_QuestTargetInfo : Window
    {
        private const float CollapsedHeight = 60f;

        private static bool _collapsed = false;

        private readonly WorldTargetInfoRequest _request;

        private List<string> _cachedLines;
        private PlanetTile _cachedTile = PlanetTile.Invalid;

        private float Height => _collapsed ? CollapsedHeight : InitialSize.y;

        public override Vector2 InitialSize => new Vector2(250f, 640f);

        public Window_QuestTargetInfo(WorldTargetInfoRequest request)
        {
            _request = request;
            draggable = false;
            absorbInputAroundWindow = false;
            closeOnAccept = false;
            closeOnCancel = false;
            resizeable = false;
            preventCameraMotion = false;
            focusWhenOpened = false;
            layer = WindowLayer.GameUI;
        }

        public override void WindowUpdate()
        {
            base.WindowUpdate();

            if(!Find.WindowStack.Windows.OfType<MainTabWindow_Quests>().Any())
                Close();
        }

        public override void PreOpen()
        {
            base.PreOpen();

            float spacing = 5f;

            windowRect = new Rect(
                1010f + spacing,
                UI.screenHeight - 35 - Height,
                InitialSize.x,
                Height);
        }

        public override void DoWindowContents(Rect inRect)
        {
            const float headerHeight = 30f;

            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
            DrawHeaderLabel(headerRect);
            DrawCollapseButton(headerRect);

            if(_collapsed)
                return;

            Widgets.DrawLineHorizontal(inRect.x, inRect.y + headerHeight - 1f, inRect.width, Color.gray);

            List<string> lines = GetLinesCached();
            if(lines.Count == 0)
            {
                Close();
                return;
            }

            Rect contentRect = new Rect(
                inRect.x,
                inRect.y + headerHeight + 10f,
                inRect.width,
                inRect.height - headerHeight - 10f);

            var listing = new Listing_Standard();
            listing.Begin(contentRect);

            foreach(string line in lines)
                listing.Label(line);

            listing.End();
        }

        private void DrawHeaderLabel(Rect rect)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;

            Rect labelRect = new Rect(rect.x, rect.y, rect.width - 40f, rect.height);
            Widgets.Label(labelRect, "QuestTargetInfo.Header".Translate());

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawCollapseButton(Rect rect)
        {
            Rect buttonRect = new Rect(rect.xMax - 28f, rect.y + 2f, 24f, 24f);
            Texture2D icon = _collapsed ? TexButton.ReorderUp : TexButton.ReorderDown;

            if(Widgets.ButtonImage(buttonRect, icon))
            {
                _collapsed = !_collapsed;

                float bottomY = windowRect.yMax;
                windowRect.yMin = bottomY - Height;
                windowRect.height = Height;
            }

            TooltipHandler.TipRegion(
                buttonRect,
                _collapsed
                    ? "QuestTargetInfo.ExpandTooltip".Translate()
                    : "QuestTargetInfo.CollapseTooltip".Translate());
        }

        private List<string> GetLinesCached()
        {
            if(_cachedLines == null || _cachedTile != _request.TargetTile)
            {
                _cachedLines = WorldTargetInfoLineBuilder.BuildLines(_request).ToList();
                _cachedTile = _request.TargetTile;
            }

            return _cachedLines;
        }
    }
}