using System;
using System.Text;
using UnityEngine;

public class EmpireModifiers {
    public float Unit_Training_Speed_Bonus { get; set; }
    public float Building_Constuction_Speed_Bonus { get; set; }
    public float Improvement_Constuction_Speed_Bonus { get; set; }
    public float Passive_Income { get; set; }
    public Yields Village_Yield_Bonus { get; set; }
    public Yields Percentage_Village_Yield_Bonus { get; set; }
    public Yields Trade_Route_Yield_Bonus { get; set; }

    public EmpireModifiers()
    {
        Unit_Training_Speed_Bonus = 0.0f;
        Building_Constuction_Speed_Bonus = 0.0f;
        Improvement_Constuction_Speed_Bonus = 0.0f;
        Passive_Income = 0.0f;
        Village_Yield_Bonus = new Yields();
        Percentage_Village_Yield_Bonus = new Yields();
        Trade_Route_Yield_Bonus = new Yields();
    }

    public void Add(EmpireModifiers modifiers)
    {
        Unit_Training_Speed_Bonus += modifiers.Unit_Training_Speed_Bonus;
        Building_Constuction_Speed_Bonus += modifiers.Building_Constuction_Speed_Bonus;
        Improvement_Constuction_Speed_Bonus += modifiers.Improvement_Constuction_Speed_Bonus;
        Passive_Income += modifiers.Passive_Income;
        Village_Yield_Bonus.Add(modifiers.Village_Yield_Bonus);
        Percentage_Village_Yield_Bonus.Add(modifiers.Percentage_Village_Yield_Bonus);
        Trade_Route_Yield_Bonus.Add(modifiers.Trade_Route_Yield_Bonus);
    }

    public bool Empty
    {
        get {
            return Unit_Training_Speed_Bonus == 0.0f && Building_Constuction_Speed_Bonus == 0.0f && Improvement_Constuction_Speed_Bonus == 0.0f && Passive_Income == 0.0f
                && Village_Yield_Bonus.Empty && Percentage_Village_Yield_Bonus.Empty;
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
            if (!Village_Yield_Bonus.Empty) {
                tooltip.Append(Environment.NewLine).Append("Village yields: ").Append(Village_Yield_Bonus);
            }
            if (!Percentage_Village_Yield_Bonus.Empty) {
                tooltip.Append(Environment.NewLine).Append("Village yield bonuses: ").Append(Percentage_Village_Yield_Bonus.Generate_String(true));
            }
            if (!Trade_Route_Yield_Bonus.Empty) {
                tooltip.Append(Environment.NewLine).Append("Trade route yield bonuses: ").Append(Trade_Route_Yield_Bonus.Generate_String(true));
            }
            return tooltip.ToString();
        }
    }
}
