public interface TradePartner {
    float Trade_Value { get; }
    Player Owner { get; }
    WorldMapHex Hex { get; }
    bool Is_Coastal { get; }
    string Name { get; }
}
