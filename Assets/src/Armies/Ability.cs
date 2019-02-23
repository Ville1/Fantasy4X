﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class Ability {
    public delegate DamageData Calculate_Damage_Delegate(Ability ability, Unit attacker, Unit target, AttackResult result);
    public delegate float Get_Fluctuating_Relative_Strength_Multiplier_Delegate(Ability ability, WorldMapHex hex, bool attacker);
    public delegate float Get_Relative_Strength_Multiplier_Delegate(Ability ability);
    public delegate float Get_Run_Stamina_Cost_Multiplier_Delegate(Ability ability, CombatMapHex hex);
    public delegate float Get_Upkeep_Multiplier_Delegate(Ability ability, WorldMapHex hex);

    public string Name { get; private set; }
    public float Potency { get; set; }
    public bool Potency_As_Percent { get; private set; }
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

    public Ability(string name, float potency, bool potency_as_percent)
    {
        Name = name;
        Potency = potency;
        Potency_As_Percent = potency_as_percent;
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="potency_as_percent"></param>
    public Ability(string name, bool potency_as_percent)
    {
        Name = name;
        Potency = 0.0f;
        Potency_As_Percent = potency_as_percent;
    }
    
    public bool Uses_Potency
    {
        get {
            return Potency != -1.0f;
        }
    }

    public Ability Clone()
    {
        Ability clone = new Ability(Name, Potency, Potency_As_Percent);
        clone.On_Calculate_Melee_Damage_As_Attacker = On_Calculate_Melee_Damage_As_Attacker;
        clone.On_Calculate_Melee_Damage_As_Defender = On_Calculate_Melee_Damage_As_Defender;
        clone.On_Calculate_Ranged_Damage_As_Attacker = On_Calculate_Ranged_Damage_As_Attacker;
        clone.On_Calculate_Ranged_Damage_As_Defender = On_Calculate_Ranged_Damage_As_Defender;
        clone.Get_Fluctuating_Relative_Strength_Multiplier_Bonus = Get_Fluctuating_Relative_Strength_Multiplier_Bonus;
        clone.Get_Relative_Strength_Multiplier_Bonus = Get_Relative_Strength_Multiplier_Bonus;
        clone.Get_Run_Stamina_Cost_Multiplier = Get_Run_Stamina_Cost_Multiplier;
        clone.Get_Upkeep_Multiplier = Get_Upkeep_Multiplier;
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
        public Dictionary<Unit.DamageType, float> New_Attack_Types { get; set; }
    }
}
