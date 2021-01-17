using System.Collections.Generic;
using System.Linq;

public class Damage
{
    public static readonly string TYPELESS_NAME = "Typeless";

    public enum Type { Slash, Thrust, Impact, Fire, Cold, Electric, Water, Wind, Earth, Acid, Light, Dark, Cosmic }
    public enum Nature { Physical, Magical, Psionic }

    public float Total { get; private set; }
    public Dictionary<Type, float> Type_Weights { get; private set; }
    public Dictionary<Nature, decimal> Nature_Proportions { get; private set; }

    public Damage()
    {
        Total = 0.0f;
        Type_Weights = new Dictionary<Type, float>();
        Nature_Proportions = new Dictionary<Nature, decimal>() {
            { Nature.Physical, 1.0m }
        };
    }

    public Damage(float damage, Nature nature, Type type)
    {
        Total = damage;
        Type_Weights = new Dictionary<Type, float>() {
            { type, 1.0f }
        };
        Nature_Proportions = new Dictionary<Nature, decimal>() {
            { nature, 1.0m }
        };
    }

    public Damage(float damage, Nature nature, Dictionary<Type, float> types)
    {
        Total = damage;
        Nature_Proportions = new Dictionary<Nature, decimal>() {
            { nature, 1.0m }
        };
        Type_Weights = Helper.Copy_Dictionary(types);
        Validate();
    }

    public Damage(float damage, Dictionary<Type, float> types)
    {
        Total = damage;
        Nature_Proportions = new Dictionary<Nature, decimal>() {
            { Damage.Nature.Physical, 1.0m }
        };
        Type_Weights = Helper.Copy_Dictionary(types);
        Validate();
    }

    public Damage(float damage, Dictionary<Nature, decimal> nature, Dictionary<Type, float> types)
    {
        Total = damage;
        Nature_Proportions = Helper.Copy_Dictionary(nature);
        Type_Weights = Helper.Copy_Dictionary(types);
        Validate();
    }
    
    public Damage Add(float damage)
    {
        Total += damage;
        return this;
    }

    public Damage Add(Damage damage)
    {
        float old_damage = Total;
        Total += damage.Total;
        float relative_new = damage.Total / (old_damage + damage.Total);
        float realtive_old = 1.0f - relative_new;
        
        Dictionary<Nature, decimal> nature = new Dictionary<Nature, decimal>();
        foreach (KeyValuePair<Nature, decimal> pair in Nature_Proportions) {
            nature.Add(pair.Key, Helper.Round(pair.Value * (decimal)realtive_old, 3));
        }
        foreach (KeyValuePair<Nature, decimal> pair in damage.Nature_Proportions) {
            if (nature.ContainsKey(pair.Key)) {
                nature[pair.Key] += Helper.Round(pair.Value * (decimal)relative_new, 3);
            } else {
                nature.Add(pair.Key, Helper.Round(pair.Value * (decimal)relative_new, 3));
            }
        }
        Nature last = Nature.Physical;
        decimal total = 0.0m;
        foreach (KeyValuePair<Nature, decimal> pair in nature) {
            total += pair.Value;
            last = pair.Key;
        }
        if (total >= 0.95m && total < 1.0m) {
            nature[last] += 1.0m - total;
        } else if (total <= 1.05m && total > 1.0m) {
            nature[last] -= total - 1.0m;
        }
        Nature_Proportions = nature;

        Dictionary<Type, float> type = new Dictionary<Type, float>();
        foreach (KeyValuePair<Type, float> pair in Type_Weights) {
            type.Add(pair.Key, pair.Value * realtive_old);
        }
        foreach (KeyValuePair<Type, float> pair in damage.Type_Weights) {
            if (type.ContainsKey(pair.Key)) {
                type[pair.Key] += pair.Value * relative_new;
            } else {
                type.Add(pair.Key, pair.Value * relative_new);
            }
        }
        Type_Weights = type;

        Validate();
        return this;
    }

    public Damage Multiply(float multiplier)
    {
        Total *= multiplier;
        return this;
    }
    
    public Damage Clone
    {
        get {
            Damage clone = new Damage();
            clone.Total = Total;
            clone.Type_Weights = Helper.Copy_Dictionary(Type_Weights);
            clone.Nature_Proportions = Helper.Copy_Dictionary(Nature_Proportions);
            return clone;
        }
    }

    public Dictionary<Nature, decimal> Natures_Ordered
    {
        get {
            return Nature_Proportions.OrderByDescending(x => x.Value).ThenBy(x => (int)x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public Dictionary<string, float> Type_Names_Ordered
    {
        get {
            Dictionary<string, float> types = new Dictionary<string, float>();
            float total = 0.0f;
            foreach (KeyValuePair<Type, float> pair in Type_Weights) {
                types.Add(pair.Key.ToString(), pair.Value);
                total += pair.Value;
            }
            if (total < 1.0f) {
                types.Add(TYPELESS_NAME, 1.0f - total);
            }
            return types.OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
    }

    public float Typeless_Damage
    {
        get {
            float total = 0.0f;
            foreach (KeyValuePair<Type, float> pair in Type_Weights) {
                total += pair.Value;
            }
            return total < 1.0f ? 1.0f - total : 0.0f;
        }
    }
    
    public override string ToString()
    {
        return Type_Weights.Count == 1 ? string.Format("{0} {1}", Helper.Float_To_String(Total, 1), Type_Weights.First().Key.ToString()) : string.Format("{0} Damage", Helper.Float_To_String(Total, 1));
    }
    
    private void Validate()
    {
        decimal total_nature = 0.0m;
        foreach (KeyValuePair<Nature, decimal> pair in Nature_Proportions) {
            total_nature += pair.Value;
        }
        if (total_nature != 1.0m) {
            CustomLogger.Instance.Warning(string.Format("Total nature {0}", total_nature));
        }
        float total_multiplier = 0.0f;
        foreach (KeyValuePair<Type, float> pair in Type_Weights) {
            total_multiplier += pair.Value;
        }
    }
}
