﻿using System.Collections.Generic;

public class Faction {
    public string Name { get; private set; }
    public int Starting_Cash { get; private set; }
    public float Passive_Income { get; private set; }
    public int Capital_Starting_Population { get; private set; }
    public List<Trainable> Units { get; private set; }
    public List<Building> Buildings { get; private set; }
    public List<Improvement> Improvements { get; private set; }
    public Technology Root_Technology { get; private set; }
    public Army Army_Prototype { get; private set; }
    public Dictionary<City.CitySize, Yields> City_Yields { get; private set; }
    public float Pop_Food_Consumption { get; private set; }
    public int Base_Max_Food_Strorage_Per_City { get; private set; }
    public float Base_Happiness { get; private set; }
    public float Happiness_From_Pops { get; private set; }
    public float Base_Health { get; private set; }
    public float Health_From_Pops { get; private set; }
    public float Base_Order { get; private set; }
    public float Order_From_Pops { get; private set; }
    public float Enemy_Cultural_Influence_Unhappiness_Multiplier { get; set; }

    public Faction(string name, int starting_cash, float base_income, int capital_starting_population, Dictionary<City.CitySize, Yields> city_yields,
        float pop_food_consumption, int max_food_storage_per_city, float base_happiness, float happiness_from_pops,
        float base_health, float health_from_pops, float base_order, float order_from_pops, float enemy_cultural_influence_unhappiness_multiplier,
        Technology root_technology, Army army_prototype)
    {
        Name = name;
        Starting_Cash = starting_cash;
        Passive_Income = base_income;
        Capital_Starting_Population = capital_starting_population;
        Units = new List<Trainable>();
        Buildings = new List<Building>();
        City_Yields = city_yields;
        Pop_Food_Consumption = pop_food_consumption;
        Base_Max_Food_Strorage_Per_City = max_food_storage_per_city;
        Base_Happiness = base_happiness;
        Happiness_From_Pops = happiness_from_pops;
        Base_Health = base_health;
        Health_From_Pops = health_from_pops;
        Base_Order = base_order;
        Order_From_Pops = order_from_pops;
        Enemy_Cultural_Influence_Unhappiness_Multiplier = enemy_cultural_influence_unhappiness_multiplier;
        Improvements = new List<Improvement>();
        Root_Technology = root_technology;
        Army_Prototype = army_prototype;
    }

    public override string ToString()
    {
        return Name;
    }
}
