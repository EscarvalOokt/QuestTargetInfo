using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal class WITab_WorldTargetInfo : WITab
    {
        private static readonly Vector2 WinSize = new Vector2(400f, 320f);

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

            var lines = WorldTargetInfoLineBuilder
                .BuildLines(request)
                .ToList();

            Rect outRect = new Rect(0f, 0f, WinSize.x, WinSize.y)
                .ContractedBy(10f);

            var viewRect = new Rect(
                0f,
                0f,
                outRect.width - 16f,
                Mathf.Max(_lastDrawnHeight, outRect.height));

            Widgets.BeginScrollView(outRect, ref _scrollPosition, viewRect);

            DrawContents(request, lines, viewRect);

            Widgets.EndScrollView();
        }

        private void DrawContents(
            WorldTargetInfoRequest request,
            List<string> lines,
            Rect viewRect)
        {
            Text.Font = GameFont.Medium;

            var titleRect = new Rect(
                viewRect.x,
                viewRect.y,
                viewRect.width,
                30f);

            Widgets.Label(titleRect, GetTitle(request));

            Rect contentRect = viewRect;
            contentRect.yMin += 35f;
            contentRect.height = 99999f;

            Text.Font = GameFont.Small;

            var listing = new Listing_Standard();
            listing.Begin(contentRect);

            foreach(string line in lines)
                listing.Label(line);

            listing.End();

            _lastDrawnHeight = contentRect.y + listing.CurHeight;
        }

        private string GetTitle(WorldTargetInfoRequest request)
        {
            if(!request.TargetLabel.NullOrEmpty())
                return request.TargetLabel;

            return "QuestTargetInfo.Header".Translate().ToString();
        }
    }
}