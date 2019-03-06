using System;
using System.Text;
using UnityEngine;

public class Spell {
    private static int current_id = 0;

    public delegate SpellResult Spell_Effect(Spell spell, Player caster, WorldMapHex hex);

    public string Name { get; private set; }
    public int Id { get; private set; }
    public float Mana_Cost { get; private set; }
    public int Cooldown { get; private set; }
    public Technology Technology_Required { get; private set; }
    public bool Requires_Target { get; private set; }
    public Spell_Effect Effect { get; private set; }
    public string Effect_Animation { get; private set; }

    public Spell(string name, float mana_cost, int cooldown, Technology technology_required, bool requires_target, Spell_Effect effect, string effect_animation = "default_effect")
    {
        Name = name;
        Mana_Cost = mana_cost;
        Cooldown = cooldown;
        Technology_Required = technology_required;
        Requires_Target = requires_target;
        Effect = effect;
        Effect_Animation = effect_animation;
        Id = current_id;
        current_id++;
    }

    public SpellResult Cast(Player caster, WorldMapHex hex)
    {
        if(Requires_Target && hex == null) {
            return new SpellResult() { Success = false, Message = "Requires a target hex" };
        }
        SpellResult result = Effect(this, caster, hex);
        if (result.Success) {
            caster.Put_On_Cooldown(this);
            caster.Mana -= Mana_Cost;
            if(!string.IsNullOrEmpty(Effect_Animation) && hex != null && (hex.Current_LoS == WorldMapHex.LoS_Status.Visible || !Main.Instance.Other_Players_Turn)) {
                //TODO: Do this in GUIManager?
                EffectManager.Instance.Play_Effect(hex, Effect_Animation);
            }
        }
        return result;
    }
    
    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder(Name);
            tooltip.Append(Environment.NewLine).Append("Mana cost: ").Append(Mathf.RoundToInt(Mana_Cost));
            if(Cooldown > 0) {
                tooltip.Append(Environment.NewLine).Append("Cooldown: ").Append(Cooldown);
            }
            return tooltip.ToString();
        }
    }

    public override string ToString()
    {
        return string.Format("{0}#{1}", Name, Id);
    }

    public class SpellResult
    {
        public SpellResult()
        {
            Success = true;
        }

        public SpellResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Has_Message { get { return !string.IsNullOrEmpty(Message); } }
    }
}
