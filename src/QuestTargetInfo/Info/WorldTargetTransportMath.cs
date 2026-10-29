using System;

namespace QuestTargetInfo.Info
{
    // Pure arithmetic shared by the game adapters and runtime-free contract tests.
    internal static class WorldTargetTransportMath
    {
        internal static float NormalizeLayerFactor(float factor)
        {
            return float.IsNaN(factor) || float.IsInfinity(factor) || factor <= 0f ? 1f : factor;
        }

        internal static int PodMaximumDistance(float layerFactor)
        {
            return (int)Math.Floor(
                WorldTargetTransportConstants.TransportPodFuelCapacity /
                (WorldTargetTransportConstants.TransportPodFuelPerTile * NormalizeLayerFactor(layerFactor)));
        }

        internal static int FixedMaximumDistance(float baseMaximum, float layerFactor)
        {
            // Passenger shuttle and ancient pod use vanilla Mathf.RoundToInt.
            return (int)Math.Round(baseMaximum / NormalizeLayerFactor(layerFactor), MidpointRounding.ToEven);
        }

        internal static int GravshipMaximumDistance(float baseMaximum, float layerFactor)
        {
            // CompPilotConsole.GetMaxLaunchDistance uses floor, unlike fixed pods/shuttles.
            return (int)Math.Floor(baseMaximum / NormalizeLayerFactor(layerFactor));
        }

        internal static float PodFuelCost(int distance, float layerFactor)
        {
            return FuelCost(
                distance,
                WorldTargetTransportConstants.TransportPodFuelPerTile,
                layerFactor,
                WorldTargetTransportConstants.TransportPodMinimumFuelCost);
        }

        internal static float ShuttleFuelCost(int distance, float layerFactor)
        {
            return FuelCost(
                distance,
                WorldTargetTransportConstants.ShuttleFuelPerTile,
                layerFactor,
                WorldTargetTransportConstants.ShuttleMinimumFuelCost);
        }

        internal static float GravshipFuelCost(int distance, float layerFactor, float fuelPerTile, float layerFuelCost)
        {
            return Math.Max(
                WorldTargetTransportConstants.GravshipMinimumFuelCost,
                layerFuelCost + distance * NormalizeLayerFactor(layerFactor) * fuelPerTile);
        }

        private static float FuelCost(int distance, float fuelPerTile, float layerFactor, float minimum)
        {
            return Math.Max(minimum, distance * fuelPerTile * NormalizeLayerFactor(layerFactor));
        }

        internal static int RequiredFuelUnits(float fuelCost)
        {
            return (int)Math.Ceiling(fuelCost);
        }

        internal static WorldTargetTransportStatus ShuttleReturnStatus(
            bool hasReturnDistance,
            int distance,
            int maximum)
        {
            if (!hasReturnDistance)
                return WorldTargetTransportStatus.RouteUnavailable;

            return distance > maximum
                ? WorldTargetTransportStatus.BeyondMaximumRange
                : WorldTargetTransportStatus.Available;
        }

        internal static WorldTargetTransportInfo CalculateShuttleFlight(
            int distanceTo,
            float outboundLayerFactor,
            int? distanceReturn,
            float returnLayerFactor)
        {
            WorldTargetFlightDistanceContext outbound = DistanceContext(
                distanceTo,
                outboundLayerFactor,
                FixedMaximumDistance(WorldTargetTransportConstants.ShuttleMaxLaunchDistance, outboundLayerFactor));
            WorldTargetFlightDistanceContext inbound = distanceReturn.HasValue
                ? DistanceContext(
                    distanceReturn.Value,
                    returnLayerFactor,
                    FixedMaximumDistance(WorldTargetTransportConstants.ShuttleMaxLaunchDistance, returnLayerFactor))
                : default;
            float fuelTo = ShuttleFuelCost(distanceTo, outboundLayerFactor);
            float fuelReturn = distanceReturn.HasValue ? ShuttleFuelCost(distanceReturn.Value, returnLayerFactor) : -1f;

            return new WorldTargetTransportInfo(
                WorldTargetTransportKind.Shuttle,
                distanceTo > outbound.MaxDistance
                    ? WorldTargetTransportStatus.BeyondMaximumRange
                    : WorldTargetTransportStatus.Available,
                distanceTo: distanceTo,
                distanceReturn: distanceReturn ?? -1,
                fuelCost: fuelTo,
                fuelReturnCost: fuelReturn,
                fuelTotalCost: distanceReturn.HasValue ? fuelTo + fuelReturn : -1f,
                flightDistance: outbound,
                returnFlightDistance: inbound,
                returnStatus: ShuttleReturnStatus(distanceReturn.HasValue, distanceReturn ?? -1, inbound.MaxDistance));
        }

        private static WorldTargetFlightDistanceContext DistanceContext(int distance, float layerFactor, int maximum)
        {
            float factor = NormalizeLayerFactor(layerFactor);
            return new WorldTargetFlightDistanceContext(
                distance,
                factor,
                maximum,
                (int)Math.Ceiling(distance * factor));
        }
    }
}