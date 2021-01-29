using System;
using System.Collections.Generic;

[Serializable]
public class WorldMapHexSaveData {
    public int Q;
    public int R;
    public int S;
    public string Internal_Name;
    public string Mineral;
    public List<WorldMapHexSeedSaveData> CombatMap_City_Seed;
    public int Current_Los;
    public List<int> Explored_By;
    public int Owner;
    public List<int> Prospected_By;
    public string Road;
    public ImprovementSaveData Improvement;
    public bool Is_Map_Edge_Road_Connection;
    public List<WorldMapHexStatusEffectSaveData> Status_Effects;
    public int Sprite_Index;
    public long Georaphic_Feature;
}

[Serializable]
public class ImprovementSaveData
{
    public string Name;
    public string Faction;
    public YieldsSaveData Special_Yield_Delta;
    public float Happiness_Delta;
    public float Health_Delta;
    public float Order_Delta;
}

[Serializable]
public class WorldMapHexStatusEffectSaveData
{
    public string Name;
    public YieldsSaveData Yield_Delta;
    public int Duration;
    public int Current_Duration;
    public int Parent_Duration;
    public float Happiness;
    public float Health;
    public float Order;
}

[Serializable]
public class WorldMapHexSeedSaveData
{
    public string Key;
    public int Value;
}
