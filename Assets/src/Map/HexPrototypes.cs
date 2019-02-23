using System.Collections.Generic;

public class HexPrototypes {
    private static HexPrototypes instance;
    private Dictionary<string, WorldMapHex> world_map_hex_prototypes;
    private Dictionary<string, CombatMapHex> combat_map_hex_prototypes;

    private HexPrototypes()
    {
        world_map_hex_prototypes = new Dictionary<string, WorldMapHex>();

        world_map_hex_prototypes.Add("grassland", new WorldMapHex("Grassland", "hex_grass", new Yields(2, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }));
        world_map_hex_prototypes.Add("plains", new WorldMapHex("Plains", "hex_plain", new Yields(1, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }));
        world_map_hex_prototypes.Add("hill", new WorldMapHex("Hill", "hex_hill", new Yields(1, 1, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 3.0f, 1, 1, true, new List<WorldMapHex.Tag>()));
        world_map_hex_prototypes.Add("mountain", new WorldMapHex("Mountain", "hex_mountain", new Yields(0, 2, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 5.0f, 3, 3, true, new List<WorldMapHex.Tag>()));
        world_map_hex_prototypes.Add("volcano", new WorldMapHex("Volcano", "hex_volcano", new Yields(0, 2, 0, 1, 0, 0, 0), 0.0f, 0.0f, 0.0f, 5.0f, 3, 3, true, new List<WorldMapHex.Tag>()));
        world_map_hex_prototypes.Add("swamp", new WorldMapHex("Swamp", "hex_swamp", new Yields(2, 0, 0, 1, 0, 0, 0), 0.0f, -1.0f, 0.0f, 3.0f, 0, 1, false, new List<WorldMapHex.Tag>()));
        world_map_hex_prototypes.Add("flower field", new WorldMapHex("Flower Field", "hex_flowers", new Yields(2, 0, 0, 0, 1, 0, 0), 1.0f, 0.0f, 0.0f, 1.0f, 0, 0, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }));
        world_map_hex_prototypes.Add("forest", new WorldMapHex("Forest", "hex_forest", new Yields(2, 0.5f, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, 2.0f, 0, 1, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest }));
        world_map_hex_prototypes.Add("forest hill", new WorldMapHex("Forest Hill", "hex_forest_hill", new Yields(1.5f, 1.5f, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, 3.0f, 1, 2, true, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest }));
        world_map_hex_prototypes.Add("mushroom forest", new WorldMapHex("Mushroom Forest", "hex_mushrooms", new Yields(2.5f, 0.5f, 0, 1, 0, 1, 0), 0.0f, 0.0f, 0.0f, 2.0f, 0, 1, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest }));
        world_map_hex_prototypes.Add("enchanted forest", new WorldMapHex("Enchanted Forest", "hex_enc_forest", new Yields(2, 0.5f, 0, 0, 0, 2, 0), 1.0f, 0.5f, 0.0f, 2.0f, 0, 1, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest }));
        world_map_hex_prototypes.Add("haunted forest", new WorldMapHex("Haunted Forest", "hex_haunted_forest", new Yields(0, 1, 0, 0, 0, 1, 0), -1.0f, 0.0f, 0.0f, 2.0f, 0, 1, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest }));
        world_map_hex_prototypes.Add("water", new WorldMapHex("Water", "hex_water", new Yields(3, 0, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, -1.0f, 0, -1, false, new List<WorldMapHex.Tag>()));
        world_map_hex_prototypes.Add("city ruins", new WorldMapHex("City Ruins", "hex_ruins", new Yields(1, 0, 0, 2, 1, 1, 1), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false, new List<WorldMapHex.Tag>()));
        world_map_hex_prototypes.Add("grave yard", new WorldMapHex("Grave Yard", "hex_grave_yard", new Yields(0, 0, 0, 0, 0, 1, 0), -1.0f, 0.0f, 0.0f, 1.0f, 0, 0, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }));

        world_map_hex_prototypes.Add("small_city", new WorldMapHex("Small City", "city_small", new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban }));
        world_map_hex_prototypes.Add("city", new WorldMapHex("City", "hex_castle", new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban }));
        world_map_hex_prototypes.Add("village", new WorldMapHex("Village", "hex_village", new Yields(1, 1, 1, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban }));


        combat_map_hex_prototypes = new Dictionary<string, CombatMapHex>();
        combat_map_hex_prototypes.Add("grass", new CombatMapHex("Grass", "cm_hex_grass", 1.0f, 0.0f, 0, 0, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open }));
        combat_map_hex_prototypes.Add("scrubs", new CombatMapHex("Scrubs", "cm_hex_scrubs", 1.0f, 0.0f, 0, 0, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open }));
        combat_map_hex_prototypes.Add("trees", new CombatMapHex("Trees", "cm_hex_forest", 2.0f, 0.0f, 0, 1, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Forest }));
        combat_map_hex_prototypes.Add("houses", new CombatMapHex("Houses", "cm_hex_houses", 2.0f, 0.0f, 0, 1, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban }));
        combat_map_hex_prototypes.Add("street", new CombatMapHex("Street", "cm_hex_paving", 1.0f, 0.0f, 0, 0, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban }));
    }

    public static HexPrototypes Instance
    {
        get {
            if(instance == null) {
                instance = new HexPrototypes();
            }
            return instance;
        }
    }

    public WorldMapHex Get_World_Map_Hex(string name)
    {
        if (!world_map_hex_prototypes.ContainsKey(name)) {
            CustomLogger.Instance.Error("Hex prototype does not exist: " + name);
            return null;
        }
        return world_map_hex_prototypes[name];
    }

    /// <summary>
    /// Execept haunted forest
    /// </summary>
    public List<string> All_Forests
    {
        get {
            return new List<string>() { "Forest", "Forest Hill", "Mushroom Forest", "Enchanted Forest" };
        }
    }

    /// <summary>
    /// Execept haunted forest, water, swamp, mountain and volcano
    /// </summary>
    public List<string> All_Non_Structures
    {
        get {
            return new List<string>() { "Grassland", "Plains", "Hill", "Flower Field", "Forest", "Forest Hill", "Mushroom Forest", "Enchanted Forest" };
        }
    }

    public CombatMapHex Get_Combat_Map_Hex(string name)
    {
        if (!combat_map_hex_prototypes.ContainsKey(name)) {
            CustomLogger.Instance.Error("Hex prototype does not exist: " + name);
            return null;
        }
        return combat_map_hex_prototypes[name];
    }
}
