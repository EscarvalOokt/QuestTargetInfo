using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoModelBuilder
    {
        public static WorldTargetInfoModel Build(WorldTargetInfoRequest request)
        {
            var sections = new List<WorldTargetInfoSectionModel>();

            WorldTargetRouteInfo route = WorldTargetRouteCalculator.Calculate(request);

            sections.Add(BuildRouteSection(request, route));

            if(route.Status == WorldTargetRouteStatus.SameTile)
                return new WorldTargetInfoModel(GetTitle(request), sections);

            sections.Add(BuildTransportSection(
                WorldTargetFlyingTransportCalculator.CalculateTransportPod(request, route)));

            WorldTargetTransportInfo shuttle =
                WorldTargetFlyingTransportCalculator.CalculateShuttle(request, route);

            if(shuttle.Status != WorldTargetTransportStatus.NoDlc)
                sections.Add(BuildTransportSection(shuttle));

            WorldTargetTransportInfo gravship =
                WorldTargetFlyingTransportCalculator.CalculateGravship(request);

            if(gravship.Status != WorldTargetTransportStatus.NoDlc)
                sections.Add(BuildTransportSection(gravship));

            return new WorldTargetInfoModel(GetTitle(request), sections);
        }

        private static WorldTargetInfoSectionModel BuildRouteSection(
            WorldTargetInfoRequest request,
            WorldTargetRouteInfo route)
        {
            var lines = new List<WorldTargetInfoLineModel>
            {
                CreateLabelValueLine(
                "QuestTargetInfo.From".Translate().ToString(),
                "QuestTargetInfo.CurrentMap".Translate().ToString())
            };

            AddRouteLayerContextLines(request, lines);
            AddRouteDistanceLine(route, lines);
            AddRouteStatusLine(route, lines);

            return new WorldTargetInfoSectionModel(
                WorldTargetInfoSectionKind.Route,
                title: null,
                statusText: null,
                GetRouteStatusKind(route.Status),
                lines);
        }

        private static void AddRouteLayerContextLines(
            WorldTargetInfoRequest request,
            List<WorldTargetInfoLineModel> lines)
        {
            if(request == null || !request.OriginTile.Valid || !request.TargetTile.Valid)
                return;

            PlanetLayer originLayer = request.OriginTile.Layer;
            PlanetLayer targetLayer = request.TargetTile.Layer;

            string originLayerLabel = GetLayerLabel(originLayer);
            string targetLayerLabel = GetLayerLabel(targetLayer);

            if(originLayer == targetLayer)
            {
                lines.Add(CreateLabelValueLine(
                    "QuestTargetInfo.Layer".Translate().ToString(),
                    originLayerLabel));

                return;
            }

            lines.Add(CreateLabelValueLine(
                "QuestTargetInfo.Route".Translate().ToString(),
                "QuestTargetInfo.LayerRoute".Translate(
                    originLayerLabel,
                    targetLayerLabel).ToString()));
        }

        private static void AddRouteDistanceLine(
            WorldTargetRouteInfo route,
            List<WorldTargetInfoLineModel> lines)
        {
            if(route.Status == WorldTargetRouteStatus.SameTile)
            {
                lines.Add(CreateLabelValueLine(
                    "QuestTargetInfo.Distance".Translate().ToString(),
                    "QuestTargetInfo.TilesValue".Translate(0).ToString()));

                return;
            }

            if(!route.HasDistance)
                return;

            lines.Add(CreateLabelValueLine(
                "QuestTargetInfo.Distance".Translate().ToString(),
                "QuestTargetInfo.TilesValue".Translate(route.Distance.DistanceTo).ToString()));
        }

        private static void AddRouteStatusLine(
            WorldTargetRouteInfo route,
            List<WorldTargetInfoLineModel> lines)
        {
            if(route.Status == WorldTargetRouteStatus.SameTile)
            {
                lines.Add(new WorldTargetInfoLineModel(
                    "QuestTargetInfo.SameTile".Translate().ToString()));

                return;
            }

            string statusLine = GetRouteStatusLine(route.Status);
            if(!statusLine.NullOrEmpty())
                lines.Add(new WorldTargetInfoLineModel(statusLine));
        }

        private static WorldTargetInfoLineModel CreateLabelValueLine(
            string label,
            string value)
        {
            return new WorldTargetInfoLineModel(
                text: null,
                label: label,
                value: value);
        }

        private static WorldTargetInfoLineModel CreateRangeLine(
            string label,
            int distance,
            int maxDistance)
        {
            return CreateLabelValueLine(
                label,
                "QuestTargetInfo.RangeValue".Translate(
                    distance,
                    maxDistance).ToString());
        }

        private static string GetLayerLabel(PlanetLayer layer)
        {
            if(layer?.Def == null)
                return "QuestTargetInfo.InvalidTarget".Translate().ToString();

            return layer.Def.LabelCap.ToString();
        }

        private static string GetRouteStatusLine(WorldTargetRouteStatus status)
        {
            switch(status)
            {
                case WorldTargetRouteStatus.InvalidOrigin:
                    return "QuestTargetInfo.InvalidOrigin".Translate().ToString();

                case WorldTargetRouteStatus.InvalidTarget:
                    return "QuestTargetInfo.InvalidTarget".Translate().ToString();

                case WorldTargetRouteStatus.NoLayerPath:
                    return "QuestTargetInfo.NoLayerPath".Translate().ToString();

                case WorldTargetRouteStatus.NoRoute:
                    return "QuestTargetInfo.RouteUnavailable".Translate().ToString();

                default:
                    return null;
            }
        }

        private static WorldTargetInfoStatusKind GetRouteStatusKind(
            WorldTargetRouteStatus status)
        {
            switch(status)
            {
                case WorldTargetRouteStatus.Available:
                case WorldTargetRouteStatus.SameTile:
                    return WorldTargetInfoStatusKind.Available;

                case WorldTargetRouteStatus.InvalidOrigin:
                case WorldTargetRouteStatus.InvalidTarget:
                case WorldTargetRouteStatus.NoLayerPath:
                case WorldTargetRouteStatus.NoRoute:
                    return WorldTargetInfoStatusKind.Error;

                default:
                    return WorldTargetInfoStatusKind.None;
            }
        }

        private static WorldTargetInfoSectionModel BuildTransportSection(
            WorldTargetTransportInfo info)
        {
            var lines = new List<WorldTargetInfoLineModel>();

            AddTransportRangeLines(info, lines);

            if(info.IsAvailable)
                AddAvailableTransportLines(info, lines);
            else
                AddUnavailableTransportLines(info, lines);

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
            lines.Add(CreateLabelValueLine(
                "QuestTargetInfo.Fuel".Translate().ToString(),
                ((int)info.FuelCost).ToString()));

            if(info.Kind != WorldTargetTransportKind.Shuttle)
                return;

            lines.Add(CreateLabelValueLine(
                "QuestTargetInfo.ReturnFuel".Translate().ToString(),
                ((int)info.FuelReturnCost).ToString()));

            lines.Add(CreateLabelValueLine(
                "QuestTargetInfo.TotalFuel".Translate().ToString(),
                ((int)info.FuelTotalCost).ToString()));
        }

        private static void AddUnavailableTransportLines(
            WorldTargetTransportInfo info,
            List<WorldTargetInfoLineModel> lines)
        {
            if(info.FuelCost >= 0f)
            {
                lines.Add(CreateLabelValueLine(
                    "QuestTargetInfo.EstimatedFuel".Translate().ToString(),
                    ((int)info.FuelCost).ToString()));
            }

            string detail = GetStatusDetailsLine(info);
            if(!detail.NullOrEmpty())
                lines.Add(new WorldTargetInfoLineModel(detail));
        }

        private static void AddTransportRangeLines(
            WorldTargetTransportInfo info,
            List<WorldTargetInfoLineModel> lines)
        {
            if(!info.FlightDistance.HasDistance)
                return;

            switch(info.Kind)
            {
                case WorldTargetTransportKind.TransportPod:
                    AddPrimaryRangeLine(info, lines);
                    AddAncientTransportPodRangeLine(info, lines);
                    return;

                case WorldTargetTransportKind.Gravship:
                    AddPrimaryRangeLine(info, lines);
                    return;

                default:
                    return;
            }
        }

        private static void AddPrimaryRangeLine(
            WorldTargetTransportInfo info,
            List<WorldTargetInfoLineModel> lines)
        {
            if(!info.FlightDistance.HasMaxDistance)
                return;

            lines.Add(CreateRangeLine(
                "QuestTargetInfo.Range".Translate().ToString(),
                info.FlightDistance.Distance,
                info.FlightDistance.MaxDistance));
        }

        private static void AddAncientTransportPodRangeLine(
            WorldTargetTransportInfo info,
            List<WorldTargetInfoLineModel> lines)
        {
            int ancientPodMaxDistance =
                WorldTargetFlyingTransportCalculator.GetLayerAdjustedFixedLaunchDistanceMax(
                    WorldTargetFlyingTransportCalculator.AncientTransportPodMaxLaunchDistance,
                    info.FlightDistance);

            if(ancientPodMaxDistance < 0)
                return;

            lines.Add(CreateRangeLine(
                "QuestTargetInfo.AncientTransportPodShort".Translate().ToString(),
                info.FlightDistance.Distance,
                ancientPodMaxDistance));
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