using System.Text;

public class Yields {
    public float Food { get; set; }
    public float Production { get; set; }
    public float Cash { get; set; }
    public float Science { get; set; }
    public float Culture { get; set; }
    public float Mana { get; set; }
    public float Faith { get; set; }
    
    public Yields()
    {
        Food = 0.0f;
        Production = 0.0f;
        Cash = 0.0f;
        Science = 0.0f;
        Culture = 0.0f;
        Mana = 0.0f;
        Faith = 0.0f;
    }

    public Yields(float food, float production, float cash, float science, float culture, float mana, float faith)
    {
        Food = food;
        Production = production;
        Cash = cash;
        Science = science;
        Culture = culture;
        Mana = mana;
        Faith = faith;
    }

    public Yields(Yields yields)
    {
        Food = yields.Food;
        Production = yields.Production;
        Cash = yields.Cash;
        Science = yields.Science;
        Culture = yields.Culture;
        Mana = yields.Mana;
        Faith = yields.Faith;
    }

    public Yields(YieldsSaveData yields)
    {
        Food = yields.Food;
        Production = yields.Production;
        Cash = yields.Cash;
        Science = yields.Science;
        Culture = yields.Culture;
        Mana = yields.Mana;
        Faith = yields.Faith;
    }

    public void Add(Yields yields)
    {
        Food += yields.Food;
        Production += yields.Production;
        Cash += yields.Cash;
        Science += yields.Science;
        Culture += yields.Culture;
        Mana += yields.Mana;
        Faith += yields.Faith;
    }

    public void Multiply_By_Percentages(Yields percentages)
    {
        Food *= (percentages.Food * 0.01f);
        Production *= (percentages.Production * 0.01f);
        Cash *= (percentages.Cash * 0.01f);
        Science *= (percentages.Science * 0.01f);
        Culture *= (percentages.Culture * 0.01f);
        Mana *= (percentages.Mana * 0.01f);
        Faith *= (percentages.Faith * 0.01f);
    }

    public void Divide_By_Number(float number)
    {
        Food /= number;
        Production /= number;
        Cash /= number;
        Science /= number;
        Culture /= number;
        Mana /= number;
        Faith /= number;
    }

    public float Total
    {
        get {
            return Food + Production + Cash + Science + Culture + Mana + Faith;
        }
    }

    public bool Empty
    {
        get {
            return Food == 0.0f && Production == 0.0f && Cash == 0.0f && Science == 0.0f && Culture == 0.0f && Mana == 0.0f && Faith == 0.0f;
        }
    }

    public override string ToString()
    {
        return Generate_String(false);
    }

    public string Generate_String(bool percent)
    {
        StringBuilder builder = new StringBuilder();
        string key_value_delimiter = ":";
        string field_delimiter = percent ? "% " : " ";
        if (Food != 0.0f) {
            builder.Append("F").Append(key_value_delimiter).Append(Helper.Float_To_String(Food, 1)).Append(field_delimiter);
        }
        if (Production != 0.0f) {
            builder.Append("P").Append(key_value_delimiter).Append(Helper.Float_To_String(Production, 1)).Append(field_delimiter);
        }
        if (Cash != 0.0f) {
            builder.Append("C").Append(key_value_delimiter).Append(Helper.Float_To_String(Cash, 1)).Append(field_delimiter);
        }
        if (Science != 0.0f) {
            builder.Append("S").Append(key_value_delimiter).Append(Helper.Float_To_String(Science, 1)).Append(field_delimiter);
        }
        if (Culture != 0.0f) {
            builder.Append("U").Append(key_value_delimiter).Append(Helper.Float_To_String(Culture, 1)).Append(field_delimiter);
        }
        if (Mana != 0.0f) {
            builder.Append("M").Append(key_value_delimiter).Append(Helper.Float_To_String(Mana, 1)).Append(field_delimiter);
        }
        if (Faith != 0.0f) {
            builder.Append("A").Append(key_value_delimiter).Append(Helper.Float_To_String(Faith, 1)).Append(field_delimiter);
        }
        if (builder.Length == 0) {
            builder.Append("None");
        }
        return builder.ToString();
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object obj)
    {
        if(!(obj is Yields)) {
            return false;
        }
        Yields y = obj as Yields;
        return Food == y.Food && Production == y.Production && Cash == y.Cash && Science == y.Science && Culture == y.Culture && Mana == y.Mana && Faith == y.Faith;
    }

    public YieldsSaveData Save_Data
    {
        get {
            return new YieldsSaveData() {
                Food = Food,
                Production = Production,
                Cash = Cash,
                Science = Science,
                Culture = Culture,
                Mana = Mana,
                Faith = Faith
            };
        }
    }
}
