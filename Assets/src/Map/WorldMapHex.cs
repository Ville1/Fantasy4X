using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class WorldMapHex : Hex {
    private static readonly Color EXPLORED_TINT = new Color(0.5f, 0.5f, 0.5f, 1.0f);
    private static readonly float UNEXPLORED_MOVEMENT_COST = 10.0f;
    private static string not_explored_texture = "clouds";
    private static long current_icon_id = 0;
    private static float FREE_EMBARK_PATHFINDING_MOVEMENT_COST = 5.0f;

    public enum InfoText { None, Coordinates, Yields, Minerals }
    public enum LoS_Status { Visible, Explored, Unexplored }
    public enum Tag { Open, Forest, Urban, Hill, Underground, Special, Timber, Arid, Game, Structure, Cursed }
    public delegate bool Hex_Requirements(WorldMapHex hex);

    public string Terrain { get; set; }
    public string Internal_Name { get; private set; }
    public Yields Base_Yields { get; set; }
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
    public StatusEffectList<HexStatusEffect> Status_Effects { get; private set; }
    public Dictionary<string, int> CombatMap_Seed { get; private set; }
    public Dictionary<string, int> CombatMap_City_Seed { get; private set; }
    public Geography Georaphic_Feature { get; private set; }

    private LoS_Status current_los;
    private InfoText current_text;
    private Map map;
    private List<Player> explored_by;
    private Player owner;
    private List<Player> prospected_by;
    private float movement_cost;
    private GameObject yields_gameobject;
    private List<GameObject> yield_icons;

    public WorldMapHex(int q, int r, GameObject parent, WorldMapHex prototype, Map map) : base(q, r, parent, map.Height, prototype)
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
        Status_Effects = new StatusEffectList<HexStatusEffect>();
        Georaphic_Feature = null;
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    public WorldMapHex(string internal_name, string terrain, string sprite, Dictionary<string, int> alternative_sprites, Yields yields, float happiness, float health, float order, float movement_cost, int elevation, int height,
        bool can_spawn_minerals, List<Tag> tags, Dictionary<string, int> combatMap_seed, Dictionary<string, int> combatMap_City_Seed) : base(sprite, alternative_sprites)
    {
        Internal_Name = internal_name;
        Terrain = terrain;
        Base_Yields = new Yields(yields);
        Base_Movement_Cost = movement_cost;
        Elevation = elevation;
        Height = height;
        Base_Happiness = happiness;
        Base_Health = health;
        Base_Order = order;
        Can_Spawn_Minerals = can_spawn_minerals;
        Tags = tags;
        CombatMap_Seed = Helper.Copy_Dictionary(combatMap_seed);
        CombatMap_City_Seed = combatMap_City_Seed != null ? Helper.Copy_Dictionary(combatMap_City_Seed) : null;
        Is_Water = false;
        Georaphic_Feature = null;
    }

    public void Change_To(WorldMapHex prototype)
    {
        Internal_Name = prototype.Internal_Name;
        Terrain = prototype.Terrain;
        Base_Yields = new Yields(prototype.Base_Yields);
        Base_Movement_Cost = prototype.Base_Movement_Cost;
        Elevation = prototype.Elevation;
        Height = prototype.Height;
        Base_Happiness = prototype.Base_Happiness;
        Base_Health = prototype.Base_Health;
        Base_Order = prototype.Base_Order;
        Can_Spawn_Minerals = prototype.Can_Spawn_Minerals;
        Is_Water = prototype.Is_Water;
        Tags = Helper.Copy_List(prototype.Tags);
        CombatMap_Seed = Helper.Copy_Dictionary(prototype.CombatMap_Seed);
        CombatMap_City_Seed = prototype.CombatMap_City_Seed != null ? Helper.Copy_Dictionary(prototype.CombatMap_City_Seed) : null;
        yields_gameobject = null;
        yield_icons = null;
        Change_To(prototype as Hex);
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

    public float Get_Movement_Cost(WorldMapEntity entity)
    {
        if(entity is Army && entity.Owner.Has_Transport && Is_Water && (entity as Army).Free_Embarkment != null && (entity as Army).Free_Embarkment == Georaphic_Feature as BodyOfWaterData) {
            return FREE_EMBARK_PATHFINDING_MOVEMENT_COST;
        }
        return Get_Movement_Cost(entity.Movement_Type);
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
            foreach(HexStatusEffect status_effect in Status_Effects) {
                y.Add(status_effect.Yield_Delta);
            }
            return y;
        }
    }

    public float Happiness
    {
        get {
            float happiness = Base_Happiness;
            if(Improvement != null) {
                happiness += Improvement.Happiness;
            }
            foreach (HexStatusEffect status_effect in Status_Effects) {
                happiness += status_effect.Happiness;
            }
            return happiness;
        }
    }

    public float Health
    {
        get {
            float health = Base_Health;
            if (Improvement != null) {
                health += Improvement.Health;
            }
            foreach (HexStatusEffect status_effect in Status_Effects) {
                health += status_effect.Health;
            }
            return health;
        }
    }

    public float Order
    {
        get {
            float order = Base_Order;
            if (Improvement != null) {
                order += Improvement.Order;
            }
            foreach (HexStatusEffect status_effect in Status_Effects) {
                order += status_effect.Order;
            }
            return order;
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
        return Passable_For(entity.Movement_Type);
    }


    public bool Passable_For(WorldMapEntity entity, WorldMapHex new_hex)
    {
        return new_hex.Passable_For(entity.Get_Movement_Type(this, new_hex));
    }

    public bool Passable_For(Unit unit)
    {
        return Passable_For(unit.Tags.Contains(Unit.Tag.Amphibious) ? Map.MovementType.Amphibious : (unit.Tags.Contains(Unit.Tag.Naval) ? Map.MovementType.Water : Map.MovementType.Land));
    }

    public bool Passable_For(List<Unit> units)
    {
        return Passable_For(Unit.Get_Movement_Type(units));
    }

    private bool Passable_For(Map.MovementType movement_type)
    {
        if(movement_type == Map.MovementType.Immobile) {
            return false;
        }
        return Base_Movement_Cost > 0.0f && (Has_Harbor || movement_type == Map.MovementType.Amphibious || (movement_type == Map.MovementType.Land && !Is_Water) ||
            (movement_type == Map.MovementType.Water && Is_Water));
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
        bool blocked = false;// ((entity.Is_Civilian && Civilian != null) || (!entity.Is_Civilian && Entity != null) && this != ignore_entity_hex);

        //TODO: Clean this up
        if (!blocked && (City != null || Village != null) && entity.Owner != null && entity.Owner.AI != null && entity.Owner.AI is WildLifeAI) {
            blocked = true;
        }
        
        return new PathfindingNode(
            Coordinates,
            GameObject.transform.position.x,
            GameObject.transform.position.y,
            (use_los && entity.Owner != null && !Is_Explored_By(entity.Owner)) ? UNEXPLORED_MOVEMENT_COST :
            !blocked ? Get_Movement_Cost(entity) : -1.0f
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
                    SpriteRenderer.sprite = SpriteManager.Instance.Get(sprite, SpriteManager.SpriteType.Terrain);
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
                    SpriteRenderer.sprite = SpriteManager.Instance.Get(sprite, SpriteManager.SpriteType.Terrain);
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
                    SpriteRenderer.sprite = SpriteManager.Instance.Get(not_explored_texture, SpriteManager.SpriteType.Terrain);
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
            return Highlight.r == 1.0f && Highlight.g == 1.0f && Highlight.b == 1.0f && Highlight.a == 1.0f && SpriteRenderer.sprite.name == sprite;
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

    new public bool Is_Owned_By(Player player)
    {
        if (Owner == null) {
            return false;
        }
        return Owner.Id == player.Id;
    }

    new public bool Is_Owned_By_Current_Player
    {
        get {
            return Is_Owned_By(Main.Instance.Current_Player);
        }
    }

    new public bool Has_Owner
    {
        get {
            return Owner != null;
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

    public void End_Round()
    {
        Status_Effects.End_Turn();
    }

    public void Apply_Status_Effect(HexStatusEffect status_effect, bool stacks)
    {
        Status_Effects.Apply_Status_Effect(status_effect, stacks);
        if (!status_effect.Yield_Delta.Empty) {
            foreach(City city in In_Work_Range_Of) {
                city.Yields_Changed();
            }
        }
    }

    public string Status_Effect_Tooltip
    {
        get {
            if(Status_Effects.Count == 0) {
                return null;
            }
            StringBuilder tooltip = new StringBuilder("Status effects:");
            foreach(HexStatusEffect status_effect in Status_Effects) {
                tooltip.Append(Environment.NewLine).Append(status_effect.Tooltip);
            }
            return tooltip.ToString();
        }
    }

    public bool Has_Mineral(Mineral.Tag tag)
    {
        return Mineral != null && Mineral.Tags.Contains(tag);
    }

    public bool Has_Mineral(Mineral.Tag tag_1, Mineral.Tag tag_2)
    {
        return Mineral != null && Mineral.Tags.Contains(tag_1) && Mineral.Tags.Contains(tag_2);
    }

    public Army Army
    {
        get {
            if(Entity == null || !(Entity is Army)) {
                return null;
            }
            return Entity as Army;
        }
    }

    public bool Is_Animated
    {
        get {
            return Animation_Sprites != null && Animation_Sprites.Count != 0 && Animation_FPS > 0.0f;
        }
    }

    public void Update(float delta_s)
    {
        if (!Is_Animated || Current_LoS == LoS_Status.Unexplored) {
            return;
        }
        animation_frame_time_left -= delta_s;
        if (animation_frame_time_left <= 0.0f) {
            animation_frame_time_left += (1.0f / Animation_FPS);
            animation_index++;
            if (animation_index >= Animation_Sprites.Count) {
                animation_index = 0;
            }
            SpriteRenderer.sprite = animation_sprites[animation_index];
        }
    }

    public bool Show_Yields
    {
        get {
            return yields_gameobject != null && yield_icons != null && yields_gameobject.activeSelf;
        }
        set {
            if(!value && !Show_Yields) {
                return;
            }
            if (value) {
                if(yields_gameobject == null) {
                    yields_gameobject = GameObject.Instantiate(
                        PrefabManager.Instance.Hex_Yields,
                        new Vector3(
                            GameObject.transform.position.x,
                            GameObject.transform.position.y,
                            GameObject.transform.position.z
                        ),
                        Quaternion.identity,
                        GameObject.transform
                    );
                    yields_gameobject.name = "Yields";
                    yield_icons = new List<GameObject>();
                }
                yields_gameobject.SetActive(true);

                List<string> icons = new List<string>();
                float[] amounts = new float[7] { Yields.Food, Yields.Production, Yields.Cash, Yields.Science, Yields.Culture, Yields.Mana, Yields.Faith };
                string[][] icon_sprites = new string[7][] {
                    new string[3] { "food_icon_half", "food_icon", "food_icon_big" },
                    new string[3] { "production_icon_half", "production_icon", "production_icon_big" },
                    new string[3] { "cash_icon_half", "cash_icon", "cash_icon_big" },
                    new string[3] { "science_icon_half", "science_icon", "science_icon_big" },
                    new string[3] { "culture_icon_half", "culture_icon", "culture_icon_big" },
                    new string[3] { "mana_icon_half", "mana_icon", "mana_icon_big" },
                    new string[3] { "faith_icon_half", "faith_icon", "faith_icon_big" }
                };
                for(int i = 0; i < amounts.Length; i++) {
                    int big_amount = (int)(amounts[i] / 5.0f);
                    float rest = amounts[i] - (big_amount * 5.0f);
                    int full_amount = (int)rest;
                    float decimals = rest - full_amount;
                    if(decimals >= 0.75f) {
                        full_amount++;
                        decimals = 0.0f;
                    }
                    for(int y = 0; y < big_amount; y++) {
                        icons.Add(icon_sprites[i][2]);
                    }
                    for (int y = 0; y < full_amount; y++) {
                        icons.Add(icon_sprites[i][1]);
                    }
                    if(decimals != 0.0f) {
                        icons.Add(icon_sprites[i][0]);
                    }
                }

                float spacing = 0.25f;
                GameObject icon_prototype = GameObject.Find(string.Format("{0}/{1}/{2}", GameObject.name, yields_gameobject.name, "Icon"));
                icon_prototype.SetActive(true);
                for (int i = 0; i < icons.Count; i++) {
                    GameObject icon_gameobject = GameObject.Instantiate(
                        icon_prototype,
                        new Vector3(
                            yields_gameobject.transform.position.x + (i * spacing),
                            yields_gameobject.transform.position.y,
                            yields_gameobject.transform.position.z
                        ),
                        Quaternion.identity,
                        yields_gameobject.transform
                    );
                    icon_gameobject.name = string.Format("Icon#{0}", current_icon_id);
                    current_icon_id = current_icon_id == long.MaxValue ? 0 : current_icon_id + 1;
                    icon_gameobject.GetComponentInChildren<SpriteRenderer>().sprite = SpriteManager.Instance.Get(icons[i], SpriteManager.SpriteType.UI);
                    icon_gameobject.GetComponentInChildren<SpriteRenderer>().sortingOrder = i;
                    yield_icons.Add(icon_gameobject);
                }
                icon_prototype.SetActive(false);
                yields_gameobject.transform.position = new Vector3(
                    GameObject.transform.position.x - (icons.Count * (spacing * 0.5f)),
                    GameObject.transform.position.y,
                    GameObject.transform.position.z
                );
            } else {
                yields_gameobject.SetActive(false);
                Helper.Delete_All(yield_icons);
            }
        }
    }

    public void Assign_To_Feature(Geography data)
    {
        if (data.Hexes.Contains(this)) {
            CustomLogger.Instance.Warning("Hex is already assigned");
            return;
        }
        data.Hexes.Add(this);
        Georaphic_Feature = data;
    }
    
    public WorldMapHexSaveData Save_Data
    {
        get {
            WorldMapHexSaveData data = new WorldMapHexSaveData();
            data.Q = Q;
            data.R = R;
            data.S = S;
            data.Internal_Name = Internal_Name;
            data.Mineral = Mineral != null ? Mineral.Internal_Name : null;
            data.CombatMap_City_Seed = CombatMap_City_Seed == null ? null : CombatMap_City_Seed.Select(x => new WorldMapHexSeedSaveData() { Key = x.Key, Value = x.Value }).ToList();
            data.Current_Los = (int)current_los;
            data.Explored_By = explored_by.Select(x => SaveManager.Get_Player_Id(x)).ToList();
            data.Owner = SaveManager.Get_Player_Id(Owner);
            data.Prospected_By = prospected_by.Select(x => SaveManager.Get_Player_Id(x)).ToList();
            data.Road = Road == null ? null : Road.Internal_Name;
            if(Improvement != null) {
                data.Improvement = new ImprovementSaveData() {
                    Name = Improvement.Name,
                    Faction = Improvement.Faction == null ? null : Improvement.Faction.Name,
                    Special_Yield_Delta = Improvement.Special_Yield_Delta == null ? null : Improvement.Special_Yield_Delta.Save_Data,
                    Happiness_Delta = Improvement.Happiness_Delta,
                    Health_Delta = Improvement.Health_Delta,
                    Order_Delta = Improvement.Order_Delta
                };
            } else {
                data.Improvement = null;
            }
            data.Is_Map_Edge_Road_Connection = Is_Map_Edge_Road_Connection;
            data.Status_Effects = Status_Effects.Select(x => new WorldMapHexStatusEffectSaveData() {
                Name = x.Name,
                Yield_Delta = x.Yield_Delta.Save_Data,
                Duration = x.Duration,
                Current_Duration = x.Current_Duration,
                Parent_Duration = x.Parent_Duration.HasValue ? x.Parent_Duration.Value : -1,
                Happiness = x.Happiness,
                Health = x.Health,
                Order = x.Order
            }).ToList();
            data.Sprite_Index = Sprite_Index;
            data.Georaphic_Feature = Georaphic_Feature == null ? -1 : Georaphic_Feature.Id;
            return data;
        }
    }

    public void Load(WorldMapHexSaveData data)
    {
        Change_To(HexPrototypes.Instance.Get_World_Map_Hex(data.Internal_Name));
        CombatMap_City_Seed = data.CombatMap_City_Seed.ToDictionary(x => x.Key, x => x.Value);
        Mineral = !string.IsNullOrEmpty(data.Mineral) ? Mineral.Get_Prototype(data.Mineral) : null;
        Current_LoS = (LoS_Status)data.Current_Los;
        explored_by = data.Explored_By.Select(x => SaveManager.Get_Player(x)).ToList();
        prospected_by = data.Prospected_By.Select(x => SaveManager.Get_Player(x)).ToList();
        owner = SaveManager.Get_Player(data.Owner);
        Is_Map_Edge_Road_Connection = data.Is_Map_Edge_Road_Connection;
        if (!string.IsNullOrEmpty(data.Road)) {
            Road = new Road(this, HexPrototypes.Instance.Get_Road(data.Road));
        }
        if(data.Improvement != null && !string.IsNullOrEmpty(data.Improvement.Name)) {
            if (string.IsNullOrEmpty(data.Improvement.Faction)) {
                Improvement = new Improvement(this, Improvement.Default.Name == data.Improvement.Name ? Improvement.Default : Improvement.Default_Water);
            } else {
                Improvement = new Improvement(this, Factions.All.First(x => x.Name == data.Improvement.Faction).Improvements.First(x => x.Name == data.Improvement.Name));
            }
        }
        foreach(WorldMapHexStatusEffectSaveData effect_data in data.Status_Effects) {
            HexStatusEffect effect = new HexStatusEffect(effect_data.Name, effect_data.Duration);
            effect.Load(effect_data);
            Status_Effects.Apply_Status_Effect(effect, true);
        }
        Sprite_Index = data.Sprite_Index;
        if(data.Georaphic_Feature >= 0) {
            Georaphic_Feature = World.Instance.Map.Assign_Geographic_Feature(this, data);
        }
    }
}
