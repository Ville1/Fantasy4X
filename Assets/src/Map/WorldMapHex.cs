using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMapHex : Hex {
    private static readonly Color EXPLORED_TINT = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    private static readonly float UNEXPLORED_MOVEMENT_COST = 10.0f;
    private static string not_explored_texture = "hex_clouds";

    public enum InfoText { None, Coordinates, Yields, Minerals }
    public enum LoS_Status { Visible, Explored, Unexplored }
    public enum Tag { Open, Forest, Urban }
    public delegate bool Hex_Requirements(WorldMapHex hex);

    public string Terrain { get; set; }
    public Yields Base_Yields { get; set; }
    public string Texture { get; private set; }
    public int Elevation { get; private set; }
    public int Height { get; private set; }
    public WorldMapEntity Entity { get; set; }
    public WorldMapEntity Civilian { get; set; }
    public List<WorldMapEntity> In_LoS_Of { get; private set; }
    public bool In_LoS_Of_City { get; set; }
    public City City { get; set; }
    public Village Village { get; set; }
    public Improvement Improvement { get; set; }
    public float Base_Happiness { get; set; }
    public float Base_Health { get; set; }
    public float Base_Order { get; set; }
    public Mineral Mineral { get; private set; }
    public bool Can_Spawn_Minerals { get; private set; }
    public List<City> In_Work_Range_Of { get; private set; }
    public Village Worked_By_Village { get; set; }
    public List<Tag> Tags { get; private set; }
    public Road Road { get; set; }
    public bool Is_Map_Edge_Road_Connection { get; set; }
    public bool Is_Water { get; set; }

    private LoS_Status current_los;
    private InfoText current_text;
    private Map map;
    private List<Player> explored_by;
    private Player owner;
    private List<Player> prospected_by;
    private float movement_cost;

    public WorldMapHex(int q, int r, GameObject parent, WorldMapHex prototype, Map map) : base(q, r, parent)
    {
        Change_To(prototype);
        this.map = map;
        current_text = InfoText.None;
        explored_by = new List<Player>();
        current_los = LoS_Status.Visible;
        In_LoS_Of = new List<WorldMapEntity>();
        In_LoS_Of_City = false;
        prospected_by = new List<Player>();
        In_Work_Range_Of = new List<City>();
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    public WorldMapHex(string terrain, string texture, Yields yields, float happiness, float health, float order, float movement_cost, int elevation, int height,
        bool can_spawn_minerals, List<Tag> tags) : base()
    {
        Terrain = terrain;
        Texture = texture;
        Base_Yields = new Yields(yields);
        Base_Movement_Cost = movement_cost;
        Elevation = elevation;
        Height = height;
        Base_Happiness = happiness;
        Base_Health = health;
        Base_Order = order;
        Can_Spawn_Minerals = can_spawn_minerals;
        Tags = new List<Tag>();
        foreach (Tag tag in tags) {
            Tags.Add(tag);
        }
        Is_Water = false;
    }

    public void Change_To(WorldMapHex prototype)
    {
        Terrain = prototype.Terrain;
        Texture = prototype.Texture;
        Base_Yields = new Yields(prototype.Base_Yields);
        Base_Movement_Cost = prototype.Base_Movement_Cost;
        Elevation = prototype.Elevation;
        Height = prototype.Height;
        Base_Happiness = prototype.Base_Happiness;
        Base_Health = prototype.Base_Health;
        Base_Order = prototype.Base_Order;
        Can_Spawn_Minerals = prototype.Can_Spawn_Minerals;
        Is_Water = prototype.Is_Water;
        Tags = new List<Tag>();
        foreach (Tag tag in prototype.Tags) {
            Tags.Add(tag);
        }
        SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(Texture, SpriteManager.SpriteType.Terrain);
    }
    
    public float Movement_Cost
    {
        get {
            if (Is_Water) {
                return -1.0f;
            }
            if(Road == null) {
                return movement_cost;
            }
            return movement_cost * (1.0f - Road.Movement_Cost_Reduction);
        }
        private set {
            movement_cost = value;
        }
    }

    public bool Has_Harbor
    {
        get {
            return (City != null && City.Is_Coastal) || (Village != null && Village.Is_Coastal);
        }
    }

    public float Water_Movement_Cost
    {
        get {
            //TODO: Can coastal citys and villages be pathed through?
            if (!Is_Water && !Has_Harbor) {
                return -1.0f;
            }
            return movement_cost;
        }
    }

    public float Base_Movement_Cost
    {
        get {
            return movement_cost;
        }
        private set {
            movement_cost = value;
        }
    }

    public float Get_Movement_Cost(Map.MovementType type)
    {
        if (type == Map.MovementType.Water || (type == Map.MovementType.Amphibious && Is_Water))
            return Water_Movement_Cost;
        return Movement_Cost;
    }

    public Yields Yields
    {
        get {
            Yields y = new Yields(Base_Yields);
            if(Improvement != null) {
                y.Add(Improvement.Yields);
            }
            return y;
        }
    }

    public float Happiness
    {
        get {
            if(Improvement == null) {
                return Base_Happiness;
            }
            return Base_Happiness + Improvement.Happiness;
        }
    }

    public float Health
    {
        get {
            if (Improvement == null) {
                return Base_Health;
            }
            return Base_Health + Improvement.Health;
        }
    }

    public float Order
    {
        get {
            if (Improvement == null) {
                return Base_Order;
            }
            return Base_Order + Improvement.Order;
        }
    }

    public List<WorldMapHex> Get_Adjancent_Hexes()
    {
        List<WorldMapHex> hexes = new List<WorldMapHex>();
        foreach(Coordinates c in Coordinates.Get_Adjanced_Coordinates().Select(x => x.Value).ToArray()) {
            if(map.Get_Hex_At(c) != null) {
                hexes.Add(map.Get_Hex_At(c));
            }
        }
        return hexes;
    }

    /// <summary>
    /// TODO: Duplicated code: CombatMapHex
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public List<WorldMapHex> Get_Hexes_Around(int range)
    {
        if (range <= 0) {
            CustomLogger.Instance.Error("Invalid argument: range = " + range);
            return null;
        }
        Dictionary<WorldMapHex, int> hexes = new Dictionary<WorldMapHex, int>();
        foreach (WorldMapHex adjancent_hex in Get_Adjancent_Hexes()) {
            Get_Hexes_Around_Recursive(adjancent_hex, range, 0, hexes);
        }
        return hexes.Select(x => x.Key).ToList();
    }

    private void Get_Hexes_Around_Recursive(WorldMapHex hex, int range, int distance, Dictionary<WorldMapHex, int> hexes)
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
        foreach (WorldMapHex adjancent_hex in hex.Get_Adjancent_Hexes()) {
            Get_Hexes_Around_Recursive(adjancent_hex, range, distance, hexes);
        }
    }
    
    /// <summary>
    /// TODO: not 100% accurate
    /// </summary>
    /// <param name="requirements"></param>
    /// <returns></returns>
    public WorldMapHex Find_Closest_Hex(Hex_Requirements requirements)
    {
        List<WorldMapHex> adjancent_hexes = Get_Adjancent_Hexes();
        foreach (WorldMapHex adjancent_hex in adjancent_hexes) {
            if (requirements(adjancent_hex)) {
                return adjancent_hex;
            }
        }
        for(int i = 1; i < 1000; i++) {
            for(int x = Coordinates.X - i; x < Coordinates.X + i; x++) {
                for (int y = Coordinates.Y - i; y < Coordinates.Y + i; y++) {
                    if(map.Get_Hex_At(x, y) != null && requirements(map.Get_Hex_At(x, y))) {
                        return map.Get_Hex_At(x, y);
                    }
                }
            }
        }
        CustomLogger.Instance.Warning("Max iterations reached");
        return null;
    }

    public InfoText Current_Text
    {
        get {
            return current_text;
        }
        set {
            current_text = value;
            switch (current_text) {
                case InfoText.None:
                    text_game_object.SetActive(false);
                    break;
                case InfoText.Coordinates:
                    Show_Coordinates = true;
                    break;
                case InfoText.Yields:
                    if(Current_LoS != LoS_Status.Visible) {
                        text_game_object.SetActive(false);
                        break;
                    }
                    text_game_object.SetActive(true);
                    TextMesh.text = Yields.ToString();
                    break;
                case InfoText.Minerals:
                    if (Can_Spawn_Minerals && Visible_To_Viewing_Player) {
                        text_game_object.SetActive(true);
                        if (prospected_by.Contains(Main.Instance.Current_Player)) {
                            if(Mineral == null) {
                                TextMesh.text = "None";
                            } else {
                                TextMesh.text = Mineral.Name;
                            }
                        } else {
                            TextMesh.text = "???";
                        }
                    } else {
                        text_game_object.SetActive(false);
                    }
                    break;
            }
        }
    }

    public bool Passable
    {
        get {
            return Movement_Cost > 0.0f && !Is_Water;
        }
    }

    public bool Passable_For(WorldMapEntity entity)
    {
        return Base_Movement_Cost > 0.0f && (Has_Harbor || entity.Movement_Type == Map.MovementType.Amphibious || (entity.Movement_Type == Map.MovementType.Land && !Is_Water) ||
            (entity.Movement_Type == Map.MovementType.Water && Is_Water));
    }

    public override PathfindingNode PathfindingNode
    {
        get {
            //TODO: ?passing through units?
            return new PathfindingNode(Coordinates, GameObject.transform.position.x, GameObject.transform.position.y, Movement_Cost);
        }
    }

    public PathfindingNode Water_PathfindingNode
    {
        get {
            //TODO: ?passing through units?
            return new PathfindingNode(Coordinates, GameObject.transform.position.x, GameObject.transform.position.y, Water_Movement_Cost);
        }
    }
    
    public PathfindingNode Get_Specific_PathfindingNode(WorldMapEntity entity, WorldMapHex ignore_entity_hex = null, bool use_los = true)
    {
        bool blocked = ((entity.Is_Civilian && Civilian != null) || (!entity.Is_Civilian && Entity != null) && this != ignore_entity_hex);
        return new PathfindingNode(
            Coordinates,
            GameObject.transform.position.x,
            GameObject.transform.position.y,
            (use_los && entity.Owner != null && !Is_Explored_By(entity.Owner)) ? UNEXPLORED_MOVEMENT_COST :
            !blocked ? Get_Movement_Cost(entity.Movement_Type) : -1.0f
        );
    }

    public LoS_Status Current_LoS
    {
        get {
            return current_los;
        }
        set {
            current_los = value;
            ///TODO: Messy highlight system
            if((Main.Instance.Other_Players_Turn && !Main.Instance.Showning_AI_Moves) || map.Reveal_All) {
                return;
            }
            switch (current_los) {
                case LoS_Status.Visible:
                    SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(Texture, SpriteManager.SpriteType.Terrain);
                    Clear_Highlight();
                    if(Entity != null) {
                        Entity.GameObject.SetActive(true);
                    }
                    if(Civilian != null) {
                        Civilian.GameObject.SetActive(true);
                    }
                    if(Improvement != null) {
                        Improvement.GameObject.SetActive(true);
                    }
                    if(Road != null) {
                        Road.Active = true;
                    }
                    break;
                case LoS_Status.Explored:
                    SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(Texture, SpriteManager.SpriteType.Terrain);
                    Clear_Highlight();
                    Highlight = EXPLORED_TINT;
                    if (Entity != null) {
                        Entity.GameObject.SetActive(false);
                    }
                    if (Civilian != null) {
                        Civilian.GameObject.SetActive(false);
                    }
                    if (Improvement != null) {
                        Improvement.GameObject.SetActive(false);
                    }
                    if (Road != null) {
                        Road.Active = true;
                    }
                    break;
                case LoS_Status.Unexplored:
                    SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(not_explored_texture, SpriteManager.SpriteType.Terrain);
                    Clear_Highlight();
                    if (Entity != null) {
                        Entity.GameObject.SetActive(false);
                    }
                    if (Civilian != null) {
                        Civilian.GameObject.SetActive(false);
                    }
                    if (Improvement != null) {
                        Improvement.GameObject.SetActive(false);
                    }
                    if (Road != null) {
                        Road.Active = false;
                    }
                    break;
            }
        }
    }

    public bool Visible_To_Viewing_Player
    {
        get {
            return Highlight.r == 1.0f && Highlight.g == 1.0f && Highlight.b == 1.0f && Highlight.a == 1.0f && SpriteRenderer.sprite.name == Texture;
        }
    }

    /// <summary>
    /// TODO: Rename function?
    /// </summary>
    /// <param name="player"></param>
    public void In_LoS(Player player, WorldMapEntity entity)
    {
        Current_LoS = LoS_Status.Visible;
        if (!explored_by.Contains(player)) {
            explored_by.Add(player);
        }
        if(entity != null) {
            In_LoS_Of.Add(entity);
            if (!entity.Last_Hexes_In_Los.Contains(this)) {
                entity.Last_Hexes_In_Los.Add(this);
            }
        } else {
            In_LoS_Of_City = true;
        }
    }

    public void Set_Explored(Player player)
    {
        if (explored_by.Contains(player)) {
            return;
        }
        if (!Main.Instance.Other_Players_Turn) {
            Current_LoS = LoS_Status.Explored;
        }
        explored_by.Add(player);
    }

    /// <summary>
    /// TODO: Rename function?
    /// </summary>
    /// <param name="player"></param>
    public void Not_In_LoS(Player player)
    {
        Current_LoS = explored_by.Contains(player) ? LoS_Status.Explored : LoS_Status.Unexplored;
        In_LoS_Of_City = false;
        In_LoS_Of.Clear();
    }

    public bool Is_Explored_By(Player player)
    {
        return explored_by.Contains(player);
    }

    public bool Is_Prospected_By(Player player)
    {
        return prospected_by.Contains(player);
    }

    public List<WorldMapHex> Get_Hexes_In_LoS(int range)
    {
        List<WorldMapHex> hexes_to_be_checked = Get_Hexes_Around(range * 2);
        List<WorldMapHex> los = new List<WorldMapHex>() { this };
        foreach(WorldMapHex hex_to_be_checked in hexes_to_be_checked) {
            List<WorldMapHex> line = map.Straight_Line(this, hex_to_be_checked);
            float lenght = Distance(hex_to_be_checked) - Elevation - hex_to_be_checked.Height;

            /*Old code
            foreach(WorldMapHex line_hex in line) {
                if(line_hex.Coordinates.Equals(this.Coordinates) || line_hex.Coordinates.Equals(hex_to_be_checked.Coordinates)) {
                    continue;
                }
                lenght += line_hex.Height;
            }*/

            //-- New code start --

            float highest_point = 0.0f;
            foreach (WorldMapHex line_hex in line) {
                if (line_hex.Coordinates.Equals(this.Coordinates) || line_hex.Coordinates.Equals(hex_to_be_checked.Coordinates) || line_hex.Height <= highest_point) {
                    continue;
                }
                highest_point = line_hex.Height;
            }

            lenght += highest_point;

            //-- New code end --


            if (Mathf.RoundToInt(lenght) <= range) {
                los.Add(hex_to_be_checked);
            }
        }
        return los;
    }

    public int Distance(WorldMapHex hex)
    {
        return map.Straight_Line(this, hex).Count - 1;
    }
    
    new public Player Owner
    {
        get {
            return owner;
        }
        set {
            owner = value;
            base.Owner = value;
            if(owner != null && Improvement == null && City == null && Village == null) {
                Improvement = new Improvement(this, Is_Water ? Improvement.Default_Water : Improvement.Default);
            } else if(owner == null && Improvement != null && Improvement.Is_Default) {
                Improvement.Delete();
                Improvement = null;
            }
            if(Improvement != null) {
                Improvement.Update_Texture();
            }
        }
    }

    public void Spawn_Mineral()
    {
        if (!Can_Spawn_Minerals) {
            return;
        }
        Mineral = Mineral.Get_Mineral_Spawn(this);
    }

    public void Prospect(Player player)
    {
        if (!prospected_by.Contains(player)) {
            prospected_by.Add(player);
        }
    }

    public bool Is_At_Map_Edge(Map map)
    {
        foreach (Coordinates c in Coordinates.Get_Adjanced_Coordinates().Select(x => x.Value).ToArray()) {
            if (map.Get_Hex_At(c) == null) {
                return true;
            }
        }
        return false;
    }

    public TradePartner Trade_Partner
    {
        get {
            if(City != null) {
                return City;
            }
            if(Village != null) {
                return Village;
            }
            return null;
        }
    }

    new public void Delete()
    {
        if(Road != null) {
            Road.Destroy();
        }
        base.Delete();
    }
}
