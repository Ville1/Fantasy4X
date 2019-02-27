using System.Collections.Generic;

public class Village : Ownable, Influencable, TradePartner
{
    private static readonly float STARTING_CULTURE = 50.0f;

    private static int current_id = 0;

    public WorldMapHex Hex { get; private set; }
    public int Id { get; private set; }
    public string Name { get; private set; }
    public Dictionary<Player, float> Cultural_Influence { get; private set; }
    public Flag Flag { get; set; }
    public bool Is_Coastal { get; private set; }

    public Village(WorldMapHex hex, Player owner)
    {
        Hex = hex;
        Hex.Village = this;
        Id = current_id;
        current_id++;
        Name = string.Format("Village #{0}", Id);
        Owner = owner;
        Hex.Owner = Owner;
        Flag = new Flag(hex);
        Cultural_Influence = new Dictionary<Player, float>() { { Owner, STARTING_CULTURE } };

        Is_Coastal = false;
        foreach(WorldMapHex adjancent_hex in Hex.Get_Adjancent_Hexes()) {
            if(adjancent_hex.Worked_By_Village != null) {
                CustomLogger.Instance.Warning(string.Format("Village #{0} overrides village #{1}'s hex assignment", Id, adjancent_hex.Worked_By_Village.Id));
            }
            adjancent_hex.Worked_By_Village = this;
            if (adjancent_hex.Is_Water) {
                Is_Coastal = true;
            }
        }
    }

    public float Cultural_Influence_Resistance {
        get {
            return 0.0f;
        }
    }

    public bool Update_Owner()
    {
        Player new_owner = Owner;
        if(Hex.Entity != null && !Hex.Entity.Is_Civilian) {
            new_owner = Hex.Entity.Owner;
        } else {
            float highest_influence = 0.0f;
            foreach(KeyValuePair<Player, float> influence_data in Cultural_Influence) {
                if(influence_data.Value > highest_influence) {
                    highest_influence = influence_data.Value;
                    new_owner = influence_data.Key;
                }
            }
        }
        if(new_owner.Id == Owner.Id) {
            return false;
        }
        Owner.Villages.Remove(this);
        foreach(City city in Owner.Cities) {
            city.Yields_Changed();
        }
        new_owner.Villages.Add(this);
        foreach (City city in new_owner.Cities) {
            city.Yields_Changed();
        }
        Owner = new_owner;
        Flag.Update_Type();
        return true;
    }

    public List<WorldMapHex> Get_Hexes_In_LoS()
    {
        List<WorldMapHex> los = Hex.Get_Hexes_Around(1);
        los.Add(Hex);
        return los;
    }

    public Yields Yields
    {
        get {
            Yields yields = new Yields(Hex.Yields);
            foreach(WorldMapHex hex in Hex.Get_Adjancent_Hexes()) {
                yields.Add(hex.Yields);
            }
            yields.Food *= 0.1f;
            yields.Production *= 0.5f;
            return yields;
        }
    }

    public bool Connected_With_Road
    {
        get {
            foreach(WorldMapHex adjacent_hex in Hex.Get_Adjancent_Hexes()) {
                if(adjacent_hex.Road != null) {
                    return true;
                }
            }
            return false;
        }
    }

    public float Trade_Value
    {
        get {
            Yields yields = new Yields(Hex.Yields);
            foreach (WorldMapHex hex in Hex.Get_Adjancent_Hexes()) {
                yields.Add(hex.Yields);
            }
            return ((yields.Total * 1.25f) + 7.0f) / 7.0f;
        }
    }
}
