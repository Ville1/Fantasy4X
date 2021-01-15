using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager
{
    public static readonly string DEFAULT_SAVE_LOCATION = "C:\\Users\\Ville\\Documents\\f4xsaves\\";

    private static SaveManager instance;

    private SaveData data;
    private string path;

    private SaveManager()
    { }

    public static SaveManager Instance
    {
        get {
            if (instance == null) {
                instance = new SaveManager();
            }
            return instance;
        }
    }

    public bool Start_Saving(string path)
    {
        try {
            this.path = path;
            data = new SaveData();
            data.Map = new List<WorldMapHexSaveData>();
            return true;
        } catch (Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            return false;
        }
    }

    public bool Start_Loading(string path)
    {
        try {
            this.path = path;
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            return true;
        } catch (Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            return false;
        }
    }

    public void Finish_Loading()
    {
        data = null;
    }

    public void Add(WorldMapHex hex)
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Saving needs to be called before Add");
        } else {
            data.Map.Add(hex.Save_Data);
        }
    }
    
    public SaveData Data
    {
        get {
            return data;
        }
    }

    public bool Finish_Saving()
    {
        try {
            File.WriteAllText(path, JsonUtility.ToJson(data, true));
            data = null;
            return true;
        } catch (Exception exception) {
            CustomLogger.Instance.Error(exception.ToString());
            data = null;
            return false;
        }
    }
}
