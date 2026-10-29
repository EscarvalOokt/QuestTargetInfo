using System.Collections.Generic;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace QuestTargetInfo.Info
{
    internal static class WorldTargetFlyingTransportCalculator
    {
        public static WorldTargetTransportInfo CalculateTransportPod(WorldTargetInfoRequest request)
        {
            return CalculateTransportPod(request, WorldTargetRouteCalculator.Calculate(request));
        }

        public static WorldTargetTransportInfo CalculateTransportPod(
            WorldTargetInfoRequest request,
            WorldTargetRouteInfo route)
        {
            WorldTargetTransportStatus status = ValidateBasicRequest(request);
            if (status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.TransportPod, status);

            status = ValidatePodOrShuttleTarget(request);
            if (status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.TransportPod, status);

            status = GetTransportStatusFromRoute(route);
            if (status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.TransportPod, status);

            int distance = route.Distance.DistanceTo;
            float layerFactor = GetLayerRangeDistanceFactor(request.TargetTile.Layer);

            WorldTargetFlightDistanceContext flightDistance = CreateFlightDistanceContext(
                distance,
                request.TargetTile.Layer,
                WorldTargetTransportMath.PodMaximumDistance(layerFactor));

            float fuelCost = WorldTargetTransportMath.PodFuelCost(distance, layerFactor);

            if (distance > flightDistance.MaxDistance)
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

        public static WorldTargetTransportInfo CalculateShuttle(WorldTargetInfoRequest request)
        {
            return CalculateShuttle(request, WorldTargetRouteCalculator.Calculate(request));
        }

        public static WorldTargetTransportInfo CalculateShuttle(
            WorldTargetInfoRequest request,
            WorldTargetRouteInfo route)
        {
            if (!ModsConfig.OdysseyActive)
                return CreateStatus(WorldTargetTransportKind.Shuttle, WorldTargetTransportStatus.NoDlc);

            WorldTargetTransportStatus status = ValidateBasicRequest(request);
            if (status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);

            status = ValidatePodOrShuttleTarget(request);
            if (status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);

            status = GetTransportStatusFromRoute(route);
            if (status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);

            return WorldTargetTransportMath.CalculateShuttleFlight(
                route.Distance.DistanceTo,
                GetLayerRangeDistanceFactor(request.TargetTile.Layer),
                route.HasReturnDistance ? route.Distance.DistanceReturn : null,
                GetLayerRangeDistanceFactor(request.OriginTile.Layer));
        }

        public static WorldTargetTransportInfo CalculateGravship(WorldTargetInfoRequest request, Map originMap = null)
        {
            if (!ModsConfig.OdysseyActive)
                return CreateStatus(WorldTargetTransportKind.Gravship, WorldTargetTransportStatus.NoDlc);

            WorldTargetTransportStatus status = ValidateBasicRequest(request);
            if (status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Gravship, status);

            originMap = originMap ?? Find.CurrentMap;
            if (originMap == null || originMap.Tile != request.OriginTile)
                return CreateStatus(WorldTargetTransportKind.Gravship, WorldTargetTransportStatus.InvalidOrigin);

            Building_GravEngine engine = FindSpawnedGravEngine(originMap);
            if (engine == null)
                return CreateStatus(WorldTargetTransportKind.Gravship, WorldTargetTransportStatus.NoVehicle);

            status = ValidateGravshipWorldObjectTarget(request, engine);
            if (status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Gravship, status);

            if (!TryGetGravshipPathFuelCost(
                    engine.Tile,
                    request.TargetTile,
                    engine.FuelPerTile,
                    out float fuelCost,
                    out int distance))
            {
                return CreateStatus(WorldTargetTransportKind.Gravship, WorldTargetTransportStatus.RouteUnavailable);
            }

            WorldTargetFlightDistanceContext flightDistance = CreateFlightDistanceContext(
                distance,
                request.TargetTile.Layer,
                WorldTargetTransportMath.GravshipMaximumDistance(
                    engine.MaxLaunchDistance,
                    GetLayerRangeDistanceFactor(request.TargetTile.Layer)));

            if (fuelCost > engine.TotalFuel)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Gravship,
                    WorldTargetTransportStatus.NotEnoughFuel,
                    distanceTo: distance,
                    fuelCost: fuelCost,
                    flightDistance: flightDistance);
            }

            if (distance > flightDistance.MaxDistance)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Gravship,
                    WorldTargetTransportStatus.BeyondMaximumRange,
                    distanceTo: distance,
                    fuelCost: fuelCost,
                    flightDistance: flightDistance);
            }

            status = ValidateGravshipLandingTarget(request, engine, out string reason);
            if (status != WorldTargetTransportStatus.Available)
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

        private static bool TryGetGravshipPathFuelCost(
            PlanetTile from,
            PlanetTile to,
            float fuelPerTile,
            out float cost,
            out int distance)
        {
            cost = -1f;
            distance = -1;
            var layerFuelCost = 0f;

            // Vanilla caches TryGetPathFuelCost by endpoints only. Re-evaluate its
            // path/cost rules so fuel-use upgrades and layer changes reach this UI.
            if (from.Layer != to.Layer)
            {
                var connections = new List<PlanetLayerConnection>();
                if (!from.Layer.TryGetPath(to.Layer, connections, out layerFuelCost))
                    return false;

                from = to.Layer.GetClosestTile_NewTemp(from);
            }

            distance = Find.WorldGrid.TraversalDistanceBetween(
                from,
                to,
                passImpassable: true,
                maxDist: int.MaxValue,
                canTraverseLayers: false);
            if (distance == int.MaxValue)
            {
                distance = -1;
                return false;
            }

            cost = WorldTargetTransportMath.GravshipFuelCost(
                distance,
                GetLayerRangeDistanceFactor(to.Layer),
                fuelPerTile,
                layerFuelCost);
            return true;
        }

        private static Building_GravEngine FindSpawnedGravEngine(Map originMap)
        {
            // The vanilla lookup caches on game ticks and also returns packed engines.
            // Read spawned engines directly so paused-map changes update this UI.
            foreach (Thing thing in originMap.listerThings.ThingsOfDef(ThingDefOf.GravEngine))
            {
                if (thing is Building_GravEngine { Spawned: true } engine && ReferenceEquals(engine.Map, originMap))
                    return engine;
            }

            return null;
        }

        private static WorldTargetTransportStatus GetTransportStatusFromRoute(WorldTargetRouteInfo route)
        {
            switch (route.Status)
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

        private static WorldTargetTransportStatus ValidateBasicRequest(WorldTargetInfoRequest request)
        {
            if (request == null || !request.OriginTile.Valid)
                return WorldTargetTransportStatus.InvalidOrigin;

            if (!request.TargetTile.Valid)
                return WorldTargetTransportStatus.InvalidTarget;

            return WorldTargetTransportStatus.Available;
        }

        private static WorldTargetTransportStatus ValidatePodOrShuttleTarget(WorldTargetInfoRequest request)
        {
            WorldObject targetObject = GetTargetWorldObject(request);

            if (targetObject != null && !targetObject.def.validLaunchTarget)
                return WorldTargetTransportStatus.InvalidWorldObject;

            if (ModsConfig.OdysseyActive && targetObject is { RequiresSignalJammerToReach: true })
            {
                return WorldTargetTransportStatus.RequiresSignalJammer;
            }

            if (targetObject == null && Find.World.Impassable(request.TargetTile))
                return WorldTargetTransportStatus.InvalidLandingTarget;

            return WorldTargetTransportStatus.Available;
        }

        private static WorldTargetTransportStatus ValidateGravshipWorldObjectTarget(
            WorldTargetInfoRequest request,
            Building_GravEngine engine)
        {
            WorldObject targetObject = GetTargetWorldObject(request);

            if (targetObject != null && !targetObject.def.validLaunchTarget)
                return WorldTargetTransportStatus.InvalidWorldObject;

            MapParent mapParent = request.TargetObject as MapParent ??
                                  Find.World.worldObjects.MapParentAt(request.TargetTile);

            if (mapParent is { RequiresSignalJammerToReach: true } && !engine.HasSignalJammer)
            {
                return WorldTargetTransportStatus.RequiresSignalJammer;
            }

            return WorldTargetTransportStatus.Available;
        }

        private static WorldTargetTransportStatus ValidateGravshipLandingTarget(
            WorldTargetInfoRequest request,
            Building_GravEngine engine,
            out string reason)
        {
            reason = null;

            if (!engine.Spawned || engine.Map == null)
                return WorldTargetTransportStatus.NoVehicle;

            if (request.TargetTile == engine.Tile && !engine.Map.listerThings.AnyThingWithDef(ThingDefOf.GravAnchor))
            {
                reason = "CannotLandOnSameTile".Translate().ToString();
                return WorldTargetTransportStatus.InvalidLandingTarget;
            }

            MapParent mapParent = Find.World.worldObjects.MapParentAt(request.TargetTile);
            if (mapParent is { HasMap: true })
                return WorldTargetTransportStatus.Available;

            var reasonBuilder = new StringBuilder();
            if (!TileFinder.IsValidTileForNewSettlement(request.TargetTile, reasonBuilder, forGravship: true))
            {
                reason = reasonBuilder.ToString();
                return WorldTargetTransportStatus.InvalidLandingTarget;
            }

            return WorldTargetTransportStatus.Available;
        }

        private static WorldObject GetTargetWorldObject(WorldTargetInfoRequest request)
        {
            if (request.TargetObject != null)
                return request.TargetObject;

            return Find.World.worldObjects.WorldObjectAt<WorldObject>(request.TargetTile);
        }

        internal static int GetLayerAdjustedFixedLaunchDistanceMax(int baseMaxDistance, PlanetLayer targetLayer)
        {
            return GetLayerAdjustedFixedLaunchDistanceMax(baseMaxDistance, GetLayerRangeDistanceFactor(targetLayer));
        }

        internal static int GetLayerAdjustedFixedLaunchDistanceMax(
            int baseMaxDistance,
            WorldTargetFlightDistanceContext flightDistance)
        {
            return GetLayerAdjustedFixedLaunchDistanceMax(
                baseMaxDistance,
                flightDistance.TargetLayerRangeDistanceFactor);
        }

        private static int GetLayerAdjustedFixedLaunchDistanceMax(float baseMaxDistance, float rangeDistanceFactor)
        {
            if (baseMaxDistance < 0f)
                return Mathf.RoundToInt(baseMaxDistance);

            return WorldTargetTransportMath.FixedMaximumDistance(baseMaxDistance, rangeDistanceFactor);
        }

        private static float GetLayerRangeDistanceFactor(PlanetLayer targetLayer)
        {
            return WorldTargetTransportMath.NormalizeLayerFactor(targetLayer?.Def?.rangeDistanceFactor ?? 1f);
        }

        private static int CalculateFuelAdjustedDistance(int distance, PlanetLayer targetLayer)
        {
            return Mathf.CeilToInt(distance * GetLayerRangeDistanceFactor(targetLayer));
        }

        private static WorldTargetFlightDistanceContext CreateFlightDistanceContext(
            int distance,
            PlanetLayer targetLayer,
            int maxDistance = -1)
        {
            float rangeDistanceFactor = GetLayerRangeDistanceFactor(targetLayer);

            return new WorldTargetFlightDistanceContext(
                distance,
                rangeDistanceFactor,
                maxDistance,
                CalculateFuelAdjustedDistance(distance, targetLayer));
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
            WorldTargetFlightDistanceContext returnFlightDistance = default,
            WorldTargetTransportStatus returnStatus = WorldTargetTransportStatus.Available)
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
                returnFlightDistance: returnFlightDistance,
                returnStatus: returnStatus);
        }
    }
}