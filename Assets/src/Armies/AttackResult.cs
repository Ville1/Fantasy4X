using System.Collections.Generic;
using System.Text;

public class AttackResult {
    public Unit Target { get; set; }
    public float Manpower_Delta { get; set; }
    public float Damage_Effectiveness { get; set; }
    public float Morale_Delta { get; set; }
    public float Stamina_Delta { get; set; }
    public bool? Can_Attack { get; set; }
    public float? Movement { get; set; }
    public float Final_Attack { get; set; }
    public float Base_Attack { get; set; }
    public float Final_Defence { get; set; }
    public float Base_Defence { get; set; }
    public List<Detail> Details { get; set; }

    public AttackResult()
    {
        Details = new List<Detail>();
    }

    public float Attack_Effectiveness
    {
        get {
            return Final_Attack / Base_Attack;
        }
    }

    public float Defence_Effectiveness
    {
        get {
            return Final_Defence / Base_Defence;
        }
    }

    public void Add_Detail(Detail detail)
    {
        if(detail.Attack_Delta == 0.0f && detail.Attack_Multiplier == 1.0f && detail.Defence_Delta == 0.0f && detail.Defence_Multiplier == 1.0f) {
            return;
        }
        Details.Add(detail);
    }

    public class Detail
    {
        public float Attack_Delta { get; set; }
        public float Attack_Multiplier { get; set; }
        public float Defence_Delta { get; set; }
        public float Defence_Multiplier { get; set; }
        public string Description { get; set; }

        public Detail()
        {
            Attack_Delta = 0.0f;
            Attack_Multiplier = 0.0f;
            Defence_Delta = 0.0f;
            Defence_Multiplier = 0.0f;
            Description = null;
        }

        public bool Has_Attack_Data
        {
            get {
                return Attack_Delta != 0.0f || Attack_Multiplier != 0.0f;
            }
        }

        public bool Has_Defence_Data
        {
            get {
                return Defence_Delta != 0.0f || Defence_Multiplier != 0.0f;
            }
        }

        public void Add(Detail detail)
        {
            Attack_Delta += detail.Attack_Delta;
            Attack_Multiplier += detail.Attack_Multiplier;
            Defence_Delta += detail.Defence_Delta;
            Defence_Multiplier += detail.Defence_Multiplier;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(Description);
            builder.Append(": ");
            if(Attack_Delta != 0.0f || Attack_Multiplier != 0.0f) {
                builder.Append("Att ");
                if(Attack_Delta != 0.0f) {
                    builder.Append(Helper.Float_To_String(Attack_Delta, 1, true));
                    if(Attack_Multiplier != 0.0f) {
                        builder.Append(" ");
                    }
                }
                if(Attack_Multiplier != 0.0f) {
                    builder.Append(Helper.Float_To_String(Attack_Multiplier * 100.0f, 0, true)).Append("%");
                }
            }
            if (Defence_Delta != 0.0f || Defence_Multiplier != 0.0f) {
                if(Attack_Delta != 0.0f || Attack_Multiplier != 0.0f) {
                    builder.Append(", ");
                }
                builder.Append("Def ");
                if (Defence_Delta != 0.0f) {
                    builder.Append(Helper.Float_To_String(Defence_Delta, 1, true));
                    if (Defence_Multiplier != 0.0f) {
                        builder.Append(" ");
                    }
                }
                if (Defence_Multiplier != 0.0f) {
                    builder.Append(Helper.Float_To_String(Defence_Multiplier * 100.0f, 0, true)).Append("%");
                }
            }
            return builder.ToString();
        }

        public Detail Clone()
        {
            Detail clone = new Detail();
            clone.Attack_Delta = Attack_Delta;
            clone.Attack_Multiplier = Attack_Multiplier;
            clone.Defence_Delta = Defence_Delta;
            clone.Defence_Multiplier = Defence_Multiplier;
            clone.Description = Description;
            return clone;
        }
    }
}
