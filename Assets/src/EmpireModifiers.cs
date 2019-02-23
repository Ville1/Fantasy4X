using System;
using System.Text;
using UnityEngine;

public class EmpireModifiers {
    public float Unit_Training_Speed_Bonus { get; set; }
    public float Building_Constuction_Speed_Bonus { get; set; }
    public float Improvement_Constuction_Speed_Bonus { get; set; }
    public float Passive_Income { get; set; }

    public EmpireModifiers()
    {
        Unit_Training_Speed_Bonus = 0.0f;
        Building_Constuction_Speed_Bonus = 0.0f;
        Improvement_Constuction_Speed_Bonus = 0.0f;
        Passive_Income = 0.0f;
    }

    public void Add(EmpireModifiers modifiers)
    {
        Unit_Training_Speed_Bonus += modifiers.Unit_Training_Speed_Bonus;
        Building_Constuction_Speed_Bonus += modifiers.Building_Constuction_Speed_Bonus;
        Improvement_Constuction_Speed_Bonus += modifiers.Improvement_Constuction_Speed_Bonus;
        Passive_Income += modifiers.Passive_Income;
    }

    public bool Empty
    {
        get {
            return Unit_Training_Speed_Bonus == 0.0f && Building_Constuction_Speed_Bonus == 0.0f && Improvement_Constuction_Speed_Bonus == 0.0f && Passive_Income == 0.0f;
        }
    }

    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder();
            tooltip.Append("Empire Modifiers");
            if(Unit_Training_Speed_Bonus != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Unit training speed bonus: ").Append(Mathf.RoundToInt(100.0f * Unit_Training_Speed_Bonus)).Append("%");
            }
            if (Building_Constuction_Speed_Bonus != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Building construction speed bonus: ").Append(Mathf.RoundToInt(100.0f * Building_Constuction_Speed_Bonus)).Append("%");
            }
            if (Improvement_Constuction_Speed_Bonus != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Improvement construction speed bonus: ").Append(Mathf.RoundToInt(100.0f * Improvement_Constuction_Speed_Bonus)).Append("%");
            }
            if (Passive_Income != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Base income: +").Append(Math.Round(Passive_Income, 1).ToString("0.0"));
            }
            return tooltip.ToString();
        }
    }
}
