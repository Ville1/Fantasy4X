using System.Collections.Generic;
using UnityEngine;

public class EffectManager {
    public enum TextType { Damage, Morale }

    private static string default_effect_name = "default_effect";
    private static string layer_name = "Effects";
    private static EffectManager instance;
    private Dictionary<string, List<Sprite>> effects;
    private Dictionary<Hex, List<EffectData>> active_effects;
    private Dictionary<Hex, List<string>> queued_effects;
    private Dictionary<Hex, List<FloatingTextData>> active_texts;
    private Dictionary<Hex, List<TextData>> queued_texts;
    private bool no_queuing;
    private bool initialization_failed;

    public float Animation_Frame_Time = 0.15f;//s
    public float Text_Float_Height = 0.05f;//???
    public float Text_Float_Time = 1.1f;//s
    public float Queue_Wait_Time = 0.3f;//s

    private class EffectData
    {
        public GameObject GameObject { get; set; }
        public string Effect_Name { get; set; }
        public int Index { get; set; }
        public float Time_Left { get; set; }
    }

    private class FloatingTextData
    {
        public GameObject GameObject { get; set; }
        public float Time_Left { get; set; }
        public Hex Hex { get; set; }
    }

    private EffectManager()
    {
        effects = new Dictionary<string, List<Sprite>>();
        no_queuing = false;
        CustomLogger.Instance.Debug("Loading sprites...");
        foreach (Sprite texture in Resources.LoadAll<Sprite>("images/effects")) {
            if (!texture.name.Contains("_")) {
                CustomLogger.Instance.Warning("Invalid effect sprite: " + texture.name + ", name is missing underscore");
                continue;
            }
            string number_string = texture.name.Substring(texture.name.LastIndexOf('_') + 1);
            int index;
            if (!int.TryParse(number_string, out index)) {
                CustomLogger.Instance.Warning("Invalid effect sprite: " + texture.name + ", name has invalid index: " + number_string);
                continue;
            }
            if (index <= 0) {
                CustomLogger.Instance.Warning("Invalid effect sprite: " + texture.name + ", name has invalid index: " + number_string);
                continue;
            }
            string name = texture.name.Substring(0, texture.name.LastIndexOf('_'));
            if (!effects.ContainsKey(name)) {
                effects.Add(name, new List<Sprite>());
                CustomLogger.Instance.Debug("New effect created: " + name);
            }
            effects[name].Insert(index - 1, texture);
            CustomLogger.Instance.Debug("Sprite added to effect: " + name + " at index: " + index);
        }
        CustomLogger.Instance.Debug("All sprites loaded");
        if (!effects.ContainsKey(default_effect_name)) {
            CustomLogger.Instance.Error("Default effect: " + default_effect_name + " is missing");
        }
    }

    /// <summary>
    /// Accessor for singleton instance
    /// </summary>
    public static EffectManager Instance
    {
        get {
            if (instance == null) {
                instance = new EffectManager();
            }
            return instance;
        }
    }

    public void Update_Target_Map()
    {
        if(active_effects != null && queued_effects != null && active_texts != null && queued_texts != null) {
            Clear_Effects();
        }
        active_effects = new Dictionary<Hex, List<EffectData>>();
        queued_effects = new Dictionary<Hex, List<string>>();
        active_texts = new Dictionary<Hex, List<FloatingTextData>>();
        queued_texts = new Dictionary<Hex, List<TextData>>();
        if (CombatManager.Instance.Map != null && CombatManager.Instance.Map.Active) {
            for (int x = 0; x < CombatManager.Instance.Map.Width; x++) {
                for (int y = 0; y < CombatManager.Instance.Map.Height; y++) {
                    CombatMapHex hex = CombatManager.Instance.Map.Get_Hex_At(x, y);
                    if(hex != null) {
                        active_effects.Add(hex, new List<EffectData>());
                        queued_effects.Add(hex, new List<string>());
                        active_texts.Add(hex, new List<FloatingTextData>());
                        queued_texts.Add(hex, new List<TextData>());
                    }
                }
            }
        } else {
            for (int x = 0; x < World.Instance.Map.Width; x++) {
                for (int y = 0; y < World.Instance.Map.Height; y++) {
                    WorldMapHex hex = World.Instance.Map.Get_Hex_At(x, y);
                    if (hex != null) {
                        active_effects.Add(hex, new List<EffectData>());
                        queued_effects.Add(hex, new List<string>());
                        active_texts.Add(hex, new List<FloatingTextData>());
                        queued_texts.Add(hex, new List<TextData>());
                    }
                }
            }
        }
    }

