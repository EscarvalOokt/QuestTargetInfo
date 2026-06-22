using RimWorld.Planet;

namespace QuestTargetInfo
{
    internal sealed class WorldTargetInfoRequest
    {
        public WorldTargetInfoRequest(
            PlanetTile originTile,
            PlanetTile targetTile,
            string targetLabel,
            WorldObject targetObject,
            WorldTargetInfoSource source)
        {
            OriginTile = originTile;
            TargetTile = targetTile;
            TargetLabel = targetLabel;
            TargetObject = targetObject;
            Source = source;
        }

        public PlanetTile OriginTile { get; }

        public PlanetTile TargetTile { get; }

        public string TargetLabel { get; }

        public WorldObject TargetObject { get; }

        public WorldTargetInfoSource Source { get; }
    }
}