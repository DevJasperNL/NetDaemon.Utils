using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;

namespace NetDaemon.Extensions.Observables.Tests
{
    [TestClass]
    public class EntityExtensionsToBooleanObservableSchedulingConcreteEntityTests
    {
        private const string On = nameof(On);
        private const string Off = nameof(Off);

        const string InitialState = On;
        const string TestEntityId = "domain.testEntity";

        private DateTime _lastChanged;
        private TestEntity _testEntity = null!;
        private Mock<IHaContext> _haContextMock = null!;
        private Subject<StateChange> _subject = null!;
        private TestScheduler _scheduler = null!;

        [TestInitialize]
        public void Initialize()
        {
            _scheduler = new TestScheduler();
            _haContextMock = new Mock<IHaContext>();

            _lastChanged = DateTime.Now;
            var initialEntityState = new EntityState { State = InitialState, LastChanged = _lastChanged };
            _subject = new Subject<StateChange>();

            _haContextMock.Setup(t => t.StateAllChanges()).Returns(_subject);
            _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(initialEntityState);

            _testEntity = new TestEntity(_haContextMock.Object, TestEntityId);
        }

        private void ChangeEntityState(string newState, DateTime changeDateTime)
        {
            var old = _testEntity.EntityState;
            _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = newState, LastChanged = changeDateTime });
            _subject.OnNext(new StateChange(_testEntity, old, _testEntity.EntityState));
        }

        [TestMethod]
        public void WhenTrueFor_Predicate_LastChangedShorterAgoThanTimeSpan_False()
        {
            // Arrange
            ChangeEntityState(Off, _lastChanged);
            var observable = _testEntity.WhenTrueFor(
                TimeSpan.FromTicks(1),
                s => s.IsOff(),
                _scheduler,
                _lastChanged.ToUniversalTime);

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
            ChangeEntityState(Off, _lastChanged);
            var observable = _testEntity.WhenTrueFor(
                TimeSpan.FromTicks(1),
                s => s.IsOff(),
                _scheduler,
                () => _lastChanged.ToUniversalTime() + TimeSpan.FromTicks(1));

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
            var observable = _testEntity.WhenTrueFor(
                TimeSpan.FromTicks(1),
                s => s.IsOff(),
                _scheduler,
                _lastChanged.ToUniversalTime);

            // Act
            bool? result = null;
            ChangeEntityState(Off, _lastChanged - TimeSpan.FromTicks(1));
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenTrueFor_Predicate_LastChangedHalfwayOfTimeSpan_TrueAfterRemainingTime()
        {
            ChangeEntityState(Off, _lastChanged);
            var observable = _testEntity.WhenTrueFor(
                TimeSpan.FromTicks(4),
                s => s.IsOff(),
                _scheduler,
                () => _lastChanged.ToUniversalTime() + TimeSpan.FromTicks(2));

            bool? result = null;
            observable.Subscribe(b => result = b);

            Assert.AreEqual(false, result);

            _scheduler.AdvanceBy(1);
            Assert.AreEqual(false, result);

            _scheduler.AdvanceBy(1);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void LimitTrueDuration_Predicate_LastChangedShorterAgoThanTimeSpan_True()
        {
            // Arrange
            ChangeEntityState(Off, _lastChanged);
            var observable = _testEntity.LimitTrueDuration(
                TimeSpan.FromMinutes(1),
                s => s.IsOff(),
                _scheduler,
                _lastChanged.ToUniversalTime);

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
            ChangeEntityState(Off, _lastChanged);
            var observable = _testEntity.LimitTrueDuration(
                TimeSpan.FromTicks(1),
                s => s.IsOff(),
                _scheduler,
                () => _lastChanged.ToUniversalTime() + TimeSpan.FromTicks(1));

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
            var observable = _testEntity.LimitTrueDuration(
                TimeSpan.FromTicks(1),
                s => s.IsOff(),
                _scheduler,
                _lastChanged.ToUniversalTime);

            // Act
            bool? result = null;
            ChangeEntityState(Off, _lastChanged - TimeSpan.FromTicks(1));
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void LimitTrueDuration_Predicate_LastChangedHalfwayOfTimeSpan_FalseAfterRemainingTime()
        {
            ChangeEntityState(Off, _lastChanged);
            var observable = _testEntity.LimitTrueDuration(
                TimeSpan.FromTicks(4),
                s => s.IsOff(),
                _scheduler,
                () => _lastChanged.ToUniversalTime() + TimeSpan.FromTicks(2));

            bool? result = null;
            observable.Subscribe(b => result = b);

            Assert.AreEqual(true, result);

            _scheduler.AdvanceBy(1);
            Assert.AreEqual(true, result);

            _scheduler.AdvanceBy(1);
            Assert.AreEqual(false, result);
        }

        public record TestEntity : Entity<TestEntity, EntityState<TestEntityAttributes>, TestEntityAttributes>
        {
            public TestEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
            {
            }
        }

        public record TestEntityAttributes;
    }
}