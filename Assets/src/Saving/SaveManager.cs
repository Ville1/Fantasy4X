using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            data.Map = new MapSaveData();
            data.Map.Hexes = new List<WorldMapHexSaveData>();
            data.Map.Cities = new List<CitySaveData>();
            data.Map.Villages = new List<VillageSaveData>();
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
            data.Map.Hexes.Add(hex.Save_Data);
        }
    }

    public void Add(City city)
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Saving needs to be called before Add");
        } else {
            data.Map.Cities.Add(city.Save_Data);
        }
    }

    public void Add(Village village)
    {
        if (data == null) {
            CustomLogger.Instance.Error("Start_Saving needs to be called before Add");
        } else {
            data.Map.Villages.Add(village.Save_Data);
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

    public static int Get_Player_Id(Player player)
    {
        if(player == null) {
            return -4;
        }
        if (player.Is_Neutral) {
            if (player.Id == Main.Instance.Neutral_Cities_Player.Id) {
                return -1;
            } else if (player.Id == Main.Instance.Bandit_Player.Id) {
                return -2;
            } else {
                return -3;
            }
        } else {
            return player.Id;
        }
    }

    public static Player Get_Player(int save_id)
    {
        switch (save_id) {
            case -1:
                return Main.Instance.Neutral_Cities_Player;
            case -2:
                return Main.Instance.Bandit_Player;
            case -3:
                return Main.Instance.Wild_Life_Player;
            case -4:
                return null;
            default:
                return Main.Instance.Players.First(x => x.Id == save_id);
        }
    }
}
