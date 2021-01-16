using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class City : Ownable, Influencable, TradePartner
{
    public static readonly float LOW_HAPPINESS_WARNING_THRESHOLD = -1.0f;
    public static readonly string LOW_HAPPINESS_TEXTURE = "low_happiness";
    public static readonly float VERY_LOW_HAPPINESS_WARNING_THRESHOLD = -10.0f;
    public static readonly string VERY_LOW_HAPPINESS_TEXTURE = "very_low_happiness";
    public static readonly float LOW_HEALTH_WARNING_THRESHOLD = -1.0f;
    public static readonly string LOW_HEALTH_TEXTURE = "low_health";
    public static readonly float VERY_LOW_HEALTH_WARNING_THRESHOLD = -10.0f;
    public static readonly string VERY_LOW_HEALTH_TEXTURE = "very_low_health";
    public static readonly float LOW_ORDER_WARNING_THRESHOLD = -1.0f;
    public static readonly string LOW_ORDER_TEXTURE = "low_order";
    public static readonly float VERY_LOW_ORDER_WARNING_THRESHOLD = -10.0f;
    public static readonly string VERY_LOW_ORDER_TEXTURE = "very_low_order";
    
    private static readonly float REFOUND_AMOUNT = 0.50f;
    private static readonly float STARTING_FOOD = 0.50f;
    private static readonly string BUILDING_READY_SOUND_EFFECT = "building_ready_sfx";
    private static readonly float STARTING_CULTURE = 50.0f;
    private static readonly float DEFAULT_CULTURAL_INFLUENCE_RANGE = 10.0f;
    private static readonly float CULTURAL_INFLUENCE_DECAY_PER_HEX = 0.025f;
    private static readonly float ENEMY_CULTURAL_INFLUENCE_AMP = 3.0f;
    private static readonly float ENEMY_CULTURAL_INFLUENCE_HAPPINESS_BREAK_POINT_1 = 0.25f;
    private static readonly float ENEMY_CULTURAL_INFLUENCE_HAPPINESS_BREAK_POINT_2 = 0.50f;
    private static readonly float ENEMY_CULTURAL_INFLUENCE_LOS_BREAK_POINT_1 = 0.33f;
    private static readonly float ENEMY_CULTURAL_INFLUENCE_LOS_BREAK_POINT_2 = 0.67f;


    private static int current_id = 0;

    public enum CitySize { Town, City, Metropolis }

    public WorldMapHex Hex { get; private set; }
    public List<WorldMapHex> Worked_Hexes { get; private set; }
    public List<WorldMapHex> Hexes_In_Work_Range { get; private set; }
    public int LoS { get; private set; }
    public int Work_Range { get; private set; }
    public Trainable Unit_Under_Production { get; private set; }
    public Building Building_Under_Production { get; private set; }
    public float Unit_Production_Acquired { get; set; }
    public float Building_Production_Acquired { get; set; }
    public int Population { get; private set; }
    public List<Building> Buildings { get; private set; }
    public CityStatistics Statistics { get; private set; }
    public int Base_Max_Food_Storage { get; private set; }
    public float Food_Stored { get; private set; }
    public int Id { get; private set; }
    public bool Full_LoS { get; private set; }
    public string Name { get; set; }
    public Flag Flag { get; set; }
    public Dictionary<Player, float> Cultural_Influence { get; private set; }
    public List<TradeRoute> Trade_Routes { get; private set; }
    public bool Is_Coastal { get; private set; }
    public Yields Last_Turn_Yields { get; private set; }
    public Army Garrison { get { return Hex.Entity != null && Hex.Entity is Army ? Hex.Entity as Army : null; } }
    public StatusEffectList<CityStatusEffect> Status_Effects { get; private set; }

    private bool update_yields;
    private Yields saved_base_yields;//TODO: what was this supposed to do?
    private Yields saved_yields;

    public City(WorldMapHex hex, Player owner, WorldMapHex water_hex_for_channel)
    {
        Hex = hex;
        Hex.City = this;
        Owner = owner;
        LoS = 3;
        Work_Range = 3;
        Population = owner.Faction.Capital_Starting_Population;
        Base_Max_Food_Storage = owner.Faction.Base_Max_Food_Strorage_Per_City;
        Worked_Hexes = new List<WorldMapHex>();
        Hexes_In_Work_Range = new List<WorldMapHex>();
        Buildings = new List<Building>();
        Update_Hex_Yields();
        Statistics = new CityStatistics();
        update_yields = false;
        saved_yields = null;
        Full_LoS = true;
        Flag = new Flag(hex);
        Trade_Routes = new List<TradeRoute>();
        Last_Turn_Yields = new Yields();
        Status_Effects = new StatusEffectList<CityStatusEffect>();

        Cultural_Influence = new Dictionary<Player, float>();
        Cultural_Influence.Add(owner, STARTING_CULTURE);
        
        Food_Stored = STARTING_FOOD * Max_Food_Stored;
        foreach(WorldMapHex range_hex in Hex.Get_Hexes_Around(Work_Range)) {
            Hexes_In_Work_Range.Add(range_hex);
            range_hex.In_Work_Range_Of.Add(this);
        }

        //Check if is coastal
        Is_Coastal = false;
        foreach(WorldMapHex adjacent_hex in Hex.Get_Adjancent_Hexes()) {
            if (adjacent_hex.Is_Water) {
                Is_Coastal = true;
                break;
            }
        }
        //Check if can be mabe coastal
        if (!Is_Coastal && water_hex_for_channel != null) {
            foreach (WorldMapHex adjacent_hex in Hex.Get_Adjancent_Hexes()) {
                foreach(WorldMapHex adjacent_hex_2 in adjacent_hex.Get_Adjancent_Hexes()) {
                    if (adjacent_hex_2.Is_Water) {
                        adjacent_hex.Change_To(water_hex_for_channel);
                        Is_Coastal = true;
                        break;
                    }
                }
            }
        }
        
        foreach(WorldMapHex hex_in_los in Get_Hexes_In_LoS()) {
            hex_in_los.Set_Explored(Owner);
        }
        Auto_Apply_Unemployed_Pops();

        Id = current_id;
        current_id++;
        Name = NameManager.Instance.Get_Name(NameManager.NameType.City, owner.Faction, false);
    }

    public void Start_Game()
    {
        update_yields = true;
        Last_Turn_Yields = Yields;
        if (Owner.Faction.Reveal_Trade_Routes) {
            foreach(TradeRoute route in Trade_Routes) {
                route.Target.Hex.Set_Explored(Owner);
            }
        }
    }

    public List<WorldMapHex> Get_Hexes_In_LoS()
    {
        List<WorldMapHex> los = new List<WorldMapHex>();
        if (Full_LoS) {
            los.Add(Hex);
            foreach (WorldMapHex hex in Hexes_In_Work_Range) {
                los.Add(hex);
            }
        } else {
            los = Hex.Get_Hexes_In_LoS(LoS);
        }

        foreach(WorldMapHex worked_hex in Worked_Hexes) {
            foreach(WorldMapHex improvement_los_hex in worked_hex.Improvement.Get_Hexes_In_LoS()) {
                if (!los.Contains(improvement_los_hex)) {
                    los.Add(improvement_los_hex);
                }
            }
        }

        //TODO: Move this to player, so it is not run reduntantly?
        foreach(KeyValuePair<Influencable, float> influence_data in Influenced_Cities_And_Villages) {
            if (!influence_data.Key.Cultural_Influence.ContainsKey(Owner)) {
                continue;
            }
            float owned_influence = influence_data.Key.Cultural_Influence[Owner];
            float total_influence = 0.0f;
            foreach(KeyValuePair<Player, float> influence_data_2 in influence_data.Key.Cultural_Influence) {
                total_influence += influence_data_2.Value;
            }
            if(total_influence == 0.0f) {
                continue;
            }
            if(owned_influence / total_influence >= ENEMY_CULTURAL_INFLUENCE_LOS_BREAK_POINT_1) {
                los.Add(influence_data.Key.Hex);
            }
            if (owned_influence / total_influence >= ENEMY_CULTURAL_INFLUENCE_LOS_BREAK_POINT_2) {
                foreach(WorldMapHex h in influence_data.Key.Hex.Get_Hexes_Around(1)) {
                    los.Add(h);
                }
            }
        }

        return los;
    }

    public bool Has_High_Cultural_Influence(Player player)
    {
        return Has_Specified_Cultural_Influence_Level(player, ENEMY_CULTURAL_INFLUENCE_LOS_BREAK_POINT_1);
    }

    public bool Has_Very_High_Cultural_Influence(Player player)
    {
        return Has_Specified_Cultural_Influence_Level(player, ENEMY_CULTURAL_INFLUENCE_LOS_BREAK_POINT_2);
    }

    private bool Has_Specified_Cultural_Influence_Level(Player player, float influence)
    {
        if (!Cultural_Influence.ContainsKey(player)) {
            return false;
        }
        float total_influence = 0.0f;
        foreach (KeyValuePair<Player, float> influence_data in Cultural_Influence) {
            total_influence += influence_data.Value;
        }
        return Cultural_Influence[player] / total_influence >= influence;
    }

    public void Change_Production(Trainable prototype, bool refound = true)
    {
        if (refound) {
            Owner.Cash += Unit_Refound();
        }
        Unit_Under_Production = prototype;
        if(prototype != null) {
            Owner.Cash -= prototype.Cost;
        }
        Unit_Production_Acquired = 0.0f;
    }

    public int Unit_Under_Production_Turns_Left
    {
        get {
            if (Yields.Production == 0) {
                return -1;
            }
            float multiplier = Unit_Under_Production is Unit ? 1.0f + Total_Unit_Training_Speed_Bonus : 1.0f;
            return Mathf.CeilToInt((Unit_Under_Production.Production_Required - Unit_Production_Acquired) / (Yields.Production * multiplier));
        }
    }

    public int Production_Time_Estimate(Trainable unit)
    {
        if (Yields.Production == 0) {
            return -1;
        }
        float multiplier = unit is Unit ? 1.0f + Total_Unit_Training_Speed_Bonus : 1.0f;
        return Mathf.CeilToInt(unit.Production_Required / (Yields.Production * multiplier));
    }

    public void Change_Production(Building prototype, bool refound = true)
    {
        if (refound) {
            Owner.Cash += Building_Refound();
        }
        Building_Under_Production = prototype;
        if(prototype != null) {
            Owner.Cash -= prototype.Cost;
        }
        Building_Production_Acquired = 0.0f;
    }

    public int Building_Under_Production_Turns_Left
    {
        get {
            if (Yields.Production == 0) {
                return -1;
            }
            return Mathf.CeilToInt((Building_Under_Production.Production_Required - Building_Production_Acquired) / (Yields.Production * (1.0f + Total_Building_Construction_Speed_Bonus)));
        }
    }

    public int Production_Time_Estimate(Building building)
    {
        if (Yields.Production == 0) {
            return -1;
        }
        return Mathf.CeilToInt(building.Production_Required / (Yields.Production * (1.0f + Total_Building_Construction_Speed_Bonus)));
    }

    public bool Can_Train(Trainable unit)
    {
        return Owner.Cash >= unit.Cost - Unit_Refound() && Owner.Has_Unlocked(unit) && (!unit.Requires_Coast || Is_Coastal);
    }

    public bool Can_Build(Building building)
    {
        return Owner.Cash >= building.Cost - Building_Refound() && Owner.Has_Unlocked(building);
    }

    public float Unit_Refound()
    {
        if(Unit_Under_Production == null) {
            return 0.0f;
        }
        return Unit_Under_Production.Cost * (1.0f - (Unit_Production_Acquired / (float)Unit_Under_Production.Production_Required));
    }

    public float Building_Refound()
    {
        if (Building_Under_Production == null) {
            return 0.0f;
        }
        return Building_Under_Production.Cost * (1.0f - (Building_Production_Acquired / (float)Building_Under_Production.Production_Required));
    }

    public Yields Get_Base_Yields(bool update_statistics)
    {
        //Hexes
        Yields yields = new Yields(Hex.Yields);
        if (update_statistics) {
            Statistics.Clear_Yields();
            Statistics.Add("Hexes", Hex.Yields);
        }
        foreach(WorldMapHex hex in Worked_Hexes) {
            yields.Add(hex.Yields);
            if (update_statistics) {
                Statistics.Add("Hexes", hex.Yields);
            }
        }

        //Villages
        foreach(Village village in Owner.Villages) {
            Yields village_yields = new Yields(village.Yields);
            village_yields.Add(Owner.EmpireModifiers.Village_Yield_Bonus);
            village_yields.Multiply_By_Percentages(Owner.EmpireModifiers.Percentage_Village_Yield_Bonus);
            village_yields.Divide_By_Number(Owner.Cities.Count);
            if (update_statistics) {
                Statistics.Add("Villages", village_yields);
            }
            yields.Add(village_yields);
        }

        //Trade routes
        foreach(TradeRoute route in Trade_Routes) {
            if(route.City.Id != Id) {
                CustomLogger.Instance.Error(string.Format("City #{0} has trade route belonging to city #{1}", Id, route.City.Id));
                continue;
            }
            if (!route.Active) {
                continue;
            }
            Yields route_yields = new Yields(route.Yields);
            route_yields.Multiply_By_Percentages(Owner.EmpireModifiers.Trade_Route_Yield_Bonus);
            if (update_statistics) {
                Statistics.Add("Trade routes", route_yields);
            }
            yields.Add(route_yields);
        }

        //Units
        if(Garrison != null) {
            Yields garrison_yields = Garrison.Get_City_Effects(this).Yields;
            if (update_statistics) {
                Statistics.Add("Units", garrison_yields);
            }
            yields.Add(garrison_yields);
        }

        Yields percentage_bonuses = new Yields(100, 100, 100, 100, 100, 100, 100);

        //Status effects
        foreach (CityStatusEffect status_effect in Status_Effects) {
            Yields status_effect_yields = new Yields(status_effect.Yield_Delta);
            Yields status_effect_precentage_yields = new Yields(status_effect.Percentage_Yield_Delta);
            if (update_statistics) {
                Statistics.Add(string.Format("{0} ({1} turn{2})", status_effect.Name, status_effect.UI_Current_Duration,
                    status_effect.UI_Current_Duration != 1 ? "s" : string.Empty), status_effect_yields);
                Statistics.Add_Precentage(string.Format("{0} ({1} turn{2})", status_effect.Name, status_effect.UI_Current_Duration,
                    status_effect.UI_Current_Duration != 1 ? "s" : string.Empty), status_effect_precentage_yields);
            }
            yields.Add(status_effect_yields);
            percentage_bonuses.Add(status_effect_precentage_yields);
        }

        //Buildings
        foreach (Building building in Buildings) {
            yields.Add(building.Yields);
            percentage_bonuses.Add(building.Percentage_Yield_Bonuses);
            if (update_statistics) {
                Statistics.Add_Precentage("Buildings", building.Percentage_Yield_Bonuses);
                Statistics.Add("Buildings", building.Yields);
            }
        }
        yields.Multiply_By_Percentages(percentage_bonuses);
        return yields;
    }

    /// <summary>
    /// </summary>
    public Yields Yields
    {
        get {
            if(saved_yields != null && !update_yields) {
                return new Yields(saved_yields);
            }

            Yields yields = new Yields(Get_Base_Yields(true));

            //MULTIPLIERS FIRST
            //Happiness
            float happiness_delta = Happiness >= 0.0f ? Helper.Break_Point_Bonus(Happiness, 10.0f, 0.25f, 0.01f) : -Helper.Break_Point_Bonus(-Happiness, 10.0f, 25.0f, 0.50f, 0.75f);
            float lesser_happiness_delta = happiness_delta / 2.0f;
            //yields.Food = rounding removed 1.0f + lesser_happiness_delta) * yields.Food;
            //Statistics.Food_Percent.Add("Happiness", lesser_happiness_delta * 100.0f);
            yields.Production *= (1.0f + lesser_happiness_delta);
            Statistics.Production_Percent.Add("Happiness", lesser_happiness_delta * 100.0f);
            yields.Cash *= (1.0f + happiness_delta);
            Statistics.Cash_Percent.Add("Happiness", happiness_delta * 100.0f);
            yields.Science *= (1.0f + happiness_delta);
            Statistics.Science_Percent.Add("Happiness", happiness_delta * 100.0f);
            yields.Culture *= (1.0f + happiness_delta);
            Statistics.Culture_Percent.Add("Happiness", happiness_delta * 100.0f);
            yields.Mana *= (1.0f + happiness_delta);
            Statistics.Mana_Percent.Add("Happiness", happiness_delta * 100.0f);
            yields.Faith *= (1.0f + happiness_delta);
            Statistics.Faith_Percent.Add("Happiness", happiness_delta * 100.0f);

            //Health
            float health_delta = Health >= 0.0f ? Helper.Break_Point_Bonus(Health, 10.0f, 25.0f, 0.1f, 0.15f) : -Helper.Break_Point_Bonus(-Health, 10.0f, 30.0f, 0.25f, 0.75f);
            float lesser_health_delta = health_delta / 2.0f;
            //yields.Food = rounding removed 1.0f + health_delta) * yields.Food);
            //Statistics.Food_Percent.Add("Health", health_delta * 100.0f);
            yields.Production *= (1.0f + health_delta);
            Statistics.Production_Percent.Add("Health", health_delta * 100.0f);
            yields.Cash *= (1.0f + lesser_health_delta);
            Statistics.Cash_Percent.Add("Health", lesser_health_delta * 100.0f);
            yields.Science *= (1.0f + lesser_health_delta);
            Statistics.Science_Percent.Add("Health", lesser_health_delta * 100.0f);
            yields.Culture *= (1.0f + lesser_health_delta);
            Statistics.Culture_Percent.Add("Health", lesser_health_delta * 100.0f);
            /*yields.Mana = rounding removed 1.0f + lesser_health_delta) * yields.Mana);
            Statistics.Mana_Percent.Add("Health", lesser_health_delta * 100.0f);
            yields.Faith = rounding removed 1.0f + lesser_health_delta) * yields.Faith);
            Statistics.Faith_Percent.Add("Health", lesser_health_delta * 100.0f);*/

            //Order
            if (Order < 0.0f) {
                float order_delta = -Helper.Break_Point_Bonus(-Order, 5.0f, 10.0f, 0.50f, 0.75f);
                float lesser_order_delta = order_delta / 2.0f;
                yields.Production *= (1.0f + lesser_order_delta);
                Statistics.Production_Percent.Add("Order", lesser_order_delta * 100.0f);
                yields.Cash *= (1.0f + order_delta);
                Statistics.Cash_Percent.Add("Order", order_delta * 100.0f);
            }

            //Food consumption
            float food_consumption = Food_Consumption;
            yields.Food -= food_consumption;
            Statistics.Food.Add("Consumed", -food_consumption);

            //Upkeep TODO: here?
            float total_upkeep = Building_Upkeep;
            yields.Cash -= total_upkeep;
            Statistics.Cash.Add("Upkeep", -total_upkeep);



            //Save
            saved_yields = new Yields(yields);
            update_yields = false;

            return yields;
        }
    }

    public float Building_Upkeep
    {
        get {
            float upkeep = 0.0f;
            float reduction = 0.0f;
            foreach (Building building in Buildings) {
                upkeep += building.Upkeep;
                reduction += building.Building_Upkeep_Reduction;
            }
            reduction = Mathf.Clamp01(reduction);
            upkeep *= (1.0f - reduction);
            return upkeep;
        }
    }

    public float Max_Mana
    {
        get {
            float max_mana = 0.0f;
            foreach(Building building in Buildings) {
                if (!building.Paused) {
                    max_mana += building.Max_Mana;
                }
            }
            return max_mana;
        }
    }

    public float Garrison_Upkeep_Reduction
    {
        get {
            float reduction = 0.0f;
            foreach(Building building in Buildings) {
                reduction += building.Garrison_Upkeep_Reduction;
            }
            return Mathf.Clamp01(reduction);
        }
    }

    public float Food_Consumption
    {
        get {
            return Owner.Faction.Pop_Food_Consumption * Population;
        }
    }


    public int Max_Food_Stored
    {
        get {
            int max = Base_Max_Food_Storage;
            foreach(Building building in Buildings) {
                max += building.Food_Storage;
            }
            return max;
        }
    }

    /// <summary>
    /// Takes EmpireModifiers into account
    /// </summary>
    public float Total_Unit_Training_Speed_Bonus
    {
        get {
            float bonus = 0.0f;
            foreach(Building building in Buildings) {
                bonus += building.Unit_Training_Speed_Bonus;
            }
            bonus += Owner.EmpireModifiers.Unit_Training_Speed_Bonus;
            return bonus;
        }
    }

    /// <summary>
    /// Takes EmpireModifiers into account
    /// </summary>
    public float Total_Building_Construction_Speed_Bonus
    {
        get {
            float bonus = 0.0f;
            foreach (Building building in Buildings) {
                bonus += building.Building_Constuction_Speed_Bonus;
            }
            bonus += Owner.EmpireModifiers.Building_Constuction_Speed_Bonus;
            return bonus;
        }
    }

    /// <summary>
    /// Takes EmpireModifiers into account
    /// </summary>
    public float Total_Improvement_Construction_Speed_Bonus
    {
        get {
            float bonus = 0.0f;
            foreach (Building building in Buildings) {
                bonus += building.Improvement_Constuction_Speed_Bonus;
            }
            bonus += Owner.EmpireModifiers.Improvement_Constuction_Speed_Bonus;
            return bonus;
        }
    }

    public float Cultural_Influence_Range
    {
        get {
            float range = DEFAULT_CULTURAL_INFLUENCE_RANGE;
            foreach(Building b in Buildings) {
                range += b.Cultural_Influence_Range;
            }
            return range < 1.0f ? 1.0f : range;
        }
    }

    /// <summary>
    /// TODO: This
    /// </summary>
    public float Cultural_Influence_Resistance
    {
        get {
            return 0.10f;
        }
    }

    public void Yields_Changed()
    {
        update_yields = true;
    }

    private Dictionary<CitySize, int> Population_Breakpoints
    {
        get {
            return new Dictionary<CitySize, int>() {
                { CitySize.Town, 0 },
                { CitySize.City, 5 },
                { CitySize.Metropolis, 15 }
            };
        }
    }

    public CitySize Size
    {
        get {
            CitySize size = CitySize.Town;
            int pop_required = 0;
            foreach (CitySize citySize in Enum.GetValues(typeof(CitySize))) {
                if (Population_Breakpoints[citySize] > pop_required && Population_Breakpoints[citySize] <= Population) {
                    size = citySize;
                    pop_required = Population_Breakpoints[citySize];
                }
            }
            return size;
        }
    }

    public float Trade_Value
    {
        get {
            float trade_value = (Population + (Last_Turn_Yields.Total / 10.0f)) * 0.2f;
            foreach(Building building in Buildings) {
                trade_value += building.Trade_Value;
            }
            if(trade_value < 1.0f) {
                return 1.0f;
            }
            return trade_value;
        }
    }

    public void Update_Hex_Yields()
    {
        Hex.Base_Yields = new Yields(Owner.Faction.City_Yields[Size]);
        update_yields = true;
    }

    public KeyValuePair<CitySize, int>? Population_Required_For_Next_City_Size()
    {
        CitySize current_size = Size;
        int current_pop_required = Population_Breakpoints[current_size];
        CitySize? next_size = null;
        foreach(KeyValuePair<CitySize, int> breakpoint in Population_Breakpoints) {
            if(breakpoint.Key != current_size && Population_Breakpoints[breakpoint.Key] > current_pop_required && (next_size == null ||
                Population_Breakpoints[next_size.Value] > Population_Breakpoints[breakpoint.Key])) {
                next_size = breakpoint.Key;
            }
        }
        if(next_size == null) {
            return null;
        }
        return new KeyValuePair<CitySize, int>(next_size.Value, Population_Breakpoints[next_size.Value]);
    }

    public int Pop_Growth_Required
    {
        get {
            return (int)Math.Pow(Population, 2) + 50;
        }
    }

    public float Pop_Growth_Acquired { get; private set; }

    public float Pop_Growth
    {
        get {
            float growth = 1.0f;

            Statistics.Growth.Clear();
            Statistics.Growth_Percent.Clear();
            Statistics.Growth.Add("Base", growth);

            //Food
            if (Yields.Food > 0.0f) {
                float food_bonus = Helper.Break_Point_Bonus(Yields.Food, 5.0f, 10.0f, 1.0f, 1.25f);
                growth += food_bonus;
                Statistics.Growth.Add("Food", food_bonus);
            }

            //Culture
            float culture_bonus = Yields.Culture * 0.05f;
            growth += culture_bonus;
            Statistics.Growth.Add("Culture", culture_bonus);

            //Happiness
            float happiness_bonus = Helper.Break_Point_Bonus(Happiness, 10.0f, 5.0f, 0.10f);
            growth += happiness_bonus;
            Statistics.Growth.Add("Happiness", happiness_bonus);

            //Health
            float health_bonus = Helper.Break_Point_Bonus(Health, 10.0f, 5.0f, 0.25f);
            growth += health_bonus;
            Statistics.Growth.Add("Health", health_bonus);

            //Buildings
            float building_additive = 0.0f;
            float building_multiplier = 1.0f;
            foreach(Building building in Buildings) {
                building_additive += building.Pop_Growth_Additive_Bonus;
                building_multiplier += building.Pop_Growth_Multiplier_Bonus;
            }
            if(building_multiplier != 1.0f) {
                growth *= building_multiplier;
                Statistics.Growth_Percent.Add("Buildings", 100.0f * (building_multiplier - 1.0f));
            }
            growth += building_additive;
            Statistics.Growth.Add("Buildings", building_additive);

            //Empire modifiers
            if(Owner.EmpireModifiers.Population_Growth_Bonus != 0.0f) {
                float empire_multiplier = 1.0f + Owner.EmpireModifiers.Population_Growth_Bonus;
                growth *= empire_multiplier;
                Statistics.Growth_Percent.Add("Empire", 100.0f * (empire_multiplier - 1.0f));
            }

            //Starvation
            if (Starvation) {
                growth -= 100.0f;
                Statistics.Growth.Add("Starvation", -100.0f);
            }

            if(growth < 0.0f) {
                growth = 0.0f;
            }

            return growth;
        }
    }

    public bool Starvation
    {
        get {
            return Food_Stored == 0.0f && (Get_Base_Yields(false).Food - Food_Consumption) <= 0.0f;
        }
    }

    public int Pop_Growth_Estimated_Turns
    {
        get {
            if(Pop_Growth == 0.0f) {
                return -1;
            }
            return Mathf.CeilToInt((Pop_Growth_Required - Pop_Growth_Acquired) / Pop_Growth);
        }
    }

    public int Unemployed_Pops {
        get {
            return Population - Worked_Hexes.Count;
        }
    }

    public void Start_Turn()
    {
        update_yields = true;
    }

    public void End_Turn()
    {
        //Income
        Last_Turn_Yields = new Yields(Yields);
        update_yields = true;
        Owner.Cash += Yields.Cash;
        Owner.Mana += Yields.Mana;

        //Culture
        Cultural_Influence[Owner] += Yields.Culture;
        foreach (KeyValuePair<Influencable, float> influence_info in Influenced_Cities_And_Villages) {
            if (!influence_info.Key.Cultural_Influence.ContainsKey(Owner)) {
                influence_info.Key.Cultural_Influence.Add(Owner, influence_info.Value);
            } else {
                influence_info.Key.Cultural_Influence[Owner] += influence_info.Value;
            }
        }

        //Training
        if (Unit_Under_Production != null) {
            Unit_Production_Acquired += Yields.Production * (1.0f + Total_Unit_Training_Speed_Bonus);
            if(Unit_Production_Acquired >= Unit_Under_Production.Production_Required) {
                //TODO: Overflow? Stacking workers
                Owner.Queue_Notification(new Notification("Unit trained: " + Unit_Under_Production.Name, Unit_Under_Production.Texture, SpriteManager.SpriteType.Unit, null, delegate () {
                    CityGUIManager.Instance.Current_City = this;
                }));
                if ((Unit_Under_Production is Worker || Unit_Under_Production is Prospector) && Hex.Civilian == null) {
                    if(Unit_Under_Production is Worker) {
                        Hex.Civilian = new Worker(Hex, Unit_Under_Production as Worker, Owner);
                    } else {
                        Hex.Civilian = new Prospector(Hex, Unit_Under_Production as Prospector, Owner);
                    }
                    Unit_Under_Production = null;
                }
                if(Unit_Under_Production is Unit) {
                    if(Hex.Entity == null) {
                        Hex.Entity = new Army(Hex, Owner.Faction.Army_Prototype, Owner, new Unit(Unit_Under_Production as Unit));
                        Unit_Under_Production = null;
                    } else if(Hex.Entity is Army && (Hex.Entity as Army).Has_Space) {
                        (Hex.Entity as Army).Add_Unit(new Unit(Unit_Under_Production as Unit));
                        Unit_Under_Production = null;
                    }
                }
            }
        }

        //Building
        if (Building_Under_Production != null) {
            Building_Production_Acquired += Yields.Production * (1.0f + Total_Building_Construction_Speed_Bonus);
            if (Building_Production_Acquired >= Building_Under_Production.Production_Required) {
                //TODO: Overflow?
                Owner.Queue_Notification(new Notification("Building completed: " + Building_Under_Production.Name, Building_Under_Production.Texture, SpriteManager.SpriteType.Building, BUILDING_READY_SOUND_EFFECT, delegate () {
                    CityGUIManager.Instance.Current_City = this;
                }));
                Buildings.Add(new Building(Building_Under_Production, this));
                Building_Under_Production = null;
                update_yields = true;
            }
        }

        //Food storage
        Food_Stored += Yields.Food;
        Food_Stored = Mathf.Clamp(Food_Stored, 0.0f, Max_Food_Stored);

        //Pop growth
        Pop_Growth_Acquired += Pop_Growth;
        if(Pop_Growth_Acquired >= Pop_Growth_Required) {
            Pop_Growth_Acquired -= Pop_Growth_Required;
            Population++;
            Update_Hex_Yields();
            Auto_Apply_Unemployed_Pops();
            Owner.Queue_Notification(new Notification(string.Format("{0} has grown to {1} population units", Name, Population), Hex.Sprite, SpriteManager.SpriteType.Terrain, null, delegate () {
                CityGUIManager.Instance.Current_City = this;
            }));
        }

        //Special building actions
        foreach(Building building in Buildings.OrderByDescending(x => x.Update_Priority).ToList()) {
            if(building.On_Turn_End != null) {
                building.On_Turn_End(building);
            }
        }

        //Improvements
        foreach(WorldMapHex hex in Hexes_In_Work_Range) {
            if(hex.Improvement != null && hex.Improvement.Update != null) {
                hex.Improvement.Update(hex.Improvement);
            }
        }

        //Notifications
        if(Happiness <= VERY_LOW_HAPPINESS_WARNING_THRESHOLD) {
            Owner.Queue_Notification(new Notification("Very low happiness", VERY_LOW_HAPPINESS_TEXTURE, SpriteManager.SpriteType.UI, null, delegate () {
                CityGUIManager.Instance.Current_City = this;
            }));
        } else if(Happiness <= LOW_HAPPINESS_WARNING_THRESHOLD) {
            Owner.Queue_Notification(new Notification("Low happiness", LOW_HAPPINESS_TEXTURE, SpriteManager.SpriteType.UI, null, delegate () {
                CityGUIManager.Instance.Current_City = this;
            }));
        }
        if (Health <= VERY_LOW_HEALTH_WARNING_THRESHOLD) {
            Owner.Queue_Notification(new Notification("Very low health", VERY_LOW_HEALTH_TEXTURE, SpriteManager.SpriteType.UI, null, delegate () {
                CityGUIManager.Instance.Current_City = this;
            }));
        } else if (Health <= LOW_HEALTH_WARNING_THRESHOLD) {
            Owner.Queue_Notification(new Notification("Low health", LOW_HEALTH_TEXTURE, SpriteManager.SpriteType.UI, null, delegate () {
                CityGUIManager.Instance.Current_City = this;
            }));
        }
        if (Order <= VERY_LOW_ORDER_WARNING_THRESHOLD) {
            Owner.Queue_Notification(new Notification("Very low order", VERY_LOW_ORDER_TEXTURE, SpriteManager.SpriteType.UI, null, delegate () {
                CityGUIManager.Instance.Current_City = this;
            }));
        } else if (Order <= LOW_ORDER_WARNING_THRESHOLD) {
            Owner.Queue_Notification(new Notification("Low order", LOW_ORDER_TEXTURE, SpriteManager.SpriteType.UI, null, delegate () {
                CityGUIManager.Instance.Current_City = this;
            }));
        }

        Status_Effects.End_Turn();
    }

    public void Auto_Apply_Unemployed_Pops()
    {
        while(Unemployed_Pops > 0) {
            update_yields = true;
            List<WorldMapHex> valid_hexes = Hexes_That_Can_Be_Worked;
            List<WorldMapHex> remove_these = new List<WorldMapHex>();
            foreach(WorldMapHex valid_hex_candidate in valid_hexes) {
                if(valid_hex_candidate.Distance(Hex) > Work_Range) {
                    remove_these.Add(valid_hex_candidate);
                }
            }
            foreach(WorldMapHex too_far_away_hex in remove_these) {
                valid_hexes.Remove(too_far_away_hex);
            }
            if(valid_hexes.Count == 0) {
                break;
            }
            float top_yields_total = 0.0f;
            int top_yields_index = -1;
            for (int i = 0; i < valid_hexes.Count; i++) {
                if(top_yields_index == -1 || valid_hexes[i].Yields.Total > top_yields_total) {
                    top_yields_index = i;
                    top_yields_total = valid_hexes[i].Yields.Total;
                }
            }
            valid_hexes[top_yields_index].Owner = Owner;
            Worked_Hexes.Add(valid_hexes[top_yields_index]);
        }
    }

    public List<WorldMapHex> Hexes_That_Can_Be_Worked
    {
        get {
            return Hexes_In_Work_Range.Where(x => x.City == null && x.Village == null && x.Owner == null &&
                x.Is_Explored_By(Owner) && (x.Army == null || x.Army.Owner.Id == Owner.Id || !x.Army.Blocks_Hex_Working)).ToList();
        }
    }

    /// <summary>
    /// TODO: Update LoS
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public bool Assing_Pop(WorldMapHex hex)
    {
        if(Unemployed_Pops <= 0 || !Hexes_In_Work_Range.Contains(hex)) {
            return false;
        }
        hex.Owner = Owner;
        Worked_Hexes.Add(hex);
        foreach (WorldMapHex adjacent_hex in hex.Get_Adjancent_Hexes()) {
            if (adjacent_hex.Improvement != null && adjacent_hex.Improvement.Update != null) {
                adjacent_hex.Improvement.Update(adjacent_hex.Improvement);
            }
        }
        update_yields = true;
        return true;
    }

    /// <summary>
    /// TODO: Update LoS
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public bool Unassing_Pop(WorldMapHex hex)
    {
        if (!Worked_Hexes.Contains(hex)) {
            return false;
        }
        hex.Owner = null;
        Worked_Hexes.Remove(hex);
        foreach(WorldMapHex adjacent_hex in hex.Get_Adjancent_Hexes()) {
            if(adjacent_hex.Improvement != null && adjacent_hex.Improvement.Update != null) {
                adjacent_hex.Improvement.Update(adjacent_hex.Improvement);
            }
        }
        update_yields = true;
        return true;
    }

    public bool Delete_Building(Building building)
    {
        if (!Buildings.Contains(building)) {
            return false;
        }
        Owner.Cash += REFOUND_AMOUNT * building.Cost;
        Buildings.Remove(building);
        update_yields = true;
        return true;
    }

    public List<Building> Get_Building_Options(bool ignore_cost)
    {
        List<Building> options = new List<Building>();
        for (int i = 0; i < Owner.Faction.Buildings.Count; i++) {
            if ((Owner.Faction.Buildings[i].Technology_Required == null || Owner.Researched_Technologies.Any(x => x.Name == Owner.Faction.Buildings[i].Technology_Required.Name))
                    && !Buildings.Any(x => x.Name == Owner.Faction.Buildings[i].Name) && (ignore_cost || Owner.Faction.Buildings[i].Cost <= Owner.Cash)) {
                options.Add(Owner.Faction.Buildings[i]);
            }
        }
        return options;
    }

    /// <summary>
    /// TODO: building requirements
    /// </summary>
    /// <param name="ignore_cost"></param>
    /// <returns></returns>
    public List<Trainable> Get_Unit_Options(bool ignore_cost)
    {
        List<Trainable> available_units = new List<Trainable>();
        for (int i = 0; i < Owner.Faction.Units.Count; i++) {
            if ((Owner.Faction.Units[i].Technology_Required == null ||
                    Owner.Researched_Technologies.Any(x => Owner.Faction.Units[i].Technology_Required.Name == x.Name)) &&
                    (!(Owner.Faction.Units[i] is Unit) || (Owner.Faction.Units[i] as Unit).Buildinds_Required == null ||
                    (Owner.Faction.Units[i] as Unit).Buildinds_Required.Count == 0 ||
                    !(Owner.Faction.Units[i] as Unit).Buildinds_Required.Any(x => !Buildings.Any(y => y.Name == x.Name && !y.Paused))) &&
                    (ignore_cost || Owner.Faction.Units[i].Cost <= Owner.Cash)) {
                available_units.Add(Owner.Faction.Units[i]);
            }
        }
        return available_units;
    }

    private float Base_Happiness
    {
        get {
            Statistics.Happiness.Clear();

            //Base
            float happiness = Owner.Faction.Base_Happiness;
            Statistics.Happiness.Add("Base", Owner.Faction.Base_Happiness);

            //Population
            float happiness_from_pops = Owner.Faction.Happiness_From_Pops;
            float building_delta = 0.0f;
            foreach (Building building in Buildings) {
                happiness_from_pops += building.Base_Happiness_From_Pops_Delta;
                building_delta += building.Happiness;
            }
            if(happiness_from_pops > 0.0f) {
                happiness_from_pops = 0.0f;
            }
            float pop_delta = happiness_from_pops * Population;
            Statistics.Happiness.Add("Population", pop_delta);
            happiness += pop_delta;

            //Hexes
            float hex_delta = 0.0f;
            foreach (WorldMapHex hex in Worked_Hexes) {
                hex_delta += hex.Happiness;
            }
            Statistics.Happiness.Add("Hexes", hex_delta);
            happiness += hex_delta;

            //Buildings
            Statistics.Happiness.Add("Buildings", building_delta);
            happiness += building_delta;

            //Health
            float health_delta = Health >= 0.0f ? Helper.Break_Point_Bonus(Health, 5.0f, 10.0f, 2.5f, 3.5f) : -Helper.Break_Point_Bonus(-Health, 5.0f, 10.0f, 2.5f, 3.5f);
            Statistics.Happiness.Add("Health", health_delta);
            happiness += health_delta;

            //Cultural influence
            float enemy_culture = 1.0f - Owned_Culture[0];
            if(enemy_culture > ENEMY_CULTURAL_INFLUENCE_HAPPINESS_BREAK_POINT_1) {
                float enemy_culture_delta = -1.0f * ENEMY_CULTURAL_INFLUENCE_AMP * Population * (enemy_culture - ENEMY_CULTURAL_INFLUENCE_HAPPINESS_BREAK_POINT_1);
                if(enemy_culture > ENEMY_CULTURAL_INFLUENCE_HAPPINESS_BREAK_POINT_2) {
                    enemy_culture_delta *= (1.0f + (enemy_culture - ENEMY_CULTURAL_INFLUENCE_HAPPINESS_BREAK_POINT_2));
                }
                enemy_culture_delta *= Owner.Faction.Enemy_Cultural_Influence_Unhappiness_Multiplier;
                Statistics.Happiness.Add("Cultural influence", enemy_culture_delta);
                happiness += enemy_culture_delta;
            }

            //Units
            if(Garrison != null) {
                float unit_delta = Garrison.Get_City_Effects(this).Happiness;
                Statistics.Happiness.Add("Units", unit_delta);
                happiness += unit_delta;
            }

            //Status effects
            foreach(CityStatusEffect status_effect in Status_Effects) {
                Statistics.Happiness.Add(string.Format("{0} ({1} turn{2})", status_effect.Name, status_effect.UI_Current_Duration,
                    status_effect.UI_Current_Duration != 1 ? "s" : string.Empty), status_effect.Happiness);
                happiness += status_effect.Happiness;
            }

            //Starvation
            if (Starvation) {
                float starvation_delta = -Population;
                Statistics.Happiness.Add("Starvation", starvation_delta);
                happiness += starvation_delta;
            }

            return happiness;
        }
    }

    public float Happiness
    {
        get {
            float happiness = Base_Happiness;

            float order_delta = Base_Order >= 0.0f ? Helper.Break_Point_Bonus(Base_Order, 2.0f, 5.0f, 1.0f, 2.0f) : -Helper.Break_Point_Bonus(-Base_Order, 5.0f, 10.0f, 5.0f, 7.5f);
            Statistics.Happiness.Add("Order", order_delta);
            happiness += order_delta;

            return happiness;
        }
    }
    
    private float Base_Health
    {
        get {
            Statistics.Health.Clear();

            //Base
            float health = Owner.Faction.Base_Health;
            Statistics.Health.Add("Base", Owner.Faction.Base_Health);

            //Population
            float building_penalty_multiplier = 1.0f;
            float health_from_pops = Owner.Faction.Health_From_Pops;
            foreach (Building building in Buildings) {
                health_from_pops += building.Base_Health_From_Pops_Delta;
                building_penalty_multiplier += building.Health_Penalty_From_Buildings_Multiplier;
            }
            if(building_penalty_multiplier < 0.0f) {
                building_penalty_multiplier = 0.0f;
            }
            if(health_from_pops > 0.0f) {
                health_from_pops = 0.0f;
            }
            float pop_delta = health_from_pops * Population;
            Statistics.Health.Add("Population", pop_delta);
            health += pop_delta;

            //Hexes
            float hex_delta = 0.0f;
            foreach (WorldMapHex hex in Worked_Hexes) {
                hex_delta += hex.Health;
            }
            Statistics.Health.Add("Hexes", hex_delta);
            health += hex_delta;

            //Buildings
            float building_delta = 0.0f;
            foreach (Building building in Buildings) {
                building_delta += building.Health >= 0.0f ? building.Health : building_penalty_multiplier * building.Health;
            }
            Statistics.Health.Add("Buildings", building_delta);
            health += building_delta;

            //Production
            float production_delta = Get_Base_Yields(false).Production * -0.1f;
            Statistics.Health.Add("Production", production_delta);
            health += production_delta;

            //Units
            if (Garrison != null) {
                float unit_delta = Garrison.Get_City_Effects(this).Health;
                Statistics.Health.Add("Units", unit_delta);
                health += unit_delta;
            }

            //Status effects
            foreach (CityStatusEffect status_effect in Status_Effects) {
                Statistics.Health.Add(string.Format("{0} ({1} turn{2})", status_effect.Name, status_effect.UI_Current_Duration,
                    status_effect.UI_Current_Duration != 1 ? "s" : string.Empty), status_effect.Health);
                health += status_effect.Health;
            }

            return health;
        }
    }

    public float Health
    {
        get {
            return Base_Health;
        }
    }

    private float Base_Order
    {
        get {
            Statistics.Order.Clear();

            //Base
            float order = Owner.Faction.Base_Order;
            Statistics.Order.Add("Base", Owner.Faction.Base_Order);

            //Population
            float order_from_pops = Owner.Faction.Order_From_Pops;
            float building_delta = 0.0f;
            foreach (Building building in Buildings) {
                order_from_pops += building.Base_Order_From_Pops_Delta;
                building_delta += building.Order;
            }
            if(order_from_pops > 0.0f) {
                order_from_pops = 0.0f;
            }
            float pop_delta = order_from_pops * Population;
            Statistics.Order.Add("Population", pop_delta);
            order += pop_delta;

            //Hexes
            float hex_delta = 0.0f;
            foreach (WorldMapHex hex in Worked_Hexes) {
                hex_delta += hex.Order;
            }
            Statistics.Order.Add("Hexes", hex_delta);
            order += hex_delta;

            //Happiness
            if (Base_Happiness < 0.0f) {
                float happiness_delta = Base_Happiness * 0.25f;
                Statistics.Order.Add("Happiness", happiness_delta);
                order += happiness_delta;
            }

            //Buildings
            Statistics.Order.Add("Buildings", building_delta);
            order += building_delta;

            //Production
            float production_delta = Get_Base_Yields(false).Production * -0.05f;
            Statistics.Order.Add("Production", production_delta);
            order += production_delta;

            //Income
            float income_delta = Get_Base_Yields(false).Cash * -0.15f;
            Statistics.Order.Add("Income", income_delta);
            order += income_delta;

            //Starvation
            if (Starvation) {
                float starvation_delta = -0.25f * Population;
                Statistics.Order.Add("Starvation", starvation_delta);
                order += starvation_delta;
            }

            if (Garrison != null) {
                //Army str
                float army_delta = Garrison.Order;
                Statistics.Order.Add("Garrison strenght", army_delta);
                order += army_delta;

                //Unit abilities
                float unit_delta = Garrison.Get_City_Effects(this).Order;
                Statistics.Order.Add("Unit abilities", unit_delta);
                order += unit_delta;
            }

            //Status effects
            foreach (CityStatusEffect status_effect in Status_Effects) {
                Statistics.Order.Add(string.Format("{0} ({1} turn{2})", status_effect.Name, status_effect.UI_Current_Duration,
                    status_effect.UI_Current_Duration != 1 ? "s" : string.Empty), status_effect.Order);
                order += status_effect.Order;
            }

            return order;
        }
    }

    public float Order
    {
        get {
            return Base_Order;
        }
    }

    public float[] Owned_Culture
    {
        get {
            float own_total = 0.0f;
            float enemy_total = 0.0f;
            foreach(KeyValuePair<Player, float> influence_info in Cultural_Influence) {
                if(influence_info.Key.Id == Owner.Id) {
                    own_total += influence_info.Value;
                } else {
                    enemy_total += influence_info.Value;
                }
            }
            if(own_total <= 0.0f) {
                return new float[3] { 0.0f, own_total, enemy_total };
            }
            if(enemy_total <= 0.0f) {
                return new float[3] { 1.0f, own_total, enemy_total };
            }
            return new float[3] { (own_total / (own_total + enemy_total)), own_total, enemy_total };
        }
    }

    public float Estimated_Owned_Culture_Change
    {
        get {
            float new_own_total = Yields.Culture;
            float new_enemy_total = 0.0f;
            foreach (KeyValuePair<Player, float> influence_info in Cultural_Influence) {
                if (influence_info.Key.Id == Owner.Id) {
                    new_own_total += influence_info.Value;
                } else {
                    new_enemy_total += influence_info.Value;
                }
            }

            foreach(KeyValuePair<City, float> influence_data in Cities_Influenced_By) {
                if(influence_data.Key.Owner.Id == Owner.Id) {
                    new_own_total += influence_data.Value;
                } else {
                    new_enemy_total += influence_data.Value;
                }
            }

            float new_ratio = new_own_total <= 0.0f ? 0.0f : (new_own_total / (new_own_total + new_enemy_total));
            return new_ratio - Owned_Culture[0];
        }
    }

    public Dictionary<Influencable, float> Influenced_Cities_And_Villages
    {
        get {
            Dictionary<Influencable, float> cities_and_villages = new Dictionary<Influencable, float>();
            if (Owner.Is_Neutral) {
                return cities_and_villages;
            }
            foreach (Player p in Main.Instance.All_Players) {
                foreach(City c in p.Cities) {
                    if (c.Hex.Is_Explored_By(Owner) && c.Id != Id) {
                        cities_and_villages.Add(c, Calculate_Influence(this, c));
                    }
                }
                foreach (Village v in p.Villages) {
                    if (v.Hex.Is_Explored_By(Owner)) {
                        cities_and_villages.Add(v, Calculate_Influence(this, v));
                    }
                }
            }
            return cities_and_villages;
        }
    }

    public Dictionary<City, float> Cities_Influenced_By
    {
        get {
            Dictionary<City, float> cities = new Dictionary<City, float>();
            foreach (Player p in Main.Instance.Players) {
                foreach (City c in p.Cities) {
                    if (Hex.Is_Explored_By(c.Owner) && c.Id != Id) {
                        cities.Add(c, Calculate_Influence(c, this));
                    }
                }
            }
            /*foreach (City c in Main.Instance.Neutral_Player.Cities) {
                if (c.Hex.Is_Explored_By(Owner) && c.Id != Id) {
                    cities.Add(c, Calculate_Influence(c, this));
                }
            }*/
            return cities;
        }
    }

    private float Calculate_Influence(City city, Influencable target)
    {
        float influence = city.Yields.Culture;
        float distance = city.Hex.Distance(target.Hex);
        if(target is Village) {
            foreach(Building building in city.Buildings) {
                if (building.Paused) {
                    continue;
                }
                influence += building.Village_Cultural_Influence;
            }
        }
        if(distance > city.Cultural_Influence_Range) {
            influence *= Mathf.Pow(1.0f - CULTURAL_INFLUENCE_DECAY_PER_HEX, distance - city.Cultural_Influence_Range);
        }
        influence *= (1.0f - target.Cultural_Influence_Resistance);
        return influence;
    }

    public bool Conquer(Player conqueror)
    {
        if(Owner.Id == conqueror.Id) {
            return false;
        }
        Owner.Cities.Remove(this);
        if(Owner.Cities.Count == 0 && !Owner.Is_Neutral) {
            Owner.Defeated = true;
        }
        Owner.Queue_Notification(new Notification("{0} has been conquered!", Hex.Sprite, SpriteManager.SpriteType.Terrain, null, delegate() {
            CameraManager.Instance.Set_Camera_Location(Hex);
        }));
        Owner = conqueror;
        Owner.Cities.Add(this);
        foreach(WorldMapHex hex in Worked_Hexes) {
            hex.Owner = Owner;
        }
        Change_Production((Trainable)null);
        Change_Production((Building)null);
        TopGUIManager.Instance.Update_GUI();
        World.Instance.Map.Update_LoS();
        return true;
    }

    public bool Add_Trade_Route(TradeRoute route)
    {
        if(route.City.Id != Id) {
            CustomLogger.Instance.Warning(string.Format("Trying to add city #{0}'s trade route to city {1}", route.City.Id, Id));
            return false;
        }
        TradeRoute replace_route = null;
        foreach(TradeRoute existing_route in Trade_Routes) {
            if (existing_route.Target.Hex.Coordinates.Equals(route.Target.Hex.Coordinates)) {
                if(!existing_route.Water_Route && route.Water_Route) {
                    //Replace land route with water route
                    replace_route = existing_route;
                    break;
                } else {
                    return false;
                }
            }
        }
        if(replace_route != null) {
            Trade_Routes.Remove(replace_route);
        }
        Trade_Routes.Add(route);
        return true;
    }

    public void Apply_Status_Effect(CityStatusEffect status_effect, bool stacks)
    {
        Status_Effects.Apply_Status_Effect(status_effect, stacks);
        if(!status_effect.Yield_Delta.Empty || !status_effect.Percentage_Yield_Delta.Empty) {
            Yields_Changed();
        }
    }

    public List<Improvement> Worked_Improvements
    {
        get {
            List<Improvement> improvements = new List<Improvement>();
            foreach(WorldMapHex hex in Worked_Hexes) {
                if(hex.Improvement != null) {
                    improvements.Add(hex.Improvement);
                }
            }
            return improvements;
        }
    }

    public void Load(CitySaveData data)
    {
        Id = data.Id;
        Name = data.Name;
        Owner.Cities.Add(this);
        Worked_Hexes = data.Worked_Hexes.Select(x => World.Instance.Map.Get_Hex_At(x.X, x.Y)).ToList();
        Hexes_In_Work_Range = data.Hexes_In_Work_Range.Select(x => World.Instance.Map.Get_Hex_At(x.X, x.Y)).ToList();
        LoS = data.LoS;
        Work_Range = data.Work_Range;
        Unit_Under_Production = string.IsNullOrEmpty(data.Unit_Under_Production) ? null : Owner.Faction.Units.First(x => x.Name == data.Unit_Under_Production);
        Building_Under_Production = string.IsNullOrEmpty(data.Building_Under_Production) ? null : Owner.Faction.Buildings.First(x => x.Name == data.Building_Under_Production);
        Unit_Production_Acquired = data.Unit_Production_Acquired;
        Building_Production_Acquired = data.Building_Production_Acquired;
        Population = data.Population;
        Base_Max_Food_Storage = data.Base_Max_Food_Storage;
        Food_Stored = data.Food_Stored;
        Full_LoS = data.Full_LoS;
        Last_Turn_Yields = new Yields(data.Last_Turn_Yields);
        update_yields = data.Update_Yields;
        saved_yields = data.Saved_Yields != null ? new Yields(data.Saved_Yields) : null;
        saved_base_yields = data.Saved_Base_Yields != null ? new Yields(data.Saved_Base_Yields) : null;
        foreach(BuildingSaveData building_data in data.Buildings) {
            Building building = new Building(Owner.Faction.Buildings.First(x => x.Name == building_data.Name), this);
            building.Load(building_data);
            Buildings.Add(building);
        }
        Cultural_Influence = data.Cultural_Influence.ToDictionary(x => SaveManager.Get_Player(x.Player), x => x.Influence);
        foreach(CityStatusEffectSaveData effect_data in data.Status_Effects) {
            CityStatusEffect effect = new CityStatusEffect(effect_data.Name, effect_data.Duration);
            effect.Load(effect_data);
            Status_Effects.Apply_Status_Effect(effect, true);
        }
    }

    public void Load_Trade_Routes(CitySaveData data)
    {
        Trade_Routes = data.Trade_Routes.Select(x => new TradeRoute(x.Path.Select(y => World.Instance.Map.Get_Hex_At(y.X, y.Y)).ToList(), this, World.Instance.Map.Get_Hex_At(x.Target.X, x.Target.Y).Trade_Partner,
            x.Water_Route)).ToList();
    }

    public CitySaveData Save_Data
    {
        get {
            CitySaveData data = new CitySaveData();
            data.Id = Id;
            data.Name = Name;
            data.Hex_X = Hex.Coordinates.X;
            data.Hex_Y = Hex.Coordinates.Y;
            data.Owner = SaveManager.Get_Player_Id(Owner);
            data.Worked_Hexes = Worked_Hexes.Select(x => new CoordinateSaveData() { X = x.Coordinates.X, Y = x.Coordinates.Y }).ToList();
            data.Hexes_In_Work_Range = Hexes_In_Work_Range.Select(x => new CoordinateSaveData() { X = x.Coordinates.X, Y = x.Coordinates.Y }).ToList();
            data.LoS = LoS;
            data.Work_Range = Work_Range;
            data.Unit_Under_Production = Unit_Under_Production == null ? null : Unit_Under_Production.Name;
            data.Building_Under_Production = Building_Under_Production == null ? null : Building_Under_Production.Name;
            data.Unit_Production_Acquired = Unit_Production_Acquired;
            data.Building_Production_Acquired = Building_Production_Acquired;
            data.Population = Population;
            data.Base_Max_Food_Storage = Base_Max_Food_Storage;
            data.Food_Stored = Food_Stored;
            data.Full_LoS = Full_LoS;
            data.Last_Turn_Yields = Last_Turn_Yields.Save_Data;
            data.Update_Yields = update_yields;
            data.Saved_Yields = saved_yields != null ? saved_yields.Save_Data : null;
            data.Saved_Base_Yields = saved_base_yields != null ? saved_base_yields.Save_Data : null;
            data.Buildings = Buildings.Select(x => x.Save_Data).ToList();
            data.Cultural_Influence = Cultural_Influence.Select(x => new CulturalInfluenceSaveData() { Player = SaveManager.Get_Player_Id(x.Key), Influence = x.Value }).ToList();
            data.Trade_Routes = Trade_Routes.Select(x => new TradeRouteSaveData() {
                Target = new CoordinateSaveData() { X = x.Target.Hex.Coordinates.X, Y = x.Target.Hex.Coordinates.Y },
                Path = x.Path.Select(y => new CoordinateSaveData() { X = y.Coordinates.X, Y = y.Coordinates.Y }).ToList(),
                Water_Route = x.Water_Route
            }).ToList();
            data.Status_Effects = Status_Effects.Select(x => new CityStatusEffectSaveData() {
                Name = x.Name,
                Duration = x.Duration,
                Current_Duration = x.Current_Duration,
                Parent_Duration = x.Parent_Duration.HasValue ? x.Parent_Duration.Value : -1,
                Happiness = x.Happiness,
                Health = x.Health,
                Order = x.Order,
                Yield_Delta = x.Yield_Delta == null ? null : x.Yield_Delta.Save_Data,
                Percentage_Yield_Delta = x.Percentage_Yield_Delta == null ? null : x.Percentage_Yield_Delta.Save_Data
            }).ToList();
            return data;
        }
    }
}
