using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;
using Reactive.Boolean;

namespace NetDaemon.Extensions.Observables
{
    public static partial class EntityExtensions
    {
        /// <summary>
        /// Returns an observable that stays true for a timeSpan once the entity closes (off).
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time true is held.</param>
        /// <param name="scheduler"></param>
        public static IObservable<bool>
            PersistOpenFor(
                this Entity entity, TimeSpan timeSpan, IScheduler scheduler) =>
            entity.PersistTrueFor(timeSpan, scheduler);

        /// <summary>
        /// Returns an observable that stays true for a timeSpan once the entity turns off.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time true is held.</param>
        /// <param name="scheduler"></param>
        public static IObservable<bool>
            PersistOnFor(
                this Entity entity, TimeSpan timeSpan, IScheduler scheduler) =>
            entity.PersistTrueFor(timeSpan, scheduler);

        /// <summary>
        /// Returns an observable that stays true for a timeSpan once the entity turns off.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time true is held.</param>
        /// <param name="scheduler"></param>
        public static IObservable<bool> PersistTrueFor(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);

            // Note: we cannot use entity.EntityState.LastChanged here as we can't be sure the previous state was a valid "true" state. E.g. it could be that the state went from "unavailable" to "false".
            return entity.ToBooleanObservable().PersistTrueFor(timeSpan, scheduler);
        }

        /// <summary>
        /// Returns an observable that stays true for a minimum timeSpan once the predicate applied on the entity state returns true.
        /// Predicate will be evaluated on all state changes, including attribute changes.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time true is held.</param>
        /// <param name="predicate"></param>
        /// <param name="scheduler"></param>
        public static IObservable<bool> PersistTrueFor(this Entity entity,
            TimeSpan timeSpan, Func<EntityState, bool> predicate, IScheduler scheduler)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(scheduler);

            return entity.ToBooleanObservable(predicate).PersistTrueFor(timeSpan, scheduler);
        }

        /// <summary>
        /// Returns an observable that stays true for a minimum timeSpan once the predicate applied on the entity state returns true.
        /// Predicate will be evaluated on all state changes, including attribute changes.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time true is held.</param>
        /// <param name="predicate"></param>
        /// <param name="scheduler"></param>
        public static IObservable<bool>
            PersistTrueFor<TEntity, TEntityState, TAttributes>(
                this Entity<TEntity, TEntityState, TAttributes> entity, TimeSpan timeSpan, Func<TEntityState, bool> predicate, IScheduler scheduler)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);
            ArgumentNullException.ThrowIfNull(predicate);

            return entity.ToBooleanObservable(predicate).PersistTrueFor(timeSpan, scheduler);
        }
    }
}