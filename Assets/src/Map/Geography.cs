using System.Collections.Generic;

public class Geography
{
    public static long current_id = 0;

    public string Name { get; set; }
    public long Id { get; set; }
    public List<WorldMapHex> Hexes { get; set; }
    public List<WorldMapHex> Harbors { get; set; }

    public Geography()
    {
        Id = current_id;
        current_id++;
        Harbors = new List<WorldMapHex>();
        Hexes = new List<WorldMapHex>();
    }

    public Geography(GeographySaveData data)
    {
        Id = data.Id;
        if(Id >= current_id) {
            current_id = Id + 1;
        }
        Name = data.Name;
        Harbors = new List<WorldMapHex>();
        Hexes = new List<WorldMapHex>();
    }
}

public class BodyOfWaterData : Geography
{
    public BodyOfWaterData() : base()
    {
        Name = string.Format("Body of water {0}", Id);
    }

    public BodyOfWaterData(GeographySaveData data) : base(data)
    { }
}

public class LandmassData : Geography
{
    public LandmassData() : base()
    {
        Name = string.Format("Landmass {0}", Id);
    }

    public LandmassData(GeographySaveData data) : base(data)
    { }
}
