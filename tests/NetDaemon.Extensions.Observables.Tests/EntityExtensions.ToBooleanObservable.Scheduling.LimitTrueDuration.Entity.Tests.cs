using NetDaemon.HassModel.Entities;

namespace NetDaemon.Extensions.Observables.Tests;

[TestClass]
public class EntityExtensionsToBooleanObservableSchedulingLimitTrueDurationEntityTests 
    : EntityExtensionsToBooleanObservableSchedulingEntityTestsSetup
{
    [TestMethod]
    public void LimitTrueDuration_LastChangedShorterAgoThanTimeSpan_True()
    {
        // Arrange
        var observable = TestEntity.LimitTrueDuration(TimeSpan.FromTicks(1), Scheduler, LastChanged.ToUniversalTime);

        // Act
        bool? result = null;
        observable.Subscribe(b => result = b);

        // Assert
        Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void LimitTrueDuration_LastChangedLongerAgoThanTimeSpan_False()
    {
        // Arrange
        var observable = TestEntity.LimitTrueDuration(
            TimeSpan.FromTicks(1), 
            Scheduler, 
            () => LastChanged.ToUniversalTime() + TimeSpan.FromTicks(1));

        // Act
        bool? result = null;
        observable.Subscribe(b => result = b);

        // Assert
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void LimitTrueDuration_SubscribeAfterTimeSpanPasses_False()
    {
        // Arrange
        var observable = TestEntity.LimitTrueDuration(TimeSpan.FromTicks(1), Scheduler, LastChanged.ToUniversalTime);

        // Act
        bool? result = null;
        ChangeEntityState(On, LastChanged - TimeSpan.FromTicks(1));
        observable.Subscribe(b => result = b);

        // Assert
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void LimitTrueDuration_LastChangedHalfwayOfTimeSpan_FalseAfterRemainingTime()
    {
        var observable = TestEntity.LimitTrueDuration(
            TimeSpan.FromTicks(4), 
            Scheduler, 
            () => LastChanged.ToUniversalTime() + TimeSpan.FromTicks(2));

        bool? result = null;
        observable.Subscribe(b => result = b);

        Assert.AreEqual(true, result);

        Scheduler.AdvanceBy(1);
        Assert.AreEqual(true, result);

        Scheduler.AdvanceBy(1);
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void LimitTrueDuration_Predicate_LastChangedShorterAgoThanTimeSpan_True()
    {
        // Arrange
        ChangeEntityState(Off, LastChanged);
        var observable = TestEntity.LimitTrueDuration(
            TimeSpan.FromMinutes(1), 
            s => s.IsOff(), 
            Scheduler,
            LastChanged.ToUniversalTime);

        // Act
        bool? result = null;
        observable.Subscribe(b => result = b);

        // Assert
        Assert.AreEqual(true, result);
    }

    [TestMethod]
    public void LimitTrueDuration_Predicate_LastChangedLongerAgoThanTimeSpan_False()
    {
        // Arrange
        ChangeEntityState(Off, LastChanged);
        var observable = TestEntity.LimitTrueDuration(
            TimeSpan.FromTicks(1), 
            s => s.IsOff(), 
            Scheduler,
            () => LastChanged.ToUniversalTime() + TimeSpan.FromTicks(1));

        // Act
        bool? result = null;
        observable.Subscribe(b => result = b);

        // Assert
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void LimitTrueDuration_Predicate_SubscribeAfterTimeSpanPasses_False()
    {
        // Arrange
        var observable = TestEntity.LimitTrueDuration(
            TimeSpan.FromTicks(1), 
            s => s.IsOff(), 
            Scheduler,
            LastChanged.ToUniversalTime);

        // Act
        bool? result = null;
        ChangeEntityState(Off, LastChanged - TimeSpan.FromTicks(1));
        observable.Subscribe(b => result = b);

        // Assert
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void LimitTrueDuration_Predicate_LastChangedHalfwayOfTimeSpan_FalseAfterRemainingTime()
    {
        ChangeEntityState(Off, LastChanged);
        var observable = TestEntity.LimitTrueDuration(
            TimeSpan.FromTicks(4),
            s => s.IsOff(),
            Scheduler,
            () => LastChanged.ToUniversalTime() + TimeSpan.FromTicks(2));

        bool? result = null;
        observable.Subscribe(b => result = b);

        Assert.AreEqual(true, result);

        Scheduler.AdvanceBy(1);
        Assert.AreEqual(true, result);

        Scheduler.AdvanceBy(1);
        Assert.AreEqual(false, result);
    }
}