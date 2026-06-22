using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoLineBuilder
    {
        private const float ShuttleFuelPerTile = 3f;
        private const float PodFuelPerTile = 2.25f;
        private const float MinFuel = 50f;

        public static IEnumerable<string> BuildLines(WorldTargetInfoRequest request)
        {
            WorldTargetRouteInfo route = WorldTargetRouteCalculator.Calculate(request);
            if(!route.HasDistance)
                yield break;

            WorldTargetDistanceInfo distance = route.Distance;

            foreach(string line in GetDistanceLines(distance))
                yield return line;

            foreach(string line in GetShuttleLines(distance))
                yield return line;

            foreach(string line in GetTransportPodLines(distance))
                yield return line;

            foreach(string line in GetGravshipLines(request))
                yield return line;
        }

        private static IEnumerable<string> GetDistanceLines(WorldTargetDistanceInfo distance)
        {
            yield return "QuestTargetInfo.EstimatedDistanceTiles".Translate(distance.DistanceTo);
        }

        private static IEnumerable<string> GetShuttleLines(WorldTargetDistanceInfo distance)
        {
            if(!ModsConfig.IsActive("ludeon.rimworld.odyssey"))
                yield break;

            float fuelTo = Mathf.Max(
                MinFuel,
                distance.DistanceTo * ShuttleFuelPerTile * distance.ToLayer.Def.rangeDistanceFactor);

            float fuelReturn = Mathf.Max(
                MinFuel,
                distance.DistanceReturn * ShuttleFuelPerTile * distance.FromLayer.Def.rangeDistanceFactor);

            float fuelTotal = fuelTo + fuelReturn;

            yield return "";
            yield return "QuestTargetInfo.Shuttle".Translate();
            yield return "QuestTargetInfo.FuelCost".Translate((int)fuelTo);
            yield return "QuestTargetInfo.FuelReturnCost".Translate((int)fuelReturn);
            yield return "QuestTargetInfo.FuelTotalCost".Translate((int)fuelTotal);
        }

        private static IEnumerable<string> GetTransportPodLines(WorldTargetDistanceInfo distance)
        {
            float fuelTo = Mathf.Max(
                MinFuel,
                distance.DistanceTo * PodFuelPerTile * distance.ToLayer.Def.rangeDistanceFactor);

            yield return "";
            yield return "QuestTargetInfo.TransportPod".Translate();
            yield return "QuestTargetInfo.FuelCost".Translate((int)fuelTo);
        }

        private static IEnumerable<string> GetGravshipLines(WorldTargetInfoRequest request)
        {
            if(!ModsConfig.IsActive("ludeon.rimworld.odyssey"))
                yield break;

            yield return "";
            yield return "QuestTargetInfo.Gravship".Translate();

            if(!(GravshipUtility.GetPlayerGravEngine(Find.CurrentMap) is Building_GravEngine engine))
            {
                yield return "QuestTargetInfo.NoGravship".Translate();
                yield break;
            }

            if(!GravshipUtility.TryGetPathFuelCost(
                    engine.Tile,
                    request.TargetTile,
                    out float fuelCost,
                    out int _,
                    fuelPerTile: engine.FuelPerTile))
            {
                yield break;
            }

            yield return "QuestTargetInfo.FuelCost".Translate((int)fuelCost);
        }
    }
}