using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal class WITab_WorldTargetInfo : WITab
    {
        private static readonly Vector2 WinSize = new Vector2(360f, 440f);

        private Vector2 _scrollPosition;
        private float _lastDrawnHeight;

        private bool _hasLastRequestKey;
        private WorldTargetInfoRequestKey _lastRequestKey;
        private WorldTargetInfoModel _cachedModel;

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
            _cachedModel = null;
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

            WorldTargetInfoModel model = GetModelCached(request);

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

        private WorldTargetInfoModel GetModelCached(
            WorldTargetInfoRequest request)
        {
            WorldTargetInfoRequestKey currentKey =
                WorldTargetInfoRequestKey.From(request);

            if(!_hasLastRequestKey || currentKey != _lastRequestKey)
            {
                _lastRequestKey = currentKey;
                _hasLastRequestKey = true;
                _cachedModel = WorldTargetInfoModelBuilder.Build(request);
                ResetScrollState();
                return _cachedModel;
            }

            if(_cachedModel == null)
                _cachedModel = WorldTargetInfoModelBuilder.Build(request);

            return _cachedModel;
        }

        private void ResetScrollState()
        {
            _scrollPosition = Vector2.zero;
            _lastDrawnHeight = 0f;
        }
    }
}