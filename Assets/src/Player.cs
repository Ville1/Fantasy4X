using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player {
    private static readonly string TECH_READY_SOUND_EFFECT = "tech_ready_sfx";
    private static readonly string BLESSING_EXPIRED_SOUND_EFFECT = "tech_ready_sfx";
    private static readonly string DEFEAT_NOTIFICATION_TEXTURE = "disband";
    private static readonly int MAX_ACTIVE_BLESSINGS = 1;

    private static int current_id;
    
    public string Name { get; private set; }
    public int? Team { get; private set; }
    public I_AI AI { get; private set; }
    public List<City> Cities { get; private set; }
    public List<Village> Villages { get; private set; }
    public List<WorldMapEntity> World_Map_Entities { get; private set; }
    public int Id { get; private set; }
    public Faction Faction { get; private set; }
    public float Cash { get; set; }
    public Technology Current_Technology { get; set; }
    public List<Technology> Researched_Technologies { get; private set; }
    public Technology Last_Technology_Researched { get; private set; }
    public Technology Root_Technology { get; private set; }
    public bool Is_Neutral { get; private set; }
    public StatusEffectList<EmpireModifierStatusEffect> Status_Effects { get; private set; }

    private Dictionary<string, object> temp_data;
    private bool defeated;
    private float score;
    private List<Notification> notification_queue;
    private float mana;
    private CooldownManager<Spell> spells_on_cooldown;
    private CooldownManager<Blessing> blessings_on_cooldown;
    private Dictionary<Blessing, int> active_blessings;

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
        World_Map_Entities = new List<WorldMapEntity>();
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
        spells_on_cooldown = new CooldownManager<Spell>();
        blessings_on_cooldown = new CooldownManager<Blessing>();
        active_blessings = new Dictionary<Blessing, int>();
        Status_Effects = new StatusEffectList<EmpireModifierStatusEffect>();
        temp_data = new Dictionary<string, object>();

        if (faction.Uses_Special_AI) {
            switch (faction.Name) {
                case "Bandits":
                    if (!ai.HasValue) {
                        CustomLogger.Instance.Error("AI level is required");
                        AI = null;
                    } else {
                        AI = new BanditAI(this, ai.Value);
                    }
                    break;
                case "Wild Life":
                    AI = new WildLifeAI(this);
                    break;
                default:
                    AI = null;
                    CustomLogger.Instance.Error(string.Format("Faction {0} (#{1}) lacks a special AI", faction.Name, faction.Id));
                    break;
            }
        } else {
            AI = ai != null ? new AI(ai.Value, this) : null;
        }
    }
    
    public void End_Turn()
    {
        foreach(WorldMapEntity entity in World_Map_Entities) {
            while(entity.Stored_Path != null && entity.Current_Movement > 0.0f) {
                if (!entity.Follow_Stored_Path()) {
                    break;
                }
            }
            entity.End_Turn();
        }
        if(Capital != null) {
            Capital.End_Turn();
        }

        //TODO: move this to city.End_Turn()?
        if(Current_Technology != null) {
            Current_Technology.Research_Acquired += Total_Science;
        }

        //Cooldowns
        spells_on_cooldown.End_Turn();
        blessings_on_cooldown.End_Turn();

        //Active blessing duration
        List<Blessing> expiring_blessings = new List<Blessing>();
        List<Blessing> blessing_keys = new List<Blessing>(active_blessings.Keys);
        foreach (Blessing active_blessing in blessing_keys) {
            active_blessings[active_blessing]--;
            if (active_blessings[active_blessing] <= 0) {
                expiring_blessings.Add(active_blessing);
            }
        }
        foreach (Blessing expiring_blessing in expiring_blessings) {
            active_blessings.Remove(expiring_blessing);
            if (expiring_blessing.Deactivation != null) {
                Blessing.BlessingResult result = expiring_blessing.Deactivation(expiring_blessing, this, false);
                expiring_blessing.Play_Animation(result);
            }
            Queue_Notification(new Notification(string.Format("{0} has expired", expiring_blessing.Name), "faith", SpriteManager.SpriteType.UI, BLESSING_EXPIRED_SOUND_EFFECT, delegate() {
                BlessingGUIManager.Instance.Active = true;
            }));
        }

        //Status effects
        Status_Effects.End_Turn();

        Update_Score();
        NotificationManager.Instance.Clear_Notifications();
    }


    public void Start_Turn()
    {
        foreach(Notification notification in notification_queue) {
            NotificationManager.Instance.Add_Notification(notification);
        }
        notification_queue.Clear();
        if (Mana >= Max_Mana && Mana_Income > 0.0f) {
            NotificationManager.Instance.Add_Notification(new Notification("Mana pool is full", "mana", SpriteManager.SpriteType.UI, null, null));
        }

        if (Current_Technology != null && Current_Technology.Is_Researched) {
            if (!Main.Instance.Other_Players_Turn) {
                AudioManager.Instance.Play_Sound_Effect(TECH_READY_SOUND_EFFECT);
            }
            Researched_Technologies.Add(Current_Technology);
            Last_Technology_Researched = Current_Technology;
            Current_Technology = null;
            SelectTechnologyPanelManager.Instance.Active = true;
            foreach (WorldMapEntity entity in World_Map_Entities) {
                if(entity is Worker) {
                    (entity as Worker).Update_Actions_List();
                }
            }
        }

        foreach(KeyValuePair<Blessing, int> active_blessing in active_blessings) {
            if(active_blessing.Key.Turn_Start != null) {
                Blessing.BlessingResult result = active_blessing.Key.Turn_Start(active_blessing.Key, this, active_blessing.Value);
                active_blessing.Key.Play_Animation(result);
            }
        }

        foreach (WorldMapEntity entity in World_Map_Entities) {
            entity.Start_Turn();
        }

        foreach(City city in Cities) {
            city.Start_Turn();
        }
    }

    public float Income
    {
        get {
            float income = EmpireModifiers.Passive_Income;
            foreach(City city in Cities) {
                income += city.Yields.Cash;
            }
            foreach(WorldMapEntity entity in World_Map_Entities) {
                income -= entity.Upkeep;
                if(entity is Army) {
                    income += (entity as Army).Raiding_Income;
                }
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

    public float Mana
    {
        get {
            return mana;
        }
        set {
            if(value > 0 && mana > Max_Mana) {
                return;
            }
            mana = Mathf.Clamp(value, 0.0f, Max_Mana);
        }
    }

    public float Max_Mana
    {
        get {
            float max_mana = EmpireModifiers.Max_Mana;
            foreach(City city in Cities) {
                max_mana += city.Max_Mana;
            }
            return max_mana;
        }
    }

    public float Mana_Income
    {
        get {
            float income = 0.0f;
            foreach (City city in Cities) {
                income += city.Yields.Mana;
            }
            foreach (WorldMapEntity entity in World_Map_Entities) {
                income -= entity.Mana_Upkeep;
            }
            return income;
        }
    }

    public float Faith_Income
    {
        get {
            float income = 0.0f;
            foreach (City city in Cities) {
                income += city.Yields.Faith;
            }
            return income;
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
            foreach (WorldMapEntity entity in Main.Instance.Current_Player.World_Map_Entities) {
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
            EmpireModifiers modifiers = new EmpireModifiers() {
                Percentage_Village_Yield_Bonus = new Yields(100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f),
                Trade_Route_Yield_Bonus = new Yields(100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f)
            };
            modifiers.Add(Faction.EmpireModifiers);
            foreach(Technology technology in Researched_Technologies) {
                modifiers.Add(technology.EmpireModifiers);
            }
            foreach(EmpireModifierStatusEffect status_effect in Status_Effects) {
                modifiers.Add(status_effect.Modifiers);
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

    public List<Spell> Available_Spells
    {
        get {
            List<Spell> spells = new List<Spell>();
            foreach(Spell faction_spell in Faction.Spells) {
                if(faction_spell.Technology_Required == null || Researched_Technologies.Any(x => x.Name == faction_spell.Technology_Required.Name)) {
                    spells.Add(faction_spell);
                }
            }
            return spells;
        }
    }

    public List<Blessing> Available_Blessings
    {
        get {
            List<Blessing> blessings = new List<Blessing>();
            foreach (Blessing faction_blessing in Faction.Blessings) {
                if (faction_blessing.Technology_Required == null || Researched_Technologies.Any(x => x.Name == faction_blessing.Technology_Required.Name)) {
                    blessings.Add(faction_blessing);
                }
            }
            return blessings;
        }
    }

    public Dictionary<Blessing, int> Active_Blessings
    {
        get {
            Dictionary<Blessing, int> blessings = new Dictionary<Blessing, int>();
            foreach (KeyValuePair<Blessing, int> active_blessing_data in active_blessings) {
                blessings.Add(active_blessing_data.Key, active_blessing_data.Value);
            }
            return blessings;
        }
    }

    public bool Can_Cast(Spell spell)
    {
        return spell.Mana_Cost <= Mana && Spell_Cooldown(spell) == 0 && (spell.Technology_Required == null || Researched_Technologies.Any(x => x.Name == spell.Technology_Required.Name));
    }

    public bool Can_Cast(Blessing blessing)
    {
        return active_blessings.Count < MAX_ACTIVE_BLESSINGS && blessing.Faith_Required <= Faith_Income && Blessing_Cooldown(blessing) == 0 &&
            !active_blessings.Any(x => x.Key.Name == blessing.Name) && (blessing.Technology_Required == null || Researched_Technologies.Any(x => x.Name == blessing.Technology_Required.Name));
    }

    public int Spell_Cooldown(Spell spell)
    {
        return spells_on_cooldown.Get_Cooldown(spell);
    }

    public int Blessing_Cooldown(Blessing blessing)
    {
        return blessings_on_cooldown.Get_Cooldown(blessing);
    }

    public void Put_On_Cooldown(Spell spell)
    {
        spells_on_cooldown.Set_Cooldown(spell);
    }

    public void Put_On_Cooldown(Blessing blessing)
    {
        blessings_on_cooldown.Set_Cooldown(blessing);
    }

    public void Apply_Blessing(Blessing blessing)
    {
        while(active_blessings.Any(x => x.Key.Name == blessing.Name)) {
            Blessing old_blessing = active_blessings.First(x => x.Key.Name == blessing.Name).Key;
            if(old_blessing.Deactivation != null) {
                Blessing.BlessingResult result = old_blessing.Deactivation(old_blessing, this, true);
                old_blessing.Play_Animation(result);
            }
            active_blessings.Remove(old_blessing);
        }
        active_blessings.Add(blessing, blessing.Duration);
    }

    public void Apply_Status_Effect(EmpireModifierStatusEffect status_effect, bool stacks)
    {
        Status_Effects.Apply_Status_Effect(status_effect, stacks);
        if(!status_effect.Modifiers.Percentage_Village_Yield_Bonus.Empty || !status_effect.Modifiers.Village_Yield_Bonus.Empty ||
            !status_effect.Modifiers.Trade_Route_Yield_Bonus.Empty) {
            foreach(City city in Cities) {
                city.Yields_Changed();
            }
        }
    }

    public void Store_Temp_Data(string key, object data)
    {
        if (temp_data.ContainsKey(key)) {
            temp_data[key] = data;
        } else {
            temp_data.Add(key, data);
        }
    }

    public T Get_Temp_Data<T>(string key, bool remove = false)
    {
        if (!remove) {
            return (T)temp_data[key];
        }
        T data = (T)temp_data[key];
        temp_data.Remove(key);
        return data;
    }

    public void Load_Pre_Map(PlayerSaveData data)
    {
        Id = data.Id;
        Team = data.Team != -1 ? data.Team : (int?)null;
        Cash = data.Cash;
        mana = data.Mana;
        Current_Technology = string.IsNullOrEmpty(data.Current_Technology) ? null : new Technology(Faction.Technologies.First(x => x.Name == data.Current_Technology), this);
        if(Current_Technology != null) {
            Current_Technology.Research_Acquired = data.Research_Acquired;
        }
        Last_Technology_Researched = string.IsNullOrEmpty(data.Last_Technology_Researched) ? null : Faction.Technologies.First(x => x.Name == data.Last_Technology_Researched);
        Researched_Technologies = data.Researched_Technologies.Select(x => Faction.Technologies.First(y => y.Name == x)).ToList();
        foreach(Technology tech in Root_Technology.All_Techs_This_Leads_To) {
            if(Researched_Technologies.Exists(x => x.Name == tech.Name)) {
                tech.Research_Acquired = tech.Research_Required;
            } else if(data.Technologies_In_Progress.Exists(x => x.Name == tech.Name)) {
                tech.Research_Acquired = data.Technologies_In_Progress.First(x => x.Name == tech.Name).Progress;
            }
        }
        foreach(EmpireModifierStatusEffectSaveData effect_data in data.Status_Effects) {
            EmpireModifierStatusEffect effect = new EmpireModifierStatusEffect(effect_data.Name, effect_data.Duration, new EmpireModifiers() {
                Unit_Training_Speed_Bonus = effect_data.Modifiers.Unit_Training_Speed_Bonus,
                Building_Constuction_Speed_Bonus = effect_data.Modifiers.Building_Constuction_Speed_Bonus,
                Improvement_Constuction_Speed_Bonus = effect_data.Modifiers.Improvement_Constuction_Speed_Bonus,
                Passive_Income = effect_data.Modifiers.Passive_Income,
                Max_Mana = effect_data.Modifiers.Max_Mana,
                Population_Growth_Bonus = effect_data.Modifiers.Population_Growth_Bonus,
                Village_Yield_Bonus = new Yields(effect_data.Modifiers.Village_Yield_Bonus),
                Percentage_Village_Yield_Bonus = new Yields(effect_data.Modifiers.Percentage_Village_Yield_Bonus),
                Trade_Route_Yield_Bonus = new Yields(effect_data.Modifiers.Trade_Route_Yield_Bonus)
            });
            effect.Current_Duration = effect_data.Current_Duration;
            Status_Effects.Apply_Status_Effect(effect, true);
        }
        foreach(CooldownSaveData cooldown_data in data.Spells_On_Cooldown) {
            spells_on_cooldown.Set_Cooldown(Faction.Spells.First(x => x.Name == cooldown_data.Name), cooldown_data.Value);
        }
        foreach (CooldownSaveData cooldown_data in data.Blessings_On_Cooldown) {
            blessings_on_cooldown.Set_Cooldown(Faction.Blessings.First(x => x.Name == cooldown_data.Name), cooldown_data.Value);
        }
        foreach (CooldownSaveData cooldown_data in data.Active_Blessings) {
            active_blessings.Add(Faction.Blessings.First(x => x.Name == cooldown_data.Name), cooldown_data.Value);
        }
    }

    public void Load_Post_Map(PlayerSaveData data)
    {
        foreach(ArmySaveData army_data in data.Armies) {
            WorldMapHex hex = World.Instance.Map.Get_Hex_At(army_data.Hex_X, army_data.Hex_Y);
            Army army = new Army(hex, Faction.Army_Prototype, this, null);
            hex.Entity = army;
            foreach(UnitSaveData unit_data in army_data.Units) {
                Unit unit = new Unit(Faction.Units.First(x => x.Name == unit_data.Name) as Unit);
                unit.Load(unit_data);
                army.Add_Unit(unit);
            }
            army.Stored_Path = army_data.Path == null || army_data.Path.Count == 0 ? null : army_data.Path.Select(x => World.Instance.Map.Get_Hex_At(x.X, x.Y)).ToList();
            army.Sleep = army_data.Sleep;
            World_Map_Entities.Add(army);
        }
        foreach(WorkerSaveData worker_data in data.Workers) {
            WorldMapHex hex = World.Instance.Map.Get_Hex_At(worker_data.Hex_X, worker_data.Hex_Y);
            Worker worker = new Worker(hex, Faction.Units.First(x => x.Name == worker_data.Name) as Worker, this);
            worker.Load(worker_data);
            World_Map_Entities.Add(worker);
        }
        foreach (ProspectorSaveData prospector_data in data.Prospectors) {
            WorldMapHex hex = World.Instance.Map.Get_Hex_At(prospector_data.Hex_X, prospector_data.Hex_Y);
            Prospector prospector = new Prospector(hex, Faction.Units.First(x => x.Name == prospector_data.Name) as Prospector, this);
            prospector.Load(prospector_data);
            World_Map_Entities.Add(prospector);
        }
        if(AI != null) {
            (AI as AI).Load(data.AI_Data);
        }
    }

    public PlayerSaveData Save_Data
    {
        get {
            PlayerSaveData data = new PlayerSaveData();
            data.Id = Id;
            data.Name = Name;
            data.AI_Level = AI != null ? (int)AI.AI_Level : -1;
            data.Faction = Faction.Name;
            data.Team = Team.HasValue ? Team.Value : -1;
            data.Cash = Cash;
            data.Mana = mana;
            data.Villages = Villages.Select(x => x.Id).ToList();
            data.Cities = Cities.Select(x => x.Id).ToList();
            data.Workers = new List<WorkerSaveData>();
            data.Prospectors = new List<ProspectorSaveData>();
            data.Armies = new List<ArmySaveData>();
            foreach(WorldMapEntity entity in World_Map_Entities) {
                if(entity is Army) {
                    data.Armies.Add((entity as Army).Save_Data);
                } else if(entity is Worker) {
                    data.Workers.Add((entity as Worker).Save_Data);
                } else if (entity is Prospector) {
                    data.Prospectors.Add((entity as Prospector).Save_Data);
                } else {
                    CustomLogger.Instance.Error("Type {0} can't be serialized", entity.GetType().FullName);
                }
            }
            data.Current_Technology = Current_Technology == null ? null : Current_Technology.Name;
            data.Research_Acquired = Current_Technology == null ? 0.0f : Current_Technology.Research_Acquired;
            data.Last_Technology_Researched = Last_Technology_Researched == null ? null : Last_Technology_Researched.Name;
            data.Researched_Technologies = Researched_Technologies.Select(x => x.Name).ToList();
            data.Technologies_In_Progress = Root_Technology.All_Techs_This_Leads_To.Where(x => x.Research_Acquired != 0.0f && x.Research_Acquired != x.Research_Required).Select(x => new TechnologySaveData() {
                Name = x.Name,
                Progress = x.Research_Acquired
            }).ToList();
            data.Status_Effects = Status_Effects.Select(x => new EmpireModifierStatusEffectSaveData() {
                Name = x.Name,
                Duration = x.Duration,
                Current_Duration = x.Current_Duration,
                Modifiers = x.Modifiers.Save_Data
            }).ToList();
            data.Spells_On_Cooldown = spells_on_cooldown.Select(x => new CooldownSaveData() { Name = x.Name, Value = spells_on_cooldown.Get_Cooldown(x) }).ToList();
            data.Blessings_On_Cooldown = blessings_on_cooldown.Select(x => new CooldownSaveData() { Name = x.Name, Value = blessings_on_cooldown.Get_Cooldown(x) }).ToList();
            data.Active_Blessings = active_blessings.Select(x => new CooldownSaveData() { Name = x.Key.Name, Value = x.Value }).ToList();
            data.AI_Data = AI == null ? null : (AI as AI).Save_Data;
            return data;
        }
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
