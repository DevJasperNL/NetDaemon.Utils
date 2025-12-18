using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;
using Reactive.Boolean;

namespace NetDaemon.Extensions.Observables;

public static partial class EntityExtensions
{
    /// <summary>
    /// Returns an observable that emits "true" once <paramref name="entity"/> does not emit closed (false) for a minimum of <paramref name="timeSpan"/>.
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "true" even if the time did not pass during runtime.
    /// Resulting observable is distinct.
    /// </summary>
    public static IObservable<bool> WhenOpenFor(this Entity entity, TimeSpan timeSpan,
        IScheduler scheduler) => entity.WhenTrueFor(timeSpan, scheduler);

    /// <summary>
    /// Returns an observable that emits "true" once <paramref name="entity"/> does not emit off (false) for a minimum of <paramref name="timeSpan"/>.
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "true" even if the time did not pass during runtime.
    /// Resulting observable is distinct.
    /// </summary>
    public static IObservable<bool> WhenOnFor(this Entity entity, TimeSpan timeSpan,
        IScheduler scheduler) => entity.WhenTrueFor(timeSpan, scheduler);

    /// <summary>
    /// Returns an observable that emits "true" once <paramref name="entity"/> does not emit off (false) for a minimum of <paramref name="timeSpan"/>.
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "true" even if the time did not pass during runtime.
    /// Resulting observable is distinct.
    /// </summary>
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
    /// Returns an observable that emits "true" once <paramref name="predicate"/> applied to the state of <paramref name="entity"/> does not emit "false" for a minimum of <paramref name="timeSpan"/>.
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "true" even if the time did not pass during runtime.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// Resulting observable is distinct.
    /// </summary>
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
    /// Returns an observable that emits "true" once <paramref name="predicate"/> applied to the state of <paramref name="entity"/> does not emit "false" for a minimum of <paramref name="timeSpan"/>.
    /// This method takes into account <c>EntityState.LastChanged</c>, meaning the returned observable can emit "true" even if the time did not pass during runtime.
    /// Predicate will be evaluated on all state changes, including attribute changes.
    /// Resulting observable is distinct.
    /// </summary>
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
}