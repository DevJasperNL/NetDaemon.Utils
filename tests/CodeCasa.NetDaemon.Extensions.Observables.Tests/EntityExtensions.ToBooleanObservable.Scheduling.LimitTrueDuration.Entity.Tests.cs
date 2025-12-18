using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Extensions.Observables.Tests;

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
        Assert.IsTrue(result);
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
        Assert.IsFalse(result);
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
        Assert.IsFalse(result);
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

        Assert.IsTrue(result);

        Scheduler.AdvanceBy(1);
        Assert.IsTrue(result);

        Scheduler.AdvanceBy(1);
        Assert.IsFalse(result);
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
        Assert.IsTrue(result);
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
        Assert.IsFalse(result);
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
        Assert.IsFalse(result);
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

        Assert.IsTrue(result);

        Scheduler.AdvanceBy(1);
        Assert.IsTrue(result);

        Scheduler.AdvanceBy(1);
        Assert.IsFalse(result);
    }
}