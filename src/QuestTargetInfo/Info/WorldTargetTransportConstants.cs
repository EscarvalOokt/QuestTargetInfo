namespace QuestTargetInfo.Info
{
    internal static class WorldTargetTransportConstants
    {
        // PassengerShuttle defines fixedLaunchDistanceMax = 62.
        internal const int ShuttleMaxLaunchDistance = 62;

        // RimWorld 1.7 PodLauncher refuelable capacity. Regular pod range is
        // floor(capacity / (fuelPerTile * destination layer factor));
        // it is not MechanoidDropPod's separately fixed range of 67.
        internal const float TransportPodFuelCapacity = 150f;

        // AncientTransportPod defines fixedLaunchDistanceMax = 53 with the XML comment:
        // "80% of full transport pod range".
        // The game does not clearly expose this before launch target selection,
        // so the info window shows it as an additional compatibility warning.
        internal const int AncientTransportPodMaxLaunchDistance = 53;

        // Passenger shuttle fuel cost used by vanilla launch calculations.
        internal const float ShuttleFuelPerTile = 3f;

        // Transport pod fuel cost used by vanilla launch calculations.
        internal const float TransportPodFuelPerTile = 2.25f;

        // CompLaunchable.TryLaunch clamps regular pod fuel consumption to 1.
        internal const float TransportPodMinimumFuelCost = 1f;

        // PassengerShuttle explicitly defines minFuelCost = 50.
        internal const float ShuttleMinimumFuelCost = 50f;

        // GravshipUtility.TryGetPathFuelCost applies this floor after layer costs.
        internal const float GravshipMinimumFuelCost = 50f;
    }
}