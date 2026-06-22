using System.Collections.Generic;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoLineBuilder
    {
        public static IEnumerable<string> BuildLines(WorldTargetInfoRequest request)
        {
            WorldTargetRouteInfo route = WorldTargetRouteCalculator.Calculate(request);

            if(route.Status == WorldTargetRouteStatus.SameTile)
            {
                yield return "QuestTargetInfo.EstimatedDistanceTiles".Translate(0);
                yield return "QuestTargetInfo.SameTile".Translate();
                yield break;
            }

            if(route.HasDistance)
            {
                foreach(string line in GetDistanceLines(route.Distance))
                    yield return line;
            }

            foreach(string line in GetTransportPodLines(request))
                yield return line;

            foreach(string line in GetShuttleLines(request))
                yield return line;

            foreach(string line in GetGravshipLines(request))
                yield return line;
        }

        private static IEnumerable<string> GetDistanceLines(
            WorldTargetDistanceInfo distance)
        {
            yield return "QuestTargetInfo.EstimatedDistanceTiles".Translate(distance.DistanceTo);
        }

        private static IEnumerable<string> GetTransportPodLines(
            WorldTargetInfoRequest request)
        {
            WorldTargetTransportInfo info =
                WorldTargetFlyingTransportCalculator.CalculateTransportPod(request);

            foreach(string line in GetTransportLines(info))
                yield return line;
        }

        private static IEnumerable<string> GetShuttleLines(
            WorldTargetInfoRequest request)
        {
            WorldTargetTransportInfo info =
                WorldTargetFlyingTransportCalculator.CalculateShuttle(request);

            if(info.Status == WorldTargetTransportStatus.NoDlc)
                yield break;

            foreach(string line in GetTransportLines(info))
                yield return line;
        }

        private static IEnumerable<string> GetGravshipLines(
            WorldTargetInfoRequest request)
        {
            WorldTargetTransportInfo info =
                WorldTargetFlyingTransportCalculator.CalculateGravship(request);

            if(info.Status == WorldTargetTransportStatus.NoDlc)
                yield break;

            foreach(string line in GetTransportLines(info))
                yield return line;
        }

        private static IEnumerable<string> GetTransportLines(
            WorldTargetTransportInfo info)
        {
            yield return "";
            yield return GetTransportTitle(info.Kind);

            if(info.IsAvailable)
            {
                foreach(string line in GetAvailableTransportLines(info))
                    yield return line;

                foreach(string line in GetAdditionalTransportLines(info))
                    yield return line;

                yield break;
            }

            if(info.FuelCost >= 0f)
                yield return "QuestTargetInfo.FuelCost".Translate((int)info.FuelCost);

            yield return GetStatusLine(info);

            foreach(string line in GetAdditionalTransportLines(info))
                yield return line;
        }

        private static IEnumerable<string> GetAvailableTransportLines(
            WorldTargetTransportInfo info)
        {
            yield return "QuestTargetInfo.FuelCost".Translate((int)info.FuelCost);

            if(info.Kind != WorldTargetTransportKind.Shuttle)
                yield break;

            yield return "QuestTargetInfo.FuelReturnCost".Translate((int)info.FuelReturnCost);
            yield return "QuestTargetInfo.FuelTotalCost".Translate((int)info.FuelTotalCost);
        }

        private static IEnumerable<string> GetAdditionalTransportLines(
            WorldTargetTransportInfo info)
        {
            if(info.Kind != WorldTargetTransportKind.TransportPod)
                yield break;

            if(info.DistanceTo < 0)
                yield break;

            if(info.DistanceTo > WorldTargetFlyingTransportCalculator.AncientTransportPodMaxLaunchDistance)
            {
                yield return "QuestTargetInfo.AncientTransportPodBeyondMaximumRange".Translate(
                    WorldTargetFlyingTransportCalculator.AncientTransportPodMaxLaunchDistance);

                yield break;
            }

            yield return "QuestTargetInfo.AncientTransportPodInRange".Translate(
                WorldTargetFlyingTransportCalculator.AncientTransportPodMaxLaunchDistance);
        }

        private static string GetTransportTitle(
            WorldTargetTransportKind kind)
        {
            switch(kind)
            {
                case WorldTargetTransportKind.TransportPod:
                    return "QuestTargetInfo.TransportPod".Translate();

                case WorldTargetTransportKind.Shuttle:
                    return "QuestTargetInfo.Shuttle".Translate();

                case WorldTargetTransportKind.Gravship:
                    return "QuestTargetInfo.Gravship".Translate();

                default:
                    return "QuestTargetInfo.Header".Translate();
            }
        }

        private static string GetStatusLine(
            WorldTargetTransportInfo info)
        {
            if(info.Status == WorldTargetTransportStatus.InvalidLandingTarget
                && !info.Reason.NullOrEmpty())
            {
                return info.Reason;
            }

            switch(info.Status)
            {
                case WorldTargetTransportStatus.NoVehicle:
                    if(info.Kind == WorldTargetTransportKind.Gravship)
                        return "QuestTargetInfo.NoGravship".Translate();

                    return "QuestTargetInfo.NoVehicle".Translate();

                case WorldTargetTransportStatus.InvalidOrigin:
                    return "QuestTargetInfo.InvalidOrigin".Translate();

                case WorldTargetTransportStatus.InvalidTarget:
                    return "QuestTargetInfo.InvalidTarget".Translate();

                case WorldTargetTransportStatus.InvalidWorldObject:
                    return "QuestTargetInfo.InvalidWorldObject".Translate();

                case WorldTargetTransportStatus.InvalidLandingTarget:
                    return "QuestTargetInfo.InvalidLandingTarget".Translate();

                case WorldTargetTransportStatus.RequiresSignalJammer:
                    return "QuestTargetInfo.RequiresSignalJammer".Translate();

                case WorldTargetTransportStatus.NoLayerPath:
                    return "QuestTargetInfo.NoLayerPath".Translate();

                case WorldTargetTransportStatus.NoRoute:
                    return "QuestTargetInfo.RouteUnavailable".Translate();

                case WorldTargetTransportStatus.BeyondMaximumRange:
                    return "QuestTargetInfo.BeyondMaximumRange".Translate();

                case WorldTargetTransportStatus.NotEnoughFuel:
                    if(info.Kind == WorldTargetTransportKind.Gravship)
                        return "QuestTargetInfo.NotEnoughFuelInGravshipTanks".Translate();

                    return "QuestTargetInfo.NotEnoughFuel".Translate();

                case WorldTargetTransportStatus.RouteUnavailable:
                    if(info.Kind == WorldTargetTransportKind.Gravship)
                        return "QuestTargetInfo.GravshipRouteUnavailable".Translate();

                    return "QuestTargetInfo.RouteUnavailable".Translate();

                default:
                    return "QuestTargetInfo.RouteUnavailable".Translate();
            }
        }
    }
}