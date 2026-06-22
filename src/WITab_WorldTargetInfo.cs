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

        public override bool IsVisible
        {
            get
            {
                return WorldTargetInfoSelectionUtility.TryCreateRequest(
                    out WorldTargetInfoRequest _);
            }
        }

        public WITab_WorldTargetInfo()
        {
            size = WinSize;
            labelKey = "QuestTargetInfo.TabLabel";
        }

        protected override void FillTab()
        {
            if(!WorldTargetInfoSelectionUtility.TryCreateRequest(
                out WorldTargetInfoRequest request))
            {
                return;
            }

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
    }
}