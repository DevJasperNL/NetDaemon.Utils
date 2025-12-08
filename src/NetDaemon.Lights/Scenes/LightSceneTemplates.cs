using NetDaemon.Lights.Extensions;

namespace NetDaemon.Lights.Scenes
{
    public class LightSceneTemplates
    {
        public static LightSceneTemplate Relax => lightEntity => lightEntity.GetRelaxSceneParameters();
        public static LightSceneTemplate NightLight => lightEntity => lightEntity.GetNightLightSceneParameters();
        public static LightSceneTemplate Concentrate => lightEntity => lightEntity.GetConcentrateSceneParameters();
        public static LightSceneTemplate Bright => lightEntity => lightEntity.GetBrightSceneParameters();
        public static LightSceneTemplate Dimmed => lightEntity => lightEntity.GetDimmedSceneParameters();
    }
}
