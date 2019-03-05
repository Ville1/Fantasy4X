public class CityStatusEffect {
    private static int current_id;

    public int Id { get; private set; }
    public string Name { get; private set; }
    public Yields Yield_Delta { get; set; }
    public Yields Percentage_Yield_Delta { get; set; }
    public int Duration { get; private set; }
    public int Current_Duration { get; set; }
    public float Happiness { get; set; }
    public float Health { get; set; }
    public float Order { get; set; }

    public CityStatusEffect(string name, int duration)
    {
        Name = name;
        Duration = duration;
        Current_Duration = Duration;
        Id = current_id;
        current_id++;
        Yield_Delta = new Yields();
        Percentage_Yield_Delta = new Yields();
        Happiness = 0.0f;
        Health = 0.0f;
        Order = 0.0f;
    }
}
