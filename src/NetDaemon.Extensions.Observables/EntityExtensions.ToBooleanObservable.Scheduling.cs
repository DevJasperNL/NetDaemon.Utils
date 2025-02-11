using System.Reactive.Concurrency;
using System.Reactive.Linq;
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

        /// <summary>
        /// Returns an observable that emits true once the entity is open (on) for a minimum timeSpan.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit true even if the time did not pass in the application.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time the entity needs to be on before true is emitted in the resulting observable.</param>
        /// <param name="scheduler"></param>
        public static IObservable<bool> WhenOpenFor(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler) => entity.WhenTrueFor(timeSpan, scheduler);

        /// <summary>
        /// Returns an observable that emits true once the entity is on for a minimum timeSpan.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit true even if the time did not pass in the application.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time the entity needs to be on before true is emitted in the resulting observable.</param>
        /// <param name="scheduler"></param>
        public static IObservable<bool> WhenOnFor(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler) => entity.WhenTrueFor(timeSpan, scheduler);

        /// <summary>
        /// Returns an observable that emits true once the entity is on for a minimum timeSpan.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit true even if the time did not pass in the application.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time the entity needs to be on before true is emitted in the resulting observable.</param>
        /// <param name="scheduler"></param>
        public static IObservable<bool> WhenTrueFor(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler) => entity.WhenTrueFor(timeSpan, scheduler, () => DateTime.UtcNow);

        internal static IObservable<bool> WhenTrueFor(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler, Func<DateTime> utcNowProvider)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);

            return Observable.Defer(() =>
            {
                var utcNow = utcNowProvider();
                var lastChangedUtc = entity.EntityState?.LastChanged?.ToUniversalTime();
                if (!entity.IsOn() || lastChangedUtc == null)
                {
                    return entity.ToBooleanObservable().WhenTrueFor(timeSpan, scheduler);
                }

                var moment = utcNow - timeSpan;
                var remainingTimeSpan = lastChangedUtc.Value - moment;
                return remainingTimeSpan.Ticks <= 0 ? 
                    entity.ToBooleanObservable().WhenTrueFor(timeSpan, scheduler).Skip(1).Prepend(true) : 
                    entity.ToBooleanObservable().WhenTrueFor(remainingTimeSpan, scheduler);
            });
        }

        /// <summary>
        /// Returns an observable that emits true once the predicate applied on the entity state returns true for a minimum timeSpan.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit true even if the time did not pass in the application.
        /// Predicate will be evaluated on all state changes, including attribute changes.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time the entity needs to be on before true is emitted in the resulting observable.</param>
        /// <param name="predicate"></param>
        /// <param name="scheduler"></param>
        public static IObservable<bool> WhenTrueFor(this Entity entity,
            TimeSpan timeSpan, Func<EntityState, bool> predicate, IScheduler scheduler) =>
            entity.WhenTrueFor(timeSpan, predicate, scheduler, () => DateTime.UtcNow);

        internal static IObservable<bool> WhenTrueFor(this Entity entity,
            TimeSpan timeSpan, Func<EntityState, bool> predicate, IScheduler scheduler, Func<DateTime> utcNowProvider)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);

            return Observable.Defer(() =>
            {
                var utcNow = utcNowProvider();
                var lastChangedUtc = entity.EntityState?.LastChanged?.ToUniversalTime();
                if (entity.EntityState == null || !predicate(entity.EntityState) || lastChangedUtc == null)
                {
                    return entity.ToBooleanObservable(predicate).WhenTrueFor(timeSpan, scheduler);
                }

                var moment = utcNow - timeSpan;
                var remainingTimeSpan = lastChangedUtc.Value - moment;
                return remainingTimeSpan.Ticks <= 0 ?
                    entity.ToBooleanObservable(predicate).WhenTrueFor(timeSpan, scheduler).Skip(1).Prepend(true) :
                    entity.ToBooleanObservable(predicate).WhenTrueFor(remainingTimeSpan, scheduler);
            });
        }

        /// <summary>
        /// Returns an observable that emits true once the predicate applied on the entity state returns true for a minimum timeSpan.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit true even if the time did not pass in the application.
        /// Predicate will be evaluated on all state changes, including attribute changes.
        /// Resulting observable is distinct.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="timeSpan">The minimum time the entity needs to be on before true is emitted in the resulting observable.</param>
        /// <param name="predicate"></param>
        /// <param name="scheduler"></param>
        public static IObservable<bool>
            WhenTrueFor<TEntity, TEntityState, TAttributes>(
                this Entity<TEntity, TEntityState, TAttributes> entity,
                TimeSpan timeSpan,
                Func<TEntityState, bool> predicate,
                IScheduler scheduler)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class => entity.WhenTrueFor(timeSpan, predicate, scheduler, () => DateTime.UtcNow);

        internal static IObservable<bool>
            WhenTrueFor<TEntity, TEntityState, TAttributes>(
                this Entity<TEntity, TEntityState, TAttributes> entity,
                TimeSpan timeSpan,
                Func<TEntityState, bool> predicate,
                IScheduler scheduler,
                Func<DateTime> utcNowProvider)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);
            ArgumentNullException.ThrowIfNull(predicate);

            return Observable.Defer(() =>
            {
                var utcNow = utcNowProvider();
                var lastChangedUtc = entity.EntityState?.LastChanged?.ToUniversalTime();
                if (entity.EntityState == null || !predicate(entity.EntityState) || lastChangedUtc == null)
                {
                    return entity.ToBooleanObservable(predicate).WhenTrueFor(timeSpan, scheduler);
                }

                var moment = utcNow - timeSpan;
                var remainingTimeSpan = lastChangedUtc.Value - moment;
                return remainingTimeSpan.Ticks <= 0 ?
                    entity.ToBooleanObservable(predicate).WhenTrueFor(timeSpan, scheduler).Skip(1).Prepend(true) :
                    entity.ToBooleanObservable(predicate).WhenTrueFor(remainingTimeSpan, scheduler);
            });
        }

        /// <summary>
        /// Returns an observable that stays true for a maximum of <paramref name="timeSpan"/>. If the <paramref name="entity"/> closes before the time has passed, the resulting observable also emits false.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit false even if the time did not pass in the application.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool> LimitOpenDuration(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler) => entity.LimitTrueDuration(timeSpan, scheduler);

        /// <summary>
        /// Returns an observable that stays true for a maximum of <paramref name="timeSpan"/>. If the <paramref name="entity"/> turns off before the time has passed, the resulting observable also emits false.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit false even if the time did not pass in the application.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool> LimitOnDuration(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler) => entity.LimitTrueDuration(timeSpan, scheduler);

        /// <summary>
        /// Returns an observable that stays true for a maximum of <paramref name="timeSpan"/>. If the <paramref name="entity"/> turns off before the time has passed, the resulting observable also emits false.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit false even if the time did not pass in the application.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool> LimitTrueDuration(this Entity entity, TimeSpan timeSpan,
            IScheduler scheduler)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);

            return Observable.Defer(() =>
                Observable.Return(entity.IsOn() &&
                                  entity.EntityState != null &&
                                  entity.EntityState.LastChanged.HasValue &&
                                  entity.EntityState.LastChanged.Value.ToUniversalTime() >= DateTime.UtcNow - timeSpan)
                    .Concat(
                        entity.ToBooleanObservable().LimitTrueDuration(timeSpan, scheduler).Skip(1)));
        }

        /// <summary>
        /// Returns an observable that stays true for a maximum of <paramref name="timeSpan"/>. If the predicate returns false before the time has passed, the resulting observable also emits false.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit false even if the time did not pass in the application.
        /// Predicate will be evaluated on all state changes, including attribute changes.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool> LimitTrueDuration(this Entity entity, TimeSpan timeSpan,
            Func<EntityState, bool> predicate, IScheduler scheduler)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);

            return Observable.Defer(() =>
                Observable.Return(entity.EntityState != null &&
                                  predicate(entity.EntityState) &&
                                  entity.EntityState.LastChanged.HasValue &&
                                  entity.EntityState.LastChanged.Value.ToUniversalTime() >= DateTime.UtcNow - timeSpan)
                    .Concat(
                        entity.ToBooleanObservable(predicate).LimitTrueDuration(timeSpan, scheduler).Skip(1)));
        }

        /// <summary>
        /// Returns an observable that stays true for a maximum of <paramref name="timeSpan"/>. If the predicate returns false before the time has passed, the resulting observable also emits false.
        /// This method takes into account EntityState.LastChanged, meaning the returned observable can emit false even if the time did not pass in the application.
        /// Predicate will be evaluated on all state changes, including attribute changes.
        /// Resulting observable is distinct.
        /// </summary>
        public static IObservable<bool> LimitTrueDuration<TEntity, TEntityState, TAttributes>(
            this Entity<TEntity, TEntityState, TAttributes> entity,
            TimeSpan timeSpan,
            Func<TEntityState, bool> predicate,
            IScheduler scheduler)
            where TEntity : Entity<TEntity, TEntityState, TAttributes>
            where TEntityState : EntityState<TAttributes>
            where TAttributes : class
        {
            ArgumentNullException.ThrowIfNull(entity);
            ArgumentNullException.ThrowIfNull(scheduler);

            return Observable.Defer(() =>
                Observable.Return(entity.EntityState != null &&
                                  predicate(entity.EntityState) &&
                                  entity.EntityState.LastChanged.HasValue &&
                                  entity.EntityState.LastChanged.Value.ToUniversalTime() >= DateTime.UtcNow - timeSpan)
                    .Concat(
                        entity.ToBooleanObservable(predicate).LimitTrueDuration(timeSpan, scheduler).Skip(1)));
        }
    }
}