using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    public class Window_QuestTargetInfo : Window
    {
        private struct DistanceInfo
        {
            public int DistanceTo;
            public int DistanceReturn;
            public PlanetLayer FromLayer;
            public PlanetLayer ToLayer;
        }

        private const float ShuttleFuelPerTile = 3f;
        private const float PodFuelPerTile = 2.25f;
        private const float MinFuel = 50f;
        private const float CollapsedHeight = 60f;

        private static bool _collapsed = false;
        
        private readonly GlobalTargetInfo _target;

        private List<string> _cachedLines;
        private int _cachedTile = -1;

        private float Height => _collapsed ? CollapsedHeight : InitialSize.y;

        public override Vector2 InitialSize => new Vector2(250f, 640f);

        public Window_QuestTargetInfo(GlobalTargetInfo target)
        {
            _target = target;
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

            if (!Find.WindowStack.Windows.OfType<MainTabWindow_Quests>().Any())
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
            float headerHeight = 30f;
            Rect headerRect = new Rect(inRect.x, inRect.y, inRect.width, headerHeight);
            DrawHeaderLabel(headerRect);
            DrawCollapseButton(headerRect);

            if (_collapsed)
                return;

            Widgets.DrawLineHorizontal(inRect.x, inRect.y + headerHeight - 1f, inRect.width, Color.gray);

            var lines = GetLinesCached();
            if (lines.Count == 0)
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
            foreach (var line in lines)
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
            var icon = _collapsed ? TexButton.ReorderUp : TexButton.ReorderDown;

            if (Widgets.ButtonImage(buttonRect, icon))
            {
                _collapsed = !_collapsed;

                float bottomY = windowRect.yMax;
                windowRect.yMin = bottomY - Height;
                windowRect.height = Height;
            }

            TooltipHandler.TipRegion(buttonRect, _collapsed
                ? "QuestTargetInfo.ExpandTooltip".Translate()
                : "QuestTargetInfo.CollapseTooltip".Translate());
        }
        private List<string> GetLinesCached()
        {
            int currentTile = _target.Tile;
            if (_cachedLines == null || _cachedTile != currentTile)
            {
                _cachedLines = GetAllInfoLines().ToList();
                _cachedTile = currentTile;
            }
            return _cachedLines;
        }
        private IEnumerable<string> GetAllInfoLines()
        {
            var distanceInfo = ComputeDistanceInfo();
            if (!distanceInfo.HasValue)
                yield break;

            var distance = distanceInfo.Value;

            foreach (var line in GetDistanceLines(distance)) yield return line;
            foreach (var line in GetShuttleLines(distance)) yield return line;
            foreach (var line in GetTransportPodLines(distance)) yield return line;
            foreach (var line in GetGravshipLines()) yield return line;
        }
        private IEnumerable<string> GetDistanceLines(DistanceInfo distance)
        {
            yield return "QuestTargetInfo.EstimatedDistanceTiles".Translate(distance.DistanceTo);
        }
        private IEnumerable<string> GetShuttleLines(DistanceInfo distance)
        {
            if (!ModsConfig.IsActive("ludeon.rimworld.odyssey"))
                yield break;

            float fuelTo = Mathf.Max(MinFuel, distance.DistanceTo * ShuttleFuelPerTile * distance.ToLayer.Def.rangeDistanceFactor);
            float fuelReturn = Mathf.Max(MinFuel, distance.DistanceReturn * ShuttleFuelPerTile * distance.FromLayer.Def.rangeDistanceFactor);
            float fuelTotal = fuelTo + fuelReturn;

            yield return "";
            yield return "QuestTargetInfo.Shuttle".Translate();
            yield return "QuestTargetInfo.FuelCost".Translate((int)fuelTo);
            yield return "QuestTargetInfo.FuelReturnCost".Translate((int)fuelReturn);
            yield return "QuestTargetInfo.FuelTotalCost".Translate((int)fuelTotal);
        }
        private IEnumerable<string> GetTransportPodLines(DistanceInfo distance)
        {
            float fuelTo = Mathf.Max(MinFuel, distance.DistanceTo * PodFuelPerTile * distance.ToLayer.Def.rangeDistanceFactor);

            yield return "";
            yield return "QuestTargetInfo.TransportPod".Translate();
            yield return "QuestTargetInfo.FuelCost".Translate((int)fuelTo);
        }
        private IEnumerable<string> GetGravshipLines()
        {
            if(!ModsConfig.IsActive("ludeon.rimworld.odyssey"))
                yield break;

            yield return "";
            yield return "QuestTargetInfo.Gravship".Translate();

            if (!(GravshipUtility.GetPlayerGravEngine(Find.CurrentMap) is Building_GravEngine engine))
            {
                yield return "QuestTargetInfo.NoGravship".Translate();
            }
            else
            {
                GravshipUtility.TryGetPathFuelCost(
                    engine.Tile, _target.Tile,
                    out float fuelCost, out int gravDist,
                    fuelPerTile: engine.FuelPerTile);

                yield return "QuestTargetInfo.FuelCost".Translate((int)fuelCost);
            }
        }
        private DistanceInfo? ComputeDistanceInfo()
        {
            var fromTile = Find.CurrentMap?.Tile ?? -1;
            var toTile = _target.Tile;
            if (fromTile < 0 || toTile < 0 || toTile >= Find.WorldGrid.TilesCount)
                return null;

            var fromLayer = fromTile.Layer;
            var toLayer = toTile.Layer;
            bool sameLayer = fromLayer == toLayer;

            int distTo = Find.WorldGrid.TraversalDistanceBetween(
                                fromTile, toTile,
                                passImpassable: true,
                                canTraverseLayers: !sameLayer);
            if (distTo <= 0) return null;

            int distReturn = Find.WorldGrid.TraversalDistanceBetween(
                                 toTile, fromTile,
                                 passImpassable: true,
                                 canTraverseLayers: !sameLayer);

            return new DistanceInfo
            {
                DistanceTo = distTo,
                DistanceReturn = distReturn,
                FromLayer = fromLayer,
                ToLayer = toLayer
            };
        }
    }
}