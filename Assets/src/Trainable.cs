public interface Trainable {
    int Production_Required { get; }
    int Cost { get; }
    float Upkeep { get; }
    string Name { get; }
    string Texture { get; }
    string Tooltip { get; }
    Technology Technology_Required { get; }
    bool Requires_Coast { get; }
}
