namespace QuestTargetInfo
{
    internal readonly struct WorldTargetFlightDistanceContext
    {
        public WorldTargetFlightDistanceContext(
            int distance,
            float targetLayerRangeDistanceFactor,
            int maxDistance = -1,
            int fuelAdjustedDistance = -1)
        {
            Distance = distance;
            TargetLayerRangeDistanceFactor = targetLayerRangeDistanceFactor;
            MaxDistance = maxDistance;
            FuelAdjustedDistance = fuelAdjustedDistance;
        }

        public int Distance { get; }

        public float TargetLayerRangeDistanceFactor { get; }

        public int MaxDistance { get; }

        public int FuelAdjustedDistance { get; }

        public bool HasDistance => Distance >= 0;

        public bool HasMaxDistance => MaxDistance >= 0;

        public bool HasFuelAdjustedDistance => FuelAdjustedDistance >= 0;

        public bool UsesAdjustedDistance =>
            HasDistance
            && TargetLayerRangeDistanceFactor > 0f
            && TargetLayerRangeDistanceFactor != 1f;
    }
}