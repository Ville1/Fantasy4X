using System;
using System.Collections.Generic;
using System.Linq;

public class HexPrototypes {
    private static HexPrototypes instance;
    private List<WorldMapHex> world_map_hex_prototypes;
    private Dictionary<string, CombatMapHex> combat_map_hex_prototypes;
    private Dictionary<string, Road> road_prototypes;

    private HexPrototypes()
    {
        world_map_hex_prototypes = new List<WorldMapHex>();
        Dictionary<string, int> default_grassland_seed = new Dictionary<string, int>();
        default_grassland_seed.Add("grass", 100);
        default_grassland_seed.Add("scrubs", 25);
        default_grassland_seed.Add("trees", 10);
        Dictionary<string, int> default_forest_seed = new Dictionary<string, int>();
        default_forest_seed.Add("grass", 50);
        default_forest_seed.Add("scrubs", 75);
        default_forest_seed.Add("trees", 200);

        world_map_hex_prototypes.Add(new WorldMapHex("grassland", "Grassland", "grass", new Yields(2, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }, default_grassland_seed, null));
        world_map_hex_prototypes.Add(new WorldMapHex("plains", "Plains", "plains", new Yields(1, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open, WorldMapHex.Tag.Arid }, new Dictionary<string, int>() { { "grass", 100 }, { "scrubs", 25 }, { "trees", 3 } }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("hill", "Hill", "hills", new Yields(1, 1, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 3.0f, 1, 1, true,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Hill, WorldMapHex.Tag.Arid }, default_grassland_seed, null));
        world_map_hex_prototypes.Add( new WorldMapHex("hill with a cave", "Hill with a Cave", "cave_hills", new Yields(1, 1, 0, 1, 0, 0, 0), 0.0f, 0.0f, 0.0f, 3.0f, 1, 1, true,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Underground }, new Dictionary<string, int>() { { "cave", 100 }, { "cave rocks", 50 } }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("mountain", "Mountain", "mountain", new Yields(0, 2, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 5.0f, 3, 3, true,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Hill, WorldMapHex.Tag.Arid }, default_grassland_seed, null));
        world_map_hex_prototypes.Add( new WorldMapHex("volcano", "Volcano", "volcano", new Yields(0, 2, 0, 1, 0, 0, 0), 0.0f, 0.0f, 0.0f, 5.0f, 3, 3, true,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Hill, WorldMapHex.Tag.Arid }, default_grassland_seed, null));
        world_map_hex_prototypes.Add(new WorldMapHex("swamp", "Swamp", "swamp", new Yields(2, 0, 0, 1, 0, 0, 0), 0.0f, -1.0f, 0.0f, 3.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Game }, new Dictionary<string, int>() { { "grass", 50 }, { "scrubs", 200 }, { "trees", 85 } }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("flower field", "Flower Field", "flower_field", new Yields(2, 0, 0, 0, 1, 0, 0), 1.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }, new Dictionary<string, int>() { { "grass", 100 }, { "scrubs", 5 }, { "trees", 5 } }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("forest", "Forest", "forest", new Yields(2, 0.5f, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, 2.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Timber, WorldMapHex.Tag.Game }, default_forest_seed, null));
        world_map_hex_prototypes.Add(new WorldMapHex("forest hill", "Forest Hill", "forest_hill", new Yields(1.5f, 1.5f, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, 3.0f, 1, 2,
            true, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Hill, WorldMapHex.Tag.Timber, WorldMapHex.Tag.Game }, default_forest_seed, null));
        world_map_hex_prototypes.Add(new WorldMapHex("mushroom forest", "Mushroom Forest", "mushrooms", new Yields(2.5f, 0.5f, 0, 1, 0, 1, 0), 0.0f, 0.0f, 0.0f, 2.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Game }, default_grassland_seed, null));
        world_map_hex_prototypes.Add(new WorldMapHex("enchanted forest", "Enchanted Forest", "enchanted_forest", new Yields(2, 0.5f, 0, 0, 0, 2, 0), 1.0f, 0.5f, 0.0f, 2.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Timber, WorldMapHex.Tag.Game }, default_forest_seed, null));
        world_map_hex_prototypes.Add(new WorldMapHex("haunted forest", "Haunted Forest", "haunted_forest", new Yields(0, 1, 0, 0, 0, 1, 0), -1.0f, 0.0f, 0.0f, 2.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Special }, default_grassland_seed, null));
        world_map_hex_prototypes.Add(new WorldMapHex("water", "Water", "water", new Yields(3, 0, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, 1.0f, 0, -1, false,
            new List<WorldMapHex.Tag>(), default_grassland_seed, null) { Is_Water = true });
        world_map_hex_prototypes.Add(new WorldMapHex("city ruins", "City Ruins", "ruins", new Yields(1, 0, 0, 2, 1, 1, 1), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Special }, new Dictionary<string, int>() { { "grass", 100 }, { "scrubs", 35 }, { "trees", 15 }, { "houses", 15 }, { "street", 5 } }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("grave yard", "Grave Yard", "graveyard", new Yields(0, 0, 0, 0, 0, 1, 0), -1.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open, WorldMapHex.Tag.Special }, new Dictionary<string, int>() { { "grass", 100 }, { "scrubs", 50 }, { "trees", 15 }, { "houses", 3 } }, null));

        world_map_hex_prototypes.Add(new WorldMapHex("small city", "Small City", "small_city", new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special }, new Dictionary<string, int>() { { "grass", 100 }, { "scrubs", 25 }, { "trees", 15 }, { "houses", 5 } },
            new Dictionary<string, int>() { { "grass", 50 }, { "scrubs", 5 }, { "trees", 15 }, { "street", 180 }, { "houses", 225 } }));
        world_map_hex_prototypes.Add(new WorldMapHex("city", "City", "kingdom_city", new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special }, new Dictionary<string, int>() { { "grass", 100 }, { "scrubs", 25 }, { "trees", 15 }, { "houses", 5 } },
            new Dictionary<string, int>() { { "grass", 50 }, { "scrubs", 5 }, { "trees", 15 }, { "street", 200 }, { "houses", 250 } }));
        world_map_hex_prototypes.Add(new WorldMapHex("village", "Village", "village", new Yields(1, 1, 1, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special }, new Dictionary<string, int>() { { "grass", 100 }, { "scrubs", 25 }, { "trees", 25 }, { "houses", 10 } },
            new Dictionary<string, int>() { { "grass", 65 }, { "scrubs", 20 }, { "trees", 15 }, { "street", 75 }, { "houses", 100 } }));
        world_map_hex_prototypes.Add(new WorldMapHex("dwarven city", "Dwarven City", "hex_dwarven_city", new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 1.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special, WorldMapHex.Tag.Underground }, new Dictionary<string, int>() { { "cave", 100 }, { "cave rocks", 50 }, { "cave houses", 5 } },
            new Dictionary<string, int>() { { "cave", 50 }, { "cave rocks", 5 }, { "cave street", 200 }, { "cave houses", 250 } }));

        combat_map_hex_prototypes = new Dictionary<string, CombatMapHex>();
        combat_map_hex_prototypes.Add("grass", new CombatMapHex("Grass", "combat_grass", 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open }));
        combat_map_hex_prototypes.Add("scrubs", new CombatMapHex("Scrubs", "combat_scrubs", 1.0f, 1.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open }));
        combat_map_hex_prototypes.Add("trees", new CombatMapHex("Trees", "forest", 2.0f, 2.0f, 0, 1, 0.50f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Forest }));
        combat_map_hex_prototypes.Add("houses", new CombatMapHex("Houses", "combat_houses", 2.0f, 0.0f, 0, 1, 1.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban }));
        combat_map_hex_prototypes.Add("street", new CombatMapHex("Street", "combat_paving", 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban }));
        combat_map_hex_prototypes.Add("cave", new CombatMapHex("Cave", "combat_cave", 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open, CombatMapHex.Tag.Underground }));
        combat_map_hex_prototypes.Add("cave rocks", new CombatMapHex("Cave Rocks", "combat_cave_rocks", 2.0f, 2.0f, 0, 0, 0.25f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Underground }));
        combat_map_hex_prototypes.Add("cave houses", new CombatMapHex("Cave Houses", "combat_cave_houses", 2.0f, 0.0f, 0, 1, 1.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban, CombatMapHex.Tag.Underground }));
        combat_map_hex_prototypes.Add("cave street", new CombatMapHex("Cave Street", "combat_cave_paving", 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban, CombatMapHex.Tag.Underground }));


        road_prototypes = new Dictionary<string, Road>();
        road_prototypes.Add("gravel road", new Road("Gravel Road", "road_ne", "road_e", 0.1f));
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
        if (!world_map_hex_prototypes.Exists(x => x.Internal_Name == name)) {
            CustomLogger.Instance.Error("Hex prototype does not exist: " + name);
            return null;
        }
        return world_map_hex_prototypes.First(x => x.Internal_Name == name);
    }

    public List<string> Get_Names(WorldMapHex.Tag? include_tag, WorldMapHex.Tag? exclude_tag = null, bool? spawns_minerals = null, bool ignore_special = true)
    {
        return Get_Names(include_tag.HasValue ? new List<WorldMapHex.Tag>() { include_tag.Value } : null,
            exclude_tag.HasValue ? new List<WorldMapHex.Tag>() { exclude_tag.Value } : null, spawns_minerals, ignore_special);
    }

    public List<string> Get_Names(List<WorldMapHex.Tag> include_tags, List<WorldMapHex.Tag> exclude_tags = null, bool? spawns_minerals = null, bool ignore_special = true)
    {
        if((include_tags == null || include_tags.Count == 0) && (exclude_tags == null || exclude_tags.Count == 0) && !spawns_minerals.HasValue) {
            throw new ArgumentException();
        }
        return world_map_hex_prototypes.Where(x => (!ignore_special || !x.Tags.Contains(WorldMapHex.Tag.Special)) && (include_tags == null ||
            x.Tags.Any(y => include_tags.Contains(y))) && (exclude_tags == null || !x.Tags.Any(y => exclude_tags.Contains(y))) &&
            (!spawns_minerals.HasValue || x.Can_Spawn_Minerals == spawns_minerals.Value)).
            Select(x => x.Terrain).ToList();
    }

    /// <summary>
    /// Except haunted forest
    /// </summary>
    public List<string> All_Forests
    {
        get {
            return new List<string>() { "Forest", "Forest Hill", "Mushroom Forest", "Enchanted Forest" };
        }
    }

    /// <summary>
    /// Except haunted forest, water, swamp, mountain and volcano
    /// </summary>
    public List<string> All_Non_Structures
    {
        get {
            return new List<string>() { "Grassland", "Plains", "Hill", "Flower Field", "Forest", "Forest Hill", "Mushroom Forest", "Enchanted Forest", "Hill with a Cave" };
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

    public Road Get_Road(string name)
    {
        if (!road_prototypes.ContainsKey(name)) {
            CustomLogger.Instance.Error("Road prototype does not exist: " + name);
            return null;
        }
        return road_prototypes[name];
    }
}
