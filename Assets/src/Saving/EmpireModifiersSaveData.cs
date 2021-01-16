using System;

[Serializable]
public class EmpireModifiersSaveData
{
    public float Unit_Training_Speed_Bonus;
    public float Building_Constuction_Speed_Bonus;
    public float Improvement_Constuction_Speed_Bonus;
    public float Passive_Income;
    public float Max_Mana;
    public float Population_Growth_Bonus;
    public YieldsSaveData Village_Yield_Bonus;
    public YieldsSaveData Percentage_Village_Yield_Bonus;
    public YieldsSaveData Trade_Route_Yield_Bonus;
}

[Serializable]
public class EmpireModifierStatusEffectSaveData
{
    public string Name;
    public int Current_Duration;
    public int Duration;
    public EmpireModifiersSaveData Modifiers;
}
