using NetDaemon.HassModel.Entities;

namespace CodeCasa.NetDaemon.Extensions.Observables;

public static partial class EntityExtensions
{
    /// <summary>
    /// Binds the boolean state of a source entity to the on/off state of a target entity using generic Home Assistant actions.
    /// When the source entity's state is <c>true</c>, the target entity will be turned on; when <c>false</c>, the target entity will be turned off.
    /// </summary>
    /// <param name="sourceEntity">The entity whose boolean state is observed.</param>
    /// <param name="targetEntity">The entity to control based on the source entity's state.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription. Dispose to stop listening to the source entity's state.</returns>
    public static IDisposable BindToOnOff(this Entity sourceEntity, IEntityCore targetEntity) => sourceEntity.ToBooleanObservable().BindToOnOff(targetEntity);

    /// <summary>
    /// Binds the boolean state of a source entity to the on/off states of multiple target entities using generic Home Assistant actions.
    /// When the source entity's state is <c>true</c>, all target entities will be turned on; when <c>false</c>, all target entities will be turned off.
    /// </summary>
    /// <param name="sourceEntity">The entity whose boolean state is observed.</param>
    /// <param name="targetEntities">The collection of entities to control based on the source entity's state. Must contain at least one entity.</param>
    /// <returns>An <see cref="IDisposable"/> representing the subscription. Dispose to stop listening to the source entity's state.</returns>
    public static IDisposable BindToOnOff(this Entity sourceEntity, IEnumerable<IEntityCore> targetEntities) => sourceEntity.ToBooleanObservable().BindToOnOff(targetEntities);
}