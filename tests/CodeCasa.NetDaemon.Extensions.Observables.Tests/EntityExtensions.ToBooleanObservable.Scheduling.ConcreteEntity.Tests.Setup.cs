using System.Reactive.Subjects;
using Microsoft.Reactive.Testing;
using Moq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Extensions.Observables.Tests;

[TestClass]
public abstract class EntityExtensionsToBooleanObservableSchedulingConcreteEntityTestsSetup
{
    protected const string On = nameof(On);
    protected const string Off = nameof(Off);

    const string InitialState = On;
    const string TestEntityId = "domain.testEntity";

    protected DateTime LastChanged;
    protected ConcreteTestEntity TestEntity = null!;
    protected TestScheduler Scheduler = null!;

    private Mock<IHaContext> _haContextMock = null!;
    private Subject<StateChange> _subject = null!;

    [TestInitialize]
    public void Initialize()
    {
        Scheduler = new TestScheduler();
        _haContextMock = new Mock<IHaContext>();

        LastChanged = DateTime.Now;
        var initialEntityState = new EntityState { State = InitialState, LastChanged = LastChanged };
        _subject = new Subject<StateChange>();

        _haContextMock.Setup(t => t.StateAllChanges()).Returns(_subject);
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(initialEntityState);

        TestEntity = new ConcreteTestEntity(_haContextMock.Object, TestEntityId);
    }

    protected void ChangeEntityState(string newState, DateTime changeDateTime)
    {
        var old = TestEntity.EntityState;
        _haContextMock.Setup(t => t.GetState(TestEntityId)).Returns(new EntityState { State = newState, LastChanged = changeDateTime });
        _subject.OnNext(new StateChange(TestEntity, old, TestEntity.EntityState));
    }

    protected record ConcreteTestEntity : Entity<ConcreteTestEntity, EntityState<TestEntityAttributes>, TestEntityAttributes>
    {
        public ConcreteTestEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
        {
        }
    }

    protected record TestEntityAttributes;
}