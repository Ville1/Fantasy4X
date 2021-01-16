using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Improvement {
    public delegate void ImprovementUpdate(Improvement improvement);

    public string Texture { get; private set; }
    public string Inactive_Texture { get; private set; }
    public Yields Base_Yields { get; private set; }
    public Yields Special_Yield_Delta { get; set; }
    public bool Is_Default { get; private set; }
    public int LoS { get; private set; }
    public WorldMapHex Hex { get; private set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject.GetComponent<SpriteRenderer>(); } }
    public bool Destroyed { get; private set; }
    public List<string> Can_Be_Build_On { get; private set; }
    public int Build_Time { get; private set; }
    public Technology Technology_Required { get; private set; }
    public bool Extracts_Minerals { get; private set; }
    public ImprovementUpdate Update { get; private set; }
    public float Happiness_Delta { get; set; }
    public float Health_Delta { get; set; }
    public float Order_Delta { get; set; }
    public bool Is_Active { get { return Hex != null && Hex.Owner != null; } }
    public bool Requires_Nearby_City { get; private set; }
    public Faction Faction { get; private set; }

    private string name;
    private float happiness;
    private float health;
    private float order;
    private bool preview;
    private Player preview_player;

    public Improvement(WorldMapHex hex, Improvement prototype, Player preview_player = null)
    {
        Hex = hex;
        Name = prototype.Name;
        Faction = prototype.Faction;
        Texture = prototype.Texture;
        Inactive_Texture = prototype.Inactive_Texture;
        Base_Yields = new Yields(prototype.Base_Yields);
        Happiness = prototype.Happiness;
        Health = prototype.Health;
        Order = prototype.Order;
        Build_Time = prototype.Build_Time;
        LoS = prototype.LoS;
        Is_Default = prototype.Is_Default;
        Can_Be_Build_On = Helper.Copy_List(prototype.Can_Be_Build_On);
        Technology_Required = prototype.Technology_Required;
        Extracts_Minerals = prototype.Extracts_Minerals;
        Requires_Nearby_City = prototype.Requires_Nearby_City;
        Update = prototype.Update;
        Happiness_Delta = 0.0f;
        Health_Delta = 0.0f;
        Order_Delta = 0.0f;
        if(Update != null) {
            Update(this);
        }

        this.preview_player = preview_player;
        preview = preview_player != null;
        if (preview) {
            return;
        }
        GameObject = new GameObject();
        GameObject.name = Name;
        GameObject.transform.position = hex.GameObject.transform.position;
        GameObject.transform.parent = hex.GameObject.transform;
        GameObject.AddComponent<SpriteRenderer>();
        SpriteRenderer.sprite = SpriteManager.Instance.Get(!Is_Active && !string.IsNullOrEmpty(Inactive_Texture) ? Inactive_Texture : Texture,
            SpriteManager.SpriteType.Improvement);
        SpriteRenderer.sortingLayerName = SortingLayer.HEXES;
        SpriteRenderer.sortingOrder = Hex.SpriteRenderer.sortingOrder + 2;
        Destroyed = false;
        GameObject.SetActive(Hex.Visible_To_Viewing_Player);
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="name"></param>
    public Improvement(Faction faction, string name, string texture, string inactive_texture, Yields base_yields, float happiness, float health, float order, int build_time, int los,
        bool extracts_minerals, List<string> can_be_build_on, Technology requires_technology, ImprovementUpdate update)
    {
        Faction = faction;
        Name = name;
        Texture = texture;
        Inactive_Texture = inactive_texture;
        Base_Yields = base_yields;
        Happiness = happiness;
        Health = health;
        Order = order;
        Build_Time = build_time;
        LoS = los;
        Extracts_Minerals = extracts_minerals;
        Can_Be_Build_On = can_be_build_on;
        Technology_Required = requires_technology;
        Update = update;
        Requires_Nearby_City = true;
    }


    public string Name
    {
        get {
            if(Extracts_Minerals && Hex != null && Hex.Mineral != null) {
                return Hex.Mineral.Name_With_Improvement + " " + name;
            }
            return name;
        }
        set {
            name = value;
        }
    }

    /// <summary>
    /// Deletes improvement's GameObject
    /// </summary>
    public void Delete()
    {
        GameObject.Destroy(GameObject);
        Destroyed = true;
    }

    public List<WorldMapHex> Get_Hexes_In_LoS()
    {
        if(LoS == 0) {
            return new List<WorldMapHex>() { Hex };
        }
        return Hex.Get_Hexes_In_LoS(LoS);
    }

    public Yields Yields
    {
        get {
            Yields yields = new Yields(Base_Yields);
            if (Extracts_Minerals && Hex != null && Hex.Mineral != null && (!preview || Hex.Is_Prospected_By(preview_player))) {
                yields.Add(Hex.Mineral.Yields);
            }
            if(Special_Yield_Delta != null) {
                yields.Add(Special_Yield_Delta);
            }
            return yields;
        }
    }

    public float Happiness
    {
        get {
            if (Extracts_Minerals && Hex != null && Hex.Mineral != null) {
                return happiness + Hex.Mineral.Happiness;
            }
            return happiness + Happiness_Delta;
        }
        set {
            happiness = value;
        }
    }

    public float Health
    {
        get {
            if (Extracts_Minerals && Hex != null && Hex.Mineral != null) {
                return health + Hex.Mineral.Health;
            }
            return health + Health_Delta;
        }
        set {
            health = value;
        }
    }

    public float Order
    {
        get {
            if (Extracts_Minerals && Hex != null && Hex.Mineral != null) {
                return order + Hex.Mineral.Order;
            }
            return order + Order_Delta;
        }
        set {
            order = value;
        }
    }

    public Improvement Preview(WorldMapHex hex, Player player)
    {
        return new Improvement(hex, this, player);
    }

    public override string ToString()
    {
        return Name;
    }

    public string Get_Tooltip(Worker action_worker = null)
    {
        StringBuilder tooltip = new StringBuilder();
        Improvement i = action_worker != null ? Preview(action_worker.Hex, action_worker.Owner) : this;
        tooltip.Append(Name);
        if(action_worker != null) {
            int estimated_turns = action_worker.Construction_Time_Estimate(this);
            tooltip.Append(" ").Append(estimated_turns).Append(" turn").Append(Helper.Plural(estimated_turns));
        }
        if (!i.Yields.Empty) {
            tooltip.Append(Environment.NewLine).Append("Yields: ").Append(i.Yields);
        }
        if(i.Happiness != 0.0f) {
            tooltip.Append(Environment.NewLine).Append("Happiness: ").Append(Math.Round(i.Happiness, 1).ToString("0.0"));
        }
        if (i.Health != 0.0f) {
            tooltip.Append(Environment.NewLine).Append("Health: ").Append(Math.Round(i.Health, 1).ToString("0.0"));
        }
        if (i.Order != 0.0f) {
            tooltip.Append(Environment.NewLine).Append("Order: ").Append(Math.Round(i.Order, 1).ToString("0.0"));
        }
        return tooltip.ToString();
    }

    public void Update_Texture()
    {
        if (string.IsNullOrEmpty(Inactive_Texture)) {
            return;
        }
        SpriteRenderer.sprite = SpriteManager.Instance.Get(Is_Active ? Texture : Inactive_Texture, SpriteManager.SpriteType.Improvement);
    }

    public static Improvement Default
    {
        get {
            return new Improvement(null, "Hut", "hut", null, new Yields(), 0.0f, 0.0f, 0.0f, 0, 0, false, new List<string>(), null, null) { Is_Default = true };
        }
    }

    public static Improvement Default_Water
    {
        get {
            return new Improvement(null, "Boat", "boat", null, new Yields(), 0.0f, 0.0f, 0.0f, 0, 0, false, new List<string>(), null, null) { Is_Default = true };
        }
    }
}
