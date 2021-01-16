using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Building {
    public static float Paused_Upkeep = 0.5f;

    public delegate void Turn_End_Delegate(Building building);

    public string Name { get; private set; }
    public string Texture { get; private set; }
    public int Production_Required { get; private set; }
    public int Cost { get; private set; }
    public float Happiness { get; private set; }
    public float Health { get; private set; }
    public float Order { get; private set; }
    public Technology Technology_Required { get; private set; }
    public City City { get; private set; }
    public Yields Base_Yields { get; private set; }
    public Yields Yield_Delta { get; set; }
    public bool Can_Be_Paused { get; private set; }
    public Dictionary<AI.Tag, float> Tags { get; set; }
    public int Food_Storage { get; set; }
    public Turn_End_Delegate On_Turn_End { get; private set; }
    public float Unit_Training_Speed_Bonus { get; private set; }
    public float Building_Constuction_Speed_Bonus { get; private set; }
    public float Improvement_Constuction_Speed_Bonus { get; private set; }
    public float Base_Happiness_From_Pops_Delta { get; set; }
    public float Base_Health_From_Pops_Delta { get; set; }
    public float Base_Order_From_Pops_Delta { get; set; }
    public float Pop_Growth_Multiplier_Bonus { get; set; }
    public float Pop_Growth_Additive_Bonus { get; set; }
    public float Building_Upkeep_Reduction { get; set; }
    public float Garrison_Upkeep_Reduction { get; set; }
    public float Cultural_Influence_Range { get; set; }
    public float Village_Cultural_Influence { get; set; }
    public float Trade_Value { get; set; }
    public float Max_Mana { get; set; }
    public float Health_Penalty_From_Buildings_Multiplier { get; set; }
    public int Update_Priority { get; set; }

    private float upkeep;
    private bool paused;
    private Yields percentage_yield_bonuses;
    
    public Building(Building prototype, City city)
    {
        Name = prototype.Name;
        Texture = prototype.Texture;
        Production_Required = prototype.Production_Required;
        Cost = prototype.Cost;
        upkeep = prototype.upkeep;
        Base_Yields = new Yields(prototype.Base_Yields);
        Percentage_Yield_Bonuses = new Yields(prototype.Percentage_Yield_Bonuses);
        Happiness = prototype.Happiness;
        Health = prototype.Health;
        Order = prototype.Order;
        Food_Storage = prototype.Food_Storage;
        Unit_Training_Speed_Bonus = prototype.Unit_Training_Speed_Bonus;
        Building_Constuction_Speed_Bonus = prototype.Building_Constuction_Speed_Bonus;
        Improvement_Constuction_Speed_Bonus = prototype.Improvement_Constuction_Speed_Bonus;
        Can_Be_Paused = prototype.Can_Be_Paused;
        Technology_Required = prototype.Technology_Required;
        City = city;
        On_Turn_End = prototype.On_Turn_End;
        Tags = new Dictionary<AI.Tag, float>();
        foreach(KeyValuePair<AI.Tag, float> pair in prototype.Tags) {
            Tags.Add(pair.Key, pair.Value);
        }
        Base_Happiness_From_Pops_Delta = prototype.Base_Happiness_From_Pops_Delta;
        Base_Health_From_Pops_Delta = prototype.Base_Health_From_Pops_Delta;
        Base_Order_From_Pops_Delta = prototype.Base_Order_From_Pops_Delta;
        Pop_Growth_Multiplier_Bonus = prototype.Pop_Growth_Multiplier_Bonus;
        Pop_Growth_Additive_Bonus = prototype.Pop_Growth_Additive_Bonus;
        Building_Upkeep_Reduction = prototype.Building_Upkeep_Reduction;
        Garrison_Upkeep_Reduction = prototype.Garrison_Upkeep_Reduction;
        Cultural_Influence_Range = prototype.Cultural_Influence_Range;
        Village_Cultural_Influence = prototype.Village_Cultural_Influence;
        Trade_Value = prototype.Trade_Value;
        Max_Mana = prototype.Max_Mana;
        Health_Penalty_From_Buildings_Multiplier = prototype.Health_Penalty_From_Buildings_Multiplier;
        Update_Priority = prototype.Update_Priority;
    }

    public Building(string name, string texture, int production_required, int cost, float upkeep, Yields yields, float happiness, float health, float order,
        float unit_training_speed_bonus, float building_constuction_speed_bonus, float improvement_constuction_speed_bonus,
        bool can_be_paused, Technology technology_required, Turn_End_Delegate on_turn_end)
    {
        Name = name;
        Texture = texture;
        Production_Required = production_required;
        Cost = cost;
        this.upkeep = upkeep;
        Base_Yields = yields;
        Happiness = happiness;
        Health = health;
        Order = order;
        Unit_Training_Speed_Bonus = unit_training_speed_bonus;
        Building_Constuction_Speed_Bonus = building_constuction_speed_bonus;
        Improvement_Constuction_Speed_Bonus = improvement_constuction_speed_bonus;
        Can_Be_Paused = can_be_paused;
        Technology_Required = technology_required;
        On_Turn_End = on_turn_end;
        Percentage_Yield_Bonuses = new Yields();
        Tags = new Dictionary<AI.Tag, float>();
        Base_Happiness_From_Pops_Delta = 0.0f;
        Base_Health_From_Pops_Delta = 0.0f;
        Base_Order_From_Pops_Delta = 0.0f;
        Pop_Growth_Multiplier_Bonus = 0.0f;
        Pop_Growth_Additive_Bonus = 0.0f;
        Building_Upkeep_Reduction = 0.0f;
        Garrison_Upkeep_Reduction = 0.0f;
        Cultural_Influence_Range = 0.0f;
        Village_Cultural_Influence = 0.0f;
        Trade_Value = 0.0f;
        Health_Penalty_From_Buildings_Multiplier = 0.0f;
        Update_Priority = 0;
    }

    public Yields Yields
    {
        get {
            if (paused) {
                return new Yields();
            }
            if(Yield_Delta != null) {
                Yields y = new Yields(Base_Yields);
                y.Add(Yield_Delta);
                return y;
            }
            return Base_Yields;
        }
    }


    public Yields Percentage_Yield_Bonuses
    {
        get {
            if (paused) {
                return new Yields();
            }
            return percentage_yield_bonuses;
        }
        set {
            percentage_yield_bonuses = value;
        }
    }

    public float Upkeep
    {
        get {
            return Paused ? upkeep * Paused_Upkeep : upkeep;
        }
    }


    public bool Paused
    {
        get {
            return paused;
        }
        set {
            if(paused == value) {
                return;
            }
            City.Yields_Changed();
            paused = value;
        }
    }

    public override string ToString()
    {
        return Name;
    }

    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder();
            tooltip.Append(Name);
            if (Paused) {
                tooltip.Append(" (Paused)");
            }
            if(Upkeep != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Upkeep: ").Append(Math.Round(Upkeep, 2).ToString("0.00"));
            }
            if(!Yields.Empty || !Base_Yields.Empty || (Yield_Delta != null && !Yield_Delta.Empty)) {
                tooltip.Append(Environment.NewLine).Append("Yields: ").Append(Yields);
                if (!Yields.Equals(Base_Yields)) {
                    tooltip.Append(Environment.NewLine).Append("- Base: ").Append(Base_Yields.ToString());
                    tooltip.Append(Environment.NewLine).Append("- Delta: ").Append(Yield_Delta.ToString());
                }
            }
            if (!percentage_yield_bonuses.Empty) {
                tooltip.Append(Environment.NewLine).Append("Yield bonuses: ").Append(Percentage_Yield_Bonuses.Generate_String(true));
                if (!Percentage_Yield_Bonuses.Equals(percentage_yield_bonuses)) {
                    tooltip.Append(Environment.NewLine).Append("- Base: ").Append(percentage_yield_bonuses.Generate_String(true));
                }
            }
            if (Happiness != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Happiness: ").Append(Math.Round(Happiness, 1).ToString("0.0"));
            }
            if (Base_Happiness_From_Pops_Delta != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Happiness penalty from population: -").Append(Math.Round(Base_Happiness_From_Pops_Delta, 2).ToString("0.00"));
            }
            if (Health != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Health: ").Append(Math.Round(Health, 1).ToString("0.0"));
            }
            if (Base_Health_From_Pops_Delta != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Health penalty from population: -").Append(Math.Round(Base_Health_From_Pops_Delta, 2).ToString("0.00"));
            }
            if (Order != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Order: ").Append(Math.Round(Order, 1).ToString("0.0"));
            }
            if (Base_Order_From_Pops_Delta != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Order penalty from population: -").Append(Math.Round(Base_Order_From_Pops_Delta, 2).ToString("0.00"));
            }
            if (Unit_Training_Speed_Bonus != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Unit training speed bonus: ").Append(Mathf.RoundToInt(100.0f * Unit_Training_Speed_Bonus)).Append("%");
            }
            if (Building_Constuction_Speed_Bonus != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Building construction speed bonus: ").Append(Mathf.RoundToInt(100.0f * Building_Constuction_Speed_Bonus)).Append("%");
            }
            if (Improvement_Constuction_Speed_Bonus != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Improvement construction speed bonus: ").Append(Mathf.RoundToInt(100.0f * Improvement_Constuction_Speed_Bonus)).Append("%");
            }
            if (Food_Storage != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Food storage: ").Append(Food_Storage);
            }
            if (Pop_Growth_Additive_Bonus != 0.0f || Pop_Growth_Multiplier_Bonus != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Population growth: ");
                if(Pop_Growth_Additive_Bonus != 0.0f) {
                    tooltip.Append("+").Append(Math.Round(Pop_Growth_Additive_Bonus, 1).ToString("0.0"));
                }
                if (Pop_Growth_Multiplier_Bonus != 0.0f) {
                    if (Pop_Growth_Additive_Bonus != 0.0f) {
                        tooltip.Append(" ");
                    }
                    tooltip.Append("+").Append(Mathf.RoundToInt(Pop_Growth_Multiplier_Bonus * 100.0f)).Append("%");
                }
            }
            if (Building_Upkeep_Reduction != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Building upkeep reduction: ").Append(Mathf.RoundToInt(100.0f * Building_Upkeep_Reduction)).Append("%");
            }
            if (Garrison_Upkeep_Reduction != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Garrison upkeep reduction: ").Append(Mathf.RoundToInt(100.0f * Garrison_Upkeep_Reduction)).Append("%");
            }
            if (Cultural_Influence_Range != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Cultural influence range: ").Append(Math.Round(Cultural_Influence_Range, 1).ToString("0.0"));
            }
            if (Village_Cultural_Influence != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Village cultural influence: ").Append(Math.Round(Village_Cultural_Influence, 1).ToString("0.0"));
            }
            if (Trade_Value != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Trade value: ").Append(Math.Round(Trade_Value, 1).ToString("0.0"));
            }
            if (Health_Penalty_From_Buildings_Multiplier != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Health penalty from buildings: ").Append(Mathf.RoundToInt(100.0f * Health_Penalty_From_Buildings_Multiplier)).Append("%");
            }
            return tooltip.ToString();
        }
    }

    public void Load(BuildingSaveData data)
    {
        paused = data.Is_Paused;
        Yield_Delta = data.Yield_Delta == null ? null : new Yields(data.Yield_Delta);
    }

    public BuildingSaveData Save_Data
    {
        get {
            return new BuildingSaveData() {
                Name = Name,
                Is_Paused = paused,
                Yield_Delta = Yield_Delta == null ? null : Yield_Delta.Save_Data
            };
        }
    }
}
