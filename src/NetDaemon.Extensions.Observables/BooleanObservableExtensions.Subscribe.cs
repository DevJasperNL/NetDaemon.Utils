using Reactive.Boolean;

namespace NetDaemon.Extensions.Observables
{
    public static class BooleanObservableExtensions
    {
        /// <summary>
        /// Subscribes actions to be executed when the observable emits true (open) or false (closed).
        /// </summary>
        public static IDisposable SubscribeOpenClosed(this IObservable<bool> observable, Action openAction, Action closeAction) => observable.SubscribeTrueFalse(openAction, closeAction);

        /// <summary>
        /// Subscribes an action to be executed when the observable emits true (open).
        /// </summary>
        public static IDisposable SubscribeOpen(this IObservable<bool> observable, Action openAction) => observable.SubscribeTrue(openAction);

        /// <summary>
        /// Subscribes an action to be executed when the observable emits true (closed).
        /// </summary>
        public static IDisposable SubscribeClosed(this IObservable<bool> observable, Action closeAction) => observable.SubscribeFalse(closeAction);

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
        public static IDisposable SubscribeOff(this IObservable<bool> observable, Action offAction) => observable.SubscribeTrue(offAction);
    }
}
