using System.Reactive.Concurrency;
using NetDaemon.HassModel.Entities;
using Reactive.Boolean;

namespace NetDaemon.Extensions.Observables
{
    public static partial class EntityExtensions
    {
        /// <summary>
        /// Returns an observable that won't emit "false" for at least <paramref name="timeSpan"/> after an initial open (true) is emitted by <paramref name="entity"/>.
        /// If a close (false) is emitted during the <paramref name="timeSpan"/>, it will be emitted immediately after the timer is completed.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool>
            OpenForAtLeast(
                this Entity entity, TimeSpan timeSpan, IScheduler scheduler) =>
            entity.TrueForAtLeast(timeSpan, scheduler);

        /// <summary>
        /// Returns an observable that won't emit "false" for at least <paramref name="timeSpan"/> after an initial on (true) is emitted by <paramref name="entity"/>.
        /// If an off (false) is emitted during the <paramref name="timeSpan"/>, it will be emitted immediately after the timer is completed.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool>
            OnForAtLeast(
                this Entity entity, TimeSpan timeSpan, IScheduler scheduler) =>
            entity.TrueForAtLeast(timeSpan, scheduler);

        /// <summary>
        /// Returns an observable that won't emit "false" for at least <paramref name="timeSpan"/> after an initial on (true) is emitted by <paramref name="entity"/>.
        /// If an off (false) is emitted during the <paramref name="timeSpan"/>, it will be emitted immediately after the timer is completed.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool> TrueForAtLeast(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);

            // Note: we cannot use entity.EntityState.LastChanged here as we can't be sure the previous state was a valid "true" state. E.g. it could be that the state went from "unavailable" to "false".
            return entity.ToBooleanObservable().TrueForAtLeast(timeSpan, scheduler);
        }

        /// <summary>
        /// Returns an observable that won't emit "false" for at least <paramref name="timeSpan"/> after an initial "true" is emitted by the <paramref name="predicate"/> applied to the state of <paramref name="entity"/>.
        /// If the <paramref name="predicate"/> emits "false" during the <paramref name="timeSpan"/>, it will be emitted immediately after the timer is completed.
        /// Predicate will be evaluated on all state changes, including attribute changes.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool> TrueForAtLeast(this Entity entity,
            TimeSpan timeSpan, Func<EntityState, bool> predicate, IScheduler scheduler)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(scheduler);

            return entity.ToBooleanObservable(predicate).TrueForAtLeast(timeSpan, scheduler);
        }

        /// <summary>
        /// Returns an observable that won't emit "false" for at least <paramref name="timeSpan"/> after an initial "true" is emitted by the <paramref name="predicate"/> applied to the state of <paramref name="entity"/>.
        /// If the <paramref name="predicate"/> emits "false" during the <paramref name="timeSpan"/>, it will be emitted immediately after the timer is completed.
        /// Predicate will be evaluated on all state changes, including attribute changes.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool>
            TrueForAtLeast<TEntity, TEntityState, TAttributes>(
                this Entity<TEntity, TEntityState, TAttributes> entity, TimeSpan timeSpan, Func<TEntityState, bool> predicate, IScheduler scheduler)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);
            ArgumentNullException.ThrowIfNull(predicate);

            return entity.ToBooleanObservable(predicate).TrueForAtLeast(timeSpan, scheduler);
        }
    }
}