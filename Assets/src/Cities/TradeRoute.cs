using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TradeRoute {
    private static readonly float FOOD_THRESHOLD = 0.25f;//Target needs to have 25% more food income for trading to start
    private static readonly float FOOD_EFFICIENCY = 0.20f;//20% of food over threshold is traded
    private static readonly float PRODUCTION_THRESHOLD = 0.25f;
    private static readonly float PRODUCTION_EFFICIENCY = 0.15f;
    private static readonly float SCIENCE_THRESHOLD = 0.25f;
    private static readonly float SCIENCE_EFFICIENCY = 0.10f;
    private static readonly float CULTURE_THRESHOLD = 0.25f;
    private static readonly float CULTURE_EFFICIENCY = 0.05f;

    private static readonly int DISTANCE_BONUS_1_THRESHOLD = 10;
    private static readonly float DISTANCE_BONUS_1 = 0.20f;
    private static readonly int DISTANCE_BONUS_2_THRESHOLD = 20;
    private static readonly float DISTANCE_BONUS_2 = 0.30f;

    private static readonly float WATER_BONUS = 0.25f;

    public List<WorldMapHex> Path { get; private set; }
    public City City { get; private set; }
    public TradePartner Target { get; private set; }
    public bool Water_Route { get; private set; }

    public TradeRoute(List<WorldMapHex> path, City city, TradePartner target, bool water_route)
    {
        Path = path;
        City = city;
        Target = target;
        Water_Route = water_route;
    }

    public Yields Yields
    {
        get {
            if (!Active) {
                return new Yields();
            }
            Yields yields = new Yields();
            float value_1 = (Target.Trade_Value * 0.9f) + (City.Trade_Value * 0.1f);
            float value_2 = (City.Trade_Value * 0.9f) + (Target.Trade_Value * 0.1f);
            yields.Cash += Mathf.Min(value_1, value_2);
            if(Target is City) {
                City target_city = Target as City;
                if(target_city.Last_Turn_Yields.Food > City.Last_Turn_Yields.Food * (1.0f + FOOD_THRESHOLD)) {
                    yields.Food += (target_city.Last_Turn_Yields.Food - (City.Last_Turn_Yields.Food * (1.0f + FOOD_THRESHOLD))) * FOOD_EFFICIENCY;
                }
                if (target_city.Last_Turn_Yields.Production > City.Last_Turn_Yields.Production * (1.0f + PRODUCTION_THRESHOLD)) {
                    yields.Production += (target_city.Last_Turn_Yields.Production - (City.Last_Turn_Yields.Production * (1.0f + PRODUCTION_THRESHOLD))) * PRODUCTION_EFFICIENCY;
                }
                if (target_city.Last_Turn_Yields.Science > City.Last_Turn_Yields.Science * (1.0f + SCIENCE_THRESHOLD)) {
                    yields.Science += (target_city.Last_Turn_Yields.Science - (City.Last_Turn_Yields.Science * (1.0f + SCIENCE_THRESHOLD))) * SCIENCE_EFFICIENCY;
                }
                if (target_city.Last_Turn_Yields.Culture > City.Last_Turn_Yields.Culture * (1.0f + CULTURE_THRESHOLD)) {
                    yields.Culture += (target_city.Last_Turn_Yields.Culture - (City.Last_Turn_Yields.Culture * (1.0f + CULTURE_THRESHOLD))) * CULTURE_EFFICIENCY;
                }
            }
            if(Path.Count >= DISTANCE_BONUS_2_THRESHOLD) {
                float multiplier = 100.0f * (1.0f + DISTANCE_BONUS_2);
                yields.Multiply_By_Percentages(new Yields(multiplier, multiplier, multiplier, multiplier, multiplier, multiplier, multiplier));
            } else if(Path.Count >= DISTANCE_BONUS_1_THRESHOLD) {
                float multiplier = 100.0f * (1.0f + DISTANCE_BONUS_1);
                yields.Multiply_By_Percentages(new Yields(multiplier, multiplier, multiplier, multiplier, multiplier, multiplier, multiplier));
            }
            if (Water_Route) {
                float multiplier = 100.0f * (1.0f + WATER_BONUS);
                yields.Multiply_By_Percentages(new Yields(multiplier, multiplier, multiplier, multiplier, multiplier, multiplier, multiplier));
            }
            if(Target is City && !(Target as City).Is_Owned_By(City.Owner) && !Target.Owner.Is_Neutral) {
                if((Target as City).Has_Very_High_Cultural_Influence(City.Owner)) {
                    float enemy_city_multiplier = 50.0f;
                    yields.Multiply_By_Percentages(new Yields(enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier));
                } else {
                    float enemy_city_multiplier = 25.0f;
                    yields.Multiply_By_Percentages(new Yields(enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier, enemy_city_multiplier));
                }
            }
            return yields;
        }
    }

    public bool Active
    {
        get {
            //TODO: Routes to high influence enemy villages?
            return !Hidden && (City.Owner.Id == Target.Owner.Id || Target.Owner.Is_Neutral || (Target is City && (Target as City).Has_High_Cultural_Influence(City.Owner)));
        }
    }

    public bool Hidden
    {
        get {
            return !Target.Hex.Is_Explored_By(City.Owner);
        }
    }

    public string Tooltip
    {
        get {
            StringBuilder builder = new StringBuilder();
            if (Hidden) {
                builder.Append("Undiscovered route");
                return builder.ToString();
            }
            if (!Active) {
                builder.Append("Inactive").Append(Environment.NewLine);
            }
            if (Water_Route) {
                builder.Append("Water route").Append(Environment.NewLine);
            }
            builder.Append("Distance: ").Append(Path.Count).Append(Environment.NewLine);
            builder.Append("Yields: ").Append(Yields.Generate_String(false));
            return builder.ToString();
        }
    }
}