    /// <summary>
    /// Plays default effect on specified tile
    /// </summary>
    /// <param name="hex"></param>
    public bool Play_Effect(Hex hex)
    {
        return Play_Effect(hex, default_effect_name);
    }

    /// <summary>
    /// Plays effect on specified tile
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="effect"></param>
    public bool Play_Effect(Hex hex, string effect)
    {
        if (!effects.ContainsKey(effect)) {
            CustomLogger.Instance.Warning("Effect not found: " + effect);
            return false;
        }

        if (active_effects[hex].Count != 0 && Animation_Frame_Time - active_effects[hex][active_effects[hex].Count - 1].Time_Left < Queue_Wait_Time) {
            if (no_queuing) {
                return false;
            }
            queued_effects[hex].Add(effect);
            return false;
        }

        GameObject game_object = new GameObject("effect_" + effect + "_0");
        game_object.transform.position = new Vector3(hex.GameObject.transform.position.x, hex.GameObject.transform.position.y, hex.GameObject.transform.position.z);
        game_object.transform.parent = hex.GameObject.transform;
        SpriteRenderer renderer = game_object.AddComponent<SpriteRenderer>();
        renderer.sprite = effects[effect][0];
        renderer.sortingLayerName = layer_name;
        active_effects[hex].Add(new EffectData() { Effect_Name = effect, GameObject = game_object, Index = 0, Time_Left = Animation_Frame_Time });
        return true;
    }

    /// <summary>
    /// Creates a floating text used to show damage, healing, etc.
    /// </summary>
    /// <param name="hex"></param>
    /// <param name="text"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool Play_Floating_Text(Hex hex, string text, TextType type = TextType.Damage)
    {
        if (active_texts[hex].Count != 0 && Text_Float_Time - active_texts[hex][active_texts[hex].Count - 1].Time_Left < Queue_Wait_Time) {
            if (no_queuing) {
                return false;
            }
            queued_texts[hex].Add(new TextData() {
                Text = text,
                Type = type
            });
            return false;
        }

        GameObject new_text = GameObject.Instantiate(type == TextType.Damage ? PrefabManager.Instance.Floating_Text : PrefabManager.Instance.Floating_Text_Morale);
        new_text.transform.parent = hex.GameObject.transform;
        new_text.GetComponentInChildren<TextMesh>().text = text;
        new_text.name = "floating_text_" + hex.Coordinates.X + "_" + hex.Coordinates.Y;
        new_text.transform.position = new Vector3(hex.GameObject.transform.position.x, hex.GameObject.transform.position.y, hex.GameObject.transform.position.z);
        //CameraManager.Instance.Set_UI_Element_On_World_GO(new_text.rectTransform, hex.GameObject);
        active_texts[hex].Add(new FloatingTextData() {
            GameObject = new_text,
            Hex = hex,
            Time_Left = Text_Float_Time
        });
        return true;
    }

