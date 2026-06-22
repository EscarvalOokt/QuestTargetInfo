using RimWorld.Planet;

namespace QuestTargetInfo
{
    internal readonly struct WorldTargetDistanceInfo
    {
        public WorldTargetDistanceInfo(
            int distanceTo,
            int distanceReturn,
            PlanetLayer fromLayer,
            PlanetLayer toLayer)
        {
            DistanceTo = distanceTo;
            DistanceReturn = distanceReturn;
            FromLayer = fromLayer;
            ToLayer = toLayer;
        }

        public int DistanceTo { get; }

        public int DistanceReturn { get; }

        public PlanetLayer FromLayer { get; }

        public PlanetLayer ToLayer { get; }
    }
}