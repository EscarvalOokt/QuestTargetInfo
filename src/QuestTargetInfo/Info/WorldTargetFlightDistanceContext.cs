namespace QuestTargetInfo.Info
{
    internal readonly struct WorldTargetFlightDistanceContext(
        int distance,
        float targetLayerRangeDistanceFactor,
        int maxDistance = -1,
        int fuelAdjustedDistance = -1)
    {
        private readonly bool _hasValue = true;

        public int Distance { get; } = distance;

        public float TargetLayerRangeDistanceFactor { get; } = targetLayerRangeDistanceFactor;

        public int MaxDistance { get; } = maxDistance;

        public int FuelAdjustedDistance { get; } = fuelAdjustedDistance;

        public bool HasDistance => _hasValue && Distance >= 0;

        public bool HasMaxDistance => _hasValue && MaxDistance >= 0;

        public bool HasFuelAdjustedDistance => _hasValue && FuelAdjustedDistance >= 0;

        public bool UsesAdjustedDistance =>
            HasDistance && TargetLayerRangeDistanceFactor > 0f && TargetLayerRangeDistanceFactor != 1f;
    }
}