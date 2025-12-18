using NetDaemon.HassModel.Entities;
using CodeCasa.NetDaemon.Lights.Extensions;

namespace CodeCasa.Lights.NetDaemon
{
    public class NetDaemonLight(ILightEntityCore lightEntity) : ILight
    {
        public string Id => lightEntity.EntityId;

        public LightParameters GetParameters() => lightEntity.GetLightParameters();

        public void ApplyTransition(LightTransition transition)
        {
            lightEntity.ApplyTransition(transition);
        }

        public ILight[] GetChildren() => lightEntity.Flatten().Select(le => new NetDaemonLight(le)).ToArray<ILight>();
    }
}
