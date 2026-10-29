using RimWorld.Planet;

namespace QuestTargetInfo.Info
{
    internal readonly struct WorldTargetDistanceInfo(
        int distanceTo,
        int distanceReturn,
        PlanetLayer fromLayer,
        PlanetLayer toLayer)
    {
        public int DistanceTo { get; } = distanceTo;

        public int DistanceReturn { get; } = distanceReturn;

        public PlanetLayer FromLayer { get; } = fromLayer;

        public PlanetLayer ToLayer { get; } = toLayer;
    }
}