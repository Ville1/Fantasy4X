using System.Collections.Generic;

public class UnitCache
{
    private static UnitCache instance;

    private Dictionary<Unit, Data> data;

    private UnitCache()
    {
        data = new Dictionary<Unit, Data>();
    }

    public static UnitCache Instance
    {
        get {
            if(instance == null) {
                instance = new UnitCache();
            }
            return instance;
        }
    }

    public void Clear()
    {
        data.Clear();
    }

    public bool Has_Movement(Unit unit, CombatMapHex hex, float movement, bool run)
    {
        return data.ContainsKey(unit) && data[unit].Movement != null && data[unit].Movement.ContainsKey(hex) && data[unit].Movement[hex].ContainsKey(movement) && data[unit].Movement[hex][movement].ContainsKey(run);
    }

    public void Save_Movement(Unit unit, CombatMapHex hex, float movement, bool run, List<CombatMapHex> hexes)
    {
        if (!data.ContainsKey(unit)) {
            data.Add(unit, new Data(hex, movement, run, hexes));
        } else if(data[unit].Movement == null) {
            data[unit].Add_Movement(hex, movement, run, hexes);
        } else if(!data[unit].Movement.ContainsKey(hex)) {
            data[unit].Movement.Add(hex, new Dictionary<float, Dictionary<bool, List<CombatMapHex>>>() { { movement, new Dictionary<bool, List<CombatMapHex>>() { { run, hexes } } } });
        } else if(!data[unit].Movement[hex].ContainsKey(movement)) {
            data[unit].Movement[hex][movement] = new Dictionary<bool, List<CombatMapHex>>() { { run, hexes } };
        } else if (!data[unit].Movement[hex][movement].ContainsKey(run)) {
            data[unit].Movement[hex][movement].Add(run, hexes);
        } else {
            data[unit].Movement[hex][movement][run] = hexes;
        }
    }

    public List<CombatMapHex> Get_Movement(Unit unit, CombatMapHex hex, float movement, bool run)
    {
        if (!Has_Movement(unit, hex, movement, run)) {
            return null;
        }
        return data[unit].Movement[hex][movement][run];
    }

    public bool Has_Attack_Range(Unit unit, CombatMapHex hex, int range)
    {
        return data.ContainsKey(unit) && data[unit].Attack_Range != null && data[unit].Attack_Range.ContainsKey(hex) && data[unit].Attack_Range[hex].ContainsKey(range);
    }

    public void Save_Attack_Range(Unit unit, CombatMapHex hex, int range, List<CombatMapHex> hexes)
    {
        if (!data.ContainsKey(unit)) {
            data.Add(unit, new Data(hex, range, hexes));
        } else if (data[unit].Attack_Range == null) {
            data[unit].Add_Attack_Range(hex, range, hexes);
        } else if (!data[unit].Attack_Range.ContainsKey(hex)) {
            data[unit].Attack_Range.Add(hex, new Dictionary<int, List<CombatMapHex>>() { { range, hexes } });
        } else if (!data[unit].Attack_Range[hex].ContainsKey(range)) {
            data[unit].Attack_Range[hex].Add(range, hexes);
        } else {
            data[unit].Attack_Range[hex][range] = hexes;
        }
    }

    public List<CombatMapHex> Get_Attack_Range(Unit unit, CombatMapHex hex, int range)
    {
        if (!Has_Attack_Range(unit, hex, range)) {
            return null;
        }
        return data[unit].Attack_Range[hex][range];
    }

    private class Data
    {
        public Dictionary<CombatMapHex, Dictionary<float, Dictionary<bool, List<CombatMapHex>>>> Movement;
        public Dictionary<CombatMapHex, Dictionary<int, List<CombatMapHex>>> Attack_Range;

        public Data(CombatMapHex hex, float movement, bool run, List<CombatMapHex> hexes)
        {
            Movement = new Dictionary<CombatMapHex, Dictionary<float, Dictionary<bool, List<CombatMapHex>>>>() { { hex, new Dictionary<float, Dictionary<bool, List<CombatMapHex>>>()
                { { movement, new Dictionary<bool, List<CombatMapHex>>() { { run, hexes } } } } } };
            Attack_Range = null;
        }

        public Data(CombatMapHex hex, int range, List<CombatMapHex> hexes)
        {
            Movement = null;
            Attack_Range = new Dictionary<CombatMapHex, Dictionary<int, List<CombatMapHex>>>() { { hex, new Dictionary<int, List<CombatMapHex>>() { { range, hexes } } } };
        }

        public void Add_Movement(CombatMapHex hex, float movement, bool run, List<CombatMapHex> hexes)
        {
            Movement = new Dictionary<CombatMapHex, Dictionary<float, Dictionary<bool, List<CombatMapHex>>>>() { { hex, new Dictionary<float, Dictionary<bool, List<CombatMapHex>>>()
                { { movement, new Dictionary<bool, List<CombatMapHex>>() { { run, hexes } } } } } };
        }

        public void Add_Attack_Range(CombatMapHex hex, int range, List<CombatMapHex> hexes)
        {
            Attack_Range = new Dictionary<CombatMapHex, Dictionary<int, List<CombatMapHex>>>() { { hex, new Dictionary<int, List<CombatMapHex>>() { { range, hexes } } } };
        }
    }
}
