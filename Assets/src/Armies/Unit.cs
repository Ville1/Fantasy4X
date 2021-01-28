using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Unit : Trainable
{
    private static readonly float zone_of_control_movement_cost = 999.0f;
    private static readonly float engagement_movement_cost = 999.0f;
    private static readonly float disengagement_movement_cost = 2.0f;
    private static readonly float damage_balancer = 10.0f;
    private static readonly float manpower_damage = 0.1f;
    private static readonly float default_morale_damage = 1.25f;
    private static readonly float default_attack_stamina_cost = 10.0f;
    private static readonly float default_ranged_stamina_cost = 10.0f;
    //private static readonly float default_run_stamina_cost = 10.0f;
    private static readonly float default_run_movement_cost = 0.50f;//Multiplicative
    private static readonly float default_stamina_regeneration = 0.1f;//Multiplicative
    private static readonly float default_morale_lost_when_running_with_no_stamina = 1.0f;//Points of morale lost per missing stamina
    private static readonly int on_rout_morale_damage_aoe_radios = 3;
    private static readonly float morale_damage_bonus_on_charge = 0.25f;
    private static readonly float ROUTED_RELATIVE_STRENGHT = 0.25f;
    private static readonly float ROUT_AOE_MORALE_LOSS = 1.00f;
    private static readonly float ROUT_AOE_MORALE_GAIN = 0.50f;
    private static readonly float[] MORALE_LOSS_ON_MELEE_ATTACK_NO_STAMINA = new float[2] { 1.0f, 0.1f };
    private static readonly float[] MORALE_LOSS_ON_RANGED_ATTACK_NO_STAMINA = new float[2] { 1.0f, 0.1f };
    private static readonly float ROUTED_ATTACK_PENALTY = 0.90f;
    private static readonly float STARTING_MORALE_WHEN_MISSING_MANPOWER = 0.5f;

    private static readonly Dictionary<AttackArch, float> ARCH_ATTACK_MULTIPLIERS = new Dictionary<AttackArch, float>() {
        { AttackArch.None, 0.1f },
        { AttackArch.Low, 0.0f },
        { AttackArch.High, -0.25f }
    };
    private static readonly Dictionary<AttackArch, float> ARCH_RANGE_MULTIPLIERS = new Dictionary<AttackArch, float>() {
        { AttackArch.None, 0.0f },
        { AttackArch.Low, 0.0f },
        { AttackArch.High, -0.25f }
    };
    private static readonly Dictionary<AttackArch, string> ARCH_DETAIL_DESCRIPTIONS = new Dictionary<AttackArch, string>() {
        { AttackArch.None, "Straight Shot" },
        { AttackArch.Low, "Arching Shot" },
        { AttackArch.High, "High Arching Shot" }
    };
    private static readonly int MAX_ELEVATION_DIFFERENCE = 1;
    private static readonly float ELEVATION_DIFFERENCE_RANGE_MULTIPLIER = 0.1f;

    private static int current_id = 0;
    
    public enum UnitType { Undefined, Infantry, Cavalry, Ship, Siege_Weapon }
    public enum ArmorType { Unarmoured, Light, Medium, Heavy }
    public enum AttackArch { None, Low, High }
    public enum Tag
    {
        Small_Shields,
        Medium_Shields,//Axe units tend to gain bonuses against shielded units
        Large_Shields,
        Wooden,//Axe again
        Undead,
        Large,
        Blocks_Hex_Working,
        Limited_Recruitment,
        Naval,
        Amphibious,
        Crewed_Single_Entity,
        Mechanical_Ranged,
        No_Move_Attack,
        Embark_Transport
    }
    public static readonly List<Tag> HIDDEN_TAGS = new List<Tag>() { Tag.Crewed_Single_Entity, Tag.Embark_Transport };

    public string Name { get; private set; }
    public UnitType Type { get; private set; }
    public string Texture { get; private set; }
    public Army Army { get; set; }
    public float Max_Movement { get; private set; }
    public float Current_Movement { get; private set; }
    public bool Can_Attack { get; private set; }
    public int Id { get; private set; }
    public int LoS { get; private set; }
    public float Max_Campaing_Map_Movement { get; private set; }
    public float Current_Campaing_Map_Movement { get; set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject.GetComponent<SpriteRenderer>(); } }
    public List<Sprite> Current_Animation { get; private set; }
    public int Animation_Index { get; private set; }
    public float Animation_FPS { get; private set; }
    public int Production_Required { get; private set; }
    public int Cost { get; private set; }
    public Technology Technology_Required { get; private set; }
    public List<Building> Buildinds_Required { get; private set; }
    public List<Ability> Abilities { get; private set; }
    public CombatMapHex Hex { get; private set; }

    public float Manpower { get; private set; }
    public float Current_Morale { get; private set; }
    public float Max_Morale { get; private set; }
    public float Current_Stamina { get; private set; }
    public float Max_Stamina { get; private set; }
    public Damage Melee_Attack { get; private set; }
    public float Charge { get; private set; }
    public float Run_Stamina_Cost { get; private set; }
    public Damage Ranged_Attack { get; private set; }
    public List<string> Ranged_Attack_Animation { get; private set; }
    public string Ranged_Attack_Effect_Name { get; private set; }
    public int Range { get; private set; }
    public int Current_Ammo { get; private set; }
    public int Max_Ammo { get; private set; }
    public float Melee_Defence { get; private set; }
    public float Ranged_Defence { get; private set; }
    public Dictionary<Damage.Type, float> Resistances { get; private set; }
    public float Morale_Value { get; private set; }
    public float Discipline { get; private set; }
    public List<Tag> Tags { get; private set; }
    public Player Owner { get { return Army != null ? Army.Owner : null; } }
    public List<StatusBar> Bars { get; private set; }
    public float Mana_Upkeep { get; private set; }
    public ArmorType Armor { get; private set; }
    public bool Requires_Coast { get { return Tags.Contains(Tag.Naval); } }

    public bool Has_Moved_This_Turn { get; private set; }
    public bool Has_Attacked_This_Turn { get; private set; }
    public bool Last_Move_This_Turn_Was_Running { get; private set; }
    public bool Wait_Turn { get; set; } 
    public bool Is_Single_Entity { get { return Tags.Contains(Tag.Crewed_Single_Entity); } }
    
    private float animation_frame_time_left;
    private bool can_run;
    private float relative_strenght;
    private float upkeep;
    private bool repeat_animation;

    public Unit(Unit prototype)
    {
        Name = prototype.Name;
        Type = prototype.Type;
        Texture = prototype.Texture;
        Max_Movement = prototype.Max_Movement;
        Current_Movement = Max_Movement;
        can_run = prototype.can_run;
        Run_Stamina_Cost = prototype.Run_Stamina_Cost;
        Max_Campaing_Map_Movement = prototype.Max_Campaing_Map_Movement;
        Current_Campaing_Map_Movement = Max_Campaing_Map_Movement;
        Production_Required = prototype.Production_Required;
        Cost = prototype.Cost;
        Upkeep = prototype.Upkeep;
        Mana_Upkeep = prototype.Mana_Upkeep;
        LoS = prototype.LoS;
        Technology_Required = prototype.Technology_Required;
        Buildinds_Required = prototype.Buildinds_Required;
        Abilities = new List<Ability>();
        foreach(Ability ability in prototype.Abilities) {
            Abilities.Add(ability.Clone());
        }
        Tags = Helper.Copy_List(prototype.Tags);
        Bars = new List<StatusBar>();

        Manpower = 1.0f;
        Max_Morale = prototype.Max_Morale;
        Current_Morale = Max_Morale;
        Max_Stamina = prototype.Max_Stamina;
        Current_Stamina = Max_Stamina;
        Melee_Attack = prototype.Melee_Attack.Clone;
        Charge = prototype.Charge;
        Ranged_Attack = prototype.Ranged_Attack == null ? null : prototype.Ranged_Attack.Clone;
        Ranged_Attack_Effect_Name = prototype.Ranged_Attack_Effect_Name;
        Ranged_Attack_Animation = prototype.Ranged_Attack_Animation != null ? Helper.Copy_List(prototype.Ranged_Attack_Animation) : null;
        Range = prototype.Range;
        Max_Ammo = prototype.Max_Ammo;
        Melee_Defence = prototype.Melee_Defence;
        Ranged_Defence = prototype.Ranged_Defence;
        Resistances = Helper.Copy_Dictionary(prototype.Resistances);
        Morale_Value = prototype.Morale_Value;
        Discipline = prototype.Discipline;
        Armor = prototype.Armor;

        Id = current_id;
        current_id++;

        Update_Relative_Strengh();
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    public Unit(string name, UnitType type, string texture, float campaing_map_movement, int production_required, int cost, float upkeep, float mana_upkeep, int los, Technology technology_required,
        List<Building> buildings_required, float movement, bool can_run, float run_stamina_cost, float morale, float stamina, Damage melee_attack,
        float charge, Damage ranged_attack, int range, int ammo, string ranged_attack_effect_name, List<string> ranged_attack_animation,
        float melee_defence, float ranged_defence, Dictionary<Damage.Type, float> resistances, float morale_value, float discipline, ArmorType armor, List<Ability> abilities, List<Tag> tags)
    {
        Name = name;
        Type = type;
        Texture = texture;
        Max_Movement = movement;
        this.can_run = can_run;
        Run_Stamina_Cost = run_stamina_cost;
        Max_Campaing_Map_Movement = campaing_map_movement;
        Production_Required = production_required;
        Cost = cost;
        Upkeep = upkeep;
        Mana_Upkeep = mana_upkeep;
        LoS = los;
        Technology_Required = technology_required;
        Buildinds_Required = buildings_required;

        Max_Morale = morale;
        Max_Stamina = stamina;
        Melee_Attack = melee_attack;
        Charge = charge;
        Ranged_Attack = ranged_attack;
        Ranged_Attack_Effect_Name = ranged_attack_effect_name;
        Range = range;
        Ranged_Attack_Animation = ranged_attack_animation;
        Max_Ammo = ammo;
        Melee_Defence = melee_defence;
        Ranged_Defence = ranged_defence;
        Resistances = resistances;
        Morale_Value = morale_value;
        Discipline = discipline;
        Abilities = abilities;
        Armor = armor;
        Tags = tags;
        Bars = new List<StatusBar>();

        Update_Relative_Strengh();
    }

    public float Upkeep
    {
        get {
            if(Army == null || Army.Hex == null) {
                return upkeep;
            }
            float multiplier = 1.0f;
            foreach(Ability ability in Abilities) {
                if(ability.Get_Upkeep_Multiplier != null) {
                    multiplier += ability.Get_Upkeep_Multiplier(ability, Army.Hex);
                }
            }
            multiplier = Mathf.Clamp(multiplier, 0.0f, 999.0f);
            return upkeep * multiplier;
        }
        set {
            upkeep = value;
        }
    }

    public bool Pathfind(CombatMapHex new_hex, bool run)
    {
        if (Hex.Is_Adjancent_To(new_hex)) {
            return Move(new_hex, run, false);
        }
        List<PathfindingNode> path = Pathfinding.Path(Hex.Map.Get_Specific_PathfindingNodes(this, new_hex), Hex.Get_Specific_PathfindingNode(this, new_hex),
            new_hex.Get_Specific_PathfindingNode(this, new_hex));
        if(path.Count == 0) {
            return false;
        }
        return Move(Hex.Map.Get_Hex_At(path[1].Coordinates), run, false);
    }

    /// <summary>
    /// Try moving to a new hex
    /// </summary>
    /// <param name="new_hex"></param>
    /// <param name="ignore_movement_restrictions"></param>
    /// <returns></returns>
    public virtual bool Move(CombatMapHex new_hex, bool run, bool ignore_movement_restrictions = false)
    {
        if (new_hex.Unit != null) {
            return false;
        }
        if (!ignore_movement_restrictions && (!Hex.Is_Adjancent_To(new_hex) || Current_Movement <= 0.0f)) {
            return false;
        }
        if(Hex != null) {
            Hex.Unit = null;
            Hex.Borders = null;
        }
        CombatMapHex old_hex = Hex;
        new_hex.Unit = this;
        Hex = new_hex;

        if(CombatUIManager.Instance.Current_Unit == this) {
            Hex.Borders = Is_Owned_By_Current_Player ? CombatMapHex.Current_Owned_Unit_Color : CombatMapHex.Current_Enemy_Unit_Color;
        } else {
            Hex.Borders = Is_Owned_By_Current_Player ? CombatMapHex.Owned_Unit_Color : CombatMapHex.Enemy_Unit_Color;
        }

        if(GameObject == null) {
            GameObject = new GameObject();
            GameObject.name = ToString();
            GameObject.transform.position = Hex.GameObject.transform.position;
            GameObject.transform.SetParent(Hex.Map.Units_GameObject.transform);
            GameObject.AddComponent<SpriteRenderer>();
            SpriteRenderer.sprite = SpriteManager.Instance.Get(Texture, SpriteManager.SpriteType.Unit);
            SpriteRenderer.sortingLayerName = SortingLayer.UNITS;
        }

        GameObject.transform.position = Hex.GameObject.transform.position;
        if (!ignore_movement_restrictions) {
            MovementInfo info = Get_Movement_Info(old_hex, Hex, run, true);
            Current_Movement -= info.Movement_Cost;
            if(Current_Movement < 0.0f) {
                Current_Movement = 0.0f;
            }
            run = info.Run;
            if(run && Uses_Stamina) {
                float run_stamina_cost = Run_Stamina_Cost + Hex.Run_Stamina_Penalty;
                float run_stamina_cost_multiplier = 1.0f;
                foreach(Ability ability in Abilities) {
                    if(ability.Get_Run_Stamina_Cost_Multiplier != null) {
                        run_stamina_cost_multiplier += ability.Get_Run_Stamina_Cost_Multiplier(ability, Hex);
                    }
                }
                if(run_stamina_cost_multiplier < 0.0f) {
                    run_stamina_cost_multiplier = 0.0f;
                }
                Current_Stamina -= run_stamina_cost;
                if(Current_Stamina < 0.0f && Uses_Morale) {
                    float previous_morale = Current_Morale;
                    Current_Morale += Current_Stamina * default_morale_lost_when_running_with_no_stamina;
                    Current_Morale = Mathf.Clamp(Current_Morale, 0.0f, Max_Morale);
                    if(Current_Morale == 0.0f && previous_morale > 0.0f) {
                        Current_Morale = Mathf.Min(1.0f, previous_morale);
                    }
                    Current_Stamina = 0.0f;
                }
            }
        }

        if (!ignore_movement_restrictions && Tags.Contains(Tag.No_Move_Attack)) {
            Can_Attack = false;
        }

        StatusBar.Update_Bars(this);
        Has_Moved_This_Turn = true;
        Last_Move_This_Turn_Was_Running = run;
        Stop_Animation();//TODO: Move / run animations?

        return true;
    }

    private MovementInfo Get_Movement_Info(CombatMapHex old_hex, CombatMapHex new_hex, bool run, bool disable_can_attack_on_disengagement)
    {
        float movement_cost = new_hex.Movement_Cost;
        if (old_hex.Is_Adjancent_To_Enemy(Owner) && new_hex.Is_Adjancent_To_Enemy(Owner)) {
            //ZoC
            movement_cost += zone_of_control_movement_cost;
            run = false;
            //TODO: UI run = false?
        } else if (!old_hex.Is_Adjancent_To_Enemy(Owner) && new_hex.Is_Adjancent_To_Enemy(Owner)) {
            //Engagement
            movement_cost += engagement_movement_cost;
        } else if (old_hex.Is_Adjancent_To_Enemy(Owner) && !new_hex.Is_Adjancent_To_Enemy(Owner)) {
            //Disengagement
            movement_cost += disengagement_movement_cost * (Math.Max(0.0f, 1.0f - Abilities.Select(x => x.Get_Disengagement_Movement_Cost == null ? 0.0f : x.Get_Disengagement_Movement_Cost(x, this)).Sum()));
            run = false;
            if (disable_can_attack_on_disengagement) {
                Can_Attack = false;
            }
            //TODO: UI run = false?
        }
        if (run) {
            movement_cost *= default_run_movement_cost;
        }
        return new MovementInfo(movement_cost, run);
    }

    public List<CombatMapHex> Get_Hexes_In_Movement_Range(bool run)
    {
        List<CombatMapHex> hexes = new List<CombatMapHex>();
        float current_movement = Current_Movement;
        if(current_movement <= 0.0f) {
            return hexes;
        }
        foreach(CombatMapHex adjancent_hex in Hex.Get_Adjancent_Hexes()) {
            Get_Hexes_In_Movement_Range_Recursive(Hex, adjancent_hex, current_movement, run, hexes);
        }
        return hexes;
    }

    private void Get_Hexes_In_Movement_Range_Recursive(CombatMapHex previous_hex, CombatMapHex hex, float movement, bool run, List<CombatMapHex> hexes)
    {
        if (hex == Hex || hex.Unit != null || movement <= 0.0f) {
            return;
        }
        if (!hexes.Contains(hex)) {
            hexes.Add(hex);
        }
        
        movement -= Get_Movement_Info(previous_hex, hex, run, false).Movement_Cost;
        if(movement <= 0.0f) {
            return;
        }

        foreach (CombatMapHex adjancent_hex in hex.Get_Adjancent_Hexes()) {
            Get_Hexes_In_Movement_Range_Recursive(hex, adjancent_hex, movement, run, hexes);
        }
    }
    
    public List<CombatMapHex> Get_Hexes_In_Attack_Range()
    {
        if(Range <= 0.0f) {
            return new List<CombatMapHex>();
        }
        List<CombatMapHex> hexes = new List<CombatMapHex>();
        foreach(CombatMapHex hex in Hex.Get_Hexes_Around(Mathf.RoundToInt(Range * (1.0f + (MAX_ELEVATION_DIFFERENCE * ELEVATION_DIFFERENCE_RANGE_MULTIPLIER))))) {
            if (In_Attack_Range(hex)) {
                hexes.Add(hex);
            }
        }
        return hexes;
    }
    
    public bool In_Attack_Range(CombatMapHex hex)
    {
        float effective_range = Range;
        effective_range *= 1.0f + ((Hex.Elevation - hex.Elevation) * ELEVATION_DIFFERENCE_RANGE_MULTIPLIER);
        if(effective_range < 1.0f) {
            effective_range = 1.0f;
        }
        effective_range *= 1.0f + ARCH_RANGE_MULTIPLIERS[Get_Attack_Arch(hex)];
        if(Hex.Distance(hex) > effective_range) {
            return false;
        }
        foreach (Ability ability in Abilities) {
            if (ability.On_Allow_Ranged_Attack != null && !ability.On_Allow_Ranged_Attack(ability, this, hex)) {
                return false;
            }
        }
        return true;
    }

    public bool Can_Run
    {
        get {
            return can_run;
        }
    }

    public bool Uses_Morale
    {
        get {
            return Max_Morale > 0.0f;
        }
    }

    public float Relative_Morale
    {
        get {
            if (!Uses_Morale) {
                return 1.0f;
            }
            return Current_Morale / Max_Morale;
        }
    }

    public float Morale_Effect
    {
        get {
            if (!Uses_Morale) {
                return 1.0f;
            }
            if(Relative_Morale >= 0.9f) {
                return 1.0f + (Relative_Morale - 0.9f);
            } else if(Relative_Morale < 0.5f) {
                return Relative_Morale + 0.5f;
            }
            return 1.0f;
        }
    }

    public bool Uses_Stamina
    {
        get {
            return Max_Stamina > 0.0f;
        }
    }

    public float Relative_Stamina
    {
        get {
            if (!Uses_Stamina) {
                return 1.0f;
            }
            return Current_Stamina / Max_Stamina;
        }
    }

    public float Stamina_Effect
    {
        get {
            if(!Uses_Stamina || Relative_Stamina >= 0.5f) {
                return 1.0f;
            }
            return Relative_Stamina + 0.5f;
        }
    }

    public float Relative_Strenght
    {
        get {
            return relative_strenght;
        }
    }

    public float Current_Relative_Strenght
    {
        get {
            return relative_strenght * Manpower * ((Stamina_Effect + 1.0f) / 2.0f) * ((Morale_Effect + 1.0f) / 2.0f) * (Is_Routed ? ROUTED_RELATIVE_STRENGHT : 1.0f);
        }
    }

    public float Get_Relative_Strenght_When_On_Hex(WorldMapHex hex, bool current_str, bool attacker)
    {
        if (!hex.Passable_For(this)) {
            return 0.0f;
        }
        float current = current_str ? Current_Relative_Strenght : Relative_Strenght;
        float multiplier = 1.0f;
        foreach(Ability ability in Abilities) {
            if(ability.Get_Fluctuating_Relative_Strength_Multiplier_Bonus != null) {
                multiplier += ability.Get_Fluctuating_Relative_Strength_Multiplier_Bonus(ability, hex, attacker);
            }
        }
        return current * multiplier;
    }

    public float Discipline_Morale_Damage_Multiplier
    {
        get {
            if(Discipline > 10.0f) {
                return Mathf.Pow(0.95f, Discipline - 10.0f);
            }
            if(Discipline < 10.0f) {
                return 1.0f + Mathf.Pow(0.015f, 1.0f / (11.0f - Discipline));
            }
            return 1.0f;
        }
    }

    public void Regen_Manpower(float regen)
    {
        if(regen < 0.0f) {
            CustomLogger.Instance.Warning("Negative regen");
            return;
        }
        Manpower = Mathf.Clamp01(Manpower + regen);
    }

    public bool Deploy(CombatMapHex hex)
    {
        if (hex.Hidden) {
            return false;
        }
        return Move(hex, false, true);
    }

    public void Undeploy()
    {
        if(Hex == null) {
            return;
        }
        Hex.Unit = null;
        Hex.Borders = null;
        Hex = null;
        StatusBar.Destroy_Bars(this);
        GameObject.Destroy(GameObject);
        GameObject = null;
    }

    public bool Is_Owned_By_Current_Player
    {
        get {
            if (CombatManager.Instance.Active_Combat) {
                return Army.Is_Owned_By(CombatManager.Instance.Current_Player);
            }
            return Army.Is_Owned_By(Main.Instance.Current_Player);
        }
    }

    public void Start_Combat()
    {
        Combat_Refill();
        Current_Morale = Max_Morale - (STARTING_MORALE_WHEN_MISSING_MANPOWER * (Max_Morale * (1.0f - Manpower)));
    }

    public void End_Combat()
    {
        Combat_Refill();
        Undeploy();
    }

    private void Combat_Refill()
    {
        Current_Morale = Max_Morale;
        Current_Stamina = Max_Stamina;
        Current_Movement = Max_Movement;
        Current_Ammo = Max_Ammo;
        Can_Attack = true;
        Has_Moved_This_Turn = false;
        Has_Attacked_This_Turn = false;
        Last_Move_This_Turn_Was_Running = false;
    }

    public void Start_Combat_Turn()
    {
        if (Hex == null) {
            return;
        }
        foreach (Ability ability in Abilities) {
            if (ability.On_Combat_Turn_Start != null) {
                ability.On_Combat_Turn_Start(ability, this);
            }
        }
    }

    public void End_Combat_Turn()
    {
        if(Hex == null) {
            return;
        }

        Current_Movement = Max_Movement;
        Can_Attack = true;

        if (Uses_Stamina && !Has_Moved_This_Turn && !Has_Attacked_This_Turn && !Hex.Is_Adjancent_To_Enemy(Owner)) {
            Current_Stamina += (default_stamina_regeneration * Max_Stamina);
            Current_Stamina = Mathf.Clamp(Current_Stamina, 0.0f, Max_Stamina);
            StatusBar.Update_Bars(this);
        }

        foreach(Ability ability in Abilities) {
            if(ability.On_Combat_Turn_End != null) {
                ability.On_Combat_Turn_End(ability, this);
            }
        }

        Has_Moved_This_Turn = false;
        Has_Attacked_This_Turn = false;
        Last_Move_This_Turn_Was_Running = false;
        Wait_Turn = false;
    }

    public bool Controllable
    {
        get {
            if(Hex == null) {
                return false;
            }
            if (!Uses_Morale) {
                return true;
            }
            return Current_Morale > 0.0f;
        }
    }

    public bool Can_Ranged_Attack
    {
        get {
            return Range > 0.0f && Ranged_Attack != null && Ranged_Attack.Total > 0.0f && (Hex == null || ((Current_Ammo > 0 || Max_Ammo <= 0) && !Hex.Is_Adjancent_To_Enemy(Owner)));
        }
    }

    public AttackResult[] Attack(Unit target, bool preview)
    {
        if (target.Army.Is_Owned_By(Army.Owner) || (!Can_Attack && !preview)) {
            return null;
        }
        if (!target.Hex.Is_Adjancent_To(Hex)) {
            return Perform_Ranged_Attack(target, preview);
        }
        return Perform_Melee_Attack(target, preview);
    }

    private AttackResult[] Perform_Melee_Attack(Unit target, bool preview)
    {
        AttackResult attacker_result = new AttackResult();
        AttackResult defender_result = new AttackResult();
        
        //Attacker's attack
        defender_result.Can_Attack = null;
        defender_result.Movement = null;
        defender_result.Stamina_Delta = -default_attack_stamina_cost;
        
        float damage = Calculate_Melee_Damage(target, defender_result);
        defender_result.Manpower_Delta = -damage;
        float morale_damage_multiplier = 1.0f;
        if (Last_Move_This_Turn_Was_Running) {
            morale_damage_multiplier += (morale_damage_bonus_on_charge * target.Discipline_Morale_Damage_Multiplier);
        }
        defender_result.Morale_Delta = (-damage * default_morale_damage * 100.0f * morale_damage_multiplier) - target.Calculate_Morale_Loss_On_Attack(true);
        defender_result.Damage_Effectiveness = damage / Calculate_Standard_Melee_Damage(target);
        

        //Defender's attack
        attacker_result.Can_Attack = false;
        attacker_result.Movement = 0.0f;
        attacker_result.Stamina_Delta = -default_attack_stamina_cost;

        damage = target.Calculate_Melee_Damage(this, attacker_result);
        attacker_result.Manpower_Delta = -damage;
        attacker_result.Morale_Delta = (-damage * default_morale_damage * 100.0f) - Calculate_Morale_Loss_On_Attack(true);
        attacker_result.Damage_Effectiveness = damage / target.Calculate_Standard_Melee_Damage(this);


        if (!preview) {
            EffectManager.Instance.Play_Effect(Hex, "melee");
            EffectManager.Instance.Play_Effect(target.Hex, "melee");
            Apply(attacker_result);
            target.Apply(defender_result);
            Has_Attacked_This_Turn = true;
            StatusBar.Update_Bars(this);
            StatusBar.Update_Bars(target);
            CombatLogManager.Instance.Print_Log(string.Format("{0} {1}{2} {3} ({4}/{5} dmg dealt, {6}/{7} dmg taken)", Name, Last_Move_This_Turn_Was_Running ? "charge" : "melee attack",
                Is_Single_Entity ? "s" : string.Empty, target.Name, Mathf.RoundToInt(defender_result.Manpower_Delta * -100.0f), Mathf.RoundToInt(defender_result.Morale_Delta * -1.0f),
                Mathf.RoundToInt(attacker_result.Manpower_Delta * -100.0f), Mathf.RoundToInt(attacker_result.Morale_Delta * -1.0f)));
            CombatTopPanelManager.Instance.Update_GUI();
        }

        return new AttackResult[2] { attacker_result, defender_result };
    }

    private float Calculate_Morale_Loss_On_Attack(bool is_melee)
    {
        if(!Uses_Morale || !Uses_Stamina) {
            return 0.0f;
        }
        float cost = is_melee ? default_attack_stamina_cost : default_ranged_stamina_cost;
        float[] data = is_melee ? MORALE_LOSS_ON_MELEE_ATTACK_NO_STAMINA : MORALE_LOSS_ON_RANGED_ATTACK_NO_STAMINA;
        if (Current_Stamina >= cost || Relative_Morale <= data[1]) {
            return 0.0f;
        }
        float loss = ((cost - Current_Stamina) / cost) * data[0] * Discipline_Morale_Damage_Multiplier;
        if(Current_Morale - loss < Max_Morale * data[1]) {
            loss = (Max_Morale * data[1]) - (Current_Morale - loss);
        }
        return loss;
    }

    private float Calculate_Standard_Melee_Damage(Unit target)
    {
        float attack = Melee_Attack.Total;
        float defence = target.Melee_Defence;
        return (((attack + damage_balancer) / (defence + damage_balancer)) * manpower_damage) * Manpower;
    }

    private float Calculate_Melee_Damage(Unit target, AttackResult result)
    {
        float attack = Melee_Attack.Total * Morale_Effect * Stamina_Effect;
        float ability_attack_delta = 0.0f;
        float ability_attack_multiplier = 1.0f;
        float defence = target.Melee_Defence * target.Morale_Effect * target.Stamina_Effect;
        float ability_defence_delta = 0.0f;
        float ability_defence_multiplier = 1.0f;

        result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = Morale_Effect - 1.0f, Description = "Morale" });
        result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = Stamina_Effect - 1.0f, Description = "Stamina" });
        result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = target.Morale_Effect - 1.0f, Description = "Morale" });
        result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = target.Stamina_Effect - 1.0f, Description = "Stamina" });

        if (Is_Routed) {
            attack *= (1.0f - ROUTED_ATTACK_PENALTY);
            result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = -ROUTED_ATTACK_PENALTY, Description = "Routed" });
        }

        //Charge TODO: double loops
        float ability_charge_multiplier = 1.0f;
        Dictionary<Damage.Type, float> melee_attack_types = Melee_Attack.Type_Weights;
        foreach (Ability ability in Abilities) {
            if (ability.On_Calculate_Melee_Damage_As_Attacker == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Melee_Damage_As_Attacker(ability, this, target, result);
            ability_charge_multiplier += data.Charge_Multiplier;
            if(data.New_Attack_Types != null) {
                melee_attack_types = data.New_Attack_Types;
            }
        }
        foreach (Ability ability in target.Abilities) {
            if (ability.On_Calculate_Melee_Damage_As_Defender == null) {
                continue;
            }
            ability_charge_multiplier += ability.On_Calculate_Melee_Damage_As_Defender(ability, this, target, result).Charge_Multiplier;
        }
        if (Last_Move_This_Turn_Was_Running) {
            float charge_bonus = Charge * Mathf.Max(ability_charge_multiplier, 0.05f);
            attack *= (1.0f + charge_bonus);
            result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = charge_bonus, Description = "Charge" });
        }

        //Base defence
        float resistance_multiplier = 0.0f;
        float total = 0.0f;
        foreach(KeyValuePair<Damage.Type, float> damage in melee_attack_types) {
            float resistance_to_type = target.Resistances.ContainsKey(damage.Key) ? target.Resistances[damage.Key] : 1.0f;
            resistance_multiplier += damage.Value * resistance_to_type;
            total += damage.Value;
        }
        if(total != 1.0f) {
            CustomLogger.Instance.Warning(string.Format("Unit {0} does not have proper melee damage types, they have sum of {1}", Name, total));
        }
        defence *= resistance_multiplier;
        result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = resistance_multiplier - 1.0f, Description = "Resistances" });

        //City defence
        if(Hex.City && CombatManager.Instance.Army_2.Id == Army.Id) {
            attack *= (1.0f + CombatManager.Instance.Hex.City.Defence_Bonus);
            result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = CombatManager.Instance.Hex.City.Defence_Bonus, Description = "City Defence" });
        } else if(target.Hex.City && CombatManager.Instance.Army_2.Id == target.Army.Id) {
            defence *= (1.0f + CombatManager.Instance.Hex.City.Defence_Bonus);
            result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = CombatManager.Instance.Hex.City.Defence_Bonus, Description = "City Defence" });
        }

        //Ability delta
        foreach (Ability ability in Abilities) {
            if (ability.On_Calculate_Melee_Damage_As_Attacker == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Melee_Damage_As_Attacker(ability, this, target, result);
            ability_attack_delta += data.Attack_Delta;
            ability_attack_multiplier += data.Attack_Multiplier;
            result.Add_Detail(new AttackResult.Detail() { Attack_Delta = data.Attack_Delta, Attack_Multiplier = data.Attack_Multiplier, Description = ability.Name });
            ability_defence_delta += data.Defence_Delta;
            ability_defence_multiplier += data.Defence_Multiplier;
            result.Add_Detail(new AttackResult.Detail() { Defence_Delta = data.Defence_Delta, Defence_Multiplier = data.Defence_Multiplier, Description = ability.Name });
        }
        foreach (Ability ability in target.Abilities) {
            if (ability.On_Calculate_Melee_Damage_As_Defender == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Melee_Damage_As_Defender(ability, this, target, result);
            ability_attack_delta += data.Attack_Delta;
            ability_attack_multiplier += data.Attack_Multiplier;
            result.Add_Detail(new AttackResult.Detail() { Attack_Delta = data.Attack_Delta, Attack_Multiplier = data.Attack_Multiplier, Description = ability.Name });
            ability_defence_delta += data.Defence_Delta;
            ability_defence_multiplier += data.Defence_Multiplier;
            result.Add_Detail(new AttackResult.Detail() { Defence_Delta = data.Defence_Delta, Defence_Multiplier = data.Defence_Multiplier, Description = ability.Name });
        }
        attack = Mathf.Max(attack + ability_attack_delta, 1.0f);
        attack *= Mathf.Max(ability_attack_multiplier, 0.05f);
        defence = Mathf.Max(defence + ability_defence_delta, 1.0f);
        defence *= Mathf.Max(ability_defence_multiplier, 0.05f);

        result.Final_Attack = attack;
        result.Base_Attack = Melee_Attack.Total;
        result.Final_Defence = defence;
        result.Base_Defence = target.Melee_Defence;

        return (((attack + damage_balancer) / (defence + damage_balancer)) * manpower_damage) * Manpower;
    }

    private AttackResult[] Perform_Ranged_Attack(Unit target, bool preview)
    {
        if(!preview && (!In_Attack_Range(target.Hex) || !Can_Ranged_Attack)) {
            return null;
        }
        
        if(preview && !Can_Ranged_Attack) {
            return Perform_Melee_Attack(target, preview);
        }

        AttackResult attacker_result = new AttackResult();
        AttackResult defender_result = new AttackResult();

        //Attacker's attack
        defender_result.Can_Attack = null;
        defender_result.Movement = null;
        defender_result.Stamina_Delta = 0.0f;
        
        float damage = Calculate_Ranged_Damage(target, defender_result);
        defender_result.Manpower_Delta = -damage;
        defender_result.Morale_Delta = -damage * default_morale_damage * 100.0f;
        defender_result.Damage_Effectiveness = damage / Calculate_Standard_Ranged_Damage(target);
        
        //Effects on attacker
        attacker_result.Can_Attack = false;
        attacker_result.Movement = 0.0f;
        attacker_result.Stamina_Delta = -default_ranged_stamina_cost;
        
        attacker_result.Manpower_Delta = 0.0f;
        attacker_result.Morale_Delta = -Calculate_Morale_Loss_On_Attack(false);

        if (!preview) {
            if(Ranged_Attack_Animation != null && Ranged_Attack_Animation.Count != 0) {
                Start_Animation(Ranged_Attack_Animation, 5.0f, false);
            } else {
                EffectManager.Instance.Play_Effect(Hex, "ranged_attack");
            }
            EffectManager.Instance.Play_Effect(target.Hex, string.IsNullOrEmpty(Ranged_Attack_Effect_Name) ? "ranged_target" : Ranged_Attack_Effect_Name);
            Apply(attacker_result);
            target.Apply(defender_result);
            Has_Attacked_This_Turn = true;
            StatusBar.Update_Bars(this);
            StatusBar.Update_Bars(target);
            if (Max_Ammo > 0) {
                Current_Ammo--;
            }
            CombatLogManager.Instance.Print_Log(string.Format("{0} ranged attack{1} {2} ({3}/{4} dmg dealt)", Name, Is_Single_Entity ? "s" : string.Empty, target.Name, Mathf.RoundToInt(defender_result.Manpower_Delta * -100.0f),
                Mathf.RoundToInt(defender_result.Morale_Delta * -1.0f)));
        }

        return new AttackResult[2] { attacker_result, defender_result };
    }

    private float Calculate_Standard_Ranged_Damage(Unit target)
    {
        float attack = Ranged_Attack.Total;
        float defence = target.Ranged_Defence;
        return (((attack + damage_balancer) / (defence + damage_balancer)) * manpower_damage) * Manpower;
    }

    public bool Morale_Affects_Ranged_Attack
    {
        get {
            return !Tags.Contains(Tag.Mechanical_Ranged);
        }
    }

    public bool Stamina_Affects_Ranged_Attack
    {
        get {
            return !Tags.Contains(Tag.Mechanical_Ranged);
        }
    }

    private float Calculate_Ranged_Damage(Unit target, AttackResult result)
    {
        float morale_effect_attack = Morale_Affects_Ranged_Attack ? (Morale_Effect + 1.0f) / 2.0f : 1.0f;
        float stamina_effect_attack = Stamina_Affects_Ranged_Attack ? Stamina_Effect : 1.0f;
        float attack = Ranged_Attack.Total * morale_effect_attack * stamina_effect_attack;
        float ability_attack_delta = 0.0f;
        float ability_attack_multiplier = 1.0f;
        float morale_effect_defence = (target.Morale_Effect + 1.0f) / 2.0f;
        float stamina_effect_defence = (target.Stamina_Effect + 1.0f) / 2.0f;
        float defence = target.Ranged_Defence * morale_effect_defence * stamina_effect_defence * (1.0f + target.Hex.Cover);
        float ability_defence_delta = 0.0f;
        float ability_defence_multiplier = 1.0f;
        float resistance_multiplier = 0.0f;
        
        result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = morale_effect_attack - 1.0f, Description = "Morale" });
        result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = stamina_effect_attack - 1.0f, Description = "Stamina" });
        result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = morale_effect_defence - 1.0f, Description = "Morale" });
        result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = stamina_effect_defence - 1.0f, Description = "Stamina" });
        result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = target.Hex.Cover, Description = "Cover" });

        AttackArch arch = Get_Attack_Arch(target.Hex);
        attack *= 1.0f + ARCH_ATTACK_MULTIPLIERS[arch];
        result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = ARCH_ATTACK_MULTIPLIERS[arch], Description = ARCH_DETAIL_DESCRIPTIONS[arch] });

        //Base defence
        float total = 0.0f;
        foreach (KeyValuePair<Damage.Type, float> damage in Ranged_Attack.Type_Weights) {
            float resistance_to_type = target.Resistances.ContainsKey(damage.Key) ? target.Resistances[damage.Key] : 1.0f;
            resistance_multiplier += damage.Value * resistance_to_type;
            total += damage.Value;
        }
        if (total != 1.0f) {
            CustomLogger.Instance.Warning(string.Format("Unit {0} does not have proper ranged damage types, they have sum of {1}", Name, total));
        }
        defence *= resistance_multiplier;
        result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = resistance_multiplier - 1.0f, Description = "Resistances" });

        //City defence
        if (Hex.City && CombatManager.Instance.Army_2.Id == Army.Id) {
            attack *= (1.0f + CombatManager.Instance.Hex.City.Defence_Bonus);
            result.Add_Detail(new AttackResult.Detail { Attack_Multiplier = CombatManager.Instance.Hex.City.Defence_Bonus, Description = "City Defence" });
        } else if (target.Hex.City && CombatManager.Instance.Army_2.Id == target.Army.Id) {
            defence *= (1.0f + CombatManager.Instance.Hex.City.Defence_Bonus);
            result.Add_Detail(new AttackResult.Detail { Defence_Multiplier = CombatManager.Instance.Hex.City.Defence_Bonus, Description = "City Defence" });
        }

        //Ability delta
        foreach (Ability ability in Abilities) {
            if (ability.On_Calculate_Ranged_Damage_As_Attacker == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Ranged_Damage_As_Attacker(ability, this, target, result);
            ability_attack_delta += data.Attack_Delta;
            ability_attack_multiplier += data.Attack_Multiplier;
            result.Add_Detail(new AttackResult.Detail() { Attack_Delta = data.Attack_Delta, Attack_Multiplier = data.Attack_Multiplier, Description = ability.Name });
            ability_defence_delta += data.Defence_Delta;
            ability_defence_multiplier += data.Defence_Multiplier;
            result.Add_Detail(new AttackResult.Detail() { Defence_Delta = data.Defence_Delta, Defence_Multiplier = data.Defence_Multiplier, Description = ability.Name });
        }
        foreach (Ability ability in target.Abilities) {
            if (ability.On_Calculate_Ranged_Damage_As_Defender == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Ranged_Damage_As_Defender(ability, this, target, result);
            ability_attack_delta += data.Attack_Delta;
            ability_attack_multiplier += data.Attack_Multiplier;
            result.Add_Detail(new AttackResult.Detail() { Attack_Delta = data.Attack_Delta, Attack_Multiplier = data.Attack_Multiplier, Description = ability.Name });
            ability_defence_delta += data.Defence_Delta;
            ability_defence_multiplier += data.Defence_Multiplier;
            result.Add_Detail(new AttackResult.Detail() { Defence_Delta = data.Defence_Delta, Defence_Multiplier = data.Defence_Multiplier, Description = ability.Name });
        }
        attack = Mathf.Max(attack + ability_attack_delta, 1.0f);
        attack *= Mathf.Max(ability_attack_multiplier, 0.05f);
        defence = Mathf.Max(defence + ability_defence_delta, 1.0f);
        defence *= Mathf.Max(ability_defence_multiplier, 0.05f);

        result.Final_Attack = attack;
        result.Base_Attack = Ranged_Attack.Total;
        result.Final_Defence = defence;
        result.Base_Defence = target.Ranged_Defence;

        return (((attack + damage_balancer) / (defence + damage_balancer)) * manpower_damage) * Manpower;
    }

    private void Apply(AttackResult result)
    {
        bool had_morale = Current_Morale > 0.0f;
        result.Target = this;
        if (result.Can_Attack.HasValue) {
            Can_Attack = result.Can_Attack.Value;
        }
        if (result.Movement.HasValue) {
            Current_Movement = result.Movement.Value;
        }
        Manpower = Mathf.Clamp01(Manpower + result.Manpower_Delta);
        Current_Morale = Mathf.Clamp(Current_Morale + result.Morale_Delta, 0.0f, Max_Morale);
        Current_Stamina = Mathf.Clamp(Current_Stamina + result.Stamina_Delta, 0.0f, Max_Stamina);
        if(result.Manpower_Delta != 0.0f) {
            EffectManager.Instance.Play_Floating_Text(Hex, Mathf.RoundToInt(result.Manpower_Delta * -100.0f).ToString());
        }
        if (result.Morale_Delta != 0.0f) {
            EffectManager.Instance.Play_Floating_Text(Hex, Mathf.RoundToInt(result.Morale_Delta).ToString(), EffectManager.TextType.Morale);
        }

        if (had_morale && Current_Morale == 0.0f) {
            CombatLogManager.Instance.Print_Log(string.Format("{0} rout{1}", Name, Is_Single_Entity ? "s" : string.Empty));
            Apply_On_Rout_Morale_AoE(Morale_Value, on_rout_morale_damage_aoe_radios);
        }

        if(Manpower == 0.0f) {
            CombatLogManager.Instance.Print_Log(string.Format("{0} {1} destroyed", Name, Is_Single_Entity ? "was" : "were"));
            if (!Is_Routed) {
                Apply_On_Rout_Morale_AoE(Morale_Value, on_rout_morale_damage_aoe_radios);
            }
            Undeploy();
        }
    }

    private void Apply_On_Rout_Morale_AoE(float morale_value, int radius)
    {
        List<Unit> newly_routed_units = new List<Unit>();
        foreach(CombatMapHex hex in Hex.Get_Hexes_Around(radius)) {
            if(hex.Unit != null && hex.Unit.Id != Id && hex.Unit.Current_Morale != 0.0f) {
                if (hex.Unit.Army.Is_Owned_By(Army.Owner)) {
                    //Morale dmg
                    float morale_delta = -Math.Max(1.0f, ((morale_value * hex.Unit.Discipline_Morale_Damage_Multiplier) - Mathf.Max(0.0f, (hex.Unit.Morale_Value - morale_value) * 0.5f)) * ROUT_AOE_MORALE_LOSS);
                    hex.Unit.Alter_Morale(morale_delta);
                    if (hex.Unit.Current_Morale == 0.0f) {
                        newly_routed_units.Add(hex.Unit);
                    }
                } else {
                    //Morale bonus
                    float morale_delta = Math.Max(1.0f, (morale_value - Mathf.Max(0.0f, (hex.Unit.Morale_Value - morale_value) * 0.5f)) * ROUT_AOE_MORALE_GAIN);
                    hex.Unit.Alter_Morale(morale_delta);
                }
            }
        }
        foreach(Unit u in newly_routed_units) {
            u.Apply_On_Rout_Morale_AoE(u.Morale_Value, on_rout_morale_damage_aoe_radios);
        }
    }

    public void Alter_Morale(float delta)
    {
        CombatLogManager.Instance.Print_Log(string.Format("{0} {1}{2} {3} morale",
            Name,
            delta > 0.0f ? "gain" : "lose",
            Is_Single_Entity ? "s" : string.Empty,
            Mathf.RoundToInt(delta)),
            CombatLogManager.LogLevel.Verbose);
        EffectManager.Instance.Play_Floating_Text(Hex, string.Format("{0}{1}", delta > 0.0f ? "+" : string.Empty, Mathf.RoundToInt(delta).ToString()), EffectManager.TextType.Morale);
        Current_Morale = Mathf.Clamp(Current_Morale + delta, 0.0f, Max_Morale);
        StatusBar.Update_Bars(this);
        if (Current_Morale == 0.0f) {
            CombatLogManager.Instance.Print_Log(string.Format("{0} rout{1}", Name, Is_Single_Entity ? "s" : string.Empty));
        }
    }

    public bool Is_Routed
    {
        get {
            return Current_Morale <= 0.0f && Uses_Morale;
        }
    }

    public bool Retreat()
    {
        if (Hex == null || !Is_Routed) {
            return false;
        }

        if (Hex.Is_At_Map_Edge) {
            Undeploy();
            return false;
        }

        Dictionary<CombatMapHex, float> egde_hexes_and_distance = new Dictionary<CombatMapHex, float>();

        foreach (CombatMapHex edge_hex in Hex.Map.Edge_Hexes) {
            Coordinates c1 = new Coordinates((int)Hex.GameObject.transform.position.x, (int)Hex.GameObject.transform.position.y);
            Coordinates c2 = new Coordinates((int)edge_hex.GameObject.transform.position.x, (int)edge_hex.GameObject.transform.position.y);
            egde_hexes_and_distance.Add(edge_hex, c1.Distance(c2));
        }

        List<CombatMapHex> edge_hexes_ordered_by_distanse = egde_hexes_and_distance.OrderBy(x => x.Value).Select(x => x.Key).ToList();

        List<PathfindingNode> retreat_path = null;
        for(int i = 0; i < edge_hexes_ordered_by_distanse.Count; i++) {
            List<PathfindingNode> path = Pathfinding.Path(Hex.Map.PathfindingNodes, Hex.PathfindingNode, edge_hexes_ordered_by_distanse[i].PathfindingNode);
            if (path.Count != 0 && (retreat_path == null || retreat_path.Count > path.Count)) {
                retreat_path = path;
                break;
            }
        }

        if (retreat_path == null) {
            //Can't find path
            return false;
        }

        if (!Move(Hex.Map.Get_Hex_At(retreat_path[1].Coordinates), Current_Stamina > 0.0f)) {
            //Can't follow path (this should not be possible result)
            return false;
        }

        /*List<PathfindingNode> shortest_path = null;
        foreach(CombatMapHex edge_hex in Hex.Map.Edge_Hexes) {
            List<PathfindingNode> path = Pathfinding.Path(Hex.Map.PathfindingNodes, Hex.PathfindingNode, edge_hex.PathfindingNode);
            if(path.Count != 0 && (shortest_path == null || shortest_path.Count > path.Count)) {
                shortest_path = path;
            }
        }

        if(shortest_path == null) {
            //Can't find path
            return false;
        }

        if (!Move(Hex.Map.Get_Hex_At(shortest_path[1].Coordinates), Current_Stamina > 0.0f)) {
            return false;
        }*/

        if (Hex.Is_At_Map_Edge) {
            Undeploy();
        }

        return Current_Movement > 0.0f;
    }

    private void Update_Relative_Strengh()
    {
        float morale = Uses_Morale ? Math.Max(Max_Morale, 50.0f) : 200.0f;
        float stamina = Uses_Stamina ? Math.Max(Max_Stamina, 50.0f) : 200.0f;
        relative_strenght = ((morale - 50.0f) / 5.0f) + ((stamina - 50.0f) / 7.5f) + ((Melee_Attack.Total * 1.25f) * (1.0f + (Charge * 0.1f))) + (Melee_Defence * 1.25f) +
            (Ranged_Attack != null ? (Ranged_Attack.Total * Range * 0.25f) : 0.0f) + (Ranged_Defence * 1.25f) + (Max_Movement * 2.0f);
        float multiplier = 1.0f;
        foreach(Ability ability in Abilities) {
            if(ability.Get_Relative_Strength_Multiplier_Bonus != null) {
                multiplier += ability.Get_Relative_Strength_Multiplier_Bonus(ability);
            }
        }
        if(multiplier <= 0.05f) {
            multiplier = 0.05f;
        }
        relative_strenght *= multiplier;
    }
    
    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder();
            tooltip.Append(Name);
            if (Type != UnitType.Undefined) {
                tooltip.Append(" (").Append(Type.ToString()).Append(")");
            }
            tooltip.Append(Environment.NewLine).Append("Melee Attack: ").Append(Mathf.RoundToInt(Melee_Attack.Total));
            if (Can_Ranged_Attack) {
                tooltip.Append(Environment.NewLine).Append("Ranged Attack: ").Append(Mathf.RoundToInt(Ranged_Attack.Total)).Append(" (");
                tooltip.Append(Environment.NewLine).Append("Range: ").Append(Range);
            }
            tooltip.Append(Environment.NewLine).Append("Melee Defence: ").Append(Mathf.RoundToInt(Melee_Defence));
            tooltip.Append(Environment.NewLine).Append("Ranged Defence: ").Append(Mathf.RoundToInt(Ranged_Defence));
            return tooltip.ToString();
        }
    }

    public AttackArch Get_Attack_Arch(CombatMapHex target_hex)
    {
        float height = 0.0f;
        float unit_height = 0.5f;
        foreach (CombatMapHex hex in Hex.Map.Straight_Line(Hex, target_hex)) {
            float hex_height = hex.Height;
            if(hex.Unit != null && hex.Elevation + unit_height > hex_height) {
                hex_height = hex.Elevation + unit_height;
            }
            if(hex_height > height) {
                height = hex_height;
            }
        }
        height -= Hex.Elevation * 0.5f;//TODO: Better math? Trigonometry?
        height += target_hex.Elevation * 0.5f;
        if(height <= 0.0f) {
            return AttackArch.None;
        }
        if(height < 1.0f) {
            return AttackArch.Low;
        }
        return AttackArch.High;
    }

    /// <summary>
    /// TODO: Duplicated code: WorldMapEntity
    /// </summary>
    /// <param name="textures"></param>
    /// <param name="animation_fps"></param>
    public void Start_Animation(List<string> textures, float animation_fps, bool repeat)
    {
        Current_Animation = new List<Sprite>();
        Animation_Index = 0;
        Animation_FPS = animation_fps;
        foreach (string t in textures) {
            Current_Animation.Add(SpriteManager.Instance.Get(t, SpriteManager.SpriteType.Unit_Animation));
        }
        animation_frame_time_left = 1.0f / Animation_FPS;
        repeat_animation = repeat;
    }

    public void Stop_Animation()
    {
        Current_Animation = null;
        Animation_Index = 0;
        Animation_FPS = 0.0f;
        SpriteRenderer.sprite = SpriteManager.Instance.Get(Texture, SpriteManager.SpriteType.Unit);
    }

    public void Update(float delta_s)
    {
        if (Hex == null || Current_Animation == null) {
            return;
        }
        animation_frame_time_left -= delta_s;
        if (animation_frame_time_left <= 0.0f) {
            animation_frame_time_left += (1.0f / Animation_FPS);
            Animation_Index++;
            if (Animation_Index >= Current_Animation.Count) {
                Animation_Index = 0;
                if (!repeat_animation) {
                    Stop_Animation();
                    return;
                }
            }
            SpriteRenderer.sprite = Current_Animation[Animation_Index];
        }
    }

    public void Load(UnitSaveData data)
    {
        Manpower = data.Manpower;
        Current_Campaing_Map_Movement = data.Movement;
    }

    public override string ToString()
    {
        return string.Format("{0} (#{1})", Name, Id);
    }

    public static Map.MovementType Get_Movement_Type(List<Unit> units, bool embarking = true)
    {
        if(units == null || units.Count == 0) {
            return Map.MovementType.Immobile;
        }
        Map.MovementType type = Map.MovementType.Land;
        int total = units.Count;
        int land_count = units.Where(x => !x.Tags.Contains(Tag.Naval) && !x.Tags.Contains(Tag.Amphibious)).ToArray().Length;
        int water_count = units.Where(x => x.Tags.Contains(Tag.Naval)).ToArray().Length;
        int amphibious_count = units.Where(x => x.Tags.Contains(Tag.Amphibious)).ToArray().Length;
        if(land_count != 0 && water_count != 0) {
            type = Map.MovementType.Immobile;
        } else if (amphibious_count == total) {
            type = Map.MovementType.Amphibious;
        } else if (water_count != 0 && land_count == 0) {
            type = Map.MovementType.Water;
        }
        if(embarking && (units[0].Army.Hex.Is_Water || (type == Map.MovementType.Land && units[0].Army.Hex.Has_Harbor && units[0].Army.Owner.Has_Transport))) {
            return Map.MovementType.Amphibious;
        }
        return type;
    }

    private class MovementInfo
    {
        public float Movement_Cost { get; set; }
        public bool Run { get; set; }

        public MovementInfo(float movement_cost, bool run)
        {
            Movement_Cost = movement_cost;
            Run = run;
        }

        public MovementInfo() { }
    }
}
