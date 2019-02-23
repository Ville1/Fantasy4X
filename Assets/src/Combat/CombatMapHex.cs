using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatMapHex : Hex {
    public enum Tag { Open, Forest, Urban }

    public static Color Owned_Unit_Color = new Color(0.0f, 0.0f, 1.0f);
    public static Color Enemy_Unit_Color = new Color(1.0f, 0.0f, 0.0f);
    public static Color Current_Owned_Unit_Color = new Color(0.0f, 1.0f, 0.0f);
    public static Color Current_Enemy_Unit_Color = new Color(1.0f, 0.0f, 0.5f);

    public string Terrain { get; set; }
    public string Texture { get; private set; }
    public float Movement_Cost { get; private set; }
    public float Run_Stamina_Penalty { get; private set; }
    public int Elevation { get; private set; }
    public int Height { get; private set; }
    public Unit Unit { get; set; }
    public CombatMap Map { get; private set; }
    public List<Tag> Tags { get; private set; }

    private bool hidden;

    public CombatMapHex(int q, int r, GameObject parent, CombatMapHex prototype, CombatMap map) : base(q, r, parent)
    {
        Change_To(prototype);
        Map = map;
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    public CombatMapHex(string terrain, string texture, float movement_cost, float run_stamina_penalty, int elevation, int height, List<Tag> tags) : base()
    {
        Terrain = terrain;
        Texture = texture;
        Movement_Cost = movement_cost;
        Run_Stamina_Penalty = run_stamina_penalty;
        Elevation = elevation;
        Height = height;
        Tags = new List<Tag>();
        foreach(Tag tag in tags) {
            Tags.Add(tag);
        }
    }

    public void Change_To(CombatMapHex prototype)
    {
        Terrain = prototype.Terrain;
        Texture = prototype.Texture;
        Movement_Cost = prototype.Movement_Cost;
        Run_Stamina_Penalty = prototype.Run_Stamina_Penalty;
        Elevation = prototype.Elevation;
        Height = prototype.Height;
        Tags = new List<Tag>();
        Tags = new List<Tag>();
        foreach (Tag tag in prototype.Tags) {
            Tags.Add(tag);
        }
        SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(Texture, SpriteManager.SpriteType.Terrain);
    }

    public bool Hidden
    {
        get {
            return hidden;
        }
        set {
            hidden = value;
            SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(hidden ? "hex_clouds" : Texture, SpriteManager.SpriteType.Terrain);
            if(Unit != null) {
                Unit.GameObject.SetActive(!hidden);
            }
        }
    }

    public List<CombatMapHex> Get_Adjancent_Hexes()
    {
        List<CombatMapHex> hexes = new List<CombatMapHex>();
        foreach (Coordinates c in Coordinates.Get_Adjanced_Coordinates().Select(x => x.Value).ToArray()) {
            if (Map.Get_Hex_At(c) != null) {
                hexes.Add(Map.Get_Hex_At(c));
            }
        }
        return hexes;
    }

    public bool Is_At_Map_Edge
    {
        get {
            foreach (Coordinates c in Coordinates.Get_Adjanced_Coordinates().Select(x => x.Value).ToArray()) {
                if (Map.Get_Hex_At(c) == null) {
                    return true;
                }
            }
            return false;
        }
    }

    public bool Is_Adjancent_To_Enemy(Player player)
    {
        foreach (CombatMapHex hex in Get_Adjancent_Hexes()) {
            if (hex.Unit != null && !hex.Unit.Army.Is_Owned_By(player)) {
                return true;
            }
        }
        return false;
    }

    public int Distance(CombatMapHex hex)
    {
        return Map.Straight_Line(this, hex).Count - 1;
    }

    /// <summary>
    /// TODO: ZoC
    /// </summary>
    public override PathfindingNode PathfindingNode
    {
        get {
            return new PathfindingNode(Coordinates, GameObject.transform.position.x, GameObject.transform.position.y, Unit == null ? Movement_Cost : -1.0f);
        }
    }

    /// <summary>
    /// TODO: ZoC
    /// </summary>
    public PathfindingNode Get_Specific_PathfindingNode(Unit unit, CombatMapHex ignore_unit_hex = null)
    {
        return new PathfindingNode(Coordinates, GameObject.transform.position.x, GameObject.transform.position.y, Unit == null || Unit == unit || ignore_unit_hex == this ? Movement_Cost : -1.0f);
    }

    public bool In_Attack_Range_Mark
    {
        get {
            return text_game_object.activeSelf;
        }
        set {
            text_game_object.SetActive(value);
            if (value) {
                TextMesh.text = "X";
            }
        }
    }

    /// <summary>
    /// TODO: Duplicated code (WorldMapHex)
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<CombatMapHex> Get_Hexes_Around(int range)
    {
        if (range <= 0) {
            CustomLogger.Instance.Error("Invalid argument: range = " + range);
            return null;
        }
        Dictionary<CombatMapHex, int> hexes = new Dictionary<CombatMapHex, int>();
        foreach (CombatMapHex adjancent_hex in Get_Adjancent_Hexes()) {
            Get_Hexes_Around_Recursive(adjancent_hex, range, 0, hexes);
        }
        return hexes.Select(x => x.Key).ToList();
    }

    private void Get_Hexes_Around_Recursive(CombatMapHex hex, int range, int distance, Dictionary<CombatMapHex, int> hexes)
    {
        if (hex == this) {
            return;
        }
        if (!hexes.ContainsKey(hex)) {
            hexes.Add(hex, distance);
        } else if (hexes[hex] <= distance) {
            return;
        } else {//hexes[hex] > distance
            hexes[hex] = distance;
        }
        distance++;
        if (distance >= range) {
            return;
        }
        foreach (CombatMapHex adjancent_hex in hex.Get_Adjancent_Hexes()) {
            Get_Hexes_Around_Recursive(adjancent_hex, range, distance, hexes);
        }
    }
}
