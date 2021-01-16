using System;
using System.Collections.Generic;

[Serializable]
public class CitySaveData
{
    public int Id;
    public string Name;
    public int Hex_X;
    public int Hex_Y;
    public int Owner;
    public List<CoordinateSaveData> Worked_Hexes;
    public List<CoordinateSaveData> Hexes_In_Work_Range;
    public int LoS;
    public int Work_Range;
    public string Unit_Under_Production;
    public string Building_Under_Production;
    public float Unit_Production_Acquired;
    public float Building_Production_Acquired;
    public int Population;
    public List<BuildingSaveData> Buildings;
    public int Base_Max_Food_Storage;
    public float Food_Stored;
    public bool Full_LoS;
    public List<CulturalInfluenceSaveData> Cultural_Influence;
    public List<TradeRouteSaveData> Trade_Routes;
    public bool Is_Coastal;
    public YieldsSaveData Last_Turn_Yields;
    public List<CityStatusEffectSaveData> Status_Effects;
    public bool Update_Yields;
    public YieldsSaveData Saved_Base_Yields;
    public YieldsSaveData Saved_Yields;
}

[Serializable]
public class VillageSaveData
{
    public int Id;
    public string Name;
    public int Hex_X;
    public int Hex_Y;
    public int Owner;
    public List<CulturalInfluenceSaveData> Cultural_Influence;
}

[Serializable]
public class CityStatusEffectSaveData
{
    public string Name;
    public YieldsSaveData Yield_Delta;
    public YieldsSaveData Percentage_Yield_Delta;
    public int Duration;
    public int Current_Duration;
    public int Parent_Duration;
    public float Happiness;
    public float Health;
    public float Order;
}

[Serializable]
public class BuildingSaveData
{
    public string Name;
    public bool Is_Paused;
    public YieldsSaveData Yield_Delta;
}

[Serializable]
public class CoordinateSaveData
{
    public int X;
    public int Y;
}

[Serializable]
public class CulturalInfluenceSaveData
{
    public int Player;
    public float Influence;
}

[Serializable]
public class TradeRouteSaveData
{
    public List<CoordinateSaveData> Path;
    public CoordinateSaveData Target;
    public bool Water_Route;
}