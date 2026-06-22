namespace QuestTargetInfo
{
    internal static class WorldTargetTransportConstants
    {
        // PassengerShuttle defines fixedLaunchDistanceMax = 62.
        internal const int ShuttleMaxLaunchDistance = 62;

        // Vanilla maximum regular transport pod range.
        // MechanoidDropPod defines fixedLaunchDistanceMax = 67 with the XML comment:
        // "maximum regular transport pod range".
        // Regular TransportPod does not define fixedLaunchDistanceMax because its range is fuel-based.
        internal const int TransportPodMaxLaunchDistance = 67;

        // AncientTransportPod defines fixedLaunchDistanceMax = 53 with the XML comment:
        // "80% of full transport pod range".
        // The game does not clearly expose this before launch target selection,
        // so the info window shows it as an additional compatibility warning.
        internal const int AncientTransportPodMaxLaunchDistance = 53;

        // Passenger shuttle fuel cost used by vanilla launch calculations.
        internal const float ShuttleFuelPerTile = 3f;

        // Transport pod fuel cost used by vanilla launch calculations.
        internal const float TransportPodFuelPerTile = 2.25f;

        // Vanilla minimum launch fuel cost.
        internal const float MinimumFuelCost = 50f;
    }
}