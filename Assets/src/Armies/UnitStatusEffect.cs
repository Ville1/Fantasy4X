public class UnitStatusEffect {
    public delegate void EffectDelegate(UnitStatusEffect effect, Unit unit);
    public enum EffectType { Debuff, Buff, Mixed }

    public string Internal_Name { get; private set; }
    public string Name { get; private set; }
    public int Turns_Max { get; private set; }
    public int Turns_Left { get; set; }
    public EffectType Effect_Type { get; private set; }
    public bool Stacks { get; private set; }
    public string Sprite_Name { get; private set; }
    public SpriteManager.SpriteType Sprite_Type { get; private set; }
    public EffectDelegate On_Turn_Start { get; private set; }
    public EffectDelegate On_Turn_End { get; private set; }
    public EffectDelegate On_Expiration { get; private set; }
    public EffectContainer Effects { get; private set; }

    public UnitStatusEffect(string internal_name, string name, int turns, EffectType type, bool stacks, string sprite, SpriteManager.SpriteType sprite_type)
    {
        Internal_Name = internal_name;
        Name = name;
        Turns_Max = turns;
        Turns_Left = turns;
        Effect_Type = type;
        Stacks = stacks;
        Sprite_Name = sprite;
        Sprite_Type = sprite_type;
        On_Turn_Start = null;
        On_Turn_End = null;
        On_Expiration = null;
        Effects = new EffectContainer();
    }

    public override string ToString()
    {
        return string.Format("{0} {1}/{2}", Internal_Name, Turns_Left, Turns_Max);
    }

    public void Start_Turn(Unit unit)
    {
        if(On_Turn_Start != null) {
            On_Turn_Start(this, unit);
        }
    }

    public void End_Turn(Unit unit)
    {
        if (On_Turn_End != null) {
            On_Turn_End(this, unit);
        }
    }

    public void Expire(Unit unit)
    {
        if (On_Expiration != null) {
            On_Expiration(this, unit);
        }
    }

    public class EffectContainer
    {
        public bool Disables_Stealth { get; set; }
        public float Movement_Delta { get; set; }
        public float Melee_Attack_Delta_Flat { get; set; }
        public float Melee_Attack_Delta_Multiplier { get; set; }
        public float Ranged_Attack_Delta_Flat { get; set; }
        public float Ranged_Attack_Delta_Multiplier { get; set; }
        public float Melee_Defence_Delta_Flat { get; set; }
        public float Melee_Defence_Delta_Multiplier { get; set; }
        public float Ranged_Defence_Delta_Flat { get; set; }
        public float Ranged_Defence_Delta_Multiplier { get; set; }

        public EffectContainer()
        {
            Disables_Stealth = false;
            Movement_Delta = 0.0f;
            Melee_Attack_Delta_Flat = 0.0f;
            Melee_Attack_Delta_Multiplier = 0.0f;
            Ranged_Attack_Delta_Flat = 0.0f;
            Ranged_Attack_Delta_Multiplier = 0.0f;
            Melee_Defence_Delta_Flat = 0.0f;
            Melee_Defence_Delta_Multiplier = 0.0f;
            Ranged_Defence_Delta_Flat = 0.0f;
            Ranged_Defence_Delta_Multiplier = 0.0f;
        }

        public EffectContainer Add(EffectContainer effects)
        {
            Disables_Stealth = effects.Disables_Stealth ? true : Disables_Stealth;
            Movement_Delta += effects.Movement_Delta;
            Melee_Attack_Delta_Flat += effects.Melee_Attack_Delta_Flat;
            Melee_Attack_Delta_Multiplier += effects.Melee_Attack_Delta_Multiplier;
            Ranged_Attack_Delta_Flat += effects.Ranged_Attack_Delta_Flat;
            Ranged_Attack_Delta_Multiplier += effects.Ranged_Attack_Delta_Multiplier;
            Melee_Defence_Delta_Flat += effects.Melee_Defence_Delta_Flat;
            Melee_Defence_Delta_Multiplier += effects.Melee_Defence_Delta_Multiplier;
            Ranged_Defence_Delta_Flat += effects.Ranged_Defence_Delta_Flat;
            Ranged_Defence_Delta_Multiplier += effects.Ranged_Defence_Delta_Multiplier;
            return this;
        }

        public void Multiply(float amount)
        {
            Movement_Delta *= amount;
            Melee_Attack_Delta_Flat *= amount;
            Melee_Attack_Delta_Multiplier *= amount;
            Ranged_Attack_Delta_Flat *= amount;
            Ranged_Attack_Delta_Multiplier *= amount;
            Melee_Defence_Delta_Flat *= amount;
            Melee_Defence_Delta_Multiplier *= amount;
            Ranged_Defence_Delta_Flat *= amount;
            Ranged_Defence_Delta_Multiplier *= amount;
        }
    }
}
