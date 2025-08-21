﻿using Reactive.Boolean;

namespace NetDaemon.Extensions.Observables;

/// <summary>
/// Provides extension methods for <see cref="IObservable{Boolean}"/>.
/// </summary>
public static partial class BooleanObservableExtensions
{
    /// <summary>
    /// Subscribes actions to be executed when the observable emits true (open) or false (closed).
    /// </summary>
    public static IDisposable SubscribeOpenClosed(this IObservable<bool> observable, Action openAction, Action closedAction) => observable.SubscribeTrueFalse(openAction, closedAction);

    /// <summary>
    /// Subscribes an action to be executed when the observable emits true (open).
    /// </summary>
    public static IDisposable SubscribeOpen(this IObservable<bool> observable, Action openAction) => observable.SubscribeTrue(openAction);

    /// <summary>
    /// Subscribes an action to be executed when the observable emits true (closed).
    /// </summary>
    public static IDisposable SubscribeClosed(this IObservable<bool> observable, Action closedAction) => observable.SubscribeFalse(closedAction);

    /// <summary>
    /// Subscribes actions to be executed when the observable emits true (on) or false (off).
    /// </summary>
    public static IDisposable SubscribeOnOff(this IObservable<bool> observable, Action onAction, Action offAction) => observable.SubscribeTrueFalse(onAction, offAction);

    /// <summary>
    /// Subscribes an action to be executed when the observable emits true (on).
    /// </summary>
    public static IDisposable SubscribeOn(this IObservable<bool> observable, Action onAction) => observable.SubscribeTrue(onAction);

    /// <summary>
    /// Subscribes an action to be executed when the observable emits false (off).
    /// </summary>
    public static IDisposable SubscribeOff(this IObservable<bool> observable, Action offAction) => observable.SubscribeFalse(offAction);
}