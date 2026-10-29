using NUnit.Framework;
using QuestTargetInfo.UI;
using UnityEngine;

namespace QuestTargetInfo.Tests;

[TestFixture]
public class RuntimeLayoutTests
{
    [Test]
    public void QuestWindowUsesActualOwnerBounds()
    {
        var owner = new Rect(100, 80, 900, 640);
        Rect result = QuestTargetWindowLayout.Calculate(owner, 1920, 1080, false);
        Assert.That(result.x, Is.EqualTo(owner.xMax + 5));
        Assert.That(result.yMax, Is.EqualTo(owner.yMax));
    }

    [Test]
    public void UsesLeftSideWhenRightSideDoesNotFit()
    {
        var owner = new Rect(600, 100, 600, 640);
        Rect result = QuestTargetWindowLayout.Calculate(owner, 1280, 900, false);
        Assert.That(result.xMax, Is.EqualTo(owner.xMin - 5));
    }

    [TestCase(1366, 768)]
    [TestCase(800, 600)]
    [TestCase(640, 360)]
    [TestCase(200, 100)]
    public void WindowStaysInsideScaledViewport(float width, float height)
    {
        Rect result = QuestTargetWindowLayout.Calculate(new Rect(0, 200, 1010, 640), width, height, false);
        Assert.That(result.xMin, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.yMin, Is.GreaterThanOrEqualTo(0));
        Assert.That(result.xMax, Is.LessThanOrEqualTo(width));
        Assert.That(result.yMax, Is.LessThanOrEqualTo(height));
        Assert.That(result.width, Is.GreaterThan(0));
        Assert.That(result.height, Is.GreaterThan(0));
    }

    [Test]
    public void CollapseKeepsBottomAnchorAndExpansionRestoresBoundedSize()
    {
        var owner = new Rect(0, 60, 1010, 640);
        Rect expanded = QuestTargetWindowLayout.Calculate(owner, 1920, 800, false);
        Rect collapsed = QuestTargetWindowLayout.Calculate(owner, 1920, 800, true);
        Assert.That(collapsed.height, Is.EqualTo(QuestTargetWindowLayout.CollapsedHeight));
        Assert.That(collapsed.yMax, Is.EqualTo(expanded.yMax));
        Assert.That(QuestTargetWindowLayout.Calculate(owner, 1920, 800, false), Is.EqualTo(expanded));
    }
}