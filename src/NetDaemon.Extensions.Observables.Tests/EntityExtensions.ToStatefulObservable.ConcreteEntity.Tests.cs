using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using System.Reactive.Subjects;

namespace NetDaemon.Extensions.Observables.Tests
{
    [TestClass]
    public class EntityExtensionsToStatefulObservableConcreteEntityTests
    {
        const string InitialState = "Initial";
        const string TestEntityId = "domain.testEntity";

        private TestEntity _testEntity = null!;
        private Mock<IHaContext> _haContextMock = null!;
        private Subject<StateChange> _subject = null!;

        [TestInitialize]
        public void Initialize()
        {
            _haContextMock = new Mock<IHaContext>();

            var initialEntityState = new EntityState { State = InitialState };
            _subject = new Subject<StateChange>();

            _haContextMock.Setup(t => t.StateAllChanges()).Returns(_subject);
            _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(initialEntityState);

            _testEntity = new TestEntity(_haContextMock.Object, TestEntityId);
        }

        private void ChangeEntityState(string newState)
        {
            var old = _testEntity.EntityState;
            _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = newState });
            _subject.OnNext(new StateChange(_testEntity, old, _testEntity.EntityState));
        }

        [TestMethod]
        public void SubscribeStateful_InitiallyReturnsState()
        {
            // Arrange
            var results = new List<StateChange<TestEntity, EntityState<TestEntityAttributes>>>();

            // Act
            _testEntity.Stateful().Subscribe(results.Add);

            // Assert
            AssertStates([InitialState], results);
        }

        [TestMethod]
        public void SubscribeStateful_InitiallyReturnsState_NotInitial()
        {
            // Arrange
            const string newState = "newState";
            var results = new List<StateChange<TestEntity, EntityState<TestEntityAttributes>>>();
            ChangeEntityState(newState);

            // Act
            _testEntity.Stateful().Subscribe(results.Add);

            // Assert
            AssertStates([newState], results);
        }

        [TestMethod]
        public void SubscribeStateful_StateIsUpdated()
        {
            // Arrange
            const string newState = "newState";
            var results = new List<StateChange<TestEntity, EntityState<TestEntityAttributes>>>();
            _testEntity.Stateful().Subscribe(results.Add);

            // Act
            ChangeEntityState(newState);

            // Assert
            AssertStates([InitialState, newState], results);
        }

        [TestMethod]
        public void SubscribeStateful_StateIsUpdatedBeforeSubscribe()
        {
            // Arrange
            const string newState = "newState";
            var results = new List<StateChange<TestEntity, EntityState<TestEntityAttributes>>>();
            var statefulObservable = _testEntity.Stateful();

            // Act
            ChangeEntityState(newState);
            statefulObservable.Subscribe(results.Add);

            // Assert
            AssertStates([newState], results);
        }

        [TestMethod]
        public void SubscribeStateful_MultipleSubscriptions()
        {
            // Arrange
            const string newState = "newState";
            var subscription1Results = new List<StateChange<TestEntity, EntityState<TestEntityAttributes>>>();
            var subscription2Results = new List<StateChange<TestEntity, EntityState<TestEntityAttributes>>>();
            var statefulObservable = _testEntity.Stateful();

            // Act
            statefulObservable.Subscribe(subscription1Results.Add);
            ChangeEntityState(newState);
            statefulObservable.Subscribe(subscription2Results.Add);

            // Assert
            AssertStates([InitialState, newState], subscription1Results);
            AssertStates([newState], subscription2Results);
        }

        private void AssertStates(
            string[] expectedStates,
            List<StateChange<TestEntity, EntityState<TestEntityAttributes>>> stateChanges)
        {
            Assert.AreEqual(expectedStates.Length, stateChanges.Count);

            for (int i = 0; i < stateChanges.Count; i++)
            {
                var stateChange = stateChanges[i];

                if (i == 0)
                {
                    Assert.IsNull(stateChange.Old);
                }
                else
                {
                    Assert.AreEqual(stateChanges[i - 1].New?.State, stateChange.Old?.State);
                }
                Assert.AreEqual(expectedStates[i], stateChange.New?.State);
            }
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