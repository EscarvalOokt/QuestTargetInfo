namespace QuestTargetInfo.Info
{
    internal readonly struct WorldTargetTransportInfo(
        WorldTargetTransportKind kind,
        WorldTargetTransportStatus status,
        int distanceTo = -1,
        int distanceReturn = -1,
        float fuelCost = -1f,
        float fuelReturnCost = -1f,
        float fuelTotalCost = -1f,
        string reason = null,
        WorldTargetFlightDistanceContext flightDistance = default,
        WorldTargetFlightDistanceContext returnFlightDistance = default,
        WorldTargetTransportStatus returnStatus = WorldTargetTransportStatus.Available)
    {
        public WorldTargetTransportKind Kind { get; } = kind;

        public WorldTargetTransportStatus Status { get; } = status;

        public int DistanceTo { get; } = distanceTo;

        public int DistanceReturn { get; } = distanceReturn;

        public float FuelCost { get; } = fuelCost;

        public float FuelReturnCost { get; } = fuelReturnCost;

        public float FuelTotalCost { get; } = fuelTotalCost;

        public string Reason { get; } = reason;

        public WorldTargetFlightDistanceContext FlightDistance { get; } = flightDistance;

        public WorldTargetFlightDistanceContext ReturnFlightDistance { get; } = returnFlightDistance;

        public WorldTargetTransportStatus ReturnStatus { get; } = returnStatus;

        public bool IsAvailable => Status == WorldTargetTransportStatus.Available;
    }
}