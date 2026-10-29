using QuestTargetInfo.Info.ViewModels;
using QuestTargetInfo.Runtime;
using QuestTargetInfo.UI;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    // ReSharper disable once InconsistentNaming
    internal class WITab_WorldTargetInfo : WITab
    {
        private static readonly Vector2 _winSize = new Vector2(360f, 480f);
        private readonly WorldTargetInfoCache _cache = new();
        private float _lastDrawnHeight;
        private Vector2 _scrollPosition;

        public WITab_WorldTargetInfo()
        {
            size = _winSize;
            labelKey = "QuestTargetInfo.TabLabel";
        }

        public override bool IsVisible
        {
            get
            {
                bool visible = QuestTargetInfoMod.Settings.ShowWorldInspectTravelTab &&
                               WorldTargetInfoSelectionUtility.TryCreateContext(out _);
                if (!visible)
                    Notify_RequestUnavailable();
                return visible;
            }
        }

        internal void Notify_RequestUnavailable()
        {
            _cache.Clear();
            ResetScrollState();
        }

        protected override void FillTab()
        {
            if (!QuestTargetInfoMod.Settings.ShowWorldInspectTravelTab ||
                !WorldTargetInfoSelectionUtility.TryCreateContext(out WorldTargetInfoContext context))
            {
                Notify_RequestUnavailable();
                return;
            }

            WorldTargetInfoModel model = _cache.Get(context, out bool selectionChanged);
            if (selectionChanged)
                ResetScrollState();

            Rect outRect = new Rect(0, 0, _winSize.x, _winSize.y).ContractedBy(10f);
            var viewRect = new Rect(0, 0, outRect.width - 16f, Mathf.Max(_lastDrawnHeight, outRect.height));
            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);
            try
            {
                _lastDrawnHeight = WorldTargetInfoDrawer.Draw(
                    viewRect,
                    model,
                    WorldTargetInfoDrawOptions.ForWorldInspectTab());
            }
            finally
            {
                Widgets.EndScrollView();
            }
        }

        private void ResetScrollState()
        {
            _scrollPosition = Vector2.zero;
            _lastDrawnHeight = 0;
        }
    }
}