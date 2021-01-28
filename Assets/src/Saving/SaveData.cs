using System;
using System.Collections.Generic;

[Serializable]
public class SaveData {
    public MapSaveData Map;
    public List<PlayerSaveData> Players;
    public int Max_Rounds;
    public int Round;
    public int Current_Player;
    public NeutralCitiesSaveData Neutral_Cities;
    public BanditsSaveData Bandits;
    public WildLifeSaveData Wild_Life;
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
    public List<WorkerSaveData> Workers;
    public List<ProspectorSaveData> Prospectors;
    public List<ArmySaveData> Armies;
    public string Current_Technology;
    public float Research_Acquired;
    public List<string> Researched_Technologies;
    public string Last_Technology_Researched;
    public List<TechnologySaveData> Technologies_In_Progress;
    public List<EmpireModifierStatusEffectSaveData> Status_Effects;
    public List<CooldownSaveData> Spells_On_Cooldown;
    public List<CooldownSaveData> Blessings_On_Cooldown;
    public List<CooldownSaveData> Active_Blessings;
    public AISaveData AI_Data;
}

[Serializable]
public class CooldownSaveData
{
    public string Name;
    public int Value;
}

[Serializable]
public class TechnologySaveData
{
    public string Name;
    public float Progress;
}

[Serializable]
public class NeutralCitiesSaveData
{
    public List<ArmySaveData> Armies;
}

[Serializable]
public class BanditsSaveData
{
    public List<ArmySaveData> Armies;
}

[Serializable]
public class WildLifeSaveData
{
    public List<ArmySaveData> Armies;
}