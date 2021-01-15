using System;

[Serializable]
public class CitySaveData
{
    public int Id;
    public string Name;
    public int Hex_X;
    public int Hex_Y;
    public int Owner;
}

[Serializable]
public class VillageSaveData
{
    public int Id;
    public string Name;
    public int Hex_X;
    public int Hex_Y;
    public int Owner;
}