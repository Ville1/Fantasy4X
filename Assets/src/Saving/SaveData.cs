using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
    public MapSaveData Map;
    public List<PlayerSaveData> Players;
    public int Max_Rounds;
    public int Round;
    public int Current_Player;
}

[Serializable]
public class MapSaveData
{
    public int Width;
    public int Height;
    public float Mineral_Spawn_Rate;
    public List<WorldMapHexSaveData> Hexes;
    public List<CitySaveData> Cities;
    public List<VillageSaveData> Villages;
}

[Serializable]
public class PlayerSaveData
{
    public int Id;
    public string Name;
    public int AI_Level;
    public string Faction;
    public int Team;
    public float Cash;
    public float Mana;
    public List<int> Cities;
    public List<int> Villages;
}
