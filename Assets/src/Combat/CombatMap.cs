using System.Collections.Generic;
using UnityEngine;

public class CombatMap {
    public static readonly string GAME_OBJECT_NAME = "CombatMap";
    public static readonly float Z_LEVEL = 0.0f;
    private static readonly float space_per_player = 0.33f;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public WorldMapHex WorldMapHex { get; private set; }
    public GameObject GameObject { get; private set; }
    public GameObject Units_GameObject { get; private set; }
    public List<CombatMapHex> Edge_Hexes { get; private set; }

    private List<List<CombatMapHex>> hexes;
    private bool visible;
    private Dictionary<int, string> seed;
    private Dictionary<int, string> city_seed;
    private int seed_max;
    private int city_seed_max;

    public CombatMap(int width, int height, WorldMapHex world_map_hex)
    {
        Width = width;
        Height = height;
        WorldMapHex = world_map_hex;

        GameObject = GameObject.Find(GAME_OBJECT_NAME);
        Units_GameObject = GameObject.Find(string.Format("{0}/Units", GAME_OBJECT_NAME));
        hexes = new List<List<CombatMapHex>>();
        Edge_Hexes = new List<CombatMapHex>();
        visible = true;

        seed_max = 0;
        city_seed_max = 0;
        seed = new Dictionary<int, string>();
        city_seed = new Dictionary<int, string>();

        foreach(KeyValuePair<string, int> hex_seed in world_map_hex.CombatMap_Seed) {
            Add_To_Seed(hex_seed.Key, hex_seed.Value);
        }
        if(world_map_hex.CombatMap_City_Seed != null && world_map_hex.CombatMap_City_Seed.Count != 0) {
            foreach (KeyValuePair<string, int> hex_seed in world_map_hex.CombatMap_City_Seed) {
                Add_To_City_Seed(hex_seed.Key, hex_seed.Value);
            }
        }

        //Generate
        for (int x = 0; x < width; x++) {
            hexes.Add(new List<CombatMapHex>());
            for (int y = 0; y < height; y++) {
                if (x + y + 1 > width / 2 && x < width * 1.5f && !((x - (height - y) + 1) > width / 2 && y > height / 2)) {
                    bool use_defenders_side = y > Space_Height_1 && city_seed_max != 0;
                    int random = RNG.Instance.Next(use_defenders_side ? city_seed_max : seed_max);
                    string prototype_name = null;
                    if (use_defenders_side) {
                        foreach (KeyValuePair<int, string> seed_pair in city_seed) {
                            if (seed_pair.Key >= random) {
                                prototype_name = seed_pair.Value;
                                break;
                            }
                        }
                    } else {
                        foreach (KeyValuePair<int, string> seed_pair in seed) {
                            if (seed_pair.Key >= random) {
                                prototype_name = seed_pair.Value;
                                break;
                            }
                        }
                    }
                    CombatMapHex hex = new CombatMapHex(x, y, GameObject, HexPrototypes.Instance.Get_Combat_Map_Hex(prototype_name), this);
                    hex.City = use_defenders_side;
                    hexes[x].Add(hex);
                } else {
                    hexes[x].Add(null);
                }
            }
        }

        //List edges
        foreach(CombatMapHex hex in All_Hexes) {
            if (hex.Is_At_Map_Edge) {
                Edge_Hexes.Add(hex);
            }
        }

        //Spread terrain
        Dictionary<string, Dictionary<string, float[]>> spread = new Dictionary<string, Dictionary<string, float[]>>();

        spread.Add("trees", new Dictionary<string, float[]>());
        spread["trees"].Add("trees", new float[2] { 10.0f, 2.0f });//Change, range
        spread["trees"].Add("scrubs", new float[2] { 1.0f, 3.0f });

        spread.Add("street", new Dictionary<string, float[]>());
        spread["street"].Add("street", new float[2] { 5.0f, 1.0f });
        spread["street"].Add("houses", new float[2] { 5.0f, 1.0f });

        spread.Add("houses", new Dictionary<string, float[]>());
        spread["houses"].Add("street", new float[2] { 5.0f, 1.0f });
        spread["houses"].Add("houses", new float[2] { 5.0f, 1.0f });

        spread.Add("cave street", new Dictionary<string, float[]>());
        spread["cave street"].Add("cave street", new float[2] { 5.0f, 1.0f });
        spread["cave street"].Add("cave houses", new float[2] { 5.0f, 1.0f });

        spread.Add("cave houses", new Dictionary<string, float[]>());
        spread["cave houses"].Add("cave street", new float[2] { 5.0f, 1.0f });
        spread["cave houses"].Add("cave houses", new float[2] { 5.0f, 1.0f });

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (hexes[x][y] != null && spread.ContainsKey(hexes[x][y].Terrain.ToLower())) {
                    foreach (KeyValuePair<string, float[]> spread_data in spread[hexes[x][y].Terrain.ToLower()]) {
                        List<CombatMapHex> affected_hexes = hexes[x][y].Get_Hexes_Around((int)spread_data.Value[1]);
                        foreach (CombatMapHex hex in affected_hexes) {
                            if (RNG.Instance.Next(100) < spread_data.Value[0] / (hex.Coordinates.Distance(hexes[x][y].Coordinates))) {
                                hex.Change_To(HexPrototypes.Instance.Get_Combat_Map_Hex(spread_data.Key));
                            }
                        }
                    }
                }
            }
        }

        //Alter terrain
        Dictionary<string, List<TerrainAlterationData>> alter = new Dictionary<string, List<TerrainAlterationData>>();

        alter.Add("grass", new List<TerrainAlterationData>());
        alter["grass"].Add(new TerrainAlterationData() {
            New_Terrain = "trees",
            Base_Chance = 1,
            Chance_Delta = new Dictionary<string, int>() {
                { "grass", 1 }
            }
        });

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (hexes[x][y] != null && alter.ContainsKey(hexes[x][y].Terrain.ToLower())) {
                    foreach (TerrainAlterationData alteration_data in alter[hexes[x][y].Terrain.ToLower()]) {
                        int chance = alteration_data.Base_Chance;
                        if (alteration_data.Chance_Delta.Count != 0) {
                            foreach (CombatMapHex adjancent_hex in hexes[x][y].Get_Adjancent_Hexes()) {
                                if (alteration_data.Chance_Delta.ContainsKey(adjancent_hex.Terrain.ToLower())) {
                                    chance += alteration_data.Chance_Delta[adjancent_hex.Terrain.ToLower()];
                                }
                            }
                        }
                        if (RNG.Instance.Next(100) < chance) {
                            hexes[x][y].Change_To(HexPrototypes.Instance.Get_Combat_Map_Hex(alteration_data.New_Terrain));
                        }
                    }
                }
            }
        }
    }

    private void Add_To_Seed(string prototype, int weight)
    {
        seed_max += weight;
        seed.Add(seed_max, prototype);
    }

    private void Add_To_City_Seed(string prototype, int weight)
    {
        city_seed_max += weight;
        city_seed.Add(city_seed_max, prototype);
    }

    /// <summary>
    /// Are map's hexes' GameObjects active?
    /// </summary>
    public bool Active
    {
        get {
            return visible;
        }
        set {
            if (visible == value) {
                return;
            }
            visible = value;
            foreach (CombatMapHex hex in All_Hexes) {
                hex.Active = value;
            }
        }
    }

    public CombatMapHex Get_Hex_At(Coordinates c)
    {
        return Get_Hex_At(c.X, c.Y);
    }

    public CombatMapHex Get_Hex_At(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height) {
            return null;
        }
        return hexes[x][y];
    }

    private List<CombatMapHex> All_Hexes
    {
        get {
            List<CombatMapHex> all_hexes = new List<CombatMapHex>();
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Width; y++) {
                    if (hexes[x][y] != null) {
                        all_hexes.Add(hexes[x][y]);
                    }
                }
            }
            return all_hexes;
        }
    }

    public CombatMapHex Random_Hex
    {
        get {
            List<CombatMapHex> all_hexes = All_Hexes;
            return all_hexes[RNG.Instance.Next(all_hexes.Count)];
        }
    }
    
    public List<PathfindingNode> PathfindingNodes
    {
        get {
            List<PathfindingNode> nodes = new List<PathfindingNode>();
            foreach (CombatMapHex hex in All_Hexes) {
                nodes.Add(hex.PathfindingNode);
            }
            return nodes;
        }
    }

    public List<PathfindingNode> Get_Specific_PathfindingNodes(Unit unit, CombatMapHex ignore_unit_hex = null)
    {
        List<PathfindingNode> nodes = new List<PathfindingNode>();
        foreach (CombatMapHex hex in All_Hexes) {
            nodes.Add(hex.Get_Specific_PathfindingNode(unit, ignore_unit_hex));
        }
        return nodes;
    }

    public void Delete()
    {
        foreach (CombatMapHex hex in All_Hexes) {
            hex.Delete();
        }
    }

    private float Space_Height_1
    {
        get {
            return Height - (Height * space_per_player);
        }
    }

    private float Space_Height_2
    {
        get {
            return Height * space_per_player;
        }
    }

    public void Set_Deployment_Mode(Army army)
    {
        foreach (CombatMapHex hex in All_Hexes) {
            if(army == null) {
                hex.Hidden = false;
            } else if(army == CombatManager.Instance.Army_1) {
                hex.Hidden = hex.R > Space_Height_2;
            } else if(army == CombatManager.Instance.Army_2) {
                hex.Hidden = hex.R < Space_Height_1;
            }
        }
    }

    public CombatMapHex Center_Of_Deployment_1
    {
        get {
            return Get_Hex_At(Mathf.RoundToInt((Width / 2.0f) * 1.33f) , Mathf.RoundToInt(Height * space_per_player * 0.5f));
        }
    }

    public CombatMapHex Center_Of_Deployment_2
    {
        get {
            return Get_Hex_At(Mathf.RoundToInt((Width / 2.0f) * 0.67f), Mathf.RoundToInt(Height - (Height * space_per_player * 0.5f)));
        }
    }

    /// <summary>
    /// TODO: Duplicated code with world map
    /// </summary>
    /// <param name="hex_1"></param>
    /// <param name="hex_2"></param>
    /// <returns></returns>
    public List<CombatMapHex> Straight_Line(CombatMapHex hex_1, CombatMapHex hex_2)
    {
        List<CombatMapHex> line = new List<CombatMapHex>();
        line.Add(hex_1);
        while (!line.Contains(hex_2)) {
            Map.Direction? closest_direction = null;
            float closest_distance = -1;
            Dictionary<Map.Direction, Coordinates> adjancent_coordinates = line[line.Count - 1].Coordinates.Get_Adjanced_Coordinates();
            foreach (KeyValuePair<Map.Direction, Coordinates> coordinate_data in adjancent_coordinates) {
                CombatMapHex h = Get_Hex_At(coordinate_data.Value);
                if (h == null) {
                    continue;
                }
                float dist = h.PathfindingNode.GameObject_Distance(hex_2.PathfindingNode.GameObject_X, hex_2.PathfindingNode.GameObject_Y);
                if (closest_direction == null || (dist < closest_distance)) {
                    closest_direction = coordinate_data.Key;
                    closest_distance = dist;
                }
            }
            line.Add(Get_Hex_At(adjancent_coordinates[closest_direction.Value]));
        }
        return line;
    }

    private class TerrainAlterationData
    {
        public TerrainAlterationData()
        {
            Chance_Delta = new Dictionary<string, int>();
        }
        public string New_Terrain { get; set; }
        public int Base_Chance { get; set; }
        public Dictionary<string, int> Chance_Delta { get; set; }
    }
}
