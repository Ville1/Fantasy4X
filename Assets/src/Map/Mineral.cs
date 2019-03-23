using System.Collections.Generic;

public class Mineral {
    public enum Tag { Smeltable, Industrial, Precious }

    private static Dictionary<string, Mineral> prototypes;

    public string Name { get; private set; }
    public string Name_With_Improvement { get; private set; }
    public Yields Yields { get; private set; }
    public float Happiness { get; private set; }
    public float Health { get; private set; }
    public float Order { get; private set; }
    public List<Tag> Tags { get; private set; }
    private int base_spawn_rate;
    private Dictionary<string, int> own_hex_spawn_rate_delta;
    private Dictionary<string, int> adjancent_hex_spawn_rate_delta;

    private Mineral(string name, string name_with_improvement, Yields yields, float happiness, float health, float order, int base_spawn_rate,
        List<Tag> tags, Dictionary<string, int> own_hex_spawn_rate_delta, Dictionary<string, int> adjancent_hex_spawn_rate_delta)
    {
        Name = name;
        Name_With_Improvement = name_with_improvement;
        Yields = new Yields(yields);
        Happiness = happiness;
        Health = health;
        Order = order;
        Tags = tags;
        this.base_spawn_rate = base_spawn_rate;
        this.own_hex_spawn_rate_delta = own_hex_spawn_rate_delta;
        this.adjancent_hex_spawn_rate_delta = adjancent_hex_spawn_rate_delta;
    }

    private static void Initialize_Prototypes()
    {
        prototypes = new Dictionary<string, Mineral>();

        prototypes.Add("copper", new Mineral("Copper", "Copper", new Yields(0, 1, 1, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 125, new List<Tag>() { Tag.Smeltable, Tag.Industrial }, new Dictionary<string, int>(), new Dictionary<string, int>()));
        prototypes.Add("iron", new Mineral("Iron", "Iron", new Yields(0, 3, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 125, new List<Tag>() { Tag.Smeltable, Tag.Industrial }, new Dictionary<string, int>(), new Dictionary<string, int>()));
        prototypes.Add("silver", new Mineral("Silver", "Silver", new Yields(0, 0, 3, 0, 0, 0, 0), 1.0f, 0.0f, -0.25f, 75, new List<Tag>() { Tag.Smeltable, Tag.Precious }, new Dictionary<string, int>(), new Dictionary<string, int>()));
        prototypes.Add("gold", new Mineral("Gold", "Gold", new Yields(0, 0, 4, 0, 0, 0, 0), 1.0f, 0.0f, -0.5f, 35, new List<Tag>() { Tag.Smeltable, Tag.Precious },
            new Dictionary<string, int>() { { "Mountain", 10 }, { "Volcano", 10 } },
            new Dictionary<string, int>()
        ));
        prototypes.Add("salt", new Mineral("Salt", "Salt", new Yields(1, 0, 1, 0, 0, 0, 0), 1.0f, 1.0f, 0.0f, 100, new List<Tag>(), new Dictionary<string, int>(), new Dictionary<string, int>()));
        prototypes.Add("gems", new Mineral("Gems", "Gem", new Yields(0, -1, 3, 0, 1, 0, 0), 1.0f, 0.0f, -0.5f, 25, new List<Tag>() { Tag.Precious },
            new Dictionary<string, int>() { { "Mountain", 5 }, { "Volcano", 5 } },
            new Dictionary<string, int>()
        ));
        prototypes.Add("crystal", new Mineral("Crystal", "Crystal", new Yields(0, -1, 1, 2, 0, 1, 1), 0.0f, 0.0f, 0.0f, 20, new List<Tag>() { },
            new Dictionary<string, int>() { { "Mountain", 10 }, { "Volcano", -10 } },
            new Dictionary<string, int>() { { "Enchanted Forest", 5 } }
        ));
        prototypes.Add("adamantine", new Mineral("Adamantine", "Adamantine", new Yields(0, 4, 1, 1, 0, 0, 0), 0.0f, 0.0f, 0.0f, 5, new List<Tag>() { Tag.Smeltable, Tag.Industrial },
            new Dictionary<string, int>() { { "Mountain", 5 }, { "Volcano", 10 } },
            new Dictionary<string, int>()
        ));
    }

    /// <summary>
    /// TODO: Unused function
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static Mineral Get_Prototype(string name)
    {
        if(prototypes == null) {
            Initialize_Prototypes();
        }

        if (!prototypes.ContainsKey(name)) {
            CustomLogger.Instance.Error("Prototype: " + name + " does not exist");
            return null;
        }
        return prototypes[name];
    }

    public static Mineral Get_Mineral_Spawn(WorldMapHex hex)
    {
        Dictionary<Mineral, int> mineral_spawn_rates = new Dictionary<Mineral, int>();
        if (prototypes == null) {
            Initialize_Prototypes();
        }
        foreach (KeyValuePair<string, Mineral> pair in prototypes) {
            int rate = pair.Value.base_spawn_rate;
            if (pair.Value.own_hex_spawn_rate_delta.ContainsKey(hex.Terrain)) {
                rate += pair.Value.own_hex_spawn_rate_delta[hex.Terrain];
            }
            if(pair.Value.adjancent_hex_spawn_rate_delta.Count != 0) {
                foreach(WorldMapHex adjancent_hex in hex.Get_Adjancent_Hexes()) {
                    if (pair.Value.adjancent_hex_spawn_rate_delta.ContainsKey(adjancent_hex.Terrain)) {
                        rate += pair.Value.adjancent_hex_spawn_rate_delta[adjancent_hex.Terrain];
                    }
                }
            }
            if(rate > 0) {
                mineral_spawn_rates.Add(pair.Value, rate);
            }
        }
        if(mineral_spawn_rates.Count == 0) {
            return null;
        }
        Dictionary<int, Mineral> minerals_with_accumulative_spawn_rates = new Dictionary<int, Mineral>();
        int max_value = 0;
        foreach(KeyValuePair<Mineral, int> pair in mineral_spawn_rates) {
            max_value += pair.Value;
            minerals_with_accumulative_spawn_rates.Add(max_value, pair.Key);
        }

        int random = RNG.Instance.Next(max_value);
        foreach (KeyValuePair<int, Mineral> pair in minerals_with_accumulative_spawn_rates) {
            if (pair.Key >= random) {
                CustomLogger.Instance.Debug("Mineral spawned on " + hex.ToString() + " : " + pair.Value.Name);
                return pair.Value;
            }
        }
        //This should not happen
        CustomLogger.Instance.Warning("This line should never be reached");
        return null;
    }
}
