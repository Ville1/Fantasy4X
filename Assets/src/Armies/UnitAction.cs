using System;
using System.Collections.Generic;

public class UnitAction {
    public delegate bool ActionEffectDelegate(Unit unit, UnitAction action, CombatMapHex hex, bool is_preview, out AttackResult[] result, out string message);

    public enum TargetingType { Self, Enemy, Ally, Ground }

    public string Name { get; private set; }
    public string Internal_Name { get; private set; }
    public ActionEffectDelegate Effect { get; private set; }
    public int Cooldown { get; private set; }
    public int Current_Cooldown { get; set; }
    public int Mana_Cost { get; private set; }
    public string Sprite_Name { get; private set; }
    public SpriteManager.SpriteType Sprite_Type { get; private set; }
    public int Range { get; private set; }
    public bool Arch_Affects_Range { get; private set; }
    public TargetingType Targeting { get; private set; }
    public int Ammo_Cost { get; private set; }
    public string Effect_Name { get; private set; }
    public bool Has_Effect_Name { get { return !string.IsNullOrEmpty(Effect_Name); } }
    public AnimationData Animation { get; private set; }
    public bool Has_Animation { get { return Animation != null; } }
    public float Relative_Strenght_Bonus_Flat { get; private set; }
    public float Relative_Strenght_Bonus_Multiplier { get; private set; }

    public UnitAction(string name, string internal_name, int cooldown, int mana_cost, int ammo_cost, int range, bool arch_affects_range, TargetingType targeting, float relative_strenght_bonus_flat,
        float relative_strenght_bonus_multiplier, string sprite_name, SpriteManager.SpriteType sprite_type, string effect_name, AnimationData animation, ActionEffectDelegate effect)
    {
        Name = name;
        Internal_Name = internal_name;
        Cooldown = cooldown;
        Mana_Cost = mana_cost;
        Ammo_Cost = ammo_cost;
        Sprite_Name = sprite_name;
        Sprite_Type = sprite_type;
        Effect = effect;
        Range = range;
        Arch_Affects_Range = arch_affects_range;
        Targeting = targeting;
        Relative_Strenght_Bonus_Flat = relative_strenght_bonus_flat;
        Relative_Strenght_Bonus_Multiplier = relative_strenght_bonus_multiplier;
        Current_Cooldown = 0;
        Effect_Name = effect_name;
        Animation = animation;
    }

    public UnitAction Clone
    {
        get {
            return new UnitAction(Name, Internal_Name, Cooldown, Mana_Cost, Ammo_Cost, Range, Arch_Affects_Range, Targeting, Relative_Strenght_Bonus_Flat, Relative_Strenght_Bonus_Multiplier, Sprite_Name, Sprite_Type, Effect_Name, Animation, Effect);
        }
    }

    public bool Activate(Unit unit, CombatMapHex hex, bool is_preview, out AttackResult[] result, out string message)
    {
        result = null;
        if (unit.Current_Ammo < Ammo_Cost) {
            message = string.Format("Out of ammo, {0} needed", Ammo_Cost.ToString());
            return false;
        }
        if (!unit.Can_Attack) {
            message = "Unit has already attacked/activated ability this turn";
            return false;
        }
        if(Current_Cooldown > 0) {
            message = "On cooldown";
            return false;
        }
        if(Mana_Cost > unit.Current_Combat_Mana) {
            message = "Not enough mana";
            return false;
        }
        if(!Get_Hexes_In_Range(this, unit).Contains(hex)) {
            message = "Out of range";
            return false;
        }
        if (Targeting == TargetingType.Enemy && (hex.Unit == null || hex.Unit.Army.Is_Owned_By(unit.Army.Owner) || !hex.Unit.Is_Visible)) {
            message = "Must target an enemy unit";
            return false;
        }
        if (Targeting == TargetingType.Ally && (hex.Unit == null || !hex.Unit.Army.Is_Owned_By(unit.Army.Owner))) {
            message = "Must target an ally unit";
            return false;
        }
        bool success = Effect(unit, this, hex, is_preview, out result, out message);
        if(success && !is_preview) {
            Current_Cooldown = Cooldown;
            unit.Current_Combat_Mana -= Mana_Cost;
        }
        return success;
    }

    public static List<CombatMapHex> Get_Hexes_In_Range(UnitAction action, Unit unit)
    {
        return action.Targeting == TargetingType.Self ? new List<CombatMapHex>() { unit.Hex } : action.Range > 1 ?
            action.Arch_Affects_Range ? unit.Get_Hexes_In_Attack_Range(action.Range) : unit.Hex.Get_Hexes_Around(action.Range) : unit.Hex.Get_Adjancent_Hexes();
    }

    public AttackResult[] Melee_Attack(Unit attacker, Unit defender, Damage damage, bool is_preview)
    {
        return attacker.Attack(defender, is_preview, new AttackData(damage, string.Format("use{0} {1} on", attacker.Is_Single_Entity ? "s" : string.Empty, Name.ToLower()), true, this));
    }

    public AttackResult[] Ranged_Attack(Unit attacker, Unit defender, Damage damage, bool is_preview)
    {
        return attacker.Attack(defender, is_preview, new AttackData(damage, string.Format("use{0} {1} on", attacker.Is_Single_Entity ? "s" : string.Empty, Name.ToLower()), false, this));
    }
    
    public void Apply_Debuff(Unit attacker, Unit defender, UnitStatusEffect debuff, bool scale_with_manpower, bool is_preview)
    {
        if(debuff.Effect_Type == UnitStatusEffect.EffectType.Buff) {
            CustomLogger.Instance.Error("Status effect is a buff");
            return;
        }
        if (is_preview) {
            return;
        }
        if (scale_with_manpower) {
            debuff.Effects.Multiply(Math.Min(1.0f, (attacker.Is_Single_Entity ? 1.0f : attacker.Manpower) / (defender.Is_Single_Entity ? 1.0f : defender.Manpower)));
        }
        defender.Apply_Status_Effect(debuff);
    }

    public override string ToString()
    {
        return Internal_Name;
    }

    public class AttackData
    {
        public Damage Damage { get; set; }
        public bool Is_Melee { get; set; }
        public bool Retaliation_Enabled { get; set; }
        public float? Stamina_Cost { get; set; }
        public string Log_Action { get; set; }
        public UnitAction Action { get; set; }

        public AttackData(Damage damage, string log, bool is_melee, UnitAction action)
        {
            Damage = damage;
            Log_Action = log;
            Is_Melee = is_melee;
            Action = action;
            Retaliation_Enabled = false;
            Stamina_Cost = null;
        }
    }
}
