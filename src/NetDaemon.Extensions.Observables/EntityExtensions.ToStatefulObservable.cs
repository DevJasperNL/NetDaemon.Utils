using NetDaemon.HassModel.Entities;

namespace NetDaemon.Extensions.Observables
{
    public static partial class EntityExtensions
    {
        /// <summary>
        /// Returns an observable that emits the entity's current state upon subscribing and will emit state changes where State.New != State.Old
        /// </summary>
        /// <remarks>
        /// As of NetDaemon version 25.5.0, the method <c>StateChangesWithCurrent</c> provides the same functionality. 
        /// </remarks>
        public static IObservable<StateChange> Stateful(this Entity entity)
            => entity.StateChangesWithCurrent();

        /// <summary>
        /// Returns an observable that emits the entity's current state upon subscribing and will emit state changes where State.New != State.Old
        /// </summary>
        /// <remarks>
        /// As of NetDaemon version 25.5.0, the method <c>StateChangesWithCurrent</c> provides the same functionality. 
        /// </remarks>
        public static IObservable<StateChange<TEntity, TEntityState>>
            Stateful<TEntity, TEntityState, TAttributes>(
                this Entity<TEntity, TEntityState, TAttributes> entity)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class
            => entity.StateChangesWithCurrent();

        /// <summary>
        /// Returns an observable that emits the entity's current state upon subscribing and will emit all state changes, including attribute changes.
        /// </summary>
        /// <remarks>
        /// As of NetDaemon version 25.5.0, the method <c>StateAllChangesWithCurrent</c> provides the same functionality. 
        /// </remarks>
        public static IObservable<StateChange> StatefulAll(this Entity entity)
            => entity.StateAllChangesWithCurrent();

        /// <summary>
        /// Returns an observable that emits the entity's current state upon subscribing and will emit all state changes, including attribute changes.
        /// </summary>
        /// <remarks>
        /// As of NetDaemon version 25.5.0, the method <c>StateAllChangesWithCurrent</c> provides the same functionality. 
        /// </remarks>
        public static IObservable<StateChange<TEntity, TEntityState>>
            StatefulAll<TEntity, TEntityState, TAttributes>(
                this Entity<TEntity, TEntityState, TAttributes> entity)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class
            => entity.StateAllChangesWithCurrent();
    }
}