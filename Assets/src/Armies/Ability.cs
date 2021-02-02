using System;
using System.Collections.Generic;
using UnityEngine;

public class Ability {
    public delegate DamageData Calculate_Damage_Delegate(Ability ability, Unit attacker, Unit target, AttackResult result);
    public delegate float Get_Fluctuating_Relative_Strength_Multiplier_Delegate(Ability ability, WorldMapHex hex, bool attacker);
    public delegate float Get_Relative_Strength_Multiplier_Delegate(Ability ability);
    public delegate float Get_Run_Stamina_Cost_Multiplier_Delegate(Ability ability, CombatMapHex hex);
    public delegate bool Allow_Ranged_Attack_Delegate(Ability ability, Unit unit, CombatMapHex hex);
    public delegate float Get_Upkeep_Multiplier_Delegate(Ability ability, WorldMapHex hex);
    public delegate CityEffects Get_City_Effects_Delegate(Ability ability, City city);
    public delegate Yields On_Worked_Hex_Raid_Delegate(Ability ability, Unit unit, WorldMapHex hex);
    public delegate void On_Turn_End_Delegate(Ability ability, Unit unit);
    public delegate void On_Turn_Start_Delegate(Ability ability, Unit unit);
    public delegate float Get_Disengagement_Movement_Cost_Multiplier_End_Delegate(Ability ability, Unit unit);
    public delegate float Get_Stealth_Delegate(Ability ability, Unit unit);
    public delegate float Get_Detection_Delegate(Ability ability, Unit unit, CombatMapHex hex);

    public string Name { get; private set; }
    public float Potency { get; set; }
    public bool Potency_As_Percent { get; private set; }
    public bool Uses_Potency { get; private set; }
    public Calculate_Damage_Delegate On_Calculate_Melee_Damage_As_Attacker { get; set; }
    public Calculate_Damage_Delegate On_Calculate_Melee_Damage_As_Defender { get; set; }
    public Calculate_Damage_Delegate On_Calculate_Ranged_Damage_As_Attacker { get; set; }
    public Calculate_Damage_Delegate On_Calculate_Ranged_Damage_As_Defender { get; set; }
    /// <summary>
    /// 0.1f => +10%
    /// </summary>
    public Get_Fluctuating_Relative_Strength_Multiplier_Delegate Get_Fluctuating_Relative_Strength_Multiplier_Bonus { get; set; }
    /// <summary>
    /// 0.1f => +10%
    /// </summary>
    public Get_Relative_Strength_Multiplier_Delegate Get_Relative_Strength_Multiplier_Bonus { get; set; }
    /// <summary>
    /// 0.1f => +10%
    /// </summary>
    public Get_Run_Stamina_Cost_Multiplier_Delegate Get_Run_Stamina_Cost_Multiplier { get; set; }
    /// <summary>
    /// 0.1f => +10%
    /// </summary>
    public Get_Upkeep_Multiplier_Delegate Get_Upkeep_Multiplier { get; set; }
    public Get_City_Effects_Delegate Get_City_Effects { get; set; }
    public Allow_Ranged_Attack_Delegate On_Allow_Ranged_Attack { get; set; }
    public On_Worked_Hex_Raid_Delegate On_Worked_Hex_Raid { get; set; }
    public On_Turn_End_Delegate On_Campaign_Turn_End { get; set; }
    public On_Turn_End_Delegate On_Combat_Turn_End { get; set; }
    public On_Turn_Start_Delegate On_Combat_Turn_Start { get; set; }
    public Get_Disengagement_Movement_Cost_Multiplier_End_Delegate Get_Disengagement_Movement_Cost { get; set; }
    public Get_Stealth_Delegate Get_Stealth { get; set; }
    public Get_Detection_Delegate Get_Detection { get; set; }

    public Ability(string name, float potency, bool potency_as_percent, bool uses_potency)
    {
        Name = name;
        Potency = potency;
        Potency_As_Percent = potency_as_percent;
        Uses_Potency = uses_potency;
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="potency_as_percent"></param>
    public Ability(string name, bool potency_as_percent, bool uses_potency)
    {
        Name = name;
        Potency = 0.0f;
        Potency_As_Percent = potency_as_percent;
        Uses_Potency = uses_potency;
    }

    public Ability Clone()
    {
        Ability clone = new Ability(Name, Potency, Potency_As_Percent, Uses_Potency);
        clone.On_Calculate_Melee_Damage_As_Attacker = On_Calculate_Melee_Damage_As_Attacker;
        clone.On_Calculate_Melee_Damage_As_Defender = On_Calculate_Melee_Damage_As_Defender;
        clone.On_Calculate_Ranged_Damage_As_Attacker = On_Calculate_Ranged_Damage_As_Attacker;
        clone.On_Calculate_Ranged_Damage_As_Defender = On_Calculate_Ranged_Damage_As_Defender;
        clone.Get_Fluctuating_Relative_Strength_Multiplier_Bonus = Get_Fluctuating_Relative_Strength_Multiplier_Bonus;
        clone.Get_Relative_Strength_Multiplier_Bonus = Get_Relative_Strength_Multiplier_Bonus;
        clone.Get_Run_Stamina_Cost_Multiplier = Get_Run_Stamina_Cost_Multiplier;
        clone.Get_Upkeep_Multiplier = Get_Upkeep_Multiplier;
        clone.Get_City_Effects = Get_City_Effects;
        clone.On_Allow_Ranged_Attack = On_Allow_Ranged_Attack;
        clone.On_Worked_Hex_Raid = On_Worked_Hex_Raid;
        clone.On_Campaign_Turn_End = On_Campaign_Turn_End;
        clone.On_Combat_Turn_End = On_Combat_Turn_End;
        clone.On_Combat_Turn_Start = On_Combat_Turn_Start;
        clone.Get_Disengagement_Movement_Cost = Get_Disengagement_Movement_Cost;
        clone.Get_Stealth = Get_Stealth;
        clone.Get_Detection = Get_Detection;
        return clone;
    }

    public override string ToString()
    {
        return string.Format("{0}{1}", Name, Uses_Potency ? (Potency_As_Percent ? string.Format(" {0}%", Mathf.RoundToInt(100.0f * Potency)) : string.Format(" {0}", Math.Round(Potency, 1))) : "");
    }

    public class DamageData
    {
        public float Attack_Delta { get; set; }
        public float Defence_Delta { get; set; }
        public float Attack_Multiplier { get; set; }
        public float Defence_Multiplier { get; set; }
        public float Charge_Multiplier { get; set; }
        /// <summary>
        /// TODO: Does not work with ranged attacks
        /// </summary>
        public Dictionary<Damage.Type, float> New_Attack_Types { get; set; }
    }

    public class CityEffects
    {
        public Yields Yields { get; set; }
        public float Happiness { get; set; }
        public float Health { get; set; }
        public float Order { get; set; }

        public CityEffects()
        {
            Yields = new Yields();
        }

        public void Add(CityEffects effects)
        {
            Yields.Add(effects.Yields);
            Happiness += effects.Happiness;
            Health += effects.Health;
            Order += effects.Order;
        }
    }
}
