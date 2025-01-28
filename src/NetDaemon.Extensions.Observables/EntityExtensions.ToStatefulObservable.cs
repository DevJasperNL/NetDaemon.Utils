using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;

namespace NetDaemon.Extensions.Observables
{
    public static partial class EntityExtensions
    {
        /// <summary>
        /// Returns an observable that emits the entity's current state upon subscribing and will emit state changes where State.New != State.Old
        /// </summary>
        public static IObservable<StateChange> Stateful(this Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return Observable.Defer(() =>
            Observable.Return(new StateChange(
            entity,
            null, // Initially, there is only a new state, so old is null.
                        entity.EntityState))
                    .Concat(entity.StateChanges()));
        }

        /// <summary>
        /// Returns an observable that emits the entity's current state upon subscribing and will emit state changes where State.New != State.Old
        /// </summary>
        public static IObservable<StateChange<TEntity, TEntityState>>
            Stateful<TEntity, TEntityState, TAttributes>(
                this Entity<TEntity, TEntityState, TAttributes> entity)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class
        {
            ArgumentNullException.ThrowIfNull(entity);

            return Observable.Defer(() =>
                    Observable.Return(new StateChange<TEntity, TEntityState>(
                            (TEntity)entity,
                            null, // Initially, there is only a new state, so old is null.
                            entity.EntityState))
                        .Concat(entity.StateChanges()));
        }

        /// <summary>
        /// Returns an observable that emits the entity's current state upon subscribing and will emit all state changes, including attribute changes.
        /// </summary>
        public static IObservable<StateChange> StatefulAll(this Entity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            return Observable.Defer(() =>
                Observable.Return(new StateChange(
                        entity,
                        null, // Initially, there is only a new state, so old is null.
                        entity.EntityState))
                    .Concat(entity.StateAllChanges()));
        }

        /// <summary>
        /// Returns an observable that emits the entity's current state upon subscribing and will emit all state changes, including attribute changes.
        /// </summary>
        public static IObservable<StateChange<TEntity, TEntityState>>
            StatefulAll<TEntity, TEntityState, TAttributes>(
                this Entity<TEntity, TEntityState, TAttributes> entity)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class
        {
            ArgumentNullException.ThrowIfNull(entity);

            return Observable.Defer(() =>
                Observable.Return(new StateChange<TEntity, TEntityState>(
                        (TEntity)entity,
                        null, // Initially, there is only a new state, so old is null.
                        entity.EntityState))
                    .Concat(entity.StateAllChanges()));
        }
    }
}