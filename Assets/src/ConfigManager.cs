using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class ConfigManager {
    private static readonly string FILE_PATH = "/Resources/config/config.json";

    private static ConfigManager instance;
    
    public Config Current_Config { get; private set; }
    private List<IConfigListener> listeners;

    private ConfigManager()
    {
        Config default_config = new Config() {
            Sound_Effect_Volume = 1.0f,
            Mute_Sound_Effects = false,
            Music_Volume = 1.0f,
            Mute_Music = false,
            AI_Action_Delay = 1.0f,
            AI_Follow_Moves = true
        };
        listeners = new List<IConfigListener>();
        
        try {
            Current_Config = JsonUtility.FromJson<Config>(File.ReadAllText(Application.dataPath + FILE_PATH));
        } catch(Exception e) {
            CustomLogger.Instance.Warning("Failed to load config file. Using and saving default config. Exception: " + e.Message);
            Save(default_config);
        }
    }

    public static ConfigManager Instance
    {
        get {
            if(instance == null) {
                instance = new ConfigManager();
            }
            return instance;
        }
    }

    public void Register_Listener(IConfigListener listener)
    {
        if (listeners.Contains(listener)) {
            CustomLogger.Instance.Warning("Listener is already registered");
            return;
        }
        listeners.Add(listener);
        listener.Update_Settings();
    }

    public void Unregister_Listener(IConfigListener listener)
    {
        if (!listeners.Contains(listener)) {
            CustomLogger.Instance.Warning("Listener is not registered");
            return;
        }
        listeners.Remove(listener);
    }

    public void Save(Config config)
    {
        Current_Config = config;
        try {
            File.WriteAllText(Application.dataPath + FILE_PATH, JsonUtility.ToJson(Current_Config, true));
        } catch (Exception e) {
            CustomLogger.Instance.Error("Failed to save config. Exception: " + e.Message);
        }
    }
}
