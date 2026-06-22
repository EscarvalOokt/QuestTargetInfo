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

        private WorldTargetInfoModel _cachedModel;
        private PlanetTile _cachedOriginTile = PlanetTile.Invalid;
        private PlanetTile _cachedTargetTile = PlanetTile.Invalid;

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

            if(ShouldClose())
                Close();
        }

        public override void PreOpen()
        {
            base.PreOpen();

            windowRect = GetInitialWindowRect();
        }

        public override void DoWindowContents(Rect inRect)
        {
            const float headerHeight = 30f;

            var headerRect = new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
            DrawHeaderLabel(headerRect);
            DrawCollapseButton(headerRect);

            if(_collapsed)
                return;

            Widgets.DrawLineHorizontal(
                inRect.x,
                inRect.y + headerHeight - 1f,
                inRect.width,
                Color.gray);

            WorldTargetInfoModel model = GetModelCached();
            if(model == null || model.Sections.Count == 0)
            {
                Close();
                return;
            }

            var contentRect = new Rect(
                inRect.x,
                inRect.y + headerHeight + 10f,
                inRect.width,
                inRect.height - headerHeight - 10f);

            WorldTargetInfoDrawer.Draw(
                contentRect,
                model,
                WorldTargetInfoDrawOptions.ForQuestWindow());
        }

        private bool ShouldClose()
        {
            switch(_request.Source)
            {
                case WorldTargetInfoSource.Quest:
                    return !Find.WindowStack.Windows.OfType<MainTabWindow_Quests>().Any();

                case WorldTargetInfoSource.WorldInspectPane:
                    return false;

                default:
                    return false;
            }
        }

        private Rect GetInitialWindowRect()
        {
            switch(_request.Source)
            {
                case WorldTargetInfoSource.Quest:
                    return GetQuestWindowRect();

                case WorldTargetInfoSource.WorldInspectPane:
                    return GetWorldInspectPaneWindowRect();

                default:
                    return GetQuestWindowRect();
            }
        }

        private Rect GetQuestWindowRect()
        {
            float spacing = 5f;

            return new Rect(
                1010f + spacing,
                UI.screenHeight - 35f - Height,
                InitialSize.x,
                Height);
        }

        private Rect GetWorldInspectPaneWindowRect()
        {
            float spacing = 12f;

            return new Rect(
                UI.screenWidth - InitialSize.x - spacing,
                UI.screenHeight - 35f - Height,
                InitialSize.x,
                Height);
        }

        private void DrawHeaderLabel(Rect rect)
        {
            Text.Anchor = TextAnchor.MiddleLeft;
            Text.Font = GameFont.Small;

            var labelRect = new Rect(rect.x, rect.y, rect.width - 40f, rect.height);
            Widgets.Label(labelRect, "QuestTargetInfo.Header".Translate());

            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawCollapseButton(Rect rect)
        {
            var buttonRect = new Rect(rect.xMax - 28f, rect.y + 2f, 24f, 24f);
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

        private WorldTargetInfoModel GetModelCached()
        {
            if(_cachedModel == null
                || _cachedOriginTile != _request.OriginTile
                || _cachedTargetTile != _request.TargetTile)
            {
                _cachedModel = WorldTargetInfoModelBuilder.Build(_request);
                _cachedOriginTile = _request.OriginTile;
                _cachedTargetTile = _request.TargetTile;
            }

            return _cachedModel;
        }
    }
}