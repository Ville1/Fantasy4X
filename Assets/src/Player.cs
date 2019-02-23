using System.Collections.Generic;
using System.Linq;

public class Player {
    private static readonly string TECH_READY_SOUND_EFFECT = "tech_ready_sfx";
    private static readonly string DEFEAT_NOTIFICATION_TEXTURE = "disband";

    private static int current_id;


    public string Name { get; private set; }
    public int? Team { get; private set; }
    public AI AI { get; private set; }
    public List<City> Cities { get; private set; }
    public List<Village> Villages { get; private set; }
    public List<WorldMapEntity> WorldMapEntitys { get; private set; }
    public int Id { get; private set; }
    public Faction Faction { get; private set; }
    public float Cash { get; set; }
    public Technology Current_Technology { get; set; }
    public List<Technology> Researched_Technologies { get; private set; }
    public Technology Last_Technology_Researched { get; private set; }
    public Technology Root_Technology { get; private set; }
    public bool Is_Neutral { get; private set; }

    private bool defeated;
    private float score;
    private List<Notification> notification_queue;
    
    public City Capital
    {
        get {
            return (Cities == null || Cities.Count == 0) ? null : Cities[0];
        }
        set {
            if (Cities.Contains(value)) {
                Cities.Remove(value);
            }
            Cities.Insert(0, value);
        }
    }

    public Player(string name, AI.Level? ai, Faction faction, bool neutral = false)
    {
        Name = name;
        AI = ai != null ? new AI(ai.Value, this) : null;
        WorldMapEntitys = new List<WorldMapEntity>();
        Id = current_id;
        current_id++;
        Faction = faction;
        Cash = Faction.Starting_Cash;
        Researched_Technologies = new List<Technology>();
        Last_Technology_Researched = null;
        Root_Technology = Faction.Root_Technology.Clone(this);
        Root_Technology.Research_Acquired = Root_Technology.Research_Required;
        defeated = false;
        notification_queue = new List<Notification>();
        Cities = new List<City>();
        Villages = new List<Village>();
        Is_Neutral = neutral;
    }

    public void End_Turn()
    {
        foreach(WorldMapEntity entity in WorldMapEntitys) {
            while(entity.Stored_Path != null && entity.Current_Movement > 0.0f) {
                if (!entity.Follow_Stored_Path()) {
                    break;
                }
            }
            entity.End_Turn();
        }
        Capital.End_Turn();

        //TODO: move this to city.End_Turn()?
        if(Current_Technology != null) {
            Current_Technology.Research_Acquired += Total_Science;
        }
        Update_Score();
        NotificationManager.Instance.Clear_Notifications();
    }


    public void Start_Turn()
    {
        foreach (WorldMapEntity entity in WorldMapEntitys) {
            entity.Start_Turn();
        }

        foreach(Notification notification in notification_queue) {
            NotificationManager.Instance.Add_Notification(notification);
        }
        notification_queue.Clear();

        if (Current_Technology != null && Current_Technology.Is_Researched) {
            if (!Main.Instance.Other_Players_Turn) {
                AudioManager.Instance.Play_Sound_Effect(TECH_READY_SOUND_EFFECT);
            }
            Researched_Technologies.Add(Current_Technology);
            Last_Technology_Researched = Current_Technology;
            Current_Technology = null;
            SelectTechnologyPanelManager.Instance.Active = true;
            foreach (WorldMapEntity entity in WorldMapEntitys) {
                if(entity is Worker) {
                    (entity as Worker).Update_Actions_List();
                }
            }
        }
    }

    public float Income
    {
        get {
            float income = Faction.Passive_Income + EmpireModifiers.Passive_Income;
            foreach(City city in Cities) {
                income += city.Yields.Cash;
            }
            foreach(WorldMapEntity entity in WorldMapEntitys) {
                income -= entity.Upkeep;
            }
            return income;
        }
    }

    public float Total_Science
    {
        get {
            float science = 0.0f;
            foreach (City city in Cities) {
                science += city.Yields.Science;
            }
            return science;
        }
    }

    /// <summary>
    /// LoS of CURRENT PLAYER
    /// </summary>
    public Dictionary<WorldMapHex, WorldMapEntity> LoS
    {
        get {
            Dictionary<WorldMapHex, WorldMapEntity> los = new Dictionary<WorldMapHex, WorldMapEntity>();

            foreach(City city in Main.Instance.Current_Player.Cities) {
                foreach (WorldMapHex hex in city.Get_Hexes_In_LoS()) {
                    if (!los.ContainsKey(hex)) {
                        los.Add(hex, null);
                    }
                }
            }
            foreach (Village village in Main.Instance.Current_Player.Villages) {
                foreach (WorldMapHex hex in village.Get_Hexes_In_LoS()) {
                    if (!los.ContainsKey(hex)) {
                        los.Add(hex, null);
                    }
                }
            }
            foreach (WorldMapEntity entity in Main.Instance.Current_Player.WorldMapEntitys) {
                foreach (WorldMapHex hex in entity.Get_Hexes_In_LoS()) {
                    if (!los.ContainsKey(hex)) {
                        los.Add(hex, entity);
                    }
                }
            }

            return los;
        }
    }

    public bool Defeated
    {
        get {
            return defeated;
        }
        set {
            defeated = value;
            if (defeated) {
                if (Main.Instance.Check_If_Game_Ended()) {
                    return;
                } else {
                    NotificationManager.Instance.Add_Notification(new Notification(string.Format("{0} has been defeated!", Name),
                        DEFEAT_NOTIFICATION_TEXTURE, SpriteManager.SpriteType.UI, TECH_READY_SOUND_EFFECT, null));
                    ScoresGUIManager.Instance.Update_Scores();
                    CustomLogger.Instance.Debug(string.Format("Player #{0} ({1}) has been defeated", Id, Name));
                    //TODO: despawn units, free cities etc...
                }
            }
        }
    }

    public void Update_Score()
    {
        score = 0.0f;
        foreach(City city in Cities) {
            score += (city.Population + city.Happiness + city.Health + city.Order + city.Yields.Total);
        }
        foreach(Technology technology in Researched_Technologies) {
            score += (technology.Research_Required / 50.0f);
        }

        ScoresGUIManager.Instance.Update_Scores();
    }


    public float Score
    {
        get {
            return score;
        }
    }

    public override string ToString()
    {
        return string.Format("{0} (#{1})", Name, Id);
    }


    public EmpireModifiers EmpireModifiers
    {
        get {
            EmpireModifiers modifiers = new EmpireModifiers();
            foreach(Technology technology in Researched_Technologies) {
                modifiers.Add(technology.EmpireModifiers);
            }
            return modifiers;
        }
    }

    public void Queue_Notification(Notification notification)
    {
        if(AI != null) {
            return;
        }
        notification_queue.Add(notification);
    }

    public bool Has_Unlocked(Trainable unit)
    {
        return unit.Technology_Required == null || (Researched_Technologies.Any(x => x.Name == unit.Technology_Required.Name));
    }

    public bool Has_Unlocked(Building building)
    {
        return building.Technology_Required == null || (Researched_Technologies.Any(x => x.Name == building.Technology_Required.Name));
    }

    public class NewPlayerData
    {
        public string Name { get; set; }
        public AI.Level? AI { get; set; }
        public Faction Faction { get; set; }

        public NewPlayerData(string name, AI.Level? ai, Faction faction)
        {
            Name = name;
            AI = ai;
            Faction = faction;
        }

        public NewPlayerData() { }

        public override string ToString()
        {
            return Name;
        }
    }
}
