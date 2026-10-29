using RimWorld.Planet;
using Verse;

namespace QuestTargetInfo.Info
{
    internal static class WorldTargetRouteCalculator
    {
        public static WorldTargetRouteInfo Calculate(WorldTargetInfoRequest request)
        {
            if (request == null || !request.OriginTile.Valid)
                return new WorldTargetRouteInfo(WorldTargetRouteStatus.InvalidOrigin, default);

            if (!request.TargetTile.Valid)
                return new WorldTargetRouteInfo(WorldTargetRouteStatus.InvalidTarget, default);

            if (request.OriginTile == request.TargetTile)
            {
                return new WorldTargetRouteInfo(
                    WorldTargetRouteStatus.SameTile,
                    new WorldTargetDistanceInfo(0, 0, request.OriginTile.Layer, request.TargetTile.Layer));
            }

            PlanetLayer fromLayer = request.OriginTile.Layer;
            PlanetLayer toLayer = request.TargetTile.Layer;
            WorldTargetRouteStatus outboundStatus = CalculateDirection(
                request.OriginTile,
                request.TargetTile,
                out int distanceTo);
            WorldTargetRouteStatus returnStatus = CalculateDirection(
                request.TargetTile,
                request.OriginTile,
                out int distanceReturn);

            return new WorldTargetRouteInfo(
                outboundStatus,
                new WorldTargetDistanceInfo(distanceTo, distanceReturn, fromLayer, toLayer),
                returnStatus);
        }

        private static WorldTargetRouteStatus CalculateDirection(PlanetTile from, PlanetTile to, out int distance)
        {
            distance = -1;
            bool crossLayer = from.Layer != to.Layer;
            if (crossLayer && !from.Layer.HasConnectionPathTo(to.Layer))
                return WorldTargetRouteStatus.NoLayerPath;

            int result = Find.WorldGrid.TraversalDistanceBetween(
                from,
                to,
                passImpassable: true,
                maxDist: int.MaxValue,
                canTraverseLayers: crossLayer);
            if (result == int.MaxValue)
                return WorldTargetRouteStatus.NoRoute;

            distance = result;
            return WorldTargetRouteStatus.Available;
        }
    }
}