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
}

[Serializable]
public class WorldMapHexSeedSaveData
{
    public string Key;
    public int Value;
}
