using NUnit.Framework;
using QuestTargetInfo.Info;

namespace QuestTargetInfo.Tests
{
    [TestFixture]
    public class CalculationContractsTests
    {
        [TestCase(1f, 66)]
        [TestCase(0.5f, 133)]
        [TestCase(2f, 33)]
        [TestCase(20f, 3)]
        [TestCase(0f, 66)]
        public void PodRangeIsLimitedByFullLauncherFuel(float factor, int maximum)
        {
            Assert.That(WorldTargetTransportMath.PodMaximumDistance(factor), Is.EqualTo(maximum));
            Assert.That(
                WorldTargetTransportMath.PodFuelCost(maximum, factor),
                Is.LessThanOrEqualTo(WorldTargetTransportConstants.TransportPodFuelCapacity));
            Assert.That(
                WorldTargetTransportMath.PodFuelCost(maximum + 1, factor),
                Is.GreaterThan(WorldTargetTransportConstants.TransportPodFuelCapacity));
        }

        [TestCase(0, 1f, 1f)]
        [TestCase(1, 1f, 2.25f)]
        [TestCase(1, 0.1f, 1f)]
        [TestCase(23, 1f, 51.75f)]
        [TestCase(66, 1f, 148.5f)]
        [TestCase(67, 1f, 150.75f)]
        public void PodFuelUsesItsOwnMinimum(int distance, float factor, float expected)
        {
            Assert.That(WorldTargetTransportMath.PodFuelCost(distance, factor), Is.EqualTo(expected).Within(0.0001f));
        }

        [TestCase(1, 1f, 50f)]
        [TestCase(17, 1f, 51f)]
        [TestCase(20, 2f, 120f)]
        public void ShuttleKeepsItsSeparateMinimum(int distance, float factor, float expected)
        {
            Assert.That(WorldTargetTransportMath.ShuttleFuelCost(distance, factor), Is.EqualTo(expected));
        }

        [TestCase(0, 1f, 10f, 0f, 50f)]
        [TestCase(10, 1f, 10f, 0f, 100f)]
        [TestCase(10, 1f, 20f, 0f, 200f)]
        [TestCase(10, 1f, 10f, 50f, 150f)]
        [TestCase(2, 20f, 10f, 0f, 400f)]
        [TestCase(10, 0.5f, 10f, 50f, 100f)]
        public void GravshipFuelUsesCurrentEngineLayerAndConnectionCosts(
            int distance,
            float factor,
            float fuelPerTile,
            float layerCost,
            float expected)
        {
            Assert.That(
                WorldTargetTransportMath.GravshipFuelCost(distance, factor, fuelPerTile, layerCost),
                Is.EqualTo(expected));
        }

        [Test]
        public void GravshipFloorsFractionalMaximumWhileFixedVehiclesRound()
        {
            Assert.Multiple(() =>
            {
                Assert.That(WorldTargetTransportMath.GravshipMaximumDistance(100f, 1.5f), Is.EqualTo(66));
                Assert.That(WorldTargetTransportMath.FixedMaximumDistance(100f, 1.5f), Is.EqualTo(67));
                Assert.That(WorldTargetTransportMath.FixedMaximumDistance(53f, 2f), Is.EqualTo(26));
                Assert.That(WorldTargetTransportMath.FixedMaximumDistance(55f, 2f), Is.EqualTo(28));
            });
        }

        [TestCase(0f, 0)]
        [TestCase(0.1f, 1)]
        [TestCase(50f, 50)]
        [TestCase(51.75f, 52)]
        [TestCase(148.5f, 149)]
        public void RequiredFuelNeverRoundsBelowTheCalculatedCost(float cost, int displayed)
        {
            Assert.That(WorldTargetTransportMath.RequiredFuelUnits(cost), Is.EqualTo(displayed));
        }

