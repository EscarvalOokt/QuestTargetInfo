using System.Collections.Generic;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoModelBuilder
    {
        public static WorldTargetInfoModel Build(WorldTargetInfoRequest request)
        {
            var sections = new List<WorldTargetInfoSectionModel>();

            WorldTargetRouteInfo route = WorldTargetRouteCalculator.Calculate(request);

            if(route.Status == WorldTargetRouteStatus.SameTile)
            {
                sections.Add(BuildSameTileRouteSection());
                return new WorldTargetInfoModel(GetTitle(request), sections);
            }

            if(route.HasDistance)
                sections.Add(BuildDistanceRouteSection(route.Distance));

            sections.Add(BuildTransportSection(
                WorldTargetFlyingTransportCalculator.CalculateTransportPod(request)));

            WorldTargetTransportInfo shuttle =
                WorldTargetFlyingTransportCalculator.CalculateShuttle(request);

            if(shuttle.Status != WorldTargetTransportStatus.NoDlc)
                sections.Add(BuildTransportSection(shuttle));

            WorldTargetTransportInfo gravship =
                WorldTargetFlyingTransportCalculator.CalculateGravship(request);

            if(gravship.Status != WorldTargetTransportStatus.NoDlc)
                sections.Add(BuildTransportSection(gravship));

            return new WorldTargetInfoModel(GetTitle(request), sections);
        }

        private static WorldTargetInfoSectionModel BuildSameTileRouteSection()
        {
            var lines = new List<WorldTargetInfoLineModel>
            {
                new WorldTargetInfoLineModel(
                    "QuestTargetInfo.EstimatedDistanceTiles".Translate(0).ToString()),

                new WorldTargetInfoLineModel(
                    "QuestTargetInfo.SameTile".Translate().ToString())
            };

            return new WorldTargetInfoSectionModel(
                WorldTargetInfoSectionKind.Route,
                title: null,
                statusText: null,
                WorldTargetInfoStatusKind.Available,
                lines);
        }

        private static WorldTargetInfoSectionModel BuildDistanceRouteSection(
            WorldTargetDistanceInfo distance)
        {
            var lines = new List<WorldTargetInfoLineModel>
            {
                new WorldTargetInfoLineModel(
                    "QuestTargetInfo.EstimatedDistanceTiles".Translate(distance.DistanceTo).ToString())
            };

            return new WorldTargetInfoSectionModel(
                WorldTargetInfoSectionKind.Route,
                title: null,
                statusText: null,
                WorldTargetInfoStatusKind.Available,
                lines);
        }

        private static WorldTargetInfoSectionModel BuildTransportSection(
            WorldTargetTransportInfo info)
        {
            var lines = new List<WorldTargetInfoLineModel>();

            if(info.IsAvailable)
                AddAvailableTransportLines(info, lines);
            else
                AddUnavailableTransportLines(info, lines);

            AddAdditionalTransportLines(info, lines);

            return new WorldTargetInfoSectionModel(
                GetSectionKind(info.Kind),
                GetTransportTitle(info.Kind),
                GetStatusSummaryText(info),
                GetStatusKind(info),
                lines);
        }

        private static void AddAvailableTransportLines(
            WorldTargetTransportInfo info,
            List<WorldTargetInfoLineModel> lines)
        {
            lines.Add(new WorldTargetInfoLineModel(
                "QuestTargetInfo.FuelCost".Translate((int)info.FuelCost).ToString()));

            if(info.Kind != WorldTargetTransportKind.Shuttle)
                return;

            lines.Add(new WorldTargetInfoLineModel(
                "QuestTargetInfo.FuelReturnCost".Translate((int)info.FuelReturnCost).ToString()));

            lines.Add(new WorldTargetInfoLineModel(
                "QuestTargetInfo.FuelTotalCost".Translate((int)info.FuelTotalCost).ToString()));
        }

        private static void AddUnavailableTransportLines(
            WorldTargetTransportInfo info,
            List<WorldTargetInfoLineModel> lines)
        {
            if(info.FuelCost >= 0f)
            {
                lines.Add(new WorldTargetInfoLineModel(
                    "QuestTargetInfo.FuelEstimate".Translate((int)info.FuelCost).ToString()));
            }

            string detail = GetStatusDetailsLine(info);
            if(!detail.NullOrEmpty())
                lines.Add(new WorldTargetInfoLineModel(detail));
        }

        private static void AddAdditionalTransportLines(
            WorldTargetTransportInfo info,
            List<WorldTargetInfoLineModel> lines)
        {
            if(info.Kind != WorldTargetTransportKind.TransportPod)
                return;

            if(info.DistanceTo < 0)
                return;

            if(info.DistanceTo > WorldTargetFlyingTransportCalculator.AncientTransportPodMaxLaunchDistance)
            {
                lines.Add(new WorldTargetInfoLineModel(
                    "QuestTargetInfo.AncientTransportPodBeyondMaximumRange".Translate(
                        WorldTargetFlyingTransportCalculator.AncientTransportPodMaxLaunchDistance).ToString()));

                return;
            }

            lines.Add(new WorldTargetInfoLineModel(
                "QuestTargetInfo.AncientTransportPodInRange".Translate(
                    WorldTargetFlyingTransportCalculator.AncientTransportPodMaxLaunchDistance).ToString()));
        }

        private static string GetTitle(WorldTargetInfoRequest request)
        {
            if(request != null && !string.IsNullOrEmpty(request.TargetLabel))
                return request.TargetLabel;

            return "QuestTargetInfo.Header".Translate().ToString();
        }

        private static WorldTargetInfoSectionKind GetSectionKind(
            WorldTargetTransportKind kind)
        {
            switch(kind)
            {
                case WorldTargetTransportKind.TransportPod:
                    return WorldTargetInfoSectionKind.TransportPod;

                case WorldTargetTransportKind.Shuttle:
                    return WorldTargetInfoSectionKind.Shuttle;

                case WorldTargetTransportKind.Gravship:
                    return WorldTargetInfoSectionKind.Gravship;

                default:
                    return WorldTargetInfoSectionKind.Route;
            }
        }

        private static string GetTransportTitle(
            WorldTargetTransportKind kind)
        {
            switch(kind)
            {
                case WorldTargetTransportKind.TransportPod:
                    return "QuestTargetInfo.TransportPod".Translate().ToString();

                case WorldTargetTransportKind.Shuttle:
                    return "QuestTargetInfo.Shuttle".Translate().ToString();

                case WorldTargetTransportKind.Gravship:
                    return "QuestTargetInfo.Gravship".Translate().ToString();

                default:
                    return "QuestTargetInfo.Header".Translate().ToString();
            }
        }

        private static string GetStatusSummaryText(
            WorldTargetTransportInfo info)
        {
            switch(info.Status)
            {
                case WorldTargetTransportStatus.Available:
                    return "QuestTargetInfo.StatusAvailable".Translate().ToString();

                case WorldTargetTransportStatus.NoVehicle:
                    return "QuestTargetInfo.StatusNoVehicle".Translate().ToString();

                case WorldTargetTransportStatus.InvalidOrigin:
                    return "QuestTargetInfo.InvalidOrigin".Translate().ToString();

                case WorldTargetTransportStatus.InvalidTarget:
                case WorldTargetTransportStatus.InvalidWorldObject:
                    return "QuestTargetInfo.StatusInvalidTarget".Translate().ToString();

                case WorldTargetTransportStatus.InvalidLandingTarget:
                    return "QuestTargetInfo.StatusCannotLand".Translate().ToString();

                case WorldTargetTransportStatus.RequiresSignalJammer:
                    return "QuestTargetInfo.StatusRequiresSignalJammer".Translate().ToString();

                case WorldTargetTransportStatus.NoLayerPath:
                case WorldTargetTransportStatus.NoRoute:
                case WorldTargetTransportStatus.RouteUnavailable:
                    return "QuestTargetInfo.StatusNoRoute".Translate().ToString();

                case WorldTargetTransportStatus.BeyondMaximumRange:
                    return "QuestTargetInfo.StatusOutOfRange".Translate().ToString();

                case WorldTargetTransportStatus.NotEnoughFuel:
                    return "QuestTargetInfo.StatusNotEnoughFuel".Translate().ToString();

                case WorldTargetTransportStatus.NoDlc:
                    return "QuestTargetInfo.StatusUnavailable".Translate().ToString();

                default:
                    return "QuestTargetInfo.StatusUnavailable".Translate().ToString();
            }
        }

        private static string GetStatusDetailsLine(
            WorldTargetTransportInfo info)
        {
            if(info.Status == WorldTargetTransportStatus.InvalidLandingTarget
                && !info.Reason.NullOrEmpty())
            {
                return "QuestTargetInfo.Reason".Translate(info.Reason).ToString();
            }

            switch(info.Status)
            {
                case WorldTargetTransportStatus.Available:
                    return null;

                case WorldTargetTransportStatus.NoVehicle:
                    if(info.Kind == WorldTargetTransportKind.Gravship)
                        return "QuestTargetInfo.NoGravship".Translate().ToString();

                    return "QuestTargetInfo.NoVehicle".Translate().ToString();

                case WorldTargetTransportStatus.InvalidOrigin:
                    return null;

                case WorldTargetTransportStatus.InvalidTarget:
                    return "QuestTargetInfo.InvalidTarget".Translate().ToString();

                case WorldTargetTransportStatus.InvalidWorldObject:
                    return "QuestTargetInfo.InvalidWorldObject".Translate().ToString();

                case WorldTargetTransportStatus.InvalidLandingTarget:
                    return "QuestTargetInfo.InvalidLandingTarget".Translate().ToString();

                case WorldTargetTransportStatus.RequiresSignalJammer:
                    return null;

                case WorldTargetTransportStatus.NoLayerPath:
                    return "QuestTargetInfo.NoLayerPath".Translate().ToString();

                case WorldTargetTransportStatus.NoRoute:
                    return "QuestTargetInfo.RouteUnavailable".Translate().ToString();

                case WorldTargetTransportStatus.BeyondMaximumRange:
                    return null;

                case WorldTargetTransportStatus.NotEnoughFuel:
                    if(info.Kind == WorldTargetTransportKind.Gravship)
                        return "QuestTargetInfo.NotEnoughFuelInGravshipTanks".Translate().ToString();

                    return null;

                case WorldTargetTransportStatus.RouteUnavailable:
                    if(info.Kind == WorldTargetTransportKind.Gravship)
                        return "QuestTargetInfo.GravshipRouteUnavailable".Translate().ToString();

                    return "QuestTargetInfo.RouteUnavailable".Translate().ToString();

                default:
                    return "QuestTargetInfo.RouteUnavailable".Translate().ToString();
            }
        }

        private static WorldTargetInfoStatusKind GetStatusKind(
            WorldTargetTransportInfo info)
        {
            switch(info.Status)
            {
                case WorldTargetTransportStatus.Available:
                    return WorldTargetInfoStatusKind.Available;

                case WorldTargetTransportStatus.NoDlc:
                case WorldTargetTransportStatus.NoVehicle:
                    return WorldTargetInfoStatusKind.Unavailable;

                case WorldTargetTransportStatus.BeyondMaximumRange:
                case WorldTargetTransportStatus.NotEnoughFuel:
                case WorldTargetTransportStatus.RequiresSignalJammer:
                    return WorldTargetInfoStatusKind.Warning;

                case WorldTargetTransportStatus.InvalidOrigin:
                case WorldTargetTransportStatus.InvalidTarget:
                case WorldTargetTransportStatus.InvalidWorldObject:
                case WorldTargetTransportStatus.InvalidLandingTarget:
                case WorldTargetTransportStatus.NoLayerPath:
                case WorldTargetTransportStatus.NoRoute:
                case WorldTargetTransportStatus.RouteUnavailable:
                    return WorldTargetInfoStatusKind.Error;

                default:
                    return WorldTargetInfoStatusKind.None;
            }
        }
    }
}