public interface Trainable {
    int Production_Required { get; }
    int Cost { get; }
    float Upkeep { get; }
    int Mana_Cost { get; }
    string Name { get; }
    string Texture { get; }
    string Tooltip { get; }
    Technology Technology_Required { get; }
    bool Requires_Coast { get; }
    bool Is_Summon { get; }
}
