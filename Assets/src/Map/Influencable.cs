using System.Collections.Generic;

public interface Influencable
{
    Dictionary<Player, float> Cultural_Influence { get; }
    string Name { get; }
    WorldMapHex Hex { get; }
    float Cultural_Influence_Resistance { get; }
    Player Owner { get; }
    bool Is_Owned_By_Current_Player { get; }
}
