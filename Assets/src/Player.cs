﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private float mana;
    private Dictionary<Spell, int> spells_on_cooldown;
    private Dictionary<Blessing, int> blessings_on_cooldown;
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
        spells_on_cooldown = new Dictionary<Spell, int>();
        blessings_on_cooldown = new Dictionary<Blessing, int>();
        active_blessings = new Dictionary<Blessing, int>();
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

        //TODO: Messy code
        //Cooldowns
        //Spells
        List<Spell> spells_off_cooldown = new List<Spell>();
        List<Spell> spell_keys = new List<Spell>(spells_on_cooldown.Keys);
        foreach(Spell spell_on_cooldown in spell_keys) {
            spells_on_cooldown[spell_on_cooldown]--;
            if(spells_on_cooldown[spell_on_cooldown] <= 0) {
                spells_off_cooldown.Add(spell_on_cooldown);
            }
        }
        foreach(Spell spell_off_cooldown in spells_off_cooldown) {
            spells_on_cooldown.Remove(spell_off_cooldown);
        }
        //Blessings
        List<Blessing> blessings_off_cooldown = new List<Blessing>();
        List<Blessing> blessing_keys = new List<Blessing>(blessings_on_cooldown.Keys);
        foreach (Blessing blessing_on_cooldown in blessing_keys) {
            blessings_on_cooldown[blessing_on_cooldown]--;
            if (blessings_on_cooldown[blessing_on_cooldown] <= 0) {
                blessings_off_cooldown.Add(blessing_on_cooldown);
            }
        }
        foreach (Blessing blessing_off_cooldown in blessings_off_cooldown) {
            blessings_on_cooldown.Remove(blessing_off_cooldown);
        }
        //Active blessing duration
        List<Blessing> expiring_blessings = new List<Blessing>();
        blessing_keys = new List<Blessing>(active_blessings.Keys);
        foreach (Blessing active_blessing in blessing_keys) {
            active_blessings[active_blessing]--;
            if (active_blessings[active_blessing] <= 0) {
                expiring_blessings.Add(active_blessing);
            }
        }
        foreach (Blessing expiring_blessing in expiring_blessings) {
            active_blessings.Remove(expiring_blessing);
            if (expiring_blessing.Deactivation != null) {
                Blessing.BlessingResult result = expiring_blessing.Deactivation(expiring_blessing, this);
                expiring_blessing.Play_Animation(result);
            }
        }

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
            foreach (WorldMapEntity entity in WorldMapEntitys) {
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

        foreach (WorldMapEntity entity in WorldMapEntitys) {
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
            foreach (WorldMapEntity entity in WorldMapEntitys) {
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
            EmpireModifiers modifiers = new EmpireModifiers() {
                Percentage_Village_Yield_Bonus = new Yields(100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f),
                Trade_Route_Yield_Bonus = new Yields(100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f, 100.0f)
            };
            modifiers.Add(Faction.EmpireModifiers);
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

    public bool Can_Cast(Spell spell)
    {
        return spell.Mana_Cost <= Mana && Spell_Cooldown(spell) == 0 && (spell.Technology_Required == null || Researched_Technologies.Any(x => x.Name == spell.Technology_Required.Name));
    }

    public bool Can_Cast(Blessing blessing)
    {
        return blessing.Faith_Required <= Faith_Income && Blessing_Cooldown(blessing) == 0 && !active_blessings.Any(x => x.Key.Name == blessing.Name) &&
            (blessing.Technology_Required == null || Researched_Technologies.Any(x => x.Name == blessing.Technology_Required.Name));
    }

    public int Spell_Cooldown(Spell spell)
    {
        return spells_on_cooldown.ContainsKey(spell) ? spells_on_cooldown[spell] : 0;
    }

    public int Blessing_Cooldown(Blessing blessing)
    {
        return blessings_on_cooldown.ContainsKey(blessing) ? blessings_on_cooldown[blessing] : 0;
    }

    public void Put_On_Cooldown(Spell spell)
    {
        if(spell.Cooldown == 0) {
            return;
        }
        if (spells_on_cooldown.ContainsKey(spell)) {
            spells_on_cooldown[spell] = spell.Cooldown;
        } else {
            spells_on_cooldown.Add(spell, spell.Cooldown);
        }
    }

    public void Put_On_Cooldown(Blessing blessing)
    {
        if (blessing.Cooldown == 0) {
            return;
        }
        if (blessings_on_cooldown.ContainsKey(blessing)) {
            blessings_on_cooldown[blessing] = blessing.Cooldown;
        } else {
            blessings_on_cooldown.Add(blessing, blessing.Cooldown);
        }
    }

    public void Apply_Blessing(Blessing blessing)
    {
        while(active_blessings.Any(x => x.Key.Name == blessing.Name)) {
            Blessing old_blessing = active_blessings.First(x => x.Key.Name == blessing.Name).Key;
            if(old_blessing.Deactivation != null) {
                Blessing.BlessingResult result = old_blessing.Deactivation(old_blessing, this);
                old_blessing.Play_Animation(result);
            }
            active_blessings.Remove(old_blessing);
        }
        active_blessings.Add(blessing, blessing.Duration);
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
