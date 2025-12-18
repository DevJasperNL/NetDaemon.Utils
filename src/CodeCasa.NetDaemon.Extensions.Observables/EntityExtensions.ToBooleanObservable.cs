using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Extensions.Observables;

public static partial class EntityExtensions
{
    /// <summary>
    /// <para>
    /// Returns a boolean observable that emits true when the entity opens and emits false when the entity closes.
    /// The observable is distinct until changed. Other state changes are ignored.
    /// </para>
    /// <para>
    /// State changes other than open or closed (on or off on the entity) are ignored. This also includes a null entity state. This is typically received when an entity is removed. This will not be emitted in the observable.
    /// </para>
    /// </summary>
    public static IObservable<bool> ToOpenClosedObservable(this Entity entity) => entity.ToBooleanObservable();

    /// <summary>
    /// <para>
    /// Returns a boolean observable that emits true when the entity turns on and emits false when the entity turns off.
    /// The observable is distinct until changed. Other state changes are ignored.
    /// </para>
    /// <para>
    /// State changes other than on or off are ignored. This also includes a null entity state. This is typically received when an entity is removed. This will not be emitted in the observable.
    /// </para>
    /// </summary>
    public static IObservable<bool> ToOnOffObservable(this Entity entity) => entity.ToBooleanObservable();

    /// <summary>
    /// <para>
    /// Returns a boolean observable that emits true when the entity turns on and emits false when the entity turns off.
    /// The observable is distinct until changed. Other state changes are ignored.
    /// </para>
    /// <para>
    /// State changes other than on or off are ignored. This also includes a null entity state. This is typically received when an entity is removed. This will not be emitted in the observable.
    /// </para>
    /// </summary>
    public static IObservable<bool> ToBooleanObservable(this Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var statefulObservable = entity.Stateful();

        var trueObservable = 
            statefulObservable
                .Where(s => s.New?.IsOn() ?? false)
                .Select(_ => true);

        var falseObservable = 
            statefulObservable
                .Where(s => s.New?.IsOff() ?? false)
                .Select(_ => false);

        return trueObservable.Merge(falseObservable).DistinctUntilChanged();
    }

    /// <summary>
    /// Returns a boolean observable that reflects the result of the provided predicate on the new state of the provided entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable is distinct until changed. A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// </summary>
    public static IObservable<bool> ToBooleanObservable(this Entity entity, Func<EntityState, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        return entity
            .StatefulAll()
            .Where(s => s.New != null)
            .Select(s => predicate(s.New!))
            .DistinctUntilChanged();
    }

    /// <summary>
    /// Returns a boolean observable that reflects the result of the provided predicate on the new state of the provided entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable is distinct until changed. A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// </summary>
    public static IObservable<bool>
        ToBooleanObservable<TEntity, TEntityState, TAttributes>(
            this Entity<TEntity, TEntityState, TAttributes> entity,
            Func<TEntityState, bool> predicate)
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        return entity
            .StatefulAll()
            .Where(s => s.New != null)
            .Select(s => predicate(s.New!))
            .DistinctUntilChanged();
    }

    /// <summary>
    /// <para>
    /// Returns a boolean observable that emits true when the entity turns on and emits false when the entity turns off.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable only emits changes and will not emit the initial state.
    /// The observable is distinct until changed. Other state changes are ignored.
    /// </para>
    /// <para>
    /// State changes other than on or off are ignored. This also includes a null entity state. This is typically received when an entity is removed. This will not be emitted in the observable.
    /// </para>
    /// </summary>
    public static IObservable<bool> ToChangesOnlyBooleanObservable(this Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        var statefulObservable = entity.StateAllChanges();
        var trueObservable =
            statefulObservable
                .Where(s => s.New?.IsOn() ?? false)
                .Select(_ => true);

        var falseObservable =
            statefulObservable
                .Where(s => s.New?.IsOff() ?? false)
                .Select(_ => false);

        return trueObservable.Merge(falseObservable).DistinctUntilChanged();
    }

    /// <summary>
    /// Returns a boolean observable that reflects the result of the provided predicate on the new state of the provided entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable only emits changes and will not emit the initial state.
    /// The observable is distinct until changed. A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// </summary>
    public static IObservable<bool> ToChangesOnlyBooleanObservable(this Entity entity, Func<EntityState, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        return entity
            .StateAllChanges()
            .Where(s => s.New != null)
            .Select(s => predicate(s.New!))
            .DistinctUntilChanged();
    }

    /// <summary>
    /// Returns a boolean observable that reflects the result of the provided predicate on the new state of the provided entity.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// The observable only emits changes and will not emit the initial state.
    /// The observable is distinct until changed. A null entity state (typically received when an entity is removed) is filtered out so the predicate doesn't have to handle null values.
    /// </summary>
    public static IObservable<bool>
        ToChangesOnlyBooleanObservable<TEntity, TEntityState, TAttributes>(
            this Entity<TEntity, TEntityState, TAttributes> entity,
            Func<TEntityState, bool> predicate)
        where TEntity : Entity<TEntity, TEntityState, TAttributes>
        where TEntityState : EntityState<TAttributes>
        where TAttributes : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(predicate);

        return entity
            .StateAllChanges()
            .Where(s => s.New != null)
            .Select(s => predicate(s.New!))
            .DistinctUntilChanged();
    }
}