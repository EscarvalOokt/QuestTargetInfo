using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetFlyingTransportCalculator
    {
        public static WorldTargetTransportInfo CalculateTransportPod(
            WorldTargetInfoRequest request)
        {
            return CalculateTransportPod(
                request,
                WorldTargetRouteCalculator.Calculate(request));
        }

        public static WorldTargetTransportInfo CalculateTransportPod(
            WorldTargetInfoRequest request,
            WorldTargetRouteInfo route)
        {
            WorldTargetTransportStatus status = ValidateBasicRequest(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.TransportPod, status);

            status = ValidatePodOrShuttleTarget(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.TransportPod, status);

            int distance;

            if(route.Status == WorldTargetRouteStatus.Available)
            {
                distance = route.Distance.DistanceTo;
            }
            else
            {
                // Keep one-way pod semantics intact: route calculation also checks
                // return distance, while transport pod only needs outbound distance.
                if(!TryGetFlightDistance(
                    request.OriginTile,
                    request.TargetTile,
                    out distance,
                    out status))
                {
                    return CreateStatus(WorldTargetTransportKind.TransportPod, status);
                }
            }

            WorldTargetFlightDistanceContext flightDistance =
                CreateFlightDistanceContext(
                    distance,
                    request.TargetTile.Layer,
                    WorldTargetTransportConstants.TransportPodMaxLaunchDistance);

            float fuelCost = CalculateFuelCost(
                distance,
                WorldTargetTransportConstants.TransportPodFuelPerTile,
                request.TargetTile.Layer);

            if(distance > flightDistance.MaxDistance)
            {
                return CreateStatus(
                    WorldTargetTransportKind.TransportPod,
                    WorldTargetTransportStatus.BeyondMaximumRange,
                    distanceTo: distance,
                    fuelCost: fuelCost,
                    flightDistance: flightDistance);
            }

            return new WorldTargetTransportInfo(
                WorldTargetTransportKind.TransportPod,
                WorldTargetTransportStatus.Available,
                distanceTo: distance,
                fuelCost: fuelCost,
                flightDistance: flightDistance);
        }

        public static WorldTargetTransportInfo CalculateShuttle(
            WorldTargetInfoRequest request)
        {
            return CalculateShuttle(
                request,
                WorldTargetRouteCalculator.Calculate(request));
        }

        public static WorldTargetTransportInfo CalculateShuttle(
            WorldTargetInfoRequest request,
            WorldTargetRouteInfo route)
        {
            if(!ModsConfig.OdysseyActive)
                return CreateStatus(WorldTargetTransportKind.Shuttle, WorldTargetTransportStatus.NoDlc);

            WorldTargetTransportStatus status = ValidateBasicRequest(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);

            status = ValidatePodOrShuttleTarget(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);

            status = GetTransportStatusFromRoute(route);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);

            int distanceTo = route.Distance.DistanceTo;
            int distanceReturn = route.Distance.DistanceReturn;

            WorldTargetFlightDistanceContext flightDistance =
                CreateFlightDistanceContext(
                    distanceTo,
                    request.TargetTile.Layer,
                    WorldTargetTransportConstants.ShuttleMaxLaunchDistance);

            WorldTargetFlightDistanceContext returnFlightDistance =
                CreateFlightDistanceContext(
                    distanceReturn,
                    request.OriginTile.Layer,
                    WorldTargetTransportConstants.ShuttleMaxLaunchDistance);

            float fuelTo = CalculateFuelCost(
                distanceTo,
                WorldTargetTransportConstants.ShuttleFuelPerTile,
                request.TargetTile.Layer);

            float fuelReturn = CalculateFuelCost(
                distanceReturn,
                WorldTargetTransportConstants.ShuttleFuelPerTile,
                request.OriginTile.Layer);

            if(distanceTo > flightDistance.MaxDistance)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Shuttle,
                    WorldTargetTransportStatus.BeyondMaximumRange,
                    distanceTo: distanceTo,
                    distanceReturn: distanceReturn,
                    fuelCost: fuelTo,
                    fuelReturnCost: fuelReturn,
                    fuelTotalCost: fuelTo + fuelReturn,
                    flightDistance: flightDistance,
                    returnFlightDistance: returnFlightDistance);
            }

            return new WorldTargetTransportInfo(
                WorldTargetTransportKind.Shuttle,
                WorldTargetTransportStatus.Available,
                distanceTo: distanceTo,
                distanceReturn: distanceReturn,
                fuelCost: fuelTo,
                fuelReturnCost: fuelReturn,
                fuelTotalCost: fuelTo + fuelReturn,
                flightDistance: flightDistance,
                returnFlightDistance: returnFlightDistance);
        }

        public static WorldTargetTransportInfo CalculateGravship(
            WorldTargetInfoRequest request)
        {
            if(!ModsConfig.OdysseyActive)
                return CreateStatus(WorldTargetTransportKind.Gravship, WorldTargetTransportStatus.NoDlc);

            WorldTargetTransportStatus status = ValidateBasicRequest(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Gravship, status);

            if(!(GravshipUtility.GetPlayerGravEngine_NewTemp(Find.CurrentMap) is Building_GravEngine engine))
                return CreateStatus(WorldTargetTransportKind.Gravship, WorldTargetTransportStatus.NoVehicle);

            status = ValidateGravshipWorldObjectTarget(request, engine);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Gravship, status);

            if(!GravshipUtility.TryGetPathFuelCost(
                engine.Tile,
                request.TargetTile,
                out float fuelCost,
                out int distance,
                fuelPerTile: engine.FuelPerTile))
            {
                return CreateStatus(WorldTargetTransportKind.Gravship, WorldTargetTransportStatus.RouteUnavailable);
            }

            WorldTargetFlightDistanceContext flightDistance =
                CreateFlightDistanceContext(
                    distance,
                    request.TargetTile.Layer,
                    engine.MaxLaunchDistance);

            if(fuelCost > engine.TotalFuel)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Gravship,
                    WorldTargetTransportStatus.NotEnoughFuel,
                    distanceTo: distance,
                    fuelCost: fuelCost,
                    flightDistance: flightDistance);
            }

            if(distance > flightDistance.MaxDistance)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Gravship,
                    WorldTargetTransportStatus.BeyondMaximumRange,
                    distanceTo: distance,
                    fuelCost: fuelCost,
                    flightDistance: flightDistance);
            }

            status = ValidateGravshipLandingTarget(request, out string reason);
            if(status != WorldTargetTransportStatus.Available)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Gravship,
                    status,
                    distanceTo: distance,
                    fuelCost: fuelCost,
                    reason: reason,
                    flightDistance: flightDistance);
            }

            return new WorldTargetTransportInfo(
                WorldTargetTransportKind.Gravship,
                WorldTargetTransportStatus.Available,
                distanceTo: distance,
                fuelCost: fuelCost,
                flightDistance: flightDistance);
        }

        private static WorldTargetTransportStatus GetTransportStatusFromRoute(
            WorldTargetRouteInfo route)
        {
            switch(route.Status)
            {
                case WorldTargetRouteStatus.Available:
                case WorldTargetRouteStatus.SameTile:
                    return WorldTargetTransportStatus.Available;

                case WorldTargetRouteStatus.InvalidOrigin:
                    return WorldTargetTransportStatus.InvalidOrigin;

                case WorldTargetRouteStatus.InvalidTarget:
                    return WorldTargetTransportStatus.InvalidTarget;

                case WorldTargetRouteStatus.NoLayerPath:
                    return WorldTargetTransportStatus.NoLayerPath;

                case WorldTargetRouteStatus.NoRoute:
                    return WorldTargetTransportStatus.NoRoute;

                default:
                    return WorldTargetTransportStatus.NoRoute;
            }
        }

        private static WorldTargetTransportStatus ValidateBasicRequest(
            WorldTargetInfoRequest request)
        {
            if(request == null || !request.OriginTile.Valid)
                return WorldTargetTransportStatus.InvalidOrigin;

            if(!request.TargetTile.Valid)
                return WorldTargetTransportStatus.InvalidTarget;

            return WorldTargetTransportStatus.Available;
        }

        private static WorldTargetTransportStatus ValidatePodOrShuttleTarget(
            WorldTargetInfoRequest request)
        {
            WorldObject targetObject = GetTargetWorldObject(request);

            if(targetObject != null && !targetObject.def.validLaunchTarget)
                return WorldTargetTransportStatus.InvalidWorldObject;

            if(ModsConfig.OdysseyActive
                && targetObject != null
                && targetObject.RequiresSignalJammerToReach)
            {
                return WorldTargetTransportStatus.RequiresSignalJammer;
            }

            if(targetObject == null && Find.World.Impassable(request.TargetTile))
                return WorldTargetTransportStatus.InvalidLandingTarget;

            return WorldTargetTransportStatus.Available;
        }

        private static WorldTargetTransportStatus ValidateGravshipWorldObjectTarget(
            WorldTargetInfoRequest request,
            Building_GravEngine engine)
        {
            WorldObject targetObject = GetTargetWorldObject(request);

            if(targetObject != null && !targetObject.def.validLaunchTarget)
                return WorldTargetTransportStatus.InvalidWorldObject;

            MapParent mapParent = request.TargetObject as MapParent
                ?? Find.World.worldObjects.MapParentAt(request.TargetTile);

            if(mapParent != null
                && mapParent.RequiresSignalJammerToReach
                && !engine.HasSignalJammer)
            {
                return WorldTargetTransportStatus.RequiresSignalJammer;
            }

            return WorldTargetTransportStatus.Available;
        }

        private static WorldTargetTransportStatus ValidateGravshipLandingTarget(
            WorldTargetInfoRequest request,
            out string reason)
        {
            reason = null;

            MapParent mapParent = Find.World.worldObjects.MapParentAt(request.TargetTile);
            if(mapParent != null && mapParent.HasMap)
                return WorldTargetTransportStatus.Available;

            var reasonBuilder = new StringBuilder();
            if(!TileFinder.IsValidTileForNewSettlement(
                request.TargetTile,
                reasonBuilder,
                forGravship: true))
            {
                reason = reasonBuilder.ToString();
                return WorldTargetTransportStatus.InvalidLandingTarget;
            }

            return WorldTargetTransportStatus.Available;
        }

        private static WorldObject GetTargetWorldObject(
            WorldTargetInfoRequest request)
        {
            if(request.TargetObject != null)
                return request.TargetObject;

            return Find.World.worldObjects.WorldObjectAt<WorldObject>(
                request.TargetTile);
        }

        private static bool TryGetFlightDistance(
            PlanetTile from,
            PlanetTile to,
            out int distance,
            out WorldTargetTransportStatus status)
        {
            distance = int.MaxValue;
            status = WorldTargetTransportStatus.Available;

            if(!from.Valid)
            {
                status = WorldTargetTransportStatus.InvalidOrigin;
                return false;
            }

            if(!to.Valid)
            {
                status = WorldTargetTransportStatus.InvalidTarget;
                return false;
            }

            bool crossLayer = from.Layer != to.Layer;
            if(crossLayer && !from.Layer.HasConnectionPathTo(to.Layer))
            {
                status = WorldTargetTransportStatus.NoLayerPath;
                return false;
            }

            distance = Find.WorldGrid.TraversalDistanceBetween(
                from,
                to,
                passImpassable: true,
                maxDist: int.MaxValue,
                canTraverseLayers: crossLayer);

            if(distance == int.MaxValue)
            {
                status = WorldTargetTransportStatus.NoRoute;
                return false;
            }

            return true;
        }

        internal static int GetLayerAdjustedFixedLaunchDistanceMax(
            int baseMaxDistance,
            PlanetLayer targetLayer)
        {
            return GetLayerAdjustedFixedLaunchDistanceMax(
                baseMaxDistance,
                GetLayerRangeDistanceFactor(targetLayer));
        }

        internal static int GetLayerAdjustedFixedLaunchDistanceMax(
            int baseMaxDistance,
            WorldTargetFlightDistanceContext flightDistance)
        {
            return GetLayerAdjustedFixedLaunchDistanceMax(
                baseMaxDistance,
                flightDistance.TargetLayerRangeDistanceFactor);
        }

        private static int GetLayerAdjustedFixedLaunchDistanceMax(
            float baseMaxDistance,
            float rangeDistanceFactor)
        {
            if(baseMaxDistance < 0f)
                return Mathf.RoundToInt(baseMaxDistance);

            if(rangeDistanceFactor <= 0f)
                rangeDistanceFactor = 1f;

            return Mathf.RoundToInt(baseMaxDistance / rangeDistanceFactor);
        }

        private static float GetLayerRangeDistanceFactor(
            PlanetLayer targetLayer)
        {
            float rangeDistanceFactor = targetLayer?.Def?.rangeDistanceFactor ?? 1f;

            if(rangeDistanceFactor <= 0f)
                return 1f;

            return rangeDistanceFactor;
        }

        private static int CalculateFuelAdjustedDistance(
            int distance,
            PlanetLayer targetLayer)
        {
            return Mathf.CeilToInt(
                distance * GetLayerRangeDistanceFactor(targetLayer));
        }

        private static WorldTargetFlightDistanceContext CreateFlightDistanceContext(
            int distance,
            PlanetLayer targetLayer,
            float baseMaxDistance = -1f)
        {
            float rangeDistanceFactor = GetLayerRangeDistanceFactor(targetLayer);

            int maxDistance = baseMaxDistance >= 0f
                ? GetLayerAdjustedFixedLaunchDistanceMax(baseMaxDistance, rangeDistanceFactor)
                : -1;

            return new WorldTargetFlightDistanceContext(
                distance,
                rangeDistanceFactor,
                maxDistance,
                CalculateFuelAdjustedDistance(distance, targetLayer));
        }

        private static float CalculateFuelCost(
            int distance,
            float fuelPerTile,
            PlanetLayer targetLayer)
        {
            return Mathf.Max(
                WorldTargetTransportConstants.MinimumFuelCost,
                distance * fuelPerTile * GetLayerRangeDistanceFactor(targetLayer));
        }

        private static WorldTargetTransportInfo CreateStatus(
            WorldTargetTransportKind kind,
            WorldTargetTransportStatus status,
            int distanceTo = -1,
            int distanceReturn = -1,
            float fuelCost = -1f,
            float fuelReturnCost = -1f,
            float fuelTotalCost = -1f,
            string reason = null,
            WorldTargetFlightDistanceContext flightDistance = default,
            WorldTargetFlightDistanceContext returnFlightDistance = default)
        {
            return new WorldTargetTransportInfo(
                kind,
                status,
                distanceTo: distanceTo,
                distanceReturn: distanceReturn,
                fuelCost: fuelCost,
                fuelReturnCost: fuelReturnCost,
                fuelTotalCost: fuelTotalCost,
                reason: reason,
                flightDistance: flightDistance,
                returnFlightDistance: returnFlightDistance);
        }
    }
}