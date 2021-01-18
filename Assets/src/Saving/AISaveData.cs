using System;
using System.Collections.Generic;

[Serializable]
public class AISaveData {
    public List<AIArmyOrderSaveData> Scouting_Armies;
    public List<AIArmyOrderSaveData> Defence_Armies;
    public List<AIArmyOrderSaveData> Main_Armies;
    public List<CoordinateInfoSaveData> Cities_Training_Scout_Armies;
    public List<DoubleCoordinateInfoSaveData> Cities_Training_Defence_Armies;
    public List<CoordinateInfoSaveData> Cities_Training_Main_Armies;
    public List<AIPlayerFloatInfoSaveData> Observed_Max_Enemy_Army_Strenght;
    public List<AIPlayerFloatInfoSaveData> Observed_Enemy_Army_Strenght_On_This_Turn;
    public List<CoordinateSaveData> Armies_Seen_This_Turn;
    public List<CoordinateSaveData> Scouted_Enemy_Cities;
    public List<AIPlayerIntInfoSaveData> Turns_Since_Army_Was_Scouted;
}

[Serializable]
public class CoordinateInfoSaveData
{
    public string Value;
    public CoordinateSaveData Coordinates;
}

[Serializable]
public class DoubleCoordinateInfoSaveData
{
    public string Value;
    public CoordinateSaveData Coordinates_1;
    public CoordinateSaveData Coordinates_2;
}

[Serializable]
public class AIArmyOrderSaveData
{
    public CoordinateSaveData Army_Coordinates;
    public CoordinateSaveData Target_Coordinates;
    public int Order;
    public bool Is_Army_Target;
}

[Serializable]
public class AIPlayerIntInfoSaveData
{
    public int Player_Id;
    public int Value;
}

[Serializable]
public class AIPlayerFloatInfoSaveData
{
    public int Player_Id;
    public float Value;
}