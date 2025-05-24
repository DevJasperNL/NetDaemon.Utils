using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using System.Reactive.Subjects;

namespace NetDaemon.Extensions.Observables.Tests;

[TestClass]
public class EntityExtensionsToBooleanObservableConcreteEntityTests
{
    private const string On = nameof(On);
    private const string Off = nameof(Off);

    const string InitialState = On;
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

    private void EntityStateNull()
    {
        var old = _testEntity.EntityState;
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(() => null);
        _subject.OnNext(new StateChange(_testEntity, old, null));
    }

    [TestMethod]
    public void ToBooleanObservable_Predicate_InitiallyReturnsState()
    {
        // Arrange
        var results = new List<bool>();

        // Act
        _testEntity.ToBooleanObservable(s => s.IsOff()).Subscribe(results.Add);

        // Assert
        CollectionAssert.AreEqual(new[] { false }, results);
    }

    [TestMethod]
    public void ToBooleanObservable_Predicate_StateChange()
    {
        // Arrange
        var results = new List<bool>();
        _testEntity.ToBooleanObservable(s => s.IsOff()).Subscribe(results.Add);

        // Act
        ChangeEntityState(Off);

        // Assert
        CollectionAssert.AreEqual(new[] { false, true }, results);
    }

    [TestMethod]
    public void ToBooleanObservable_Predicate_StateIsUpdatedBeforeSubscribe()
    {
        // Arrange
        var results = new List<bool>();
        var statefulObservable = _testEntity.ToBooleanObservable(s => s.IsOff());

        // Act
        ChangeEntityState(Off);
        statefulObservable.Subscribe(results.Add);

        // Assert
        CollectionAssert.AreEqual(new[] { true }, results);
    }

    [TestMethod]
    public void ToBooleanObservable_Predicate_Distinct()
    {
        // Arrange
        var results = new List<bool>();
        _testEntity.ToBooleanObservable(s => s.IsOff()).Subscribe(results.Add);

        // Act
        ChangeEntityState(On);

        // Assert
        CollectionAssert.AreEqual(new[] { false }, results);
    }
        
    [TestMethod]
    public void ToBooleanObservable_Predicate_NullIgnored()
    {
        // Arrange
        var results = new List<bool>();
        _testEntity.ToBooleanObservable(s => s.IsOff()).Subscribe(results.Add);

        // Act
        EntityStateNull();

        // Assert
        CollectionAssert.AreEqual(new[] { false }, results);
    }

    [TestMethod]
    public void ToChangesOnlyBooleanObservable_Predicate_InitiallyDoesNotReturnState()
    {
        // Arrange
        var results = new List<bool>();

        // Act
        _testEntity.ToChangesOnlyBooleanObservable(s => s.IsOff()).Subscribe(results.Add);

        // Assert
        Assert.AreEqual(0, results.Count);
    }

    [TestMethod]
    public void ToChangesOnlyBooleanObservable_Predicate_StateChange()
    {
        // Arrange
        var results = new List<bool>();
        _testEntity.ToChangesOnlyBooleanObservable(s => s.IsOff()).Subscribe(results.Add);

        // Act
        ChangeEntityState(Off);

        // Assert
        CollectionAssert.AreEqual(new[] { true }, results);
    }

    [TestMethod]
    public void ToChangesOnlyBooleanObservable_Predicate_Distinct()
    {
        // Arrange
        var results = new List<bool>();
        _testEntity.ToChangesOnlyBooleanObservable(s => s.IsOff()).Subscribe(results.Add);

        // Act
        ChangeEntityState(On);
        ChangeEntityState(Off);
        ChangeEntityState(Off);

        // Assert
        CollectionAssert.AreEqual(new[] { false, true }, results);
    }

    [TestMethod]
    public void ToChangesOnlyBooleanObservable_Predicate_NullIgnored()
    {
        // Arrange
        var results = new List<bool>();
        _testEntity.ToChangesOnlyBooleanObservable(s => s.IsOff()).Subscribe(results.Add);

        // Act
        EntityStateNull();

        // Assert
        Assert.AreEqual(0, results.Count);
    }

    public record TestEntity : Entity<TestEntity, EntityState<TestEntityAttributes>, TestEntityAttributes>
    {
        public TestEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
        {
        }
    }

    public record TestEntityAttributes;
}