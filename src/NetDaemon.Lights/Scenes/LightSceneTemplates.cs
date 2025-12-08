using NetDaemon.Lights.Extensions;

namespace NetDaemon.Lights.Scenes
{
    /// <summary>
    /// Provides predefined light scene templates that can be applied to any light entity.
    /// Each template automatically adapts to the light's capabilities to generate appropriate parameters.
    /// </summary>
    public class LightSceneTemplates
    {
        /// <summary>
        /// Gets a template that generates parameters for a relaxing scene (warm color temperature, medium brightness).
        /// </summary>
        public static LightSceneTemplate Relax => lightEntity => lightEntity.GetRelaxSceneParameters();

        /// <summary>
        /// Gets a template that generates parameters for a nightlight scene (very warm color temperature, very low brightness).
        /// </summary>
        public static LightSceneTemplate NightLight => lightEntity => lightEntity.GetNightLightSceneParameters();

        /// <summary>
        /// Gets a template that generates parameters for a concentrate scene (cool/neutral color temperature, high brightness).
        /// </summary>
        public static LightSceneTemplate Concentrate => lightEntity => lightEntity.GetConcentrateSceneParameters();

        /// <summary>
        /// Gets a template that generates parameters for a bright scene (warm color temperature, maximum brightness).
        /// </summary>
        public static LightSceneTemplate Bright => lightEntity => lightEntity.GetBrightSceneParameters();

        /// <summary>
        /// Gets a template that generates parameters for a dimmed scene (warm color temperature, low brightness).
        /// </summary>
        public static LightSceneTemplate Dimmed => lightEntity => lightEntity.GetDimmedSceneParameters();
    }
}
