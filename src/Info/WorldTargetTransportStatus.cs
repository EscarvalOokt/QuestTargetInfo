namespace QuestTargetInfo
{
    internal enum WorldTargetTransportStatus
    {
        Available,

        NoDlc,
        NoVehicle,

        InvalidOrigin,
        InvalidTarget,
        InvalidWorldObject,
        InvalidLandingTarget,

        RequiresSignalJammer,

        NoLayerPath,
        NoRoute,

        BeyondMaximumRange,
        NotEnoughFuel,

        RouteUnavailable
    }
}