using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class Map
{
    public static readonly string GAME_OBJECT_NAME = "Map";
    public static readonly float Z_LEVEL = 0.0f;

    public enum Direction { North_East, East, South_East, South_West, West, North_West }
    public enum MovementType { Land, Water, Amphibious }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float Mineral_Spawn_Rate { get; private set; }
    public GameObject GameObject { get; private set; }
    /// <summary>
    /// TODO: Might be buggy
    /// </summary>
    public bool Reveal_All { get; set; }
    public List<City> Cities { get; private set; }
    public List<Village> Villages { get; private set; }
    
    private List<List<WorldMapHex>> hexes;
    private bool visible;
    private Dictionary<int, string> seed;
    private int seed_max;
    private List<WorldMapHex> edge_hexes;
    private WorldMapEntity dummy_boat;

    public Map(int width, int height, float mineral_spawn_rate)
    {
        CustomLogger.Instance.Debug("Generating Map");
        Stopwatch stopwatch_total = Stopwatch.StartNew();

        Width = width;
        Height = height;
        Mineral_Spawn_Rate = mineral_spawn_rate;
        GameObject = GameObject.Find(GAME_OBJECT_NAME);

        Cities = new List<City>();
        Villages = new List<Village>();

        hexes = new List<List<WorldMapHex>>();
        edge_hexes = new List<WorldMapHex>();
        visible = true;
        dummy_boat = new Worker("Dummy Boat", 3.0f, MovementType.Water, 2, "ship", new List<string>() { "peasant_working_1", "peasant_working_2" }, 3.0f, new List<Improvement>(),
            1.0f, 50, 100, 1.0f, null);

        //Generate
        Stopwatch stopwatch = Stopwatch.StartNew();
        for (int x = 0; x < width; x++) {
            hexes.Add(new List<WorldMapHex>());
            for (int y = 0; y < height; y++) {
                if(x + y + 1 > width / 2 && x < width * 1.5f && !((x - (height - y) + 1) > width / 2 && y > height / 2)) {
                    int random = RNG.Instance.Next(seed_max);
                    string prototype_name = null;
                    foreach (KeyValuePair<int, string> seed_pair in Seed) {
                        if (seed_pair.Key >= random) {
                            prototype_name = seed_pair.Value;
                            break;
                        }
                    }
                    hexes[x].Add(new WorldMapHex(x, y, GameObject, HexPrototypes.Instance.Get_World_Map_Hex(prototype_name), this));
                } else {
                    hexes[x].Add(null);
                }
            }

        }
        CustomLogger.Instance.Debug(string.Format("Map seeded in: {0} ms", stopwatch.ElapsedMilliseconds));
        stopwatch = Stopwatch.StartNew();

        //Spread terrain
        Dictionary<string, Dictionary<string, float[]>> spread = new Dictionary<string, Dictionary<string, float[]>>();

        spread.Add("forest", new Dictionary<string, float[]>());
        spread["forest"].Add("forest", new float[2] { 13.0f, 2.0f });//Change, range

        spread.Add("plains", new Dictionary<string, float[]>());
        spread["plains"].Add("plains", new float[2] { 3.0f, 3.0f });

        spread.Add("hill", new Dictionary<string, float[]>());
        spread["hill"].Add("hill", new float[2] { 10.0f, 2.0f });

        spread.Add("mountain", new Dictionary<string, float[]>());
        spread["mountain"].Add("mountain", new float[2] { 5.0f, 1.0f });
        spread["mountain"].Add("hill", new float[2] { 10.0f, 3.0f });

        spread.Add("forest hill", new Dictionary<string, float[]>());
        spread["forest hill"].Add("forest", new float[2] { 10.0f, 2.0f });
        spread["forest hill"].Add("hill", new float[2] { 10.0f, 2.0f });

        spread.Add("flower field", new Dictionary<string, float[]>());
        spread["flower field"].Add("flower field", new float[2] { 2.0f, 1.0f });

        spread.Add("enchanted forest", new Dictionary<string, float[]>());
        spread["enchanted forest"].Add("enchanted forest", new float[2] { 2.0f, 1.0f });

        spread.Add("haunted forest", new Dictionary<string, float[]>());
        spread["haunted forest"].Add("haunted forest", new float[2] { 2.0f, 1.0f });

        spread.Add("mushroom forest", new Dictionary<string, float[]>());
        spread["mushroom forest"].Add("mushroom forest", new float[2] { 2.0f, 1.0f });

        spread.Add("water", new Dictionary<string, float[]>());
        spread["water"].Add("water", new float[2] { 45.0f, 1.0f });

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (hexes[x][y] != null && spread.ContainsKey(hexes[x][y].Terrain.ToLower())) {
                    foreach (KeyValuePair<string, float[]> spread_data in spread[hexes[x][y].Terrain.ToLower()]) {
                        List<WorldMapHex> affected_hexes = hexes[x][y].Get_Hexes_Around((int)spread_data.Value[1]);
                        foreach (WorldMapHex hex in affected_hexes) {
                            if (RNG.Instance.Next(100) < spread_data.Value[0] / (hex.Coordinates.Distance(hexes[x][y].Coordinates))) {
                                hex.Change_To(HexPrototypes.Instance.Get_World_Map_Hex(spread_data.Key));
                            }
                        }
                    }
                }
            }
        }
        CustomLogger.Instance.Debug(string.Format("Terrain spread in: {0} ms", stopwatch.ElapsedMilliseconds));
        stopwatch = Stopwatch.StartNew();

        //List edges
        foreach (WorldMapHex hex in All_Hexes) {
            if (hex.Is_At_Map_Edge(this)) {
                edge_hexes.Add(hex);
            }
        }
        CustomLogger.Instance.Debug(string.Format("Edges listed in: {0} ms", stopwatch.ElapsedMilliseconds));
        stopwatch = Stopwatch.StartNew();

        //Alter terrain
        Dictionary<string, List<TerrainAlterationData>> alter = new Dictionary<string, List<TerrainAlterationData>>();

        alter.Add("grassland", new List<TerrainAlterationData>());
        alter["grassland"].Add(new TerrainAlterationData() {
            New_Terrain = "plains",
            Base_Chance = 0,
            Chance_Delta = new Dictionary<string, int>() {
                { "hill", 5 },
                { "mountain", 10 },
                { "flower field", -10 },
                { "swamp", -25 },
                { "water", -35 }
            }
        });
        alter["grassland"].Add(new TerrainAlterationData() {
            New_Terrain = "swamp",
            Base_Chance = 0,
            Chance_Delta = new Dictionary<string, int>() {
                { "swamp", 3 },
                { "water", 5 }
            }
        });

        alter.Add("mountain", new List<TerrainAlterationData>());
        alter["mountain"].Add(new TerrainAlterationData() {
            New_Terrain = "volcano",
            Base_Chance = 5
        });

        alter.Add("hill", new List<TerrainAlterationData>());
        alter["hill"].Add(new TerrainAlterationData() {
            New_Terrain = "forest hill",
            Base_Chance = 0,
            Chance_Delta = new Dictionary<string, int>() {
                { "forest", 10 },
                { "forest hill", 10 }
            }
        });

        alter.Add("forest", new List<TerrainAlterationData>());
        alter["forest"].Add(new TerrainAlterationData() {
            New_Terrain = "forest hill",
            Base_Chance = 0,
            Chance_Delta = new Dictionary<string, int>() {
                { "hill", 10 },
                { "forest hill", 10 },
                { "mountain", 25 }
            }
        });
        alter["forest"].Add(new TerrainAlterationData() {
            New_Terrain = "enchanted forest",
            Base_Chance = 1,
            Chance_Delta = new Dictionary<string, int>() {
                { "enchanted forest", 20 },
                { "flower field", 5 },
                { "haunted forest", -100 }
            }
        });
        alter["forest"].Add(new TerrainAlterationData() {
            New_Terrain = "haunted forest",
            Base_Chance = 1,
            Chance_Delta = new Dictionary<string, int>() {
                { "haunted forest", 20 },
                { "flower field", -10 },
                { "enchanted forest", -100 }
            }
        });
        alter["forest"].Add(new TerrainAlterationData() {
            New_Terrain = "mushroom forest",
            Base_Chance = 1,
            Chance_Delta = new Dictionary<string, int>() {
                { "mushroom forest", 20 }
            }
        });

        alter.Add("plains", new List<TerrainAlterationData>());
        alter["plains"].Add(new TerrainAlterationData() {
            New_Terrain = "grassland",
            Base_Chance = 0,
            Chance_Delta = new Dictionary<string, int>() {
                { "flower field", 10 },
                { "swamp", 35 },
                { "water", 50 }
            }
        });

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (hexes[x][y] != null && alter.ContainsKey(hexes[x][y].Terrain.ToLower())) {
                    foreach (TerrainAlterationData alteration_data in alter[hexes[x][y].Terrain.ToLower()]) {
                        int chance = alteration_data.Base_Chance;
                        if(alteration_data.Chance_Delta.Count != 0) {
                            foreach (WorldMapHex adjancent_hex in hexes[x][y].Get_Adjancent_Hexes()) {
                                if (alteration_data.Chance_Delta.ContainsKey(adjancent_hex.Terrain.ToLower())) {
                                    chance += alteration_data.Chance_Delta[adjancent_hex.Terrain.ToLower()];
                                }
                            }
                        }
                        if(RNG.Instance.Next(100) < chance) {
                            hexes[x][y].Change_To(HexPrototypes.Instance.Get_World_Map_Hex(alteration_data.New_Terrain));
                        }
                    }
                }
            }
        }
        CustomLogger.Instance.Debug(string.Format("Terrain altered in: {0} ms", stopwatch.ElapsedMilliseconds));
        stopwatch = Stopwatch.StartNew();

        //Spawn minerals
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (hexes[x][y] != null && RNG.Instance.Next(100) < (int)(Mineral_Spawn_Rate * 100)) {
                    hexes[x][y].Spawn_Mineral();
                }
            }
        }
        CustomLogger.Instance.Debug(string.Format("Minerals spawned in: {0} ms", stopwatch.ElapsedMilliseconds));
        CustomLogger.Instance.Debug(string.Format("Map generated in: {0} ms", stopwatch_total.ElapsedMilliseconds));

        //Map_Mode = WorldMapHex.InfoText.Coordinates;
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

    private Dictionary<int, string> Seed
    {
        get {
            if(seed != null) {
                return seed;
            }
            seed_max = 0;
            seed = new Dictionary<int, string>();

            Add_To_Seed("grassland", 1000);
            Add_To_Seed("forest", 75);
            Add_To_Seed("plains", 20);
            Add_To_Seed("flower field", 10);
            Add_To_Seed("enchanted forest", 2);
            Add_To_Seed("forest hill", 5);
            Add_To_Seed("grave yard", 3);
            Add_To_Seed("haunted forest", 2);
            Add_To_Seed("hill", 10);
            Add_To_Seed("mountain", 10);
            Add_To_Seed("mushroom forest", 3);
            Add_To_Seed("city ruins", 3);
            //Add_To_Seed("volcano", 0);
            Add_To_Seed("water", 40);//Orig 4
            //Add_To_Seed("swamp", 0);

            return seed;
        }
    }

    private void Add_To_Seed(string prototype, int weight)
    {
        seed_max += weight;
        seed.Add(seed_max, prototype);
    }

    public void Spawn_Cities_Villages_And_Roads(int neutral_cities, int max_villages)
    {
        CustomLogger.Instance.Debug("Spawing cities, villages and roads");
        Stopwatch stopwatch = Stopwatch.StartNew();
        Stopwatch stopwatch_total = Stopwatch.StartNew();

        List<WorldMapHex> all_hexes = All_Hexes;
        float no_cities_radius_center_min = 0.20f * ((Width + Height) / 2.0f);
        float no_cities_radius_center_max = 0.40f * ((Width + Height) / 2.0f);
        float no_cities_radius_player = 0.35f * ((Width + Height) / 2.0f);
        float no_cities_radius_neutral = 0.25f * ((Width + Height) / 2.0f);
        float no_cities_radius_village = 0.15f * ((Width + Height) / 2.0f);
        WorldMapHex center_hex = hexes[Width / 2][Height / 2];
        int iterations = 0;
        int max_iterations = 10000;
        bool ignore_distances = false;
        //Player capitals
        foreach (Player player in Main.Instance.Players) {
            iterations = 0;
            ignore_distances = false;
            while (true) {
                WorldMapHex random_hex = all_hexes[RNG.Instance.Next(all_hexes.Count)];
                if (!random_hex.Passable || random_hex.Has_Owner) {
                    continue;
                }
                bool too_close = false;
                foreach (Player possibly_player_settled in Main.Instance.Players) {
                    if (possibly_player_settled.Capital == null) {
                        continue;
                    }
                    if (possibly_player_settled.Capital.Hex.Distance(random_hex) <= no_cities_radius_player) {
                        too_close = true;
                        break;
                    }
                }
                if(random_hex.Distance(center_hex) <= no_cities_radius_center_min || random_hex.Distance(center_hex) >= no_cities_radius_center_max) {
                    too_close = true;
                }
                if (!too_close || ignore_distances) {
                    City capital = new City(random_hex, player, HexPrototypes.Instance.Get_World_Map_Hex("water"));
                    player.Capital = capital;
                    random_hex.Change_To(HexPrototypes.Instance.Get_World_Map_Hex("city"));
                    capital.Update_Hex_Yields();
                    random_hex.Owner = player;
                    Cities.Add(capital);
                    break;
                }
                iterations++;
                if(iterations > max_iterations) {
                    CustomLogger.Instance.Warning("Failed to find a good city (P#" + player.Id + ") spot after " + max_iterations + " iterations, ignoring distance to other cities and center");
                    ignore_distances = true;
                }
            }
        }
        CustomLogger.Instance.Debug(string.Format("Capitals spawned in: {0} ms", stopwatch.ElapsedMilliseconds));
        stopwatch = Stopwatch.StartNew();

        //Neutral cities
        bool failed_to_spawn = false;
        for(int i = 0; i < neutral_cities; i++) {
            iterations = 0;
            while (true) {
                WorldMapHex random_hex = all_hexes[RNG.Instance.Next(all_hexes.Count)];
                if(!random_hex.Passable || random_hex.Has_Owner) {
                    continue;
                }
                bool too_close = false;
                //Check distance to players
                foreach (Player possibly_player_settled in Main.Instance.Players) {
                    if (possibly_player_settled.Capital.Hex.Distance(random_hex) <= no_cities_radius_player) {
                        too_close = true;
                        break;
                    }
                }
                //Check distance to already spawned neutral cities
                if (!too_close) {
                    foreach (City already_spawned_city in Main.Instance.Neutral_Player.Cities) {
                        if (already_spawned_city.Hex.Distance(random_hex) <= no_cities_radius_neutral) {
                            too_close = true;
                            break;
                        }
                    }
                }
                if (!too_close) {
                    City city = new City(random_hex, Main.Instance.Neutral_Player, HexPrototypes.Instance.Get_World_Map_Hex("water"));
                    Main.Instance.Neutral_Player.Cities.Add(city);
                    random_hex.Change_To(HexPrototypes.Instance.Get_World_Map_Hex("small_city"));
                    city.Update_Hex_Yields();
                    random_hex.Owner = Main.Instance.Neutral_Player;
                    int defenders = RNG.Instance.Next(2, 5);
                    Army garrison = new Army(random_hex, Main.Instance.Neutral_Player.Faction.Army_Prototype, Main.Instance.Neutral_Player, null);
                    for(int j = 0; j < defenders; j++) {
                        garrison.Add_Unit(new Unit(Main.Instance.Neutral_Player.Faction.Units.First(x => x.Name == "Town Guard") as Unit));
                    }
                    Cities.Add(city);
                    break;
                }
                iterations++;
                if (iterations > max_iterations) {
                    CustomLogger.Instance.Warning("Failed to spawn neutral cities after " + max_iterations + " iterations (" + i + "/" + neutral_cities + " spawned)");
                    failed_to_spawn = true;
                    break;
                }
            }
            if (failed_to_spawn) {
                break;
            }
        }
        CustomLogger.Instance.Debug(string.Format("Neutral cities spawned in: {0} ms", stopwatch.ElapsedMilliseconds));
        stopwatch = Stopwatch.StartNew();

        //Villages
        int village_iterations = 1000 + (max_villages * 100);
        for(int i = 0; i < village_iterations && Villages.Count < max_villages; i++) {
            WorldMapHex random_hex = all_hexes[RNG.Instance.Next(all_hexes.Count)];
            if(!random_hex.Passable || random_hex.Has_Owner) {
                continue;
            }
            bool too_close = false;
            //Check distance to players
            foreach (Player possibly_player_settled in Main.Instance.Players) {
                if (possibly_player_settled.Capital.Hex.Distance(random_hex) <= no_cities_radius_village) {
                    too_close = true;
                    break;
                }
            }
            //Check distance to neutral cities
            if (!too_close) {
                foreach (City already_spawned_city in Main.Instance.Neutral_Player.Cities) {
                    if (already_spawned_city.Hex.Distance(random_hex) <= no_cities_radius_village) {
                        too_close = true;
                        break;
                    }
                }
            }
            //Check distance to already spawned villages
            if (!too_close) {
                foreach (Village already_spawned_village in Main.Instance.Neutral_Player.Villages) {
                    if (already_spawned_village.Hex.Distance(random_hex) <= no_cities_radius_village) {
                        too_close = true;
                        break;
                    }
                }
            }
            //Check adjancent hexes
            if (!too_close) {
                foreach(WorldMapHex hex in random_hex.Get_Adjancent_Hexes()) {
                    if(hex.Worked_By_Village != null || hex.In_Work_Range_Of.Count != 0) {
                        too_close = true;
                        break;
                    }
                }
            }
            if (too_close) {
                continue;
            }
            Village village = new Village(random_hex, Main.Instance.Neutral_Player);
            random_hex.Change_To(HexPrototypes.Instance.Get_World_Map_Hex("village"));
            Main.Instance.Neutral_Player.Villages.Add(village);
            Villages.Add(village);
        }
        CustomLogger.Instance.Debug(string.Format("Villages spawned in: {0} ms", stopwatch.ElapsedMilliseconds));
        stopwatch = Stopwatch.StartNew();

        //Roads
        float max_road_distance_multiplier = 1.0f;
        List<WorldMapHex> road_hexes = new List<WorldMapHex>();
        List<WorldMapHex> edge_connections = new List<WorldMapHex>();
        int edge_connection_count = RNG.Instance.Next(3 * ((Width + Height) / 80), 6 * ((Width + Height) / 40));
        while (edge_connections.Count < edge_connection_count) {
            WorldMapHex connection = edge_hexes[RNG.Instance.Next(0, edge_hexes.Count - 1)];
            if (edge_connections.Contains(connection)) {
                continue;
            }
            connection.Is_Map_Edge_Road_Connection = true;
            edge_connections.Add(connection);
        }
        foreach (City city in Cities) {
            Spawn_Roads(city.Hex, Cities.Select(x => x.Hex).ToList(), 10.0f, 20.0f * max_road_distance_multiplier, 75.0f, 0.075f, 0.05f, HexPrototypes.Instance.Get_Road("gravel road"), ref road_hexes);
            Spawn_Roads(city.Hex, Villages.Where(x => !x.Connected_With_Road).Select(x => x.Hex).ToList(), 10.0f, 15.0f * max_road_distance_multiplier, 25.0f, 0.25f, 0.025f, HexPrototypes.Instance.Get_Road("gravel road"), ref road_hexes);
            Spawn_Roads(city.Hex, Villages.Where(x => x.Connected_With_Road).Select(x => x.Hex).ToList(), 5.0f, 7.0f * max_road_distance_multiplier, 5.0f, 0.35f, 0.025f, HexPrototypes.Instance.Get_Road("gravel road"), ref road_hexes);
            Spawn_Roads(city.Hex, edge_connections, 7.0f, 15.0f * max_road_distance_multiplier, 75.0f, 0.075f, 0.05f, HexPrototypes.Instance.Get_Road("gravel road"), ref road_hexes);
        }
        foreach(Village village in Villages) {
            Spawn_Roads(village.Hex, Villages.Select(x => x.Hex).ToList(), 7.0f, 10.0f * max_road_distance_multiplier, 5.0f, 0.25f, 0.025f, HexPrototypes.Instance.Get_Road("gravel road"), ref road_hexes);
            Spawn_Roads(village.Hex, edge_connections, 5.0f, 10.0f * max_road_distance_multiplier, 75.0f, 0.075f, 0.05f, HexPrototypes.Instance.Get_Road("gravel road"), ref road_hexes);
        }
        foreach (WorldMapHex road_hex in road_hexes) {
            road_hex.Road.Update_Graphics();
        }
        CustomLogger.Instance.Debug(string.Format("Roads spawned in: {0} ms", stopwatch.ElapsedMilliseconds));
        stopwatch = Stopwatch.StartNew();

        //Water trade routes
        List<TradePartner> partners = Cities.Where(x => x.Is_Coastal).Select(x => x as TradePartner).Union(
            Villages.Where(x => x.Is_Coastal).Select(x => x as TradePartner)).ToList();
        foreach (City city in Cities) {
            if (!city.Is_Coastal) {
                continue;
            }
            foreach(TradePartner partner in partners) {
                List<WorldMapHex> path = Path(city.Hex, partner.Hex, dummy_boat, false);
                if(path.Count == 0) {
                    continue;
                }
                city.Add_Trade_Route(new TradeRoute(path, city, partner, true));
            }
        }

        CustomLogger.Instance.Debug(string.Format("Water trade routes spawned in: {0} ms", stopwatch.ElapsedMilliseconds));
        CustomLogger.Instance.Debug(string.Format("Cities, villages and roads spawned in: {0} ms", stopwatch_total.ElapsedMilliseconds));
    }

    private void Spawn_Roads(WorldMapHex hex, List<WorldMapHex> possible_connetions, float optimal_distance, float max_direct_distance, float base_change, float change_decay_per_hex,
        float change_increase_per_hex, Road road_prototype, ref List<WorldMapHex> road_hexes)
    {
        Dictionary<WorldMapHex, float> distances = new Dictionary<WorldMapHex, float>();
        Dictionary<WorldMapHex, List<WorldMapHex>> paths = new Dictionary<WorldMapHex, List<WorldMapHex>>();

        foreach (WorldMapHex connection_hex in possible_connetions) {
            if(hex.Coordinates.Distance(connection_hex.Coordinates) > max_direct_distance) {
                continue;
            }
            List<WorldMapHex> path = Path(hex, connection_hex, null, false, false, true);
            if (path.Count == 0) {
                continue;
            }
            distances.Add(connection_hex, path.Count);//TODO: Use movement point usage for distance?
            paths.Add(connection_hex, path);
        }
        if (distances.Count == 0) {
            return;
        }
        foreach (KeyValuePair<WorldMapHex, float> distance in distances) {
            float change = base_change;
            if (distance.Value < optimal_distance) {
                change = (100.0f - ((100.0f - change) * Mathf.Pow(1.0f - change_increase_per_hex, optimal_distance - distance.Value)));
            } else if (distance.Value > optimal_distance) {
                change *= Mathf.Pow(1.0f - change_decay_per_hex, distance.Value - optimal_distance);
            }
            if (RNG.Instance.Next(0, 100) <= Mathf.RoundToInt(change)) {
                //Spawn road
                foreach (WorldMapHex path_hex in paths[distance.Key]) {
                    if (path_hex.Road != null || path_hex.City != null || path_hex.Village != null) {
                        continue;
                    }
                    path_hex.Road = new Road(path_hex, road_prototype);
                    road_hexes.Add(path_hex);
                }
                if(hex.City != null && distance.Key.Trade_Partner != null) {
                    //Create trade route
                    hex.City.Add_Trade_Route(new TradeRoute(paths[distance.Key], hex.City, distance.Key.Trade_Partner, false));
                }
                if(hex.Trade_Partner != null && distance.Key.City != null) {
                    //Create trade route
                    List<WorldMapHex> reverse_path = new List<WorldMapHex>();
                    for(int i = paths[distance.Key].Count - 1; i >= 0; i--) {
                        reverse_path.Add(paths[distance.Key][i]);
                    }
                    distance.Key.City.Add_Trade_Route(new TradeRoute(reverse_path, distance.Key.City, hex.Trade_Partner, false));
                }

                if (distance.Key.City == null && distance.Key.Village == null && distance.Key.Road == null) {
                    //Map edge connection
                    distance.Key.Road = new Road(distance.Key, road_prototype);
                    road_hexes.Add(distance.Key);
                }
            }
        }
    }

    private List<WorldMapHex> All_Hexes
    {
        get {
            List<WorldMapHex> all_hexes = new List<WorldMapHex>();
            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Width; y++) {
                    if(hexes[x][y] != null) {
                        all_hexes.Add(hexes[x][y]);
                    }
                }
            }
            return all_hexes;
        }
    }

    public WorldMapHex Random_Hex
    {
        get {
            return All_Hexes[RNG.Instance.Next(All_Hexes.Count)];
        }
    }

    public WorldMapHex Get_Hex_At(Coordinates c)
    {
        return Get_Hex_At(c.X, c.Y);
    }

    public WorldMapHex Get_Hex_At(int x, int y)
    {
        if(x < 0 || y < 0 || x >= Width || y >= Height) {
            return null;
        }
        return hexes[x][y];
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
            foreach (WorldMapHex hex in All_Hexes) {
                hex.Active = value;
            }
        }
    }
    
    public List<PathfindingNode> Get_Specific_PathfindingNodes(WorldMapEntity entity, bool use_los = true, WorldMapHex ignore_entity_hex = null)
    {
        List<PathfindingNode> nodes = new List<PathfindingNode>();
        foreach (WorldMapHex hex in All_Hexes) {
            nodes.Add(hex.Get_Specific_PathfindingNode(entity, ignore_entity_hex, use_los));
        }
        return nodes;
    }

    public List<PathfindingNode> Get_PathfindingNodes(bool road_spawning = false, bool water = false)
    {
        List<PathfindingNode> nodes = new List<PathfindingNode>();
        foreach (WorldMapHex hex in All_Hexes) {
            nodes.Add(water ? hex.Water_PathfindingNode : hex.PathfindingNode);
            if(road_spawning && hex.Road != null) {
                nodes[nodes.Count - 1].Cost *= 0.5f;
            }
        }
        return nodes;
    }

    /// <summary>
    /// Deletes all tile GameObjects
    /// </summary>
    public void Delete()
    {
        foreach (WorldMapHex hex in All_Hexes) {
            hex.Delete();
        }
    }

    public void Clear_Highlights()
    {
        foreach (WorldMapHex hex in All_Hexes) {
            hex.Clear_Highlight();
        }
    }
    
    public WorldMapHex.InfoText Map_Mode
    {
        get {
            return hexes[Width / 2][Height / 2].Current_Text;
        }
        set {
            foreach(WorldMapHex hex in All_Hexes) {
                hex.Current_Text = value;
            }
        }
    }

    public void Start_Game()
    {
        foreach (City city in Cities) {
            city.Start_Game();
        }
        Update_LoS();
    }

    public void Update_LoS(WorldMapEntity entity)
    {
        foreach (WorldMapHex hex in entity.Last_Hexes_In_Los) {
            if(!hex.In_LoS_Of_City && hex.In_LoS_Of.Count == 1) {
                hex.Not_In_LoS(Main.Instance.Current_Player);
            }
        }
        entity.Last_Hexes_In_Los.Clear();
        foreach (WorldMapHex hex in entity.Get_Hexes_In_LoS()) {
            hex.In_LoS(Main.Instance.Current_Player, entity);
            if(hex.City != null) {
                hex.City.Flag.Update_Type();
            }
            if (hex.Village != null) {
                hex.Village.Flag.Update_Type();
            }
            if (hex.Entity != null) {
                hex.Entity.Flag.Update_Type();
            }
            if (hex.Civilian != null) {
                hex.Civilian.Flag.Update_Type();
            }
        }
    }

    /// <summary>
    /// Also updates flags
    /// </summary>
    public void Update_LoS()
    {
        foreach (WorldMapHex hex in All_Hexes) {
            hex.Not_In_LoS(Main.Instance.Current_Player);
        }
        foreach(KeyValuePair<WorldMapHex, WorldMapEntity> los_and_source in Main.Instance.Current_Player.LoS) {
            los_and_source.Key.In_LoS(Main.Instance.Current_Player, los_and_source.Value);
        }
        /*foreach(WorldMapHex hex in Main.Instance.Current_Player.Capital.Get_Hexes_In_LoS()) {
            hex.In_LoS(Main.Instance.Current_Player, null);
        }
        foreach(WorldMapEntity entity in Main.Instance.Current_Player.WorldMapEntitys) {
            foreach (WorldMapHex hex in entity.Get_Hexes_In_LoS()) {
                hex.In_LoS(Main.Instance.Current_Player, entity);
            }
        }*/
    }

    public List<WorldMapHex> Straight_Line(WorldMapHex hex_1, WorldMapHex hex_2)
    {
        List<WorldMapHex> line = new List<WorldMapHex>();
        line.Add(hex_1);
        while (!line.Contains(hex_2)) {
            Direction? closest_direction = null;
            float closest_distance = -1;
            Dictionary<Direction, Coordinates> adjancent_coordinates = line[line.Count - 1].Coordinates.Get_Adjanced_Coordinates();
            foreach (KeyValuePair<Direction, Coordinates> coordinate_data in adjancent_coordinates) {
                WorldMapHex h = Get_Hex_At(coordinate_data.Value);
                if(h == null) {
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

    public List<WorldMapHex> Path(WorldMapHex hex_1, WorldMapHex hex_2, WorldMapEntity entity, bool use_los = true, bool include_start_and_end = false,
        bool road_spawning = false, bool water = false)
    {
        List<PathfindingNode> node_path = entity != null ?
            Pathfinding.Path(Get_Specific_PathfindingNodes(entity, use_los), hex_1.Get_Specific_PathfindingNode(entity, null, use_los), hex_2.Get_Specific_PathfindingNode(entity, null, use_los)) :
            (water ? Pathfinding.Path(Get_PathfindingNodes(road_spawning, true), hex_1.Water_PathfindingNode, hex_2.Water_PathfindingNode) :
            Pathfinding.Path(Get_PathfindingNodes(road_spawning), hex_1.PathfindingNode, hex_2.PathfindingNode));
        if(node_path.Count == 0) {
            return new List<WorldMapHex>();
        }
        if (!include_start_and_end) {
            node_path.RemoveAt(node_path.Count - 1);
            node_path.RemoveAt(0);
        }
        List<WorldMapHex> path = new List<WorldMapHex>();
        for(int i = 0; i < node_path.Count; i++) {
            path.Add(Get_Hex_At(node_path[i].Coordinates));
        }
        return path;
    }

    public void Update(float delta_s)
    {
        for (int x = 0; x < Width; x++) {
            for (int y = 0; y < Width; y++) {
                if (hexes[x][y] != null) {
                    if(hexes[x][y].Entity != null) {
                        hexes[x][y].Entity.Update(delta_s);
                    }
                    if (hexes[x][y].Civilian != null) {
                        hexes[x][y].Civilian.Update(delta_s);
                    }
                }
            }
        }
    }
}
