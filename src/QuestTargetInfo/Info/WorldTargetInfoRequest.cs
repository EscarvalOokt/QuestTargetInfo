using RimWorld.Planet;

namespace QuestTargetInfo.Info
{
    internal sealed class WorldTargetInfoRequest(
        PlanetTile originTile,
        PlanetTile targetTile,
        string targetLabel,
        WorldObject targetObject,
        WorldTargetInfoSource source)
    {
        public PlanetTile OriginTile { get; } = originTile;

        public PlanetTile TargetTile { get; } = targetTile;

        public string TargetLabel { get; } = targetLabel;

        public WorldObject TargetObject { get; } = targetObject;

        public WorldTargetInfoSource Source { get; } = source;
    }
}