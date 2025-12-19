using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace CodeCasa.Lights.NetDaemon.Generated;

internal record LightEntity : Entity<LightEntity, EntityState<LightAttributes>, LightAttributes>, ILightEntityCore
{
    public LightEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public LightEntity(IEntityCore entity) : base(entity)
    {
    }
}