using CodeCasa.Lights.NetDaemon.Extensions;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.Lights.NetDaemon
{
    /// <summary>
    /// Adapts a NetDaemon light entity core to the <see cref="ILight"/> interface.
    /// </summary>
    public class NetDaemonLight(ILightEntityCore lightEntity) : ILight
    {
        /// <summary>
        /// Gets the unique identifier for this light entity.
        /// </summary>
        public string Id => lightEntity.EntityId;

        /// <summary>
        /// Gets the current parameters of the light entity.
        /// </summary>
        /// <returns>A <see cref="LightParameters"/> object representing the current state of the light.</returns>
        public LightParameters GetParameters() => lightEntity.GetLightParameters();

        /// <summary>
        /// Applies a light transition to this light entity.
        /// </summary>
        /// <param name="transition">The transition to apply to the light.</param>
        public void ApplyTransition(LightTransition transition)
        {
            lightEntity.ApplyTransition(transition);
        }

        /// <summary>
        /// Gets all child lights if this light represents a group.
        /// </summary>
        /// <returns>An array of child light entities wrapped as <see cref="ILight"/> instances, or an empty array if this light has no children.</returns>
        public ILight[] GetChildren() => lightEntity.Flatten().Select(le => new NetDaemonLight(le)).ToArray<ILight>();
    }
}
