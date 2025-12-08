using NetDaemon.HassModel.Entities;

namespace NetDaemon.Lights.Scenes;

/// <summary>
/// A delegate that generates light parameters for a specific light entity.
/// This is used to create scene templates that can be applied to any light to generate appropriate parameters for that scene.
/// </summary>
/// <param name="lightEntity">The light entity to generate parameters for.</param>
/// <returns>A <see cref="LightParameters"/> object representing the appropriate light state for the scene.</returns>
public delegate LightParameters LightSceneTemplate(ILightEntityCore lightEntity);