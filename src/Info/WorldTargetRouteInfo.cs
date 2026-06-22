namespace QuestTargetInfo
{
    internal readonly struct WorldTargetRouteInfo
    {
        public WorldTargetRouteInfo(
            WorldTargetRouteStatus status,
            WorldTargetDistanceInfo distance)
        {
            Status = status;
            Distance = distance;
        }

        public WorldTargetRouteStatus Status { get; }

        public WorldTargetDistanceInfo Distance { get; }

        public bool HasDistance => Status == WorldTargetRouteStatus.Available;
    }
}