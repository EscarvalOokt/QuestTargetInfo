namespace QuestTargetInfo.Info
{
    internal readonly struct WorldTargetRouteInfo(
        WorldTargetRouteStatus status,
        WorldTargetDistanceInfo distance,
        WorldTargetRouteStatus? returnStatus = null)
    {
        public WorldTargetRouteStatus Status { get; } = status;

        public WorldTargetDistanceInfo Distance { get; } = distance;

        public WorldTargetRouteStatus ReturnStatus { get; } = returnStatus ?? status;

        public bool HasDistance => Status == WorldTargetRouteStatus.Available;

        public bool HasReturnDistance =>
            (ReturnStatus == WorldTargetRouteStatus.Available || ReturnStatus == WorldTargetRouteStatus.SameTile) &&
            Distance.DistanceReturn >= 0;
    }
}