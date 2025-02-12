using NetDaemon.HassModel.Entities;

namespace NetDaemon.Extensions.Observables.Tests
{
    [TestClass]
    public class EntityExtensionsToBooleanObservableSchedulingWhenTrueForConcreteEntityTests
        : EntityExtensionsToBooleanObservableSchedulingConcreteEntityTestsSetup
    {
        [TestMethod]
        public void WhenTrueFor_Predicate_LastChangedShorterAgoThanTimeSpan_False()
        {
            // Arrange
            ChangeEntityState(Off, LastChanged);
            var observable = TestEntity.WhenTrueFor(
                TimeSpan.FromTicks(1),
                s => s.IsOff(),
                Scheduler,
                LastChanged.ToUniversalTime);

            // Act
            bool? result = null;
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void WhenTrueFor_Predicate_LastChangedLongerAgoThanTimeSpan_True()
        {
            // Arrange
            ChangeEntityState(Off, LastChanged);
            var observable = TestEntity.WhenTrueFor(
                TimeSpan.FromTicks(1),
                s => s.IsOff(),
                Scheduler,
                () => LastChanged.ToUniversalTime() + TimeSpan.FromTicks(1));

            // Act
            bool? result = null;
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenTrueFor_Predicate_SubscribeAfterTimeSpanPasses_True()
        {
            // Arrange
            var observable = TestEntity.WhenTrueFor(
                TimeSpan.FromTicks(1),
                s => s.IsOff(),
                Scheduler,
                LastChanged.ToUniversalTime);

            // Act
            bool? result = null;
            ChangeEntityState(Off, LastChanged - TimeSpan.FromTicks(1));
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenTrueFor_Predicate_LastChangedHalfwayOfTimeSpan_TrueAfterRemainingTime()
        {
            ChangeEntityState(Off, LastChanged);
            var observable = TestEntity.WhenTrueFor(
                TimeSpan.FromTicks(4),
                s => s.IsOff(),
                Scheduler,
                () => LastChanged.ToUniversalTime() + TimeSpan.FromTicks(2));

            bool? result = null;
            observable.Subscribe(b => result = b);

            Assert.AreEqual(false, result);

            Scheduler.AdvanceBy(1);
            Assert.AreEqual(false, result);

            Scheduler.AdvanceBy(1);
            Assert.AreEqual(true, result);
        }
    }
}