using NUnit.Framework;
using QuestTargetInfo.Runtime;

namespace QuestTargetInfo.Tests;

[TestFixture]
public class RuntimeCacheTests
{
    [Test]
    public void PausedGameStillRefreshesDynamicModelAfterHalfSecond()
    {
        var cache = new TimedModelCache<int, int, int, int>();
        var geometryCalls = 0;
        var modelCalls = 0;
        int Read(double time) => cache.Get(1, 1, time, () => ++geometryCalls, _ => ++modelCalls);

        Assert.That(Read(10), Is.EqualTo(1));
        Assert.That(Read(10.499), Is.EqualTo(1));
        Assert.That(Read(10.5), Is.EqualTo(2));
        Assert.That(Read(10.5), Is.EqualTo(2));
        Assert.That(geometryCalls, Is.EqualTo(1), "Dynamic refresh must reuse the route geometry.");
    }

    [Test]
    public void SettingsAndTargetPresentationChangesRefreshImmediatelyWithoutRecalculatingRoute()
    {
        var cache = new TimedModelCache<int, (int Target, int Settings), int, int>();
        var geometryCalls = 0;
        var modelCalls = 0;

        int Read(int target, int settings) =>
            cache.Get(1, (target, settings), 20, () => ++geometryCalls, _ => ++modelCalls);

        Assert.That(Read(1, 1), Is.EqualTo(1));
        Assert.That(Read(1, 2), Is.EqualTo(2));
        Assert.That(Read(2, 2), Is.EqualTo(3));
        Assert.That(geometryCalls, Is.EqualTo(1));
    }

    [Test]
    public void ChangedGeometryAndClearedLifecycleCannotReuseOldRoute()
    {
        var cache = new TimedModelCache<int, int, int, int>();
        var geometryCalls = 0;
        int Read(int key) => cache.Get(key, 1, 20, () => ++geometryCalls, route => route);

        Assert.That(Read(1), Is.EqualTo(1));
        Assert.That(Read(2), Is.EqualTo(2));
        cache.Clear();
        Assert.That(Read(2), Is.EqualTo(3));
    }

    [Test]
    public void ClockResetCannotLeaveModelStale()
    {
        var cache = new TimedModelCache<int, int, int, int>();
        var builds = 0;
        cache.Get(1, 1, 50, () => 1, _ => ++builds);
        Assert.That(cache.Get(1, 1, 1, () => 1, _ => ++builds), Is.EqualTo(2));
    }

    [Test]
    public void WorldMapLayerAndMovingTargetArePartOfGeometryIdentity()
    {
        object world = new();
        object map = new();
        object surface = new();
        object orbit = new();
        var original = new WorldTargetGeometryKey(world, map, surface, orbit, 10, 20);

        Assert.That(original, Is.EqualTo(new WorldTargetGeometryKey(world, map, surface, orbit, 10, 20)));
        Assert.That(original, Is.Not.EqualTo(new WorldTargetGeometryKey(new object(), map, surface, orbit, 10, 20)));
        Assert.That(original, Is.Not.EqualTo(new WorldTargetGeometryKey(world, new object(), surface, orbit, 10, 20)));
        Assert.That(original, Is.Not.EqualTo(new WorldTargetGeometryKey(world, map, surface, surface, 10, 20)));
        Assert.That(original, Is.Not.EqualTo(new WorldTargetGeometryKey(world, map, surface, orbit, 11, 20)));
        Assert.That(original, Is.Not.EqualTo(new WorldTargetGeometryKey(world, map, surface, orbit, 10, 21)));
    }

    [Test]
    public void DirectionalConnectionChangeRefreshesGeometryWithoutChangingSelection()
    {
        object world = new();
        object map = new();
        object surface = new();
        object orbit = new();
        var outboundOnly = new WorldTargetTopologySnapshot(
            (surface, null, 100, true, 0),
            (surface, orbit, 0, false, 50),
            (orbit, null, 10, true, 0));
        var roundTrip = new WorldTargetTopologySnapshot(
            (surface, null, 100, true, 0),
            (surface, orbit, 0, false, 50),
            (orbit, null, 10, true, 0),
            (orbit, surface, 0, false, 0));
        var before = new WorldTargetGeometryKey(world, map, surface, orbit, 1, 2, outboundOnly);
        var after = new WorldTargetGeometryKey(world, map, surface, orbit, 1, 2, roundTrip);

        Assert.That(before.Equals(after), Is.False);
        Assert.That(
            before.HasSameEndpoints(after),
            Is.True,
            "Topology refresh must not reset the scroll position of the selected target.");
    }

    [Test]
    public void EquivalentTopologySnapshotsReuseGeometry()
    {
        object layer = new();
        var first = new WorldTargetTopologySnapshot((layer, null, 100, true, 0));
        var second = new WorldTargetTopologySnapshot((layer, null, 100, true, 0));
        Assert.That(first, Is.EqualTo(second));
        Assert.That(first, Is.Not.EqualTo(new WorldTargetTopologySnapshot((layer, null, 100, false, 0))));
        Assert.That(first, Is.Not.EqualTo(new WorldTargetTopologySnapshot((new object(), null, 100, true, 0))));
    }
}