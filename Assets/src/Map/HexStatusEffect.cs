using System.Text;

public class HexStatusEffect : IStatusEffect {
    private static int current_id;

    public int Id { get; private set; }
    public string Name { get; private set; }
    public Yields Yield_Delta { get; set; }
    public int Duration { get; private set; }
    public int Current_Duration { get; set; }
    public int? Parent_Duration { get; set; }
    public float Happiness { get; set; }
    public float Health { get; set; }
    public float Order { get; set; }

    public HexStatusEffect(string name, int duration)
    {
        Name = name;
        Duration = duration;
        Current_Duration = Duration;
        Parent_Duration = null;
        Id = current_id;
        current_id++;
        Yield_Delta = new Yields();
        Happiness = 0.0f;
        Health = 0.0f;
        Order = 0.0f;
    }

    public int UI_Current_Duration
    {
        get {
            return Parent_Duration.HasValue ? Parent_Duration.Value : Current_Duration;
        }
    }

    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder(Name);
            tooltip.Append(" (").Append(UI_Current_Duration).Append("t)");
            if (!Yield_Delta.Empty) {
                tooltip.Append(" ").Append(Yield_Delta.Generate_String(false));
            }
            if (Happiness != 0.0f) {
                tooltip.Append(" ").Append(Helper.Float_To_String(Happiness, 1, true));
            }
            if (Health != 0.0f) {
                tooltip.Append(" ").Append(Helper.Float_To_String(Health, 1, true));
            }
            if (Order != 0.0f) {
                tooltip.Append(" ").Append(Helper.Float_To_String(Order, 1, true));
            }
            return tooltip.ToString();
        }
    }

    public void Load(WorldMapHexStatusEffectSaveData data)
    {
        Current_Duration = data.Current_Duration;
        Parent_Duration = data.Parent_Duration == -1 ? (int?)null : data.Parent_Duration;
        Yield_Delta = new Yields(data.Yield_Delta);
        Happiness = data.Happiness;
        Health = data.Health;
        Order = data.Order;
    }
}