        [Test]
        public void AbsentDistanceCannotLookLikeAZeroLengthFlight()
        {
            WorldTargetFlightDistanceContext missing = default;
            var sameTile = new WorldTargetFlightDistanceContext(0, 1f, 62, 0);
            Assert.Multiple(() =>
            {
                Assert.That(missing.HasDistance, Is.False);
                Assert.That(missing.HasMaxDistance, Is.False);
                Assert.That(missing.HasFuelAdjustedDistance, Is.False);
                Assert.That(sameTile.HasDistance, Is.True);
                Assert.That(sameTile.HasMaxDistance, Is.True);
                Assert.That(sameTile.HasFuelAdjustedDistance, Is.True);
                Assert.That(sameTile.Distance, Is.Zero);
            });
        }

        [Test]
        public void MissingReturnDoesNotBlockOutboundOrInventReturnFuel()
        {
            WorldTargetTransportInfo result = WorldTargetTransportMath.CalculateShuttleFlight(30, 1f, null, 1f);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsAvailable, Is.True);
                Assert.That(result.FuelCost, Is.EqualTo(90f));
                Assert.That(result.ReturnStatus, Is.EqualTo(WorldTargetTransportStatus.RouteUnavailable));
                Assert.That(result.DistanceReturn, Is.EqualTo(-1));
                Assert.That(result.ReturnFlightDistance.HasDistance, Is.False);
                Assert.That(result.FuelReturnCost, Is.EqualTo(-1f));
                Assert.That(result.FuelTotalCost, Is.EqualTo(-1f));
            });
        }

        [Test]
        public void ReturnLayerCanExceedRangeWithoutChangingOutboundAvailability()
        {
            WorldTargetTransportInfo result = WorldTargetTransportMath.CalculateShuttleFlight(30, 1f, 30, 3f);
            Assert.Multiple(() =>
            {
                Assert.That(result.IsAvailable, Is.True);
                Assert.That(result.ReturnStatus, Is.EqualTo(WorldTargetTransportStatus.BeyondMaximumRange));
                Assert.That(result.FlightDistance.MaxDistance, Is.EqualTo(62));
                Assert.That(result.ReturnFlightDistance.MaxDistance, Is.EqualTo(21));
                Assert.That(result.FuelReturnCost, Is.EqualTo(270f));
                Assert.That(result.FuelTotalCost, Is.EqualTo(360f));
            });
        }

        [Test]
        public void OutboundRangeStatusIsIndependentFromAValidReturn()
        {
            WorldTargetTransportInfo result = WorldTargetTransportMath.CalculateShuttleFlight(63, 1f, 1, 1f);
            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(WorldTargetTransportStatus.BeyondMaximumRange));
                Assert.That(result.ReturnStatus, Is.EqualTo(WorldTargetTransportStatus.Available));
                Assert.That(result.FuelCost, Is.EqualTo(189f));
                Assert.That(result.FuelReturnCost, Is.EqualTo(50f));
                Assert.That(result.FuelTotalCost, Is.EqualTo(239f));
            });
        }

        [Test]
        public void DirectionalRoutePreservesOutboundWhenNoReturnPathExists()
        {
            var route = new WorldTargetRouteInfo(
                WorldTargetRouteStatus.Available,
                new WorldTargetDistanceInfo(30, -1, null, null),
                WorldTargetRouteStatus.NoLayerPath);
            Assert.Multiple(() =>
            {
                Assert.That(route.HasDistance, Is.True);
                Assert.That(route.Distance.DistanceTo, Is.EqualTo(30));
                Assert.That(route.HasReturnDistance, Is.False);
            });
        }

        [Test]
        public void DirectionalRouteDoesNotInventOutboundFromAValidReturn()
        {
            var route = new WorldTargetRouteInfo(
                WorldTargetRouteStatus.NoRoute,
                new WorldTargetDistanceInfo(-1, 30, null, null),
                WorldTargetRouteStatus.Available);
            Assert.Multiple(() =>
            {
                Assert.That(route.HasDistance, Is.False);
                Assert.That(route.HasReturnDistance, Is.True);
                Assert.That(route.Distance.DistanceReturn, Is.EqualTo(30));
            });
        }
    }
}