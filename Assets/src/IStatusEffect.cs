public interface IStatusEffect {
    string Name { get; }
    int Duration { get; }
    int Current_Duration { get; set; }
}
