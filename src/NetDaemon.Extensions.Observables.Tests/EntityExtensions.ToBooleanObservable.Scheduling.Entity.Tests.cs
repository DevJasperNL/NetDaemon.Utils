using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;

namespace NetDaemon.Extensions.Observables.Tests
{
    [TestClass]
    public class EntityExtensionsToBooleanObservableSchedulingEntityTests
    {
        private const string On = nameof(On);
        private const string Off = nameof(Off);

        const string InitialState = On;
        const string TestEntityId = "domain.testEntity";

        private DateTime _lastChanged;
        private Entity _testEntity = null!;
        private Mock<IHaContext> _haContextMock = null!;
        private Subject<StateChange> _subject = null!;
        private IScheduler _scheduler = null!;

        [TestInitialize]
        public void Initialize()
        {
            _scheduler = new Mock<IScheduler>().Object;
            _haContextMock = new Mock<IHaContext>();

            _lastChanged = DateTime.Now;
            var initialEntityState = new EntityState { State = InitialState, LastChanged = _lastChanged };
            _subject = new Subject<StateChange>();

            _haContextMock.Setup(t => t.StateAllChanges()).Returns(_subject);
            _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(initialEntityState);

            _testEntity = new Entity(_haContextMock.Object, TestEntityId);
        }

        private void ChangeEntityState(string newState, DateTime changeDateTime)
        {
            var old = _testEntity.EntityState;
            _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = newState, LastChanged = changeDateTime });
            _subject.OnNext(new StateChange(_testEntity, old, _testEntity.EntityState));
        }

        [TestMethod]
        public void WhenTrueFor_LastChangedShorterAgoThanTimeSpan_False()
        {
            // Arrange
            var observable = _testEntity.WhenTrueFor(TimeSpan.FromMinutes(1), _scheduler);

            // Act
            bool? result = null;
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void WhenTrueFor_LastChangedLongerAgoThanTimeSpan_True()
        {
            // Arrange
            ChangeEntityState(On, _lastChanged - TimeSpan.FromMinutes(2));
            var observable = _testEntity.WhenTrueFor(TimeSpan.FromMinutes(1), _scheduler);
            
            // Act
            bool? result = null;
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenTrueFor_SubscribeAfterTimeSpanPasses_True()
        {
            // Arrange
            var observable = _testEntity.WhenTrueFor(TimeSpan.FromMinutes(1), _scheduler);
            
            // Act
            bool? result = null;
            ChangeEntityState(On, _lastChanged - TimeSpan.FromMinutes(2));
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void WhenTrueFor_Predicate_LastChangedShorterAgoThanTimeSpan_False()
        {
            // Arrange
            ChangeEntityState(Off, _lastChanged);
            var observable = _testEntity.WhenTrueFor(TimeSpan.FromMinutes(1), s => s.IsOff(), _scheduler);

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
            ChangeEntityState(Off, _lastChanged - TimeSpan.FromMinutes(2));
            var observable = _testEntity.WhenTrueFor(TimeSpan.FromMinutes(1), s => s.IsOff(), _scheduler);

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
            var observable = _testEntity.WhenTrueFor(TimeSpan.FromMinutes(1), s => s.IsOff(), _scheduler);

            // Act
            bool? result = null;
            ChangeEntityState(Off, _lastChanged - TimeSpan.FromMinutes(2));
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void LimitTrueDuration_LastChangedShorterAgoThanTimeSpan_True()
        {
            // Arrange
            var observable = _testEntity.LimitTrueDuration(TimeSpan.FromMinutes(1), _scheduler);

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
            ChangeEntityState(On, _lastChanged - TimeSpan.FromMinutes(2));
            var observable = _testEntity.LimitTrueDuration(TimeSpan.FromMinutes(1), _scheduler);

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
            var observable = _testEntity.LimitTrueDuration(TimeSpan.FromMinutes(1), _scheduler);

            // Act
            bool? result = null;
            ChangeEntityState(On, _lastChanged - TimeSpan.FromMinutes(2));
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void LimitTrueDuration_Predicate_LastChangedShorterAgoThanTimeSpan_True()
        {
            // Arrange
            ChangeEntityState(Off, _lastChanged);
            var observable = _testEntity.LimitTrueDuration(TimeSpan.FromMinutes(1), s => s.IsOff(), _scheduler);

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
            ChangeEntityState(Off, _lastChanged - TimeSpan.FromMinutes(2));
            var observable = _testEntity.LimitTrueDuration(TimeSpan.FromMinutes(1), s => s.IsOff(), _scheduler);

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
            var observable = _testEntity.LimitTrueDuration(TimeSpan.FromMinutes(1), s => s.IsOff(), _scheduler);

            // Act
            bool? result = null;
            ChangeEntityState(Off, _lastChanged - TimeSpan.FromMinutes(2));
            observable.Subscribe(b => result = b);

            // Assert
            Assert.AreEqual(false, result);
        }
    }
}