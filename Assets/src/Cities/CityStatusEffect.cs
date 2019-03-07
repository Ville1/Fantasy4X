public class CityStatusEffect : HexStatusEffect {
    public Yields Percentage_Yield_Delta { get; set; }

    public CityStatusEffect(string name, int duration) : base(name, duration)
    {
        Percentage_Yield_Delta = new Yields();
    }
}
