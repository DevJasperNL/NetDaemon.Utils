using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using System.Reactive.Subjects;

namespace NetDaemon.Extensions.Observables.Tests;

[TestClass]
public class ObservableExtensionsUtilsTests
{
    private const string Unavailable = nameof(Unavailable);
    const string InitialState = "Initial";
    const string TestEntityId = "domain.testEntity";

    private BehaviorSubject<bool> _observable = null!;
    private Entity _testEntity = null!;
    private Mock<IHaContext> _haContextMock = null!;
    private Subject<StateChange> _subject = null!;
        
    [TestInitialize]
    public void Initialize()
    {
        _observable = new BehaviorSubject<bool>(true);

        _haContextMock = new Mock<IHaContext>();

        var initialEntityState = new EntityState { State = InitialState };
        _subject = new Subject<StateChange>();

        _haContextMock.Setup(t => t.StateAllChanges()).Returns(_subject);
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(initialEntityState);

        _testEntity = new Entity(_haContextMock.Object, TestEntityId);
    }

    private void ChangeEntityState(string newState)
    {
        var old = _testEntity.EntityState;
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = newState });
        _subject.OnNext(new StateChange(_testEntity, old, _testEntity.EntityState));
    }

    [TestMethod]
    public void TriggerWhenEntitiesBecomeAvailable_Available_InitiallyReturnsState()
    {
        // Arrange
        var results = new List<bool>();

        // Act
        _observable.RepeatWhenEntitiesBecomeAvailable(_testEntity).Subscribe(results.Add);

        // Assert
        CollectionAssert.AreEqual(new[] { true }, results);
    }

    [TestMethod]
    public void TriggerWhenEntitiesBecomeAvailable_Unavailable_InitiallyReturnsState()
    {
        // Arrange
        var results = new List<bool>();
        ChangeEntityState(Unavailable);

        // Act
        _observable.RepeatWhenEntitiesBecomeAvailable(_testEntity).Subscribe(results.Add);

        // Assert
        CollectionAssert.AreEqual(new[] { true }, results);
    }

    [TestMethod]
    public void TriggerWhenEntitiesBecomeAvailable_Available_ObservableStateChanges()
    {
        // Arrange
        var results = new List<bool>();
        _observable.RepeatWhenEntitiesBecomeAvailable(_testEntity).Subscribe(results.Add);

        // Act
        _observable.OnNext(false);

        // Assert
        CollectionAssert.AreEqual(new[] { true, false }, results);
    }

    [TestMethod]
    public void TriggerWhenEntitiesBecomeAvailable_Unavailable_ObservableStateChanges()
    {
        // Arrange
        var results = new List<bool>();
        ChangeEntityState(Unavailable);
        _observable.RepeatWhenEntitiesBecomeAvailable(_testEntity).Subscribe(results.Add);

        // Act
        _observable.OnNext(false);

        // Assert
        CollectionAssert.AreEqual(new[] { true, false }, results);
    }

    [TestMethod]
    public void TriggerWhenEntitiesBecomeAvailable_TrueStateRepeatedOnAvailable()
    {
        // Arrange
        var results = new List<bool>();
        ChangeEntityState(Unavailable);
        _observable.RepeatWhenEntitiesBecomeAvailable(_testEntity).Subscribe(results.Add);

        // Act
        ChangeEntityState($"Not {Unavailable}");

        // Assert
        CollectionAssert.AreEqual(new[] { true, true }, results);
    }

    [TestMethod]
    public void TriggerWhenEntitiesBecomeAvailable_FalseStateRepeatedOnAvailable()
    {
        // Arrange
        var results = new List<bool>();
        ChangeEntityState(Unavailable);
        _observable.OnNext(false);
        _observable.RepeatWhenEntitiesBecomeAvailable(_testEntity).Subscribe(results.Add);

        // Act
        ChangeEntityState($"Not {Unavailable}");

        // Assert
        CollectionAssert.AreEqual(new[] { false, false }, results);
    }

    [TestMethod]
    public void TriggerWhenEntitiesBecomeAvailable_OtherStatesDoNotCauseRepeat()
    {
        // Arrange
        var results = new List<bool>();
        _observable.RepeatWhenEntitiesBecomeAvailable(_testEntity).Subscribe(results.Add);

        // Act
        ChangeEntityState("Some other state");

        // Assert
        CollectionAssert.AreEqual(new[] { true }, results);
    }
}