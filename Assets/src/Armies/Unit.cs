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
    private static readonly float morale_gained_on_rout_multiplier = 0.5f;
    private static readonly float morale_damage_bonus_on_charge = 0.25f;

    private static int current_id = 0;

    public enum DamageType { Slash, Thrust, Impact }
    public enum UnitType { Undefined, Infantry, Cavalry }
    public enum Tag
    {
        Small_Shields,
        Medium_Shields,//Axe units tend to gain bonuses against shielded units
        Large_Shields,
        Wooden,//Axe again
        Undead
    }

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
    public float Melee_Attack { get; private set; }
    public float Charge { get; private set; }
    public float Run_Stamina_Cost { get; private set; }
    public Dictionary<DamageType, float> Melee_Attack_Types { get; private set; }
    public float Ranged_Attack { get; private set; }
    public Dictionary<DamageType, float> Ranged_Attack_Types { get; private set; }
    public int Range { get; private set; }
    public int Current_Ammo { get; private set; }
    public int Max_Ammo { get; private set; }
    public float Melee_Defence { get; private set; }
    public float Ranged_Defence { get; private set; }
    public Dictionary<DamageType, float> Resistances { get; private set; }
    public float Morale_Value { get; private set; }
    public float Discipline { get; private set; }
    public List<Tag> Tags { get; private set; }
    public Player Owner { get { return Army != null ? Army.Owner : null; } }
    public List<StatusBar> Bars { get; private set; }

    public bool Has_Moved_This_Turn { get; private set; }
    public bool Last_Move_This_Turn_Was_Running { get; private set; }

    public bool Wait_Turn { get; set; }

    public bool Requires_Coast { get { return false; } }

    private float animation_frame_time_left;
    private bool can_run;
    private float relative_strenght;
    private float upkeep;

    public Unit(Unit prototype)
    {
        Name = prototype.Name;
        Type = prototype.Type;
        Texture = prototype.Texture;
        Max_Movement = prototype.Max_Movement;
        Current_Movement = Max_Movement;
        can_run = prototype.can_run;
        Max_Campaing_Map_Movement = prototype.Max_Campaing_Map_Movement;
        Current_Campaing_Map_Movement = Max_Campaing_Map_Movement;
        Production_Required = prototype.Production_Required;
        Cost = prototype.Cost;
        Upkeep = prototype.Upkeep;
        LoS = prototype.LoS;
        Technology_Required = prototype.Technology_Required;
        Buildinds_Required = prototype.Buildinds_Required;
        Abilities = new List<Ability>();
        foreach(Ability ability in prototype.Abilities) {
            Abilities.Add(ability.Clone());
        }
        Tags = new List<Tag>();
        foreach(Tag t in prototype.Tags) {
            Tags.Add(t);
        }
        Bars = new List<StatusBar>();

        Manpower = 1.0f;
        Max_Morale = prototype.Max_Morale;
        Current_Morale = Max_Morale;
        Max_Stamina = prototype.Max_Stamina;
        Current_Stamina = Max_Stamina;
        Melee_Attack = prototype.Melee_Attack;
        Charge = prototype.Charge;
        Melee_Attack_Types = Helper.Copy_Dictionary(prototype.Melee_Attack_Types);
        Ranged_Attack = prototype.Ranged_Attack;
        Ranged_Attack_Types = Helper.Copy_Dictionary(prototype.Ranged_Attack_Types);
        Range = prototype.Range;
        Max_Ammo = prototype.Max_Ammo;
        Melee_Defence = prototype.Melee_Defence;
        Ranged_Defence = prototype.Ranged_Defence;
        Resistances = Helper.Copy_Dictionary(prototype.Resistances);
        Morale_Value = prototype.Morale_Value;
        Discipline = prototype.Discipline;

        Id = current_id;
        current_id++;

        Update_Relative_Strengh();
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="type"></param>
    /// <param name="texture"></param>
    /// <param name="campaing_map_movement"></param>
    /// <param name="production_required"></param>
    /// <param name="cost"></param>
    /// <param name="upkeep"></param>
    /// <param name="los"></param>
    /// <param name="technology_required"></param>
    /// <param name="buildings_required"></param>
    /// <param name="movement"></param>
    /// <param name="can_run"></param>
    /// <param name="morale"></param>
    /// <param name="stamina"></param>
    /// <param name="melee_attack"></param>
    /// <param name="melee_attack_types"></param>
    /// <param name="charge"></param>
    /// <param name="ranged_attack"></param>
    /// <param name="ranged_attack_types"></param>
    /// <param name="range"></param>
    /// <param name="ammo"></param>
    /// <param name="melee_defence"></param>
    /// <param name="ranged_defence"></param>
    /// <param name="resistances"></param>
    /// <param name="morale_value"></param>
    /// <param name="discipline"></param>
    public Unit(string name, UnitType type, string texture, float campaing_map_movement, int production_required, int cost, float upkeep, int los, Technology technology_required,
        List<Building> buildings_required, float movement, bool can_run, float run_stamina_cost, float morale, float stamina, float melee_attack, Dictionary<DamageType, float> melee_attack_types,
        float charge, float ranged_attack, Dictionary<DamageType, float> ranged_attack_types, int range, int ammo, float melee_defence, float ranged_defence,
        Dictionary<DamageType, float> resistances, float morale_value, float discipline, List<Ability> abilities, List<Tag> tags)
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
        LoS = los;
        Technology_Required = technology_required;
        Buildinds_Required = buildings_required;

        Max_Morale = morale;
        Max_Stamina = stamina;
        Melee_Attack = melee_attack;
        Charge = charge;
        Melee_Attack_Types = melee_attack_types;
        Ranged_Attack = ranged_attack;
        Ranged_Attack_Types = ranged_attack_types;
        Range = range;
        Max_Ammo = ammo;
        Melee_Defence = melee_defence;
        Ranged_Defence = ranged_defence;
        Resistances = resistances;
        Morale_Value = morale_value;
        Discipline = discipline;
        Abilities = abilities;
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
            SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(Texture, SpriteManager.SpriteType.Unit);
            SpriteRenderer.sortingLayerName = SortingLayer.UNITS;
        }

        GameObject.transform.position = Hex.GameObject.transform.position;
        StatusBar.Update_Bars(this);
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
                if(Current_Stamina < 0.0f) {
                    Current_Morale += Current_Stamina * default_morale_lost_when_running_with_no_stamina;
                    Current_Morale = Mathf.Clamp(Current_Morale, 0.0f, Max_Morale);
                    Current_Stamina = 0.0f;
                }
            }
        }

        Has_Moved_This_Turn = true;
        Last_Move_This_Turn_Was_Running = run;

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
            movement_cost += disengagement_movement_cost;
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

    /// <summary>
    /// TODO: Arching shots
    /// </summary>
    /// <returns></returns>
    public List<CombatMapHex> Get_Hexes_In_Attack_Range()
    {
        if(Range <= 0.0f) {
            return new List<CombatMapHex>();
        }
        return Hex.Get_Hexes_Around(Range);
    }

    /// <summary>
    /// TODO: Arching shots
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public bool In_Attack_Range(CombatMapHex hex)
    {
        return Hex.Distance(hex) <= Range;
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
            return relative_strenght * Manpower * ((Stamina_Effect + 1.0f) / 2.0f) * ((Morale_Effect + 1.0f) / 2.0f);
        }
    }

    public float Get_Relative_Strenght_When_On_Hex(WorldMapHex hex, bool current_str, bool attacker)
    {
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
    }

    public void End_Combat()
    {
        Combat_Refill();
    }

    private void Combat_Refill()
    {
        Current_Morale = Max_Morale;
        Current_Stamina = Max_Stamina;
        Current_Movement = Max_Movement;
        Current_Ammo = Max_Ammo;
        Can_Attack = true;
        Has_Moved_This_Turn = false;
        Last_Move_This_Turn_Was_Running = false;
    }

    public void End_Combat_Turn()
    {
        if(Hex == null) {
            return;
        }

        Current_Movement = Max_Movement;
        Can_Attack = true;

        if(Uses_Stamina && !Has_Moved_This_Turn && !Hex.Is_Adjancent_To_Enemy(Owner)) {
            Current_Stamina += (default_stamina_regeneration * Max_Stamina);
            Current_Stamina = Mathf.Clamp(Current_Stamina, 0.0f, Max_Stamina);
            StatusBar.Update_Bars(this);
        }

        Has_Moved_This_Turn = false;
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
            return Range > 0.0f && Ranged_Attack > 0.0f && (Current_Ammo > 0 || Max_Ammo <= 0) && !Hex.Is_Adjancent_To_Enemy(Owner);
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
        defender_result.Morale_Delta = -damage * default_morale_damage * 100.0f * morale_damage_multiplier;
        defender_result.Damage_Effectiveness = damage / Calculate_Standard_Melee_Damage(target);
        

        //Defender's attack
        attacker_result.Can_Attack = false;
        attacker_result.Movement = 0.0f;
        attacker_result.Stamina_Delta = -default_attack_stamina_cost;

        damage = target.Calculate_Melee_Damage(this, attacker_result);
        attacker_result.Manpower_Delta = -damage;
        attacker_result.Morale_Delta = -damage * default_morale_damage * 100.0f;
        attacker_result.Damage_Effectiveness = damage / target.Calculate_Standard_Melee_Damage(this);


        if (!preview) {
            EffectManager.Instance.Play_Effect(Hex, "melee");
            EffectManager.Instance.Play_Effect(target.Hex, "melee");
            Apply(attacker_result);
            target.Apply(defender_result);
            StatusBar.Update_Bars(this);
            StatusBar.Update_Bars(target);
            CombatLogManager.Instance.Print_Log(string.Format("{0} {1} {2} ({3}/{4} dmg dealt, {5}/{6} dmg taken)", Name, Last_Move_This_Turn_Was_Running ? "charges" : "melee attacks",
                target.Name, Mathf.RoundToInt(defender_result.Manpower_Delta * -100.0f), Mathf.RoundToInt(defender_result.Morale_Delta * -1.0f), Mathf.RoundToInt(attacker_result.Manpower_Delta * -100.0f),
                Mathf.RoundToInt(attacker_result.Morale_Delta * -1.0f)));
        }

        return new AttackResult[2] { attacker_result, defender_result };
    }

    private float Calculate_Standard_Melee_Damage(Unit target)
    {
        float attack = Melee_Attack;
        float defence = target.Melee_Defence;
        return (((attack + damage_balancer) / (defence + damage_balancer)) * manpower_damage) * Manpower;
    }

    private float Calculate_Melee_Damage(Unit target, AttackResult result)
    {
        float attack = Melee_Attack * Morale_Effect * Stamina_Effect;
        float ability_attack_delta = 0.0f;
        float ability_attack_multiplier = 1.0f;
        float defence = target.Melee_Defence * target.Morale_Effect * target.Stamina_Effect;
        float ability_defence_delta = 0.0f;
        float ability_defence_multiplier = 1.0f;

        //Charge TODO: double loops
        float ability_charge_multiplier = 1.0f;
        Dictionary<DamageType, float> melee_attack_types = Melee_Attack_Types;
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
            attack *= (1.0f + (Charge * Mathf.Max(ability_charge_multiplier, 0.05f)));
        }

        //Base defence
        float resistance_multiplier = 0.0f;
        float total = 0.0f;
        foreach(KeyValuePair<DamageType, float> damage in melee_attack_types) {
            float resistance_to_type = target.Resistances.ContainsKey(damage.Key) ? target.Resistances[damage.Key] : 1.0f;
            resistance_multiplier += damage.Value * resistance_to_type;
            total += damage.Value;
        }
        if(total != 1.0f) {
            CustomLogger.Instance.Warning(string.Format("Unit {0} does not have proper melee damage types, they have sum of {1}", Name, total));
        }
        defence *= resistance_multiplier;

        //Ability delta
        foreach (Ability ability in Abilities) {
            if (ability.On_Calculate_Melee_Damage_As_Attacker == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Melee_Damage_As_Attacker(ability, this, target, result);
            ability_attack_delta += data.Attack_Delta;
            ability_attack_multiplier += data.Attack_Multiplier;
            ability_defence_delta += data.Defence_Delta;
            ability_defence_multiplier += data.Defence_Multiplier;
        }
        foreach (Ability ability in target.Abilities) {
            if (ability.On_Calculate_Melee_Damage_As_Defender == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Melee_Damage_As_Defender(ability, this, target, result);
            ability_attack_delta += data.Attack_Delta;
            ability_attack_multiplier += data.Attack_Multiplier;
            ability_defence_delta += data.Defence_Delta;
            ability_defence_multiplier += data.Defence_Multiplier;
        }
        attack = Mathf.Max(attack + ability_attack_delta, 1.0f);
        attack *= Mathf.Max(ability_attack_multiplier, 0.05f);
        defence = Mathf.Max(defence + ability_defence_delta, 1.0f);
        defence *= Mathf.Max(ability_defence_multiplier, 0.05f);

        result.Final_Attack = attack;
        result.Base_Attack = Melee_Attack;
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
        attacker_result.Morale_Delta = 0.0f;

        if (!preview) {
            EffectManager.Instance.Play_Effect(Hex, "ranged_attack");
            EffectManager.Instance.Play_Effect(target.Hex, "ranged_target");
            Apply(attacker_result);
            target.Apply(defender_result);
            StatusBar.Update_Bars(this);
            StatusBar.Update_Bars(target);
            if (Max_Ammo > 0) {
                Current_Ammo--;
            }
            CombatLogManager.Instance.Print_Log(string.Format("{0} ranged attacks {1} ({2}/{3} dmg dealt)", Name, target.Name, Mathf.RoundToInt(defender_result.Manpower_Delta * -100.0f),
                Mathf.RoundToInt(defender_result.Morale_Delta * -1.0f)));
        }

        return new AttackResult[2] { attacker_result, defender_result };
    }

    private float Calculate_Standard_Ranged_Damage(Unit target)
    {
        float attack = Ranged_Attack;
        float defence = target.Ranged_Defence;
        return (((attack + damage_balancer) / (defence + damage_balancer)) * manpower_damage) * Manpower;
    }

    private float Calculate_Ranged_Damage(Unit target, AttackResult result)
    {
        float attack = Ranged_Attack * ((Morale_Effect + 1.0f) / 2.0f) * Stamina_Effect;
        float ability_attack_delta = 0.0f;
        float ability_attack_multiplier = 1.0f;
        float defence = target.Ranged_Defence * ((target.Morale_Effect + 1.0f) / 2.0f) * ((target.Stamina_Effect + 1.0f) / 2.0f);
        float ability_defence_delta = 0.0f;
        float ability_defence_multiplier = 1.0f;
        float resistance_multiplier = 0.0f;
        float total = 0.0f;
        foreach (KeyValuePair<DamageType, float> damage in Ranged_Attack_Types) {
            float resistance_to_type = target.Resistances.ContainsKey(damage.Key) ? target.Resistances[damage.Key] : 1.0f;
            resistance_multiplier += damage.Value * resistance_to_type;
            total += damage.Value;
        }
        if (total != 1.0f) {
            CustomLogger.Instance.Warning(string.Format("Unit {0} does not have proper ranged damage types, they have sum of {1}", Name, total));
        }
        defence *= resistance_multiplier;

        //Ability delta
        foreach (Ability ability in Abilities) {
            if (ability.On_Calculate_Ranged_Damage_As_Attacker == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Ranged_Damage_As_Attacker(ability, this, target, result);
            ability_attack_delta += data.Attack_Delta;
            ability_attack_multiplier += data.Attack_Multiplier;
            ability_defence_delta += data.Defence_Delta;
            ability_defence_multiplier += data.Defence_Multiplier;
        }
        foreach (Ability ability in target.Abilities) {
            if (ability.On_Calculate_Ranged_Damage_As_Defender == null) {
                continue;
            }
            Ability.DamageData data = ability.On_Calculate_Ranged_Damage_As_Defender(ability, this, target, result);
            ability_attack_delta += data.Attack_Delta;
            ability_attack_multiplier += data.Attack_Multiplier;
            ability_defence_delta += data.Defence_Delta;
            ability_defence_multiplier += data.Defence_Multiplier;
        }
        attack = Mathf.Max(attack + ability_attack_delta, 1.0f);
        attack *= Mathf.Max(ability_attack_multiplier, 0.05f);
        defence = Mathf.Max(defence + ability_defence_delta, 1.0f);
        defence *= Mathf.Max(ability_defence_multiplier, 0.05f);

        result.Final_Attack = attack;
        result.Base_Attack = Ranged_Attack;
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
            EffectManager.Instance.Play_Floating_Text(Hex, Mathf.RoundToInt(result.Morale_Delta).ToString(), EffectManager.TextType.Morale);
        }

        if(had_morale && Current_Morale == 0.0f) {
            CombatLogManager.Instance.Print_Log(string.Format("{0} routs", Name));
            Apply_On_Rout_Morale_AoE(Morale_Value, on_rout_morale_damage_aoe_radios);
        }

        if(Manpower == 0.0f) {
            Undeploy();
        }
    }

    private void Apply_On_Rout_Morale_AoE(float effect, int radius)
    {
        List<Unit> newly_routed_units = new List<Unit>();
        foreach(CombatMapHex hex in Hex.Get_Hexes_Around(radius)) {
            if(hex.Unit != null && hex.Unit.Id != Id && hex.Unit.Current_Morale != 0.0f) {
                if (hex.Unit.Army.Is_Owned_By(Army.Owner)) {
                    //Morale dmg
                    float morale_delta = effect * hex.Unit.Discipline_Morale_Damage_Multiplier;
                    hex.Unit.Current_Morale = Mathf.Clamp(hex.Unit.Current_Morale - morale_delta, 0.0f, hex.Unit.Max_Morale);
                    CombatLogManager.Instance.Print_Log(string.Format("{0} loses {1} morale", hex.Unit.Name, Mathf.RoundToInt(morale_delta)), CombatLogManager.LogLevel.Verbose);
                    EffectManager.Instance.Play_Floating_Text(hex, Mathf.RoundToInt(morale_delta).ToString(), EffectManager.TextType.Morale);
                    if (hex.Unit.Current_Morale == 0.0f) {
                        newly_routed_units.Add(hex.Unit);
                        CombatLogManager.Instance.Print_Log(string.Format("{0} routs", hex.Unit.Name));
                    }
                } else {
                    //Morale bonus
                    float morale_delta = effect * morale_gained_on_rout_multiplier;
                    CombatLogManager.Instance.Print_Log(string.Format("{0} gains {1} morale", hex.Unit.Name, Mathf.RoundToInt(morale_delta)), CombatLogManager.LogLevel.Verbose);
                    EffectManager.Instance.Play_Floating_Text(hex, "+" + Mathf.RoundToInt(morale_delta).ToString(), EffectManager.TextType.Morale);
                    hex.Unit.Current_Morale = Mathf.Clamp(hex.Unit.Current_Morale + morale_delta, 0.0f, hex.Unit.Max_Morale);
                }
            }
        }
        foreach(Unit u in newly_routed_units) {
            u.Apply_On_Rout_Morale_AoE(u.Morale_Value, on_rout_morale_damage_aoe_radios);
        }
    }

    public bool Retreat()
    {
        if (Hex == null || Current_Morale > 0.0f || !Uses_Morale) {
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
        relative_strenght = ((Max_Morale - 50.0f) / 2.0f) + ((Max_Stamina - 50.0f) / 2.0f) + Melee_Attack + Melee_Defence + (Ranged_Attack * Range * 0.25f) +
            Ranged_Defence + (Max_Movement * 2.0f);
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
            if(Type != UnitType.Undefined) {
                tooltip.Append(Environment.NewLine).Append("Type: ").Append(Type.ToString());
            }
            if(Tags.Count != 0) {
                tooltip.Append(Environment.NewLine).Append("Tags: ");
                foreach(Tag t in Tags) {
                    tooltip.Append(Helper.Parse_To_Human_Readable(t.ToString())).Append(", ");
                }
                tooltip.Remove(tooltip.Length - 2, 2);
            }
            tooltip.Append(Environment.NewLine).Append("Upkeep: ").Append(Math.Round(Upkeep, 2).ToString("0.00"));
            tooltip.Append(Environment.NewLine).Append("Relative Strenght: ").Append(Mathf.RoundToInt(Relative_Strenght));
            tooltip.Append(Environment.NewLine).Append("Morale: ").Append(Mathf.RoundToInt(Max_Morale));
            tooltip.Append(Environment.NewLine).Append("Stamina: ").Append(Mathf.RoundToInt(Max_Stamina));
            tooltip.Append(Environment.NewLine).Append("Movement: ").Append(Mathf.RoundToInt(Max_Movement)).Append(" / ").Append(Mathf.RoundToInt(Max_Campaing_Map_Movement));
            tooltip.Append(Environment.NewLine).Append("LoS: ").Append(LoS);
            tooltip.Append(Environment.NewLine).Append("Melee Attack: ").Append(Mathf.RoundToInt(Melee_Attack)).Append(" (");
            int i = 0;
            foreach(KeyValuePair<DamageType, float> pair in Melee_Attack_Types) {
                tooltip.Append(pair.Key.ToString()).Append(": ").Append(Mathf.RoundToInt(pair.Value * 100.0f)).Append("%");
                if(i != Melee_Attack_Types.Count - 1) {
                    tooltip.Append(", ");
                }
                i++;
            }
            tooltip.Append(")");
            tooltip.Append(Environment.NewLine).Append("Charge Bonus: ").Append(Mathf.RoundToInt(Charge * 100.0f)).Append("%");
            if (Ranged_Attack > 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Ranged Attack: ").Append(Mathf.RoundToInt(Ranged_Attack)).Append(" (");
                i = 0;
                foreach (KeyValuePair<DamageType, float> pair in Ranged_Attack_Types) {
                    tooltip.Append(pair.Key.ToString()).Append(": ").Append(Mathf.RoundToInt(pair.Value * 100.0f)).Append("%");
                    if (i != Ranged_Attack_Types.Count - 1) {
                        tooltip.Append(", ");
                    }
                    i++;
                }
                tooltip.Append(")");
                tooltip.Append(Environment.NewLine).Append("Range: ").Append(Range);
                tooltip.Append(Environment.NewLine).Append("Ammo: ").Append(Max_Ammo);
            }
            tooltip.Append(Environment.NewLine).Append("Melee Defence: ").Append(Mathf.RoundToInt(Melee_Defence));
            tooltip.Append(Environment.NewLine).Append("Ranged Defence: ").Append(Mathf.RoundToInt(Ranged_Defence));
            tooltip.Append(Environment.NewLine).Append("Discipline: ").Append(Mathf.RoundToInt(Discipline));
            if(Abilities.Count != 0) {
                tooltip.Append(Environment.NewLine).Append("Abilities:");
                foreach(Ability ability in Abilities) {
                    tooltip.Append(Environment.NewLine).Append("- ").Append(ability.Name);
                    if (ability.Uses_Potency) {
                        tooltip.Append(" ");
                        if (ability.Potency_As_Percent) {
                            tooltip.Append(Mathf.RoundToInt(100.0f * ability.Potency)).Append("%");
                        } else {
                            tooltip.Append(Math.Round(ability.Potency, 1));
                        }
                    }
                }
            }
            return tooltip.ToString();
        }
    }

    public override string ToString()
    {
        return string.Format("{0} (#{1})", Name, Id);
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
