using System;
using System.Collections.Generic;
using System.Text;

public class Blessing : ICooldown {
    private static int current_id = 0;

    public delegate BlessingResult Blessing_Effect_Activation(Blessing blessing, Player caster);
    public delegate BlessingResult Blessing_Effect_Deactivation(Blessing blessing, Player caster, bool interrupted);
    public delegate BlessingResult Blessing_Effect_Turn_Start(Blessing blessing, Player caster, int turns_left);
    public delegate AI.BlessingPreference Advanced_AI_Guidance_Delegate(Blessing blessing, Player caster, Dictionary<AI.Tag, float> priorities);

    public string Name { get; private set; }
    public int Id { get; private set; }
    public float Faith_Required { get; private set; }
    public int Cooldown { get; private set; }
    public int Duration { get; private set; }
    public Technology Technology_Required { get; private set; }
    public Blessing_Effect_Activation Activation { get; private set; }
    public Blessing_Effect_Deactivation Deactivation { get; private set; }
    public Blessing_Effect_Turn_Start Turn_Start { get; private set; }
    public string Effect_Animation { get; private set; }
    public AIBlessingCastingGuidance AI_Casting_Guidance { get; set; }
    public Advanced_AI_Guidance_Delegate Advanced_AI_Casting_Guidance { get; set; }

    public Blessing(string name, float faith_required, int cooldown, int duration, Technology technology_required, Blessing_Effect_Activation activation, Blessing_Effect_Deactivation deactivation,
        Blessing_Effect_Turn_Start turn_start, string effect_animation = "default_effect")
    {
        Name = name;
        Faith_Required = faith_required;
        Cooldown = cooldown;
        Duration = duration;
        Technology_Required = technology_required;
        Activation = activation;
        Deactivation = deactivation;
        Turn_Start = turn_start;
        Effect_Animation = effect_animation;
        Id = current_id;
        current_id++;
    }

    public BlessingResult Cast(Player caster)
    {
        BlessingResult result = Activation != null ? Activation(this, caster) : Turn_Start(this, caster, Duration);
        if (result.Success) {
            caster.Put_On_Cooldown(this);
            caster.Apply_Blessing(this);
            Play_Animation(result);
            List<Player> notifications_send = new List<Player>();
            foreach(WorldMapHex hex in result.Affected_Hexes) {
                if (hex.Owner != null && !hex.Is_Owned_By(caster) && !notifications_send.Contains(hex.Owner)) {
                    hex.Owner.Queue_Notification(new Notification(string.Format("{0} casted {1} on {2}", caster.Name, Name, hex.City != null ? hex.City.Name : hex.ToString()),
                        hex.Sprite, SpriteManager.SpriteType.Terrain, null, delegate () {
                            CameraManager.Instance.Set_Camera_Location(hex);
                        }
                    ));
                }
            }
        }
        return result;
    }

    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder(Name);
            tooltip.Append(Environment.NewLine).Append("Faith required: ").Append(Helper.Float_To_String(Faith_Required, 1));
            tooltip.Append(Environment.NewLine).Append("Duration: ").Append(Duration);
            tooltip.Append(Environment.NewLine).Append("Cooldown: ").Append(Cooldown);
            return tooltip.ToString();
        }
    }

    public void Play_Animation(BlessingResult result)
    {
        if (string.IsNullOrEmpty(Effect_Animation)) {
            return;
        }
        foreach (WorldMapHex hex in result.Affected_Hexes) {
            if (hex.Current_LoS != WorldMapHex.LoS_Status.Visible && Main.Instance.Other_Players_Turn) {
                continue;
            }
            EffectManager.Instance.Play_Effect(hex, Effect_Animation);
        }
    }

    public class BlessingResult
    {
        public BlessingResult()
        {
            Success = true;
            Affected_Hexes = new List<WorldMapHex>();
        }

        public BlessingResult(bool success, string message, List<WorldMapHex> affected_hexes)
        {
            Success = success;
            Message = message;
            Affected_Hexes = affected_hexes == null ? new List<WorldMapHex>() : affected_hexes;
        }

        public BlessingResult(bool success, string message, WorldMapHex affected_hex)
        {
            Success = success;
            Message = message;
            Affected_Hexes = affected_hex == null ? new List<WorldMapHex>() : new List<WorldMapHex>() { affected_hex };
        }

        public bool Success { get; set; }
        public string Message { get; set; }
        public bool Has_Message { get { return !string.IsNullOrEmpty(Message); } }
        public List<WorldMapHex> Affected_Hexes { get; set; }
    }

    public class AIBlessingCastingGuidance
    {
        public enum TargetType { Caster, Enemy_Players, All_Players }
        public enum RequiredVision { Enemy_Cities }

        public Dictionary<AI.Tag, float> Effect_Priorities { get; private set; }
        public TargetType Target { get; private set; }
        public RequiredVision? Required_Vision { get; private set; }

        public AIBlessingCastingGuidance(TargetType target, RequiredVision? required_vision, Dictionary<AI.Tag, float> effect_priorities)
        {
            Target = target;
            Required_Vision = required_vision;
            Effect_Priorities = effect_priorities;
        }
    }

    public override string ToString()
    {
        return string.Format("{0} #{1}", Name, Id);
    }
}
