using System.Collections.Generic;
using System.Linq;

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

    public void Clear(CombatMapHex hex)
    {
        foreach(KeyValuePair<Unit, Data> pair in data) {
            if(pair.Value.Attack_Range != null) {
                foreach (KeyValuePair<CombatMapHex, Dictionary<int, List<CombatMapHex>>> pair_2 in pair.Value.Attack_Range) {
                    List<int> clear_ranged = new List<int>();
                    clear_ranged.AddRange(pair_2.Value.Where(x => x.Value.Contains(hex)).Select(x => x.Key));
                    foreach (int i in clear_ranged) {
                        pair_2.Value.Remove(i);
                    }
                }
            }
            if(pair.Value.Movement != null) {
                foreach (KeyValuePair<CombatMapHex, Dictionary<float, Dictionary<bool, MovementData>>> pair_2 in pair.Value.Movement) {
                    foreach (KeyValuePair<float, Dictionary<bool, MovementData>> pair_3 in pair_2.Value) {
                        List<bool> clear_run = new List<bool>();
                        clear_run.AddRange(pair_3.Value.Where(x => x.Value.Clear_Area.Contains(hex)).Select(x => x.Key));
                        foreach (bool b in clear_run) {
                            pair_3.Value.Remove(b);
                        }
                    }
                }
            }
        }
    }

    public bool Has_Movement(Unit unit, CombatMapHex hex, float movement, bool run)
    {
        return data.ContainsKey(unit) && data[unit].Movement != null && data[unit].Movement.ContainsKey(hex) && data[unit].Movement[hex].ContainsKey(movement) && data[unit].Movement[hex][movement].ContainsKey(run);
    }

    public void Save_Movement(Unit unit, CombatMapHex hex, float movement, bool run, List<CombatMapHex> hexes, List<CombatMapHex> clear_area)
    {
        if (!data.ContainsKey(unit)) {
            data.Add(unit, new Data(hex, movement, run, hexes, clear_area));
        } else if(data[unit].Movement == null) {
            data[unit].Add_Movement(hex, movement, run, hexes, clear_area);
        } else if(!data[unit].Movement.ContainsKey(hex)) {
            data[unit].Movement.Add(hex, new Dictionary<float, Dictionary<bool, MovementData>>() { { movement, new Dictionary<bool, MovementData>() { { run, new MovementData(hexes, clear_area) } } } });
        } else if(!data[unit].Movement[hex].ContainsKey(movement)) {
            data[unit].Movement[hex][movement] = new Dictionary<bool, MovementData>() { { run, new MovementData(hexes, clear_area) } };
        } else if (!data[unit].Movement[hex][movement].ContainsKey(run)) {
            data[unit].Movement[hex][movement].Add(run, new MovementData(hexes, clear_area));
        } else {
            data[unit].Movement[hex][movement][run] = new MovementData(hexes, clear_area);
        }
    }

    public List<CombatMapHex> Get_Movement(Unit unit, CombatMapHex hex, float movement, bool run)
    {
        if (!Has_Movement(unit, hex, movement, run)) {
            return null;
        }
        return data[unit].Movement[hex][movement][run].Hexes;
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
        public Dictionary<CombatMapHex, Dictionary<float, Dictionary<bool, MovementData>>> Movement;
        public Dictionary<CombatMapHex, Dictionary<int, List<CombatMapHex>>> Attack_Range;

        public Data(CombatMapHex hex, float movement, bool run, List<CombatMapHex> hexes, List<CombatMapHex> hexes_plus_1)
        {
            Movement = new Dictionary<CombatMapHex, Dictionary<float, Dictionary<bool, MovementData>>>() { { hex, new Dictionary<float, Dictionary<bool, MovementData>>()
                { { movement, new Dictionary<bool, MovementData>() { { run, new MovementData(hexes, hexes_plus_1)} } } } } };
            Attack_Range = null;
        }

        public Data(CombatMapHex hex, int range, List<CombatMapHex> hexes)
        {
            Movement = null;
            Attack_Range = new Dictionary<CombatMapHex, Dictionary<int, List<CombatMapHex>>>() { { hex, new Dictionary<int, List<CombatMapHex>>() { { range, hexes } } } };
        }

        public void Add_Movement(CombatMapHex hex, float movement, bool run, List<CombatMapHex> hexes, List<CombatMapHex> hexes_plus_1)
        {
            Movement = new Dictionary<CombatMapHex, Dictionary<float, Dictionary<bool, MovementData>>>() { { hex, new Dictionary<float, Dictionary<bool, MovementData>>()
                { { movement, new Dictionary<bool, MovementData>() { { run, new MovementData(hexes, hexes_plus_1) } } } } } };
        }

        public void Add_Attack_Range(CombatMapHex hex, int range, List<CombatMapHex> hexes)
        {
            Attack_Range = new Dictionary<CombatMapHex, Dictionary<int, List<CombatMapHex>>>() { { hex, new Dictionary<int, List<CombatMapHex>>() { { range, hexes } } } };
        }
    }

    private class MovementData
    {
        public List<CombatMapHex> Hexes { get; set; }
        public List<CombatMapHex> Clear_Area { get; set; }

        public MovementData(List<CombatMapHex> hexes, List<CombatMapHex> clear_area)
        {
            Hexes = hexes;
            Clear_Area = clear_area;
        }
    }
}
