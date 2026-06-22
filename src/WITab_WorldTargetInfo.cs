using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal class WITab_WorldTargetInfo : WITab
    {
        private static readonly Vector2 WinSize = new Vector2(320f, 400f);

        private Vector2 _scrollPosition;
        private float _lastDrawnHeight;

        private bool _hasLastRequestKey;
        private WorldTargetInfoRequestKey _lastRequestKey;

        public override bool IsVisible
        {
            get
            {
                bool visible = WorldTargetInfoSelectionUtility.TryCreateRequest(
                    out WorldTargetInfoRequest _);

                if(!visible)
                    Notify_RequestUnavailable();

                return visible;
            }
        }

        public WITab_WorldTargetInfo()
        {
            size = WinSize;
            labelKey = "QuestTargetInfo.TabLabel";
        }

        internal void Notify_RequestUnavailable()
        {
            _hasLastRequestKey = false;
            ResetScrollState();
        }

        protected override void FillTab()
        {
            if(!WorldTargetInfoSelectionUtility.TryCreateRequest(
                out WorldTargetInfoRequest request))
            {
                Notify_RequestUnavailable();
                return;
            }

            ResetScrollIfTargetChanged(request);

            WorldTargetInfoModel model = WorldTargetInfoModelBuilder.Build(request);

            Rect outRect = new Rect(0f, 0f, WinSize.x, WinSize.y)
                .ContractedBy(10f);

            var viewRect = new Rect(
                0f,
                0f,
                outRect.width - 16f,
                Mathf.Max(_lastDrawnHeight, outRect.height));

            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            _lastDrawnHeight = WorldTargetInfoDrawer.Draw(
                viewRect,
                model,
                WorldTargetInfoDrawOptions.ForWorldInspectTab());

            Widgets.EndScrollView();
        }

        private void ResetScrollIfTargetChanged(
            WorldTargetInfoRequest request)
        {
            var currentKey = WorldTargetInfoRequestKey.From(request);

            if(!_hasLastRequestKey)
            {
                _lastRequestKey = currentKey;
                _hasLastRequestKey = true;
                ResetScrollState();
                return;
            }

            if(currentKey == _lastRequestKey)
                return;

            _lastRequestKey = currentKey;
            ResetScrollState();
        }

        private void ResetScrollState()
        {
            _scrollPosition = Vector2.zero;
            _lastDrawnHeight = 0f;
        }
    }
}