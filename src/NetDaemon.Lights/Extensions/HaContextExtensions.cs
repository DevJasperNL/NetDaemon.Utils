using NetDaemon.HassModel;
using NetDaemon.Lights.Generated;

namespace NetDaemon.Lights.Extensions
{
    public static class HaContextExtensions
    {
        public static string[] FlattenLight(this IHaContext haContext, string lightEntityId)
        {
            return new LightEntity(haContext, lightEntityId).Flatten().Select(e => e.EntityId).ToArray();
        }

        public static string[] FlattenLights(this IHaContext haContext, IEnumerable<string> lightEntityIds) =>
            haContext.FlattenLights(lightEntityIds.ToArray());

        public static string[] FlattenLights(this IHaContext haContext, params string[] lightEntityIds)
        {
            return lightEntityIds.Select(id => new LightEntity(haContext, id)).SelectMany(le => le.Flatten())
                .Select(e => e.EntityId).Distinct().ToArray();
        }
    }
}
