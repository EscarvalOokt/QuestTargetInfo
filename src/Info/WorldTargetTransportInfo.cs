namespace QuestTargetInfo
{
    internal readonly struct WorldTargetTransportInfo
    {
        public WorldTargetTransportInfo(
            WorldTargetTransportKind kind,
            WorldTargetTransportStatus status,
            int distanceTo = -1,
            int distanceReturn = -1,
            float fuelCost = -1f,
            float fuelReturnCost = -1f,
            float fuelTotalCost = -1f,
            string reason = null,
            WorldTargetFlightDistanceContext flightDistance = default,
            WorldTargetFlightDistanceContext returnFlightDistance = default)
        {
            Kind = kind;
            Status = status;
            DistanceTo = distanceTo;
            DistanceReturn = distanceReturn;
            FuelCost = fuelCost;
            FuelReturnCost = fuelReturnCost;
            FuelTotalCost = fuelTotalCost;
            Reason = reason;
            FlightDistance = flightDistance;
            ReturnFlightDistance = returnFlightDistance;
        }

        public WorldTargetTransportKind Kind { get; }

        public WorldTargetTransportStatus Status { get; }

        public int DistanceTo { get; }

        public int DistanceReturn { get; }

        public float FuelCost { get; }

        public float FuelReturnCost { get; }

        public float FuelTotalCost { get; }

        public string Reason { get; }

        public WorldTargetFlightDistanceContext FlightDistance { get; }

        public WorldTargetFlightDistanceContext ReturnFlightDistance { get; }

        public bool IsAvailable => Status == WorldTargetTransportStatus.Available;
    }
}