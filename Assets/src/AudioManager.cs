using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour, IConfigListener {
    public static AudioManager Instance;

    public GameObject Sound_Effect_Source_GO;
    public GameObject Music_Source_GO;

    private AudioSource sound_effect_source;
    private AudioSource music_source;
    private Dictionary<string, AudioClip> sound_effects;
    private Dictionary<string, AudioClip> music;
    private bool initialized;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;

        sound_effect_source = Sound_Effect_Source_GO.GetComponent<AudioSource>();
        music_source = Music_Source_GO.GetComponent<AudioSource>();
        sound_effects = new Dictionary<string, AudioClip>();
        music = new Dictionary<string, AudioClip>();
        initialized = false;
    }

    private void Initialize()
    {
        foreach (AudioClip clip in Resources.LoadAll<AudioClip>("audio/sound_effects")) {
            sound_effects.Add(clip.name, clip);
            CustomLogger.Instance.Debug("Sound effect loaded: " + clip.name);
        }
        foreach (AudioClip clip in Resources.LoadAll<AudioClip>("audio/music")) {
            music.Add(clip.name, clip);
            CustomLogger.Instance.Debug("Music track loaded: " + clip.name);
        }
        initialized = true;
        ConfigManager.Instance.Register_Listener(this);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {

    }

    /// <summary>
    /// 0.0f - 1.0f
    /// </summary>
    public float Sound_Effect_Volume
    {
        get {
            return sound_effect_source.volume;
        }
        set {
            sound_effect_source.volume = Mathf.Clamp01(value);
        }
    }

    public bool Mute_Sound_Effects
    {
        get {
            return sound_effect_source.mute;
        }
        set {
            sound_effect_source.mute = value;
        }
    }

    /// <summary>
    /// 0.0f - 1.0f
    /// </summary>
    public float Music_Volume
    {
        get {
            return music_source.volume;
        }
        set {
            music_source.volume = Mathf.Clamp01(value);
        }
    }

    public bool Mute_Music
    {
        get {
            return music_source.mute;
        }
        set {
            music_source.mute = value;
        }
    }

    public bool Mute_All
    {
        get {
            return Mute_Sound_Effects && Mute_Music;
        }
        set {
            Mute_Sound_Effects = value;
            Mute_Music = value;
        }
    }

    public void Update_Settings()
    {
        Sound_Effect_Volume = ConfigManager.Instance.Current_Config.Sound_Effect_Volume;
        Mute_Sound_Effects = ConfigManager.Instance.Current_Config.Mute_Sound_Effects;
        Music_Volume = ConfigManager.Instance.Current_Config.Music_Volume;
        Mute_Music = ConfigManager.Instance.Current_Config.Mute_Music;
    }

    public void Play_Sound_Effect(string name)
    {
        if (!initialized) {
            Initialize();
        }
        if (!sound_effects.ContainsKey(name)) {
            CustomLogger.Instance.Warning("Sound effect " + name + " does not exist!");
            return;
        }
        sound_effect_source.clip = sound_effects[name];
        sound_effect_source.Play();
    }

    public void Play_Music(string track)
    {
        if (!initialized) {
            Initialize();
        }
        if (!music.ContainsKey(track)) {
            CustomLogger.Instance.Warning("Track " + track + " does not exist!");
            return;
        }
        music_source.clip = music[track];
        music_source.Play();
    }
}
