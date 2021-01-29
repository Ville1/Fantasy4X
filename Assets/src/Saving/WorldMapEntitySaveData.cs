using System;
using System.Collections.Generic;

[Serializable]
public class WorkerSaveData
{
    public int Hex_X;
    public int Hex_Y;
    public string Name;
    public float Movement;
    public float Improvement_Progress;
    public string Improvement_Under_Construction;
    public List<CoordinateSaveData> Path;
    public bool Sleep;
}

[Serializable]
public class ProspectorSaveData
{
    public int Hex_X;
    public int Hex_Y;
    public string Name;
    public float Movement;
    public int Prospect_Progress;
    public bool Prospecting;
    public List<CoordinateSaveData> Path;
    public bool Sleep;
}

[Serializable]
public class ArmySaveData
{
    public int Hex_X;
    public int Hex_Y;
    public List<UnitSaveData> Units;
    public List<CoordinateSaveData> Path;
    public bool Sleep;
    public long Free_Embarkment;
}

[Serializable]
public class UnitSaveData
{
    public string Name;
    public float Manpower;
    public float Movement;
}
