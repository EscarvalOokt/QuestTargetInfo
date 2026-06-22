using RimWorld.Planet;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetRouteCalculator
    {
        public static WorldTargetRouteInfo Calculate(WorldTargetInfoRequest request)
        {
            if(request == null || !request.OriginTile.Valid)
                return new WorldTargetRouteInfo(WorldTargetRouteStatus.InvalidOrigin, default);

            if(!request.TargetTile.Valid)
                return new WorldTargetRouteInfo(WorldTargetRouteStatus.InvalidTarget, default);

            if(request.OriginTile == request.TargetTile)
            {
                return new WorldTargetRouteInfo(
                    WorldTargetRouteStatus.SameTile,
                    default);
            }

            PlanetLayer fromLayer = request.OriginTile.Layer;
            PlanetLayer toLayer = request.TargetTile.Layer;
            bool crossLayer = fromLayer != toLayer;

            if(crossLayer && !CanTravelBetweenLayers(fromLayer, toLayer))
                return new WorldTargetRouteInfo(WorldTargetRouteStatus.NoLayerPath, default);

            int distanceTo = Find.WorldGrid.TraversalDistanceBetween(
                request.OriginTile,
                request.TargetTile,
                passImpassable: true,
                maxDist: int.MaxValue,
                canTraverseLayers: crossLayer);

            if(distanceTo == int.MaxValue)
                return new WorldTargetRouteInfo(WorldTargetRouteStatus.NoRoute, default);

            int distanceReturn = Find.WorldGrid.TraversalDistanceBetween(
                request.TargetTile,
                request.OriginTile,
                passImpassable: true,
                maxDist: int.MaxValue,
                canTraverseLayers: crossLayer);

            if(distanceReturn == int.MaxValue)
                return new WorldTargetRouteInfo(WorldTargetRouteStatus.NoRoute, default);

            return new WorldTargetRouteInfo(
                WorldTargetRouteStatus.Available,
                new WorldTargetDistanceInfo(
                    distanceTo,
                    distanceReturn,
                    fromLayer,
                    toLayer));
        }

        private static bool CanTravelBetweenLayers(PlanetLayer fromLayer, PlanetLayer toLayer)
        {
            return fromLayer.HasConnectionPathTo(toLayer)
                && toLayer.HasConnectionPathTo(fromLayer);
        }
    }
}