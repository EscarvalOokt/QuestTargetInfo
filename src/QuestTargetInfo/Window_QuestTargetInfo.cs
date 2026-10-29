using QuestTargetInfo.Info.ViewModels;
using QuestTargetInfo.Runtime;
using QuestTargetInfo.UI;
using RimWorld;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    // ReSharper disable once InconsistentNaming
    internal class Window_QuestTargetInfo : Window
    {
        private static bool _collapsed;
        private readonly WorldTargetInfoCache _cache = new();
        private readonly MainTabWindow_Quests _owner;
        private readonly Quest _quest;
        private float _lastDrawnHeight;
        private Vector2 _scrollPosition;

        public Window_QuestTargetInfo(MainTabWindow_Quests owner, Quest quest)
        {
            _owner = owner;
            _quest = quest;
            draggable = false;
            absorbInputAroundWindow = false;
            closeOnAccept = false;
            closeOnCancel = false;
            resizeable = false;
            preventCameraMotion = false;
            focusWhenOpened = false;
            // Stay above the quest pane when a narrow viewport requires their bounds to overlap.
            // Select can also run inside the pane's PreOpen, before that pane enters WindowStack.
            layer = WindowLayer.Dialog;
        }

        protected override float Margin => 10f;

        public override Vector2 InitialSize =>
            new Vector2(QuestTargetWindowLayout.PreferredWidth, QuestTargetWindowLayout.PreferredHeight);

        internal bool IsFor(MainTabWindow_Quests owner, Quest quest) =>
            ReferenceEquals(_owner, owner) && ReferenceEquals(_quest, quest);

        public override void WindowUpdate()
        {
            base.WindowUpdate();
            if (!HasOwner() || !WorldTargetInfoContext.TryCreateForQuest(_quest, out _))
            {
                Close();
                return;
            }

            UpdateLayout();
        }

        public override void PreOpen()
        {
            base.PreOpen();
            UpdateLayout();
        }

        public override void PostClose()
        {
            _cache.Clear();
            base.PostClose();
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (!HasOwner() || !WorldTargetInfoContext.TryCreateForQuest(_quest, out WorldTargetInfoContext context))
            {
                Close();
                return;
            }

            const float headerHeight = 30f;
            var headerRect = new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
            DrawHeader(headerRect);
            if (_collapsed)
                return;

            Widgets.DrawLineHorizontal(inRect.x, inRect.y + headerHeight - 1f, inRect.width, Color.gray);

            WorldTargetInfoModel model = _cache.Get(context, out bool selectionChanged);
            if (selectionChanged)
            {
                _scrollPosition = Vector2.zero;
                _lastDrawnHeight = 0;
            }

            var outRect = new Rect(
                inRect.x,
                inRect.y + headerHeight + 10f,
                inRect.width,
                Mathf.Max(0, inRect.height - headerHeight - 10f));
            if (outRect.width <= 16f || outRect.height <= 0)
                return;

            var viewRect = new Rect(0, 0, outRect.width - 16f, Mathf.Max(_lastDrawnHeight, outRect.height));
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
            try
            {
                _lastDrawnHeight = WorldTargetInfoDrawer.Draw(
                    viewRect,
                    model,
                    WorldTargetInfoDrawOptions.ForQuestWindow());
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

        private bool HasOwner()
        {
            return Current.ProgramState == ProgramState.Playing &&
                   QuestTargetInfoMod.Settings.ShowQuestTravelWindow &&
                   Find.WindowStack != null &&
                   Find.WindowStack.Windows.Contains(_owner);
        }

        private void UpdateLayout()
        {
            windowRect = QuestTargetWindowLayout.Calculate(
                _owner.windowRect,
                Verse.UI.screenWidth,
                Verse.UI.screenHeight,
                _collapsed);
        }

        private void DrawHeader(Rect rect)
        {
            TextAnchor previousAnchor = Text.Anchor;
            GameFont previousFont = Text.Font;
            try
            {
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.Font = GameFont.Small;
                Widgets.Label(
                    new Rect(rect.x, rect.y, Mathf.Max(0, rect.width - 40f), rect.height),
                    "QuestTargetInfo.Header".Translate());
            }
            finally
            {
                Text.Anchor = previousAnchor;
                Text.Font = previousFont;
            }

            var buttonRect = new Rect(rect.xMax - 28f, rect.y + 2f, 24f, 24f);
            Texture2D icon = _collapsed ? TexButton.ReorderUp : TexButton.ReorderDown;
            if (Widgets.ButtonImage(buttonRect, icon))
            {
                _collapsed = !_collapsed;
                UpdateLayout();
            }

            TooltipHandler.TipRegion(
                buttonRect,
                _collapsed
                    ? "QuestTargetInfo.ExpandTooltip".Translate()
                    : "QuestTargetInfo.CollapseTooltip".Translate());
        }
    }
}