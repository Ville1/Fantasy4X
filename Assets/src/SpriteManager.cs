using System.Collections.Generic;
using UnityEngine;

public class SpriteManager
{
    public enum SpriteType { Terrain, Character, Skill, UI, Unit, Unit_Animation, Improvement, Building };
    private static SpriteManager instance;

    private Dictionary<SpriteType, string> prefixes;
    private Dictionary<string, Sprite> sprites;
    private bool suppress_error_logging;

    private SpriteManager()
    {
        suppress_error_logging = false;
        prefixes = new Dictionary<SpriteType, string>();
        sprites = new Dictionary<string, Sprite>();

        prefixes.Add(SpriteType.Terrain, "terrain");
        prefixes.Add(SpriteType.Character, "character");
        prefixes.Add(SpriteType.Skill, "skill");
        prefixes.Add(SpriteType.UI, "ui");
        prefixes.Add(SpriteType.Unit, "unit");
        prefixes.Add(SpriteType.Unit_Animation, "unit_anim");
        prefixes.Add(SpriteType.Improvement, "improvement");
        prefixes.Add(SpriteType.Building, "building");

        CustomLogger.Instance.Debug("Loading sprites...");
        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/terrain")) {
            sprites.Add(prefixes[SpriteType.Terrain] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Terrain sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/characters")) {
            sprites.Add(prefixes[SpriteType.Character] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Character sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/skills")) {
            sprites.Add(prefixes[SpriteType.Skill] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Skill sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/ui")) {
            sprites.Add(prefixes[SpriteType.UI] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("UI sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/units")) {
            sprites.Add(prefixes[SpriteType.Unit] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Unit sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/units/animations")) {
            sprites.Add(prefixes[SpriteType.Unit_Animation] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Animation sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/improvements")) {
            sprites.Add(prefixes[SpriteType.Improvement] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Improvement sprite loaded: " + texture.name);
        }

        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/buildings")) {
            sprites.Add(prefixes[SpriteType.Building] + "_" + texture.name, texture);
            CustomLogger.Instance.Debug("Building sprite loaded: " + texture.name);
        }

        CustomLogger.Instance.Debug("All sprites loaded");
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static SpriteManager Instance
    {
        get {
            if (instance == null) {
                instance = new SpriteManager();
            }
            return instance;
        }
    }

    /// <summary>
    /// Get sprite by name and type
    /// </summary>
    /// <param name="sprite_name"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Sprite Get_Sprite(string sprite_name, SpriteType type)
    {
        if (sprites.ContainsKey(prefixes[type] + "_" + sprite_name)) {
            return sprites[prefixes[type] + "_" + sprite_name];
        }
        if (!suppress_error_logging) {
            CustomLogger.Instance.Warning("Sprite " + prefixes[type] + "_" + sprite_name + " does not exist!");
        }
        return null;
    }
}