    /// <summary>
    /// Updates effect animations
    /// </summary>
    /// <param name="delta_time"></param>
    public void Update(float delta_time)
    {
        if(active_effects == null && !initialization_failed) {
            //TODO: This does not prevent log spam
            CustomLogger.Instance.Error("active_effects == null");
            initialization_failed = true;
            return;
        }
        //Effects
        foreach (KeyValuePair<Hex, List<EffectData>> tile_effects in active_effects) {
            List<EffectData> effects_that_have_ended = new List<EffectData>();
            foreach (EffectData active_effect in tile_effects.Value) {
                float current_time = active_effect.Time_Left - delta_time;
                if (current_time <= 0) {
                    active_effect.Index++;
                    if (effects[active_effect.Effect_Name].Count <= active_effect.Index) {
                        effects_that_have_ended.Add(active_effect);
                        GameObject.Destroy(active_effect.GameObject);
                        continue;
                    }
                    active_effect.Time_Left = current_time + Animation_Frame_Time;
                    active_effect.GameObject.GetComponent<SpriteRenderer>().sprite = effects[active_effect.Effect_Name][active_effect.Index];
                } else {
                    active_effect.Time_Left = current_time;
                }
            }
            foreach (EffectData data in effects_that_have_ended) {
                tile_effects.Value.Remove(data);
            }
        }
        //Queued effects
        no_queuing = true;
        foreach (KeyValuePair<Hex, List<string>> tile_queued_effects in queued_effects) {
            if (tile_queued_effects.Value.Count == 0) {
                continue;
            }
            if (Play_Effect(tile_queued_effects.Key, tile_queued_effects.Value[0])) {
                tile_queued_effects.Value.RemoveAt(0);
            }
        }
        no_queuing = false;

        //Texts
        foreach (KeyValuePair<Hex, List<FloatingTextData>> tile_texts in active_texts) {
            List<FloatingTextData> texts_that_have_ended = new List<FloatingTextData>();
            foreach (FloatingTextData data in tile_texts.Value) {
                data.Time_Left -= delta_time;
                if (data.Time_Left <= 0.0f) {
                    texts_that_have_ended.Add(data);
                    GameObject.Destroy(data.GameObject.gameObject);
                    continue;
                }
                data.GameObject.gameObject.transform.position = new Vector3(
                    data.GameObject.gameObject.transform.position.x,
                    data.GameObject.gameObject.transform.position.y + (Text_Float_Height * (Text_Float_Time - data.Time_Left)),
                    data.GameObject.gameObject.transform.position.z
                );
            }
            foreach (FloatingTextData data in texts_that_have_ended) {
                tile_texts.Value.Remove(data);
            }
        }
        //Queued texts
        no_queuing = true;
        foreach (KeyValuePair<Hex, List<TextData>> tile_queued_texts in queued_texts) {
            if (tile_queued_texts.Value.Count == 0) {
                continue;
            }
            if (Play_Floating_Text(tile_queued_texts.Key, tile_queued_texts.Value[0].Text, tile_queued_texts.Value[0].Type)) {
                tile_queued_texts.Value.RemoveAt(0);
            }
        }
        no_queuing = false;
    }

    /// <summary>
    /// Removes all currently active effects and queued effects
    /// </summary>
    public void Clear_Effects()
    {
        foreach (KeyValuePair<Hex, List<EffectData>> tile_effects in active_effects) {
            foreach (EffectData data in tile_effects.Value) {
                GameObject.Destroy(data.GameObject);
            }
            tile_effects.Value.Clear();
        }
        foreach (KeyValuePair<Hex, List<string>> queued_tile_effects in queued_effects) {
            queued_tile_effects.Value.Clear();
        }

        foreach (KeyValuePair<Hex, List<FloatingTextData>> tile_texts in active_texts) {
            foreach (FloatingTextData data in tile_texts.Value) {
                GameObject.Destroy(data.GameObject);
            }
            tile_texts.Value.Clear();
        }
        foreach (KeyValuePair<Hex, List<TextData>> queued_tile_texts in queued_texts) {
            queued_tile_texts.Value.Clear();
        }
    }

    private class TextData
    {
        public string Text { get; set; }
        public TextType Type { get; set; }
    }
}