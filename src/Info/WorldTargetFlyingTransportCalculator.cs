using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetFlyingTransportCalculator
    {
        private const float ShuttleFuelPerTile = 3f;
        private const float PodFuelPerTile = 2.25f;
        private const float MinFuel = 50f;

        // Vanilla maximum regular transport pod range.
        // MechanoidDropPod defines fixedLaunchDistanceMax = 67 with the XML comment:
        // "maximum regular transport pod range".
        // Regular TransportPod does not define fixedLaunchDistanceMax because its range is fuel-based.
        internal const int TransportPodMaxLaunchDistance = 67;

        // AncientTransportPod defines fixedLaunchDistanceMax = 53 with the XML comment:
        // "80% of full transport pod range".
        // The game does not clearly expose this before launch target selection,
        // so the info window shows it as an additional compatibility warning.
        internal const int AncientTransportPodMaxLaunchDistance = 53;

        public static WorldTargetTransportInfo CalculateTransportPod(
            WorldTargetInfoRequest request)
        {
            WorldTargetTransportStatus status = ValidateBasicRequest(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.TransportPod, status);

            status = ValidatePodOrShuttleTarget(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.TransportPod, status);

            if(!TryGetFlightDistance(
                request.OriginTile,
                request.TargetTile,
                out int distance,
                out status))
            {
                return CreateStatus(WorldTargetTransportKind.TransportPod, status);
            }

            float fuelCost = CalculateFuelCost(
                distance,
                PodFuelPerTile,
                request.TargetTile.Layer);

            if(distance > TransportPodMaxLaunchDistance)
            {
                return CreateStatus(
                    WorldTargetTransportKind.TransportPod,
                    WorldTargetTransportStatus.BeyondMaximumRange,
                    distanceTo: distance,
                    fuelCost: fuelCost);
            }

            return new WorldTargetTransportInfo(
                WorldTargetTransportKind.TransportPod,
                WorldTargetTransportStatus.Available,
                distanceTo: distance,
                fuelCost: fuelCost);
        }

        public static WorldTargetTransportInfo CalculateShuttle(
            WorldTargetInfoRequest request)
        {
            if(!ModsConfig.OdysseyActive)
                return CreateStatus(WorldTargetTransportKind.Shuttle, WorldTargetTransportStatus.NoDlc);

            WorldTargetTransportStatus status = ValidateBasicRequest(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);

            status = ValidatePodOrShuttleTarget(request);
            if(status != WorldTargetTransportStatus.Available)
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);

            if(!TryGetFlightDistance(
                request.OriginTile,
                request.TargetTile,
                out int distanceTo,
                out status))
            {
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);
            }

            if(!TryGetFlightDistance(
                request.TargetTile,
                request.OriginTile,
                out int distanceReturn,
                out status))
            {
                return CreateStatus(WorldTargetTransportKind.Shuttle, status);
            }

            float fuelTo = CalculateFuelCost(
                distanceTo,
                ShuttleFuelPerTile,
                request.TargetTile.Layer);

            float fuelReturn = CalculateFuelCost(
                distanceReturn,
                ShuttleFuelPerTile,
                request.OriginTile.Layer);

            return new WorldTargetTransportInfo(
                WorldTargetTransportKind.Shuttle,
                WorldTargetTransportStatus.Available,
                distanceTo: distanceTo,
                distanceReturn: distanceReturn,
                fuelCost: fuelTo,
                fuelReturnCost: fuelReturn,
                fuelTotalCost: fuelTo + fuelReturn);
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

            int maxDistance = Mathf.FloorToInt(
                engine.MaxLaunchDistance / request.TargetTile.Layer.Def.rangeDistanceFactor);

            if(fuelCost > engine.TotalFuel)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Gravship,
                    WorldTargetTransportStatus.NotEnoughFuel,
                    distanceTo: distance,
                    fuelCost: fuelCost);
            }

            if(distance > maxDistance)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Gravship,
                    WorldTargetTransportStatus.BeyondMaximumRange,
                    distanceTo: distance,
                    fuelCost: fuelCost);
            }

            status = ValidateGravshipLandingTarget(request, out string reason);
            if(status != WorldTargetTransportStatus.Available)
            {
                return CreateStatus(
                    WorldTargetTransportKind.Gravship,
                    status,
                    distanceTo: distance,
                    fuelCost: fuelCost,
                    reason: reason);
            }

            return new WorldTargetTransportInfo(
                WorldTargetTransportKind.Gravship,
                WorldTargetTransportStatus.Available,
                distanceTo: distance,
                fuelCost: fuelCost);
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

        private static float CalculateFuelCost(
            int distance,
            float fuelPerTile,
            PlanetLayer targetLayer)
        {
            return Mathf.Max(
                MinFuel,
                distance * fuelPerTile * targetLayer.Def.rangeDistanceFactor);
        }

        private static WorldTargetTransportInfo CreateStatus(
            WorldTargetTransportKind kind,
            WorldTargetTransportStatus status,
            int distanceTo = -1,
            float fuelCost = -1f,
            string reason = null)
        {
            return new WorldTargetTransportInfo(
                kind,
                status,
                distanceTo: distanceTo,
                fuelCost: fuelCost,
                reason: reason);
        }
    }
}