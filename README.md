# NetDaemon.Extensions.Observables

Collection of extension methods meant to enhance NetDaemon entities with stateful and boolean observables allowing for more robust implementations and a more intuitive coding experience.

For more information on NetDaemon, click [here](https://netdaemon.xyz/).

## Example Usage

```cs
public Example(
    SunEntities sunEntities, 
    CoverEntities coverEntities)
{
    const int curtainCloseSunElevation = 1;
    var sunIsDown = sunEntities.Sun
            .ToBooleanObservable(e => e.Attributes?.Elevation <= curtainCloseSunElevation);
    
    sunIsDown.SubscribeTrueFalse(
        () =>
        {
            coverEntities.LivingRoomFrontWindowCurtain.CloseCover();
            coverEntities.LivingRoomBackWindowCurtain.CloseCover();
        },
        () =>
        {
            coverEntities.LivingRoomFrontWindowCurtain.OpenCover();
            coverEntities.LivingRoomBackWindowCurtain.OpenCover();
        });
}
```

**Breakdown:**

```cs
sunEntities.Sun.ToBooleanObservable(e => e.Attributes?.Elevation <= curtainCloseSunElevation);
```
Create a stateful `IObservable<bool>` from the sun entity `Sun` which emits true when `Elevation` is smaller than `curtainCloseSunElevation`, false otherwise. Stateful in this context means that the observable will immediately emit the current value returned from the predicate when an observer is subscribed.

```cs
sunIsDown.SubscribeTrueFalse(
        () =>
        {
            coverEntities.LivingRoomFrontWindowCurtain.CloseCover();
            coverEntities.LivingRoomBackWindowCurtain.CloseCover();
        },
        () =>
        {
            coverEntities.LivingRoomFrontWindowCurtain.OpenCover();
            coverEntities.LivingRoomBackWindowCurtain.OpenCover();
        });
```

SubscribeTrueFalse is an extension method on IObservable<bool> that assigns an Action for when true is emitted as well as when false is emitted.

This implementation will immediately open or close the covers depending on the initial result of the predicate as well as update whenever the result of the predicate changes.

## Stateful

As an addition to the `StateChanges` and `StateAllChanges` methods provided by `NetDaemon`, this library provides the methods `Stateful` and `StatefulAll`. The observable resulting from these methods will emit the current state of the entity upon subscribing to the observable. This allows a single subscription for both initial state and state changes.

**Example**
```cs
var frontWindowCurtainOpen = coverEntities.LivingRoomFrontWindowCurtain.ToOpenClosedObservable();

frontWindowCurtainOpen.SubscribeTrue(() => lightEntities.LivingRoomLights.TurnOff());
```
When starting this automation, the living room lights will immediately turn off if the front window curtain is open. It will also turn off every time the front window curtain is opened after that.

## Boolean Observables

This library also contains a set of extension methods to convert entities to implementations of `IObservable<bool>`. If no predicate is provided, the `On` state is mapped to true, the `Off` state is mapped to false.

The usage of `ToBooleanObservable` (or one of the aliases `ToOnOffObservable` or `ToOpenClosedObservable`) on an entity will use `StateChanges` (or `StateAllChanges` when a predicate is provided) to result in a stateful `IObservable<bool>`.

In case a stateful observable is not desired, the method `ToChangesOnlyBooleanObservable` can be used.

The use of stateful `IObservable<bool>` implementations enables the use of logic operators and scheduling extensions provided by the `Reactive.Boolean` library. This allows for improved readability of implementations.

For a more detailed documentation on extension methods for `IObservable<bool>`, check out the documentation for `Reactive.Boolean` [here](https://github.com/DevJasperNL/Reactive.Boolean).

**Example**

The following example shows the usage of boolean (`Not`, `Or` and `And`) and scheduling (`LimitTrueDuration`) logic to write a complex automation that is still easy to understand and will trigger on startup.
```cs
var noOneAsleep = inputBooleanEntities.JasperAsleep.ToBooleanObservable()
    .Or(inputBooleanEntities.AnonAsleep.ToBooleanObservable()).Not();

var closetDoorOpenShorterThanOneMin = binarySensorEntities.BedroomClosetDoorSensorContact
    .ToOpenClosedObservable().LimitTrueDuration(TimeSpan.FromMinutes(5), scheduler);

noOneAsleep.And(closetDoorOpenShorterThanOneMin).SubscribeTrueFalse(
    () => lightEntities.ClosetLight.TurnOn(),
    () => lightEntities.ClosetLight.TurnOff());
```

Note that even though scheduling is mostly handled by the `Reactive.Boolean` library, knowledge of the `Entity` does improve some scheduling methods. In the cases of `WhenTrueFor` and `LimitTrueDuration` both `LastChanged` and the passing of time are used to evaluate whether a true is emitted.

## Unavailability of entities

This library also implements the utility method `RepeatWhenEntitiesBecomeAvailable<T>`. This method can be called on a `IObservable<T>` to repeat a value when one of the provided entities become available.

**Example**

The following example will re-apply the correct state as soon as the plug becomes available (is plugged in).
```cs
const int nightTimeSunElevation = 0;
var nightTime = sunEntities.Sun
    .ToBooleanObservable(e => e.Attributes?.Elevation <= nightTimeSunElevation);

nightTime
    .RepeatWhenEntitiesBecomeAvailable(switchEntities.LivingRoomChristmasTreeLights)
    .SubscribeTrueFalse(
        () => switchEntities.LivingRoomChristmasTreeLights.TurnOn(),
        () => switchEntities.LivingRoomChristmasTreeLights.TurnOff());
```