public class CityStatusEffect : HexStatusEffect {
    public Yields Percentage_Yield_Delta { get; set; }

    public CityStatusEffect(string name, int duration) : base(name, duration)
    {
        Percentage_Yield_Delta = new Yields();
    }

    public void Load(CityStatusEffectSaveData data)
    {
        Current_Duration = data.Current_Duration;
        Parent_Duration = data.Parent_Duration == -1 ? (int?)null : data.Parent_Duration;
        Yield_Delta = new Yields(data.Yield_Delta);
        Percentage_Yield_Delta = new Yields(data.Percentage_Yield_Delta);
        Happiness = data.Happiness;
        Health = data.Health;
        Order = data.Order;
    }
}
