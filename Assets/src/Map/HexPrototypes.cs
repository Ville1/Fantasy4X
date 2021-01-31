using System;
using System.Collections.Generic;
using System.Linq;

public class HexPrototypes {
    private static HexPrototypes instance;
    private List<WorldMapHex> world_map_hex_prototypes;
    private Dictionary<string, CombatMapHex> combat_map_hex_prototypes;
    private List<Road> road_prototypes;

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

        world_map_hex_prototypes.Add(new WorldMapHex("grassland", "Grassland", "grass_3", new Dictionary<string, int>() { { "grass_2", 35 }, { "grass_4", 35 } }, new Yields(2, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }, new Dictionary<string, int>() {
                { "grass", 250 },
                { "scrubs", 15 },
                { "trees", 5 },
                { "flower field", 3 },
                { "plains", 1 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("plains", "Plains", "plains_2", null, new Yields(1, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open, WorldMapHex.Tag.Arid }, new Dictionary<string, int>() {
                { "grass", 250 },
                { "plains", 50 },
                { "scrubs", 15 },
                { "trees", 3 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("hill", "Hill", "hills_2", null, new Yields(1, 1, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 3.0f, 1, 1, true,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Hill, WorldMapHex.Tag.Arid }, new Dictionary<string, int>() {
                { "grass", 200 },
                { "scrubs", 15 },
                { "trees", 10 },
                { "plains", 5 }
            }, null));
        world_map_hex_prototypes.Add( new WorldMapHex("hill with a cave", "Hill with a Cave", "hills_2_cave", null, new Yields(1, 1, 0, 1, 0, 0, 0), 0.0f, 0.0f, 0.0f, 3.0f, 1, 1, true,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Underground }, new Dictionary<string, int>() {
                { "cave", 100 },
                { "cave rocks", 50 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("mountain", "Mountain", "mountain_2", null, new Yields(0, 2, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 5.0f, 3, 3, true,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Hill, WorldMapHex.Tag.Arid }, new Dictionary<string, int>() {
                { "grass", 150 },
                { "scrubs", 25 },
                { "trees", 10 }
            }, null));
        world_map_hex_prototypes.Add( new WorldMapHex("volcano", "Volcano", "volcano_2", null, new Yields(0, 2, 0, 1, 0, 0, 0), 0.0f, 0.0f, 0.0f, 5.0f, 3, 3, true,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Hill, WorldMapHex.Tag.Arid }, new Dictionary<string, int>() {
                { "grass", 150 },
                { "scrubs", 25 },
                { "trees", 10 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("swamp", "Swamp", "swamp_1", null, new Yields(2, 0, 0, 1, 0, 0, 0), 0.0f, -1.0f, 0.0f, 3.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Game }, new Dictionary<string, int>() {
                { "grass", 50 },
                { "scrubs", 200 },
                { "trees", 85 }
            }, null));
        world_map_hex_prototypes.First(x => x.Internal_Name == "swamp").Add_Animation(new List<string>() { "swamp_1", "swamp_2" }, 0.4f);
        world_map_hex_prototypes.Add(new WorldMapHex("flower field", "Flower Field", "flower_field_2", new Dictionary<string, int>() { { "flower_field_3", 33 } }, new Yields(2, 0, 0, 0, 1, 0, 0), 1.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }, new Dictionary<string, int>() {
                { "grass", 100 },
                { "scrubs", 5 },
                { "trees", 5 },
                { "flower field", 25 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("forest", "Forest", "forest_2", null, new Yields(2, 0.5f, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, 2.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Timber, WorldMapHex.Tag.Game }, new Dictionary<string, int>() {
                { "grass", 65 },
                { "scrubs", 75 },
                { "trees", 200 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("forest hill", "Forest Hill", "forest_hill_2", null, new Yields(1.5f, 1.5f, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, 3.0f, 1, 2,
            true, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Hill, WorldMapHex.Tag.Timber, WorldMapHex.Tag.Game }, new Dictionary<string, int>() {
                { "grass", 65 },
                { "scrubs", 75 },
                { "trees", 200 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("mushroom forest", "Mushroom Forest", "mushrooms_2", null, new Yields(2.5f, 0.5f, 0, 1, 0, 1, 0), 0.0f, 0.0f, 0.0f, 2.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Game }, default_grassland_seed, null));
        world_map_hex_prototypes.Add(new WorldMapHex("enchanted forest", "Enchanted Forest", "enchanted_forest_2", null, new Yields(2, 0.5f, 0, 0, 0, 2, 0), 1.0f, 0.5f, 0.0f, 2.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Timber, WorldMapHex.Tag.Game }, new Dictionary<string, int>() {
                { "enchanted trees", 180 },
                { "trees", 20 },
                { "scrubs", 75 },
                { "grass", 50 },
                { "flower field", 25 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("haunted forest", "Haunted Forest", "haunted_forest_2", null, new Yields(0, 1, 0, 0, 0, 1, 0), -1.0f, 0.0f, 0.0f, 2.0f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest, WorldMapHex.Tag.Special, WorldMapHex.Tag.Cursed }, new Dictionary<string, int>() {
                { "grass", 65 },
                { "scrubs", 75 },
                { "trees", 200 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("water", "Water", "water", null, new Yields(3, 0, 0, 0, 0, 0, 0), 0.0f, 0.25f, 0.0f, 1.0f, 0, -1, false,
            new List<WorldMapHex.Tag>(), new Dictionary<string, int>() {
                { "water", 100 }
            }, null) { Is_Water = true });
        world_map_hex_prototypes.Add(new WorldMapHex("city ruins", "City Ruins", "ruins_2", null, new Yields(1, 0, 0, 2, 1, 1, 1), 0.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Special, WorldMapHex.Tag.Structure }, new Dictionary<string, int>() {
                { "grass", 100 },
                { "scrubs", 35 },
                { "trees", 15 },
                { "houses", 15 },
                { "street", 5 }
            }, null));
        world_map_hex_prototypes.Add(new WorldMapHex("grave yard", "Grave Yard", "placeholder", null, new Yields(0, 0, 0, 0, 0, 1, 0), -1.0f, 0.0f, 0.0f, 1.0f, 0, 0, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open, WorldMapHex.Tag.Special, WorldMapHex.Tag.Structure, WorldMapHex.Tag.Cursed }, new Dictionary<string, int>() {
                { "grass", 100 },
                { "scrubs", 50 },
                { "trees", 15 },
                { "houses", 3 }
            }, null));

        world_map_hex_prototypes.Add(new WorldMapHex("small city", "Small City", "city_small", null, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special, WorldMapHex.Tag.Structure }, new Dictionary<string, int>() {
                { "grass", 100 },
                { "scrubs", 25 },
                { "trees", 15 },
                { "houses", 5 }
            },
            new Dictionary<string, int>() {
                { "grass", 50 },
                { "scrubs", 5 },
                { "trees", 15 },
                { "street", 180 },
                { "houses", 225 }
            }));
        world_map_hex_prototypes.Add(new WorldMapHex("city", "City", "kingdom_city", null, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special, WorldMapHex.Tag.Structure }, new Dictionary<string, int>() {
                { "grass", 100 },
                { "scrubs", 25 },
                { "trees", 15 },
                { "houses", 5 }
            },
            new Dictionary<string, int>() {
                { "grass", 50 },
                { "scrubs", 5 },
                { "trees", 15 },
                { "street", 200 },
                { "houses", 250 }
            }));
        world_map_hex_prototypes.Add(new WorldMapHex("village", "Village", "village", null, new Yields(1, 1, 1, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special, WorldMapHex.Tag.Structure }, new Dictionary<string, int>() {
                { "grass", 100 },
                { "scrubs", 25 },
                { "trees", 25 },
                { "houses", 10 }
            },
            new Dictionary<string, int>() {
                { "grass", 65 },
                { "scrubs", 20 },
                { "trees", 15 },
                { "street", 75 },
                { "houses", 100 }
            }));
        world_map_hex_prototypes.Add(new WorldMapHex("dwarven city", "Dwarven City", "dwarf_city_2", null, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special, WorldMapHex.Tag.Underground, WorldMapHex.Tag.Structure }, new Dictionary<string, int>() {
                { "cave", 100 },
                { "cave rocks", 50 },
                { "cave houses", 5 }
            },
            new Dictionary<string, int>() {
                { "cave", 50 },
                { "cave rocks", 5 },
                { "cave street", 200 },
                { "cave houses", 250 }
            }));
        world_map_hex_prototypes.Add(new WorldMapHex("elven city", "Elven City", "elf_city", null, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.5f, 0, 1, false,
            new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Urban, WorldMapHex.Tag.Special, WorldMapHex.Tag.Forest, WorldMapHex.Tag.Structure }, new Dictionary<string, int>() {
                { "grass", 95 },
                { "flower field", 5 },
                { "scrubs", 25 },
                { "trees", 25 },
                { "houses", 10 }
            },
            new Dictionary<string, int>() {
                { "grass", 60 },
                { "flower field", 5 },
                { "scrubs", 20 },
                { "trees", 15 },
                { "street", 75 },
                { "houses", 100 }
            }));

        combat_map_hex_prototypes = new Dictionary<string, CombatMapHex>();
        combat_map_hex_prototypes.Add("water", new CombatMapHex("Water", "water", null, 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open, CombatMapHex.Tag.Water }));
        combat_map_hex_prototypes.Add("grass", new CombatMapHex("Grass", "grass_3", new Dictionary<string, int>() { { "grass_2", 25 }, { "grass_4", 35 } }, 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open }));
        combat_map_hex_prototypes.Add("plains", new CombatMapHex("Plains", "plains_2", new Dictionary<string, int>(), 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open }));
        combat_map_hex_prototypes.Add("flower field", new CombatMapHex("Flower Field", "flower_field_2", new Dictionary<string, int>() { { "flower_field_3", 33 } }, 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open }));
        combat_map_hex_prototypes.Add("scrubs", new CombatMapHex("Scrubs", "combat_bushes_1", new Dictionary<string, int>() { { "combat_bushes_2", 50 } }, 1.0f, 1.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open }));
        combat_map_hex_prototypes.Add("trees", new CombatMapHex("Trees", "combat_trees_2", new Dictionary<string, int>() { { "combat_trees_1", 40 } }, 2.0f, 2.0f, 0, 1, 0.50f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Forest }));
        combat_map_hex_prototypes.Add("enchanted trees", new CombatMapHex("Enchanted Trees", "combat_enchanted_forest", new Dictionary<string, int>() { { "combat_enchanted_forest_2", 50 } }, 2.0f, 2.0f, 0, 1, 0.50f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Forest }));
        combat_map_hex_prototypes.Add("houses", new CombatMapHex("Houses", "combat_houses", null, 2.0f, 0.0f, 0, 1, 1.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban }));
        combat_map_hex_prototypes.Add("street", new CombatMapHex("Street", "combat_paving", null, 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban }));
        combat_map_hex_prototypes.Add("cave", new CombatMapHex("Cave", "combat_cave", null, 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Open, CombatMapHex.Tag.Underground }));
        combat_map_hex_prototypes.Add("cave rocks", new CombatMapHex("Cave Rocks", "combat_cave_rocks", null, 2.0f, 2.0f, 0, 0, 0.25f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Underground }));
        combat_map_hex_prototypes.Add("cave houses", new CombatMapHex("Cave Houses", "combat_cave_houses", null, 2.0f, 0.0f, 0, 1, 1.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban, CombatMapHex.Tag.Underground }));
        combat_map_hex_prototypes.Add("cave street", new CombatMapHex("Cave Street", "combat_cave_paving", null, 1.0f, 0.0f, 0, 0, 0.0f, new List<CombatMapHex.Tag>() { CombatMapHex.Tag.Urban, CombatMapHex.Tag.Underground }));


        road_prototypes = new List<Road>();
        road_prototypes.Add(new Road("Gravel Road", "gravel road", "road_ne", "road_e", 0.1f));
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

    public List<string> All_Internal_Names
    {
        get {
            return world_map_hex_prototypes.Select(x => x.Internal_Name).ToList();
        }
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
        if (!road_prototypes.Exists(x => x.Internal_Name == name)) {
            CustomLogger.Instance.Error("Road prototype does not exist: " + name);
            return null;
        }
        return road_prototypes.First(x => x.Internal_Name == name);
    }
}
