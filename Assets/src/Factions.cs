using System.Collections.Generic;
using System.Linq;

public class Factions {
    private static bool initialized = false;
    private static List<Faction> all;
    private static Faction neutral_cities;

    public static List<Faction> All
    {
        get {
            if (!initialized) {
                Initialize();
            }
            return all;
        }
    }

    public static Faction Neutral_Cities
    {
        get {
            if (!initialized) {
                Initialize();
            }
            return neutral_cities;
        }
    }

    private static void Initialize()
    {
        all = new List<Faction>();


        /*
         * 
         * Prayer: festivals?
         * bonuses from excess food
         * 
         * 
         */

        all.Add(new Faction("Kingdom", 150, 1, new Dictionary<City.CitySize, Yields>() {
            { City.CitySize.Town, new Yields(2, 1, 2, 1, 1, 0, 0) },
            { City.CitySize.City, new Yields(1, 2, 3, 2, 1, 0, 0) },
            { City.CitySize.Metropolis, new Yields(0, 2, 5, 3, 2, 0, 0) }
        }, 3.0f, 100, 1.0f, -0.40f, 1.0f, -0.30f, 1.5f, -0.20f, 1.0f,
        new Technology("Root", 5, new List<AI.Tag>()), new Army("Army", "default_unit", 10), new EmpireModifiers() {
            Passive_Income = 3.0f,
            Percentage_Village_Yield_Bonus = new Yields(10.0f, 10.0f, 10.0f, 0.0f, 0.0f, 0.0f, 0.0f)
        }));
        Faction Kingdom = all[0];


        Technology Conscription = new Technology("Conscription", 20, new List<AI.Tag>() { AI.Tag.Military });
        Kingdom.Root_Technology.Link(Conscription, 0);
        Technology Professional_Army = new Technology("Professional Army", 50, new List<AI.Tag>() { AI.Tag.Military });
        Conscription.Link(Professional_Army, 3);
        Technology Military_Science = new Technology("Military Science", 105, new List<AI.Tag>() { AI.Tag.Military });
        Professional_Army.Link(Military_Science, 3);
        Technology Military_History = new Technology("Military History", 240, new List<AI.Tag>() { AI.Tag.Military });
        Military_Science.Link(Military_History, 3);

        Technology Combined_Arms = new Technology("Combined Arms", 85, new List<AI.Tag>() { AI.Tag.Military });
        Professional_Army.Link(Combined_Arms, 2);
        Technology Logistics = new Technology("Logistics", 220, new List<AI.Tag>() { AI.Tag.Military });
        Combined_Arms.Link(Logistics, 3);
        
        Technology Topography = new Technology("Topography", 280, new List<AI.Tag>());
        Topography.EmpireModifiers = new EmpireModifiers() {
            Improvement_Constuction_Speed_Bonus = 0.05f
        };
        Logistics.Link(Topography, 3);


        Technology Ballistics = new Technology("Ballistics", 450, new List<AI.Tag>() { AI.Tag.Military });
        Topography.Link(Ballistics, 3);

        Technology Shock_Tactics = new Technology("Shock Tactics", 245, new List<AI.Tag>() { AI.Tag.Military });
        Combined_Arms.Link(Shock_Tactics, 2);
        
        Technology Metal_Casting = new Technology("Metal Casting", 25, new List<AI.Tag>() { AI.Tag.Production });
        Kingdom.Root_Technology.Link(Metal_Casting, 3);
        Technology Construction = new Technology("Construction", 45, new List<AI.Tag>() { AI.Tag.Production, AI.Tag.Order, AI.Tag.Happiness, AI.Tag.Health, AI.Tag.Food });
        Metal_Casting.Link(Construction, 2);
        Technology Alloys = new Technology("Alloys", 35, new List<AI.Tag>() { AI.Tag.Cash });
        Metal_Casting.Link(Alloys, 1);

        Technology Heavy_Industry = new Technology("Heavy Industry", 90, new List<AI.Tag>());
        Construction.Link(Heavy_Industry, 3);
        Technology Metallurgy = new Technology("Metallurgy", 260, new List<AI.Tag>());
        Heavy_Industry.Link(Metallurgy, 3);
        Technology Machinery = new Technology("Machinery", 340, new List<AI.Tag>() { AI.Tag.Cash, AI.Tag.Production, AI.Tag.Science, AI.Tag.Culture });
        Metallurgy.Link(Machinery, 3);

        Technology Forestry = new Technology("Forestry", 180, new List<AI.Tag>() { AI.Tag.Food, AI.Tag.Mana, AI.Tag.Science });
        Heavy_Industry.Link(Forestry, 2);
        Forestry.Link(Topography);
        Technology Fertilizer = new Technology("Fertilizer", 300, new List<AI.Tag>());
        Forestry.Link(Fertilizer, 3);

        Technology Public_Services = new Technology("Public Services", 20, new List<AI.Tag>() { AI.Tag.Happiness, AI.Tag.Science });
        Kingdom.Root_Technology.Link(Public_Services, 6);
        Technology Education = new Technology("Education", 60, new List<AI.Tag>() { AI.Tag.Science, AI.Tag.Production });
        Public_Services.Link(Education, 3);
        Education.Link(Military_Science);
        
        Technology Chemistry = new Technology("Chemistry", 105, new List<AI.Tag>());
        Education.Link(Chemistry, 3);
        Chemistry.Link(Metallurgy);
        Technology Biology = new Technology("Biology", 200, new List<AI.Tag>());
        Chemistry.Link(Biology, 3);
        Biology.Link(Fertilizer);
        
        Technology Bureaucracy = new Technology("Bureaucracy", 115, new List<AI.Tag>() { AI.Tag.Cash });
        Education.Link(Bureaucracy, 4);
        Technology Economics = new Technology("Economics", 175, new List<AI.Tag>() { AI.Tag.Cash });
        Bureaucracy.Link(Economics, 3);
        Technology Mercantilism = new Technology("Mercantilism", 315, new List<AI.Tag>());
        Mercantilism.EmpireModifiers = new EmpireModifiers() {
            Passive_Income = 1.0f,
            Village_Yield_Bonus = new Yields(0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f)
        };
        Economics.Link(Mercantilism, 3);

        Technology Law_and_Order = new Technology("Law and Order", 140, new List<AI.Tag>() { AI.Tag.Order });
        Bureaucracy.Link(Law_and_Order, 4);
        Professional_Army.Link(Law_and_Order);
        
        Technology Mysticism = new Technology("Mysticism", 65, new List<AI.Tag>());
        Public_Services.Link(Mysticism, 5);
        Technology Theology = new Technology("Theology", 75, new List<AI.Tag>() { AI.Tag.Happiness, AI.Tag.Faith });
        Mysticism.Link(Theology, 4);
        Technology Theocracy = new Technology("Theocracy", 190, new List<AI.Tag>());
        Theology.Link(Theocracy, 3);
        
        Technology Wizardy = new Technology("Wizardy", 205, new List<AI.Tag>());
        Theology.Link(Wizardy, 4);
        Technology Geomancy = new Technology("Geomancy", 240, new List<AI.Tag>());
        Wizardy.Link(Geomancy, 2);
        Technology Destruction = new Technology("Destruction", 270, new List<AI.Tag>());
        Wizardy.Link(Destruction, 3);
        
        Technology Engineering = new Technology("Engineering", 75, new List<AI.Tag>());
        Construction.Link(Engineering, 5);
        Education.Link(Engineering);
        Technology Sanitation = new Technology("Sanitation", 190, new List<AI.Tag>() { AI.Tag.Health });
        Engineering.Link(Sanitation, 4);
        Technology Health_Care = new Technology("Health Care", 290, new List<AI.Tag>() { AI.Tag.Health });
        Sanitation.Link(Health_Care, 3);

        Technology Highways = new Technology("Highways", 165, new List<AI.Tag>() { AI.Tag.Happiness, AI.Tag.Cash });
        Highways.EmpireModifiers = new EmpireModifiers() {
            Percentage_Village_Yield_Bonus = new Yields(10.0f, 10.0f, 10.0f, 10.0f, 10.0f, 10.0f, 10.0f)
        };
        Engineering.Link(Highways, 3);
        Technology Diplomatic_Missions = new Technology("Diplomatic Missions", 300, new List<AI.Tag>() { AI.Tag.Culture });
        Highways.Link(Diplomatic_Missions, 3);

        Technology Acoustics = new Technology("Acoustics", 220, new List<AI.Tag>() { AI.Tag.Culture, AI.Tag.Happiness });
        Engineering.Link(Acoustics, 2);
        Technology Architecture = new Technology("Architecture", 325, new List<AI.Tag>() { AI.Tag.Production });
        Architecture.EmpireModifiers = new EmpireModifiers() {
            Building_Constuction_Speed_Bonus = 0.05f
        };
        Acoustics.Link(Architecture, 3);
        



        Kingdom.Improvements.Add(new Improvement("Farm", "farm", "farm_inactive", new Yields(2, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 10, 0, false, new List<string>() { "Grassland", "Plains", "Flower Field" }, null, null));
        Kingdom.Improvements.Add(new Improvement("Plantation", "plantation", "plantation_inactive", new Yields(0, 0, 1, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 13, 0, false, new List<string>() { "Grassland", "Flower Field" }, null, null));
        Kingdom.Improvements.Add(new Improvement("Hunting Lodge", "hunting_lodge", "hunting_lodge_inactive", new Yields(1.0f, 0.0f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 7, 0, false, HexPrototypes.Instance.All_Forests, null, null));
        Kingdom.Improvements.Add(new Improvement("Logging Camp", "logging_camp", "logging_camp_inactive", new Yields(0, 1, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 10, 0, false, HexPrototypes.Instance.All_Forests.Where(x => x != "Mushroom Forest").ToList(), null, null));
        Kingdom.Improvements.Add(new Improvement("Quarry", "quarry", "quarry_inactive", new Yields(0, 2, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 20, 0, false, new List<string>() { "Hill", "Plains" }, null, null));
        Kingdom.Improvements.Add(new Improvement("Mine", "mine", "mine_inactive", new Yields(-1, 1, 0, 0, 0, 0, 0), 0.0f, -0.5f, 0.0f, 17, 0, true, new List<string>() { "Hill", "Forest Hill", "Mountain", "Volcano" }, null, null));

        Kingdom.Improvements.Add(new Improvement("Windmill", "windmill", "windmill_inactive", new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 15, 0, false, new List<string>() { "Hill" }, Engineering, 
            delegate(Improvement improvement) {
                int adjancent_agriculture = 0;
                foreach(WorldMapHex hex in improvement.Hex.Get_Adjancent_Hexes()) {
                    if(hex.Improvement != null && hex.Owner != null && (hex.Improvement.Name == "Farm" || hex.Improvement.Name == "Plantation")) {
                        adjancent_agriculture++;
                    }
                }
                improvement.Special_Yield_Delta = new Yields();
                if(adjancent_agriculture >= 1) {
                    improvement.Special_Yield_Delta.Food += 1;
                }
                if (adjancent_agriculture >= 2) {
                    improvement.Special_Yield_Delta.Cash += 1;
                }
                if (adjancent_agriculture >= 3) {
                    improvement.Special_Yield_Delta.Production += 1;
                }
            }
        ));

        Kingdom.Improvements.Add(new Improvement("Lumber Mill", "large_hut", "large_hut_inactive", new Yields(-1, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 15, 0, false, HexPrototypes.Instance.All_Non_Structures, Construction,
            delegate (Improvement improvement) {
                int adjancent_lumber_camps = 0;
                foreach (WorldMapHex hex in improvement.Hex.Get_Adjancent_Hexes()) {
                    if (hex.Improvement != null && hex.Owner != null && hex.Improvement.Name == "Logging Camp") {
                        adjancent_lumber_camps++;
                    }
                }
                improvement.Special_Yield_Delta = new Yields();
                if (adjancent_lumber_camps >= 1) {
                    improvement.Special_Yield_Delta.Production += 1;
                }
                if (adjancent_lumber_camps >= 2) {
                    improvement.Special_Yield_Delta.Production += 1;
                }
                if (adjancent_lumber_camps >= 3) {
                    improvement.Special_Yield_Delta.Production += 1;
                    improvement.Special_Yield_Delta.Cash += 1;
                }
            }
        ));

        Kingdom.Improvements.Add(new Improvement("Forester's Lodge", "forester", "forester_inactive", new Yields(1, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 10, 0, false,
            HexPrototypes.Instance.All_Forests.Concat(new List<string>() { "Haunted Forest", "Swamp" }).ToList(), Forestry,
            delegate (Improvement improvement) {
                improvement.Special_Yield_Delta = new Yields();
                improvement.Health_Delta = 0.0f;
                switch (improvement.Hex.Terrain) {
                    case "Mushroom Forest":
                        improvement.Special_Yield_Delta = new Yields(0, 0, 0, 1, 0, 1, 0);
                        break;
                    case "Enchanted Forest":
                        improvement.Special_Yield_Delta = new Yields(0, 0, 0, 0, 0, 2, 0);
                        break;
                    case "Haunted Forest":
                        improvement.Special_Yield_Delta = new Yields(-1, 0, 0, 1, 0, 1, 0);
                        break;
                    case "Swamp":
                        improvement.Special_Yield_Delta = new Yields(0, 0, 1, 1, 0, 0, 0);
                        break;
                    default://Forest, forest hill
                        improvement.Health_Delta = 0.25f;
                        break;
                }
            }
        ));


        Kingdom.Buildings.Add(new Building("Royal Statue", "placeholder", 50, 100, 1.0f, new Yields(0, 0, 0, 0, 1, 0, 0), 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, null, null) {
            Village_Cultural_Influence = 1.0f
        });
        Kingdom.Buildings.Add(new Building("Workshop", "placeholder", 130, 85, 1.0f, new Yields(0, 2, 0, 1, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, null, null));
        Kingdom.Buildings.Add(new Building("Tavern", "placeholder", 100, 75, 2.0f, new Yields(-1, 0, 1, 0, 1, 0, 0), 2.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, true, null, null) {
            Cultural_Influence_Range = 1.0f
        });
        Kingdom.Buildings.Add(new Building("Market Square", "placeholder", 85, 40, 1.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, null, null) {
            Percentage_Yield_Bonuses = new Yields(10, 0, 10, 0, 0, 0, 0)
        });
        Kingdom.Buildings.Add(new Building("Barracks", "placeholder", 130, 60, 1.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, false, Conscription, null) {
            Tags = new Dictionary<AI.Tag, float>() { { AI.Tag.Military, 5.0f } }
        });
        Kingdom.Buildings.Add(new Building("Chapel", "placeholder", 160, 120, 1.0f, new Yields(0, 0, 0, 0, 0, 0, 1), 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Public_Services, null));
        Kingdom.Buildings.Add(new Building("Library", "placeholder", 145, 90, 2.0f, new Yields(0, 0, 0, 2, 1, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Public_Services, null) {
            Percentage_Yield_Bonuses = new Yields(0, 0, 0, 10, 0, 0, 0)
        });
        Kingdom.Buildings.Add(new Building("Mint", "placeholder", 145, 80, 1.0f, new Yields(0, 0, 3, 0, 0, 0, 0), 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Alloys,
            delegate(Building b) {
                int bonus_minerals = 0;
                foreach(WorldMapHex hex in b.City.Worked_Hexes) {
                    if(hex.Improvement != null && hex.Improvement.Extracts_Minerals && hex.Mineral != null &&
                        (hex.Mineral.Name == "Copper" || hex.Mineral.Name == "Silver" || hex.Mineral.Name == "Gold")) {
                        bonus_minerals++;
                    }
                }
                if(bonus_minerals != 0) {
                    b.Yield_Delta = new Yields(0, 0, bonus_minerals, 0, 0, 0, 0);
                } else {
                    b.Yield_Delta = null;
                }
            }
        ));
        Kingdom.Buildings.Add(new Building("Forge", "placeholder", 120, 100, 2.0f, new Yields(0, 3, 0, 0, 0, 0, 0), 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Metal_Casting,
            delegate (Building b) {
                int bonus_minerals = 0;
                foreach (WorldMapHex hex in b.City.Worked_Hexes) {
                    if (hex.Improvement != null && hex.Improvement.Extracts_Minerals && hex.Mineral != null &&
                        (hex.Mineral.Name == "Copper" || hex.Mineral.Name == "Iron" || hex.Mineral.Name == "Adamantine")) {
                        bonus_minerals++;
                    }
                }
                if (bonus_minerals != 0) {
                    b.Yield_Delta = new Yields(0, bonus_minerals, 0, 0, 0, 0, 0);
                } else {
                    b.Yield_Delta = null;
                }
            }
        ));
        Kingdom.Buildings.Add(new Building("School", "placeholder", 125, 70, 2.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Education,
            delegate (Building b) {
                float bonus_science = b.City.Population * 0.25f;
                b.Yield_Delta = new Yields(0.0f, 0.0f, 0.0f, bonus_science, 0.0f, 0.0f, 0.0f);
            }
        ) { Tags = new Dictionary<AI.Tag, float> { { AI.Tag.Science, 3.0f } } });
        Kingdom.Buildings.Add(new Building("Tax Office", "placeholder", 250, 145, 1.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Bureaucracy,
            delegate (Building b) {
                float bonus_cash = b.City.Population * 0.33f;
                b.Yield_Delta = new Yields(0.0f, 0.0f, bonus_cash, 0.0f, 0.0f, 0.0f, 0.0f);
            }
        ) { Tags = new Dictionary<AI.Tag, float> { { AI.Tag.Cash, 5.0f } } });
        Kingdom.Buildings.Add(new Building("Weapon Smithy", "placeholder", 190, 200, 2.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, -1.0f, 0.0f, 0.1f, 0.0f, 0.0f, false, Combined_Arms, null) {
            Garrison_Upkeep_Reduction = 0.1f
        });
        Kingdom.Buildings.Add(new Building("Stable", "placeholder", 140, 85, 1.0f, new Yields(-1, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Professional_Army, null) {
            Tags = new Dictionary<AI.Tag, float>() { { AI.Tag.Military, 5.0f } }
        });
        Kingdom.Buildings.Add(new Building("Church", "placeholder", 220, 175, 3.0f, new Yields(0, 0, 0, 0, 1, 0, 3), 3.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Theology, null));
        Kingdom.Buildings.Add(new Building("Inn", "placeholder", 270, 240, 4.0f, new Yields(-3, 0, 1, 0, 0, 0, 0), 4.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, true, Highways,
            delegate (Building b) {
                float bonus_cash = b.City.Yields.Culture * 0.50f;
                b.Yield_Delta = new Yields(0.0f, 0.0f, bonus_cash, 0.0f, 0.0f, 0.0f, 0.0f);
            }
        ) {
            Tags = new Dictionary<AI.Tag, float> { { AI.Tag.Cash, 5.0f } },
            Cultural_Influence_Range = 2.0f
        });
        Kingdom.Buildings.Add(new Building("Bank", "placeholder", 270, 290, 3.0f, new Yields(0, 0, 2, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Economics, null) {
            Percentage_Yield_Bonuses = new Yields(0, 0, 25, 0, 0, 0, 0)
        });
        Kingdom.Buildings.Add(new Building("Townhall", "placeholder", 150, 150, 1.0f, new Yields(0, 0, 0, 1, 0, 0, 0), 0.0f, 0.0f, 2.0f, 0.0f, 0.1f, 0.0f, true, Construction, null));
        Kingdom.Buildings.Add(new Building("Granary", "placeholder", 125, 110, 1.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Construction, null) {
            Food_Storage = 50,
            Pop_Growth_Additive_Bonus = 0.1f
        });
        Kingdom.Buildings.Add(new Building("Park", "placeholder", 90, 175, 1.0f, new Yields(0, 0, 0, 0, 1, 0, 0), 2.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Construction, null));
        Kingdom.Buildings.Add(new Building("Guardhouse", "placeholder", 290, 205, 2.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 3.0f, 0.0f, 0.0f, 0.0f, true, Law_and_Order, null));
        Kingdom.Buildings.Add(new Building("Theatre", "placeholder", 310, 275, 5.0f, new Yields(0, 0, 0, 0, 4, 0, 0), 5.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Acoustics, null));
        Kingdom.Buildings.Add(new Building("Sever System", "placeholder", 375, 200, 3.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Sanitation, null) {
            Base_Health_From_Pops_Delta = 0.05f
        });
        Kingdom.Buildings.Add(new Building("Military Academy", "placeholder", 330, 350, 4.0f, new Yields(0, 0, 0, 1, 1, 0, 0), 0.0f, 0.0f, 0.0f, 0.1f, 0.0f, 0.0f, false, Military_Science, null));
        Kingdom.Buildings.Add(new Building("Hospital", "placeholder", 375, 470, 5.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 4.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Health_Care, null) {
            Pop_Growth_Additive_Bonus = 0.25f,
            Pop_Growth_Multiplier_Bonus = 0.25f
        });
        Kingdom.Buildings.Add(new Building("Clock Tower", "placeholder", 550, 390, 1.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Machinery, null) {
            Percentage_Yield_Bonuses = new Yields(0, 5, 5, 5, 5, 0, 0),
            Building_Upkeep_Reduction = 0.1f
        });
        Kingdom.Buildings.Add(new Building("Chancery", "placeholder", 360, 500, 3.0f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Diplomatic_Missions, null) {
            Percentage_Yield_Bonuses = new Yields(0, 0, 0, 0, 25, 0, 0),
            Cultural_Influence_Range = 5.0f
        });

        Kingdom.Units.Add(new Worker("Peasant", 2.0f, 2, "peasant", new List<string>() { "peasant_working_1", "peasant_working_2" }, 3.0f, new List<Improvement>()
            { Kingdom.Improvements.First(x => x.Name == "Farm"), Kingdom.Improvements.First(x => x.Name == "Plantation"), Kingdom.Improvements.First(x => x.Name == "Hunting Lodge"),
            Kingdom.Improvements.First(x => x.Name == "Logging Camp"), Kingdom.Improvements.First(x => x.Name == "Quarry"), Kingdom.Improvements.First(x => x.Name == "Mine"),
            Kingdom.Improvements.First(x => x.Name == "Windmill"), Kingdom.Improvements.First(x => x.Name == "Lumber Mill"), Kingdom.Improvements.First(x => x.Name == "Forester's Lodge")},
            1.0f, 25, 15, 0.5f, null));

        Kingdom.Units.Add(new Prospector("Prospector", 2.0f, 2, "prospector", new List<string>() { "prospecting_1", "prospecting_2", "prospecting_3", "prospecting_4" }, 6.0f, 100, 50, 0.75f, Education, 10));

        ///Vanguard deployment units?
        Kingdom.Units.Add(new Unit("Scout", Unit.UnitType.Infantry, "scout", 3.0f, 50, 50, 0.5f, 3, null, null, 2.0f, true, 9.0f, 75.0f, 100.0f,
            9.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.5f }, { Unit.DamageType.Thrust, 0.5f } }, 0.1f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            10.0f, 7.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.5f }, { Unit.DamageType.Thrust, 0.75f }, { Unit.DamageType.Impact, 1.0f } }, 7.0f, 7.0f, new List<Ability>() {
                AbilityPrototypes.Instance.Get("forest combat bonus", 0.10f)
            }, new List<Unit.Tag>()));
        Kingdom.Units.Add(new Unit("Peasant Militia", Unit.UnitType.Infantry, "default_spear_unit_2", 2.0f, 35, 20, 0.25f, 2, null, new List<Building>(),
            2.0f, true, 10.0f, 50.0f, 90.0f,
            10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Thrust, 0.95f }, { Unit.DamageType.Slash, 0.05f } }, 0.25f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            7.0f, 4.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.50f }, { Unit.DamageType.Thrust, 0.75f }, { Unit.DamageType.Impact, 1.0f } }, 4.0f, 5.0f, new List<Ability>() {
                AbilityPrototypes.Instance.Get("anti cavalry", 0.20f),
                AbilityPrototypes.Instance.Get("urban combat bonus", 0.05f),
                AbilityPrototypes.Instance.Get("city defence bonus", 0.10f),
                AbilityPrototypes.Instance.Get("village defence bonus", 0.25f),
                AbilityPrototypes.Instance.Get("upkeep reduction on village", 1.00f)
            }, new List<Unit.Tag>()));
        Kingdom.Units.Add(new Unit("Levy Spearman", Unit.UnitType.Infantry, "default_spear_unit", 2.0f, 100, 75, 0.5f, 2, Conscription, new List<Building>() { Kingdom.Buildings.First(x => x.Name == "Barracks") },
            2.0f, true, 10.0f, 75.0f, 100.0f,
            10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Thrust, 0.95f }, { Unit.DamageType.Slash, 0.05f } }, 0.25f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            15.0f, 13.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.75f }, { Unit.DamageType.Thrust, 0.90f }, { Unit.DamageType.Impact, 1.0f } }, 7.0f, 8.0f, new List<Ability>() {
                AbilityPrototypes.Instance.Get("anti cavalry", 0.25f),
                AbilityPrototypes.Instance.Get("charge resistance", 0.25f)
            }, new List<Unit.Tag>() { Unit.Tag.Medium_Shields }));
        Kingdom.Units.Add(new Unit("Levy Archer", Unit.UnitType.Infantry, "default_bow_unit", 2.0f, 100, 75, 0.5f, 2, Conscription, new List<Building>() { Kingdom.Buildings.First(x => x.Name == "Barracks") },
            2.0f, true, 10.0f, 75.0f, 100.0f,
            8.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.5f }, { Unit.DamageType.Thrust, 0.5f } }, 0.10f,
            10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Thrust, 1.0f } }, 6, 20,
            8.0f, 5.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.50f }, { Unit.DamageType.Thrust, 0.75f }, { Unit.DamageType.Impact, 1.0f } }, 7.0f, 7.0f, new List<Ability>(), new List<Unit.Tag>()));
        Kingdom.Units.Add(new Unit("Kettlehat", Unit.UnitType.Infantry, "kettle_hat", 2.0f, 200, 200, 1.0f, 2, Professional_Army, new List<Building>() { Kingdom.Buildings.First(x => x.Name == "Barracks") },
            2.0f, true, 10.0f, 100.0f, 110.0f,
            15.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.75f }, { Unit.DamageType.Thrust, 0.25f } }, 0.25f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            20.0f, 22.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 1.05f }, { Unit.DamageType.Thrust, 1.10f }, { Unit.DamageType.Impact, 0.90f } }, 10.0f, 10.0f, new List<Ability>(),
            new List<Unit.Tag>() { Unit.Tag.Medium_Shields }));
        Kingdom.Units.Add(new Unit("Longbowman", Unit.UnitType.Infantry, "default_bow_unit", 2.0f, 190, 225, 1.0f, 2, Professional_Army, new List<Building>() { Kingdom.Buildings.First(x => x.Name == "Barracks") },
            2.0f, true, 10.0f, 100.0f, 100.0f,
            11.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.75f }, { Unit.DamageType.Thrust, 0.25f } }, 0.25f,
            12.0f, new Dictionary<Unit.DamageType, float>(), 8, 20,
            12.0f, 10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.90f }, { Unit.DamageType.Thrust, 0.80f }, { Unit.DamageType.Impact, 1.0f } }, 10.0f, 10.0f, new List<Ability>(),
            new List<Unit.Tag>() { Unit.Tag.Small_Shields }));
        Kingdom.Units.Add(new Unit("Outrider", Unit.UnitType.Cavalry, "default_unit", 4.0f, 175, 250, 1.5f, 2, Professional_Army, new List<Building>() { Kingdom.Buildings.First(x => x.Name == "Barracks") },
            2.0f, true, 5.0f, 110.0f, 100.0f,
            13.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.75f }, { Unit.DamageType.Thrust, 0.25f } }, 0.50f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            10.0f, 10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 1.05f }, { Unit.DamageType.Thrust, 0.90f }, { Unit.DamageType.Impact, 1.0f } }, 10.0f, 9.0f, new List<Ability>() {
                AbilityPrototypes.Instance.Get("lance charge", 0.50f)
            },
            new List<Unit.Tag>()));
        /*Kingdom.Units.Add(new Unit("Dummy", Unit.UnitType.Infantry, "default_unit", 2.0f, 10, 10, 0.1f, 5, Acoustics, new List<Building>(),
            2.0f, true, 0.0f, 0.0f,
            10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Impact, 1.00f } }, 0.25f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            10.0f, 10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 1.00f }, { Unit.DamageType.Thrust, 1.00f }, { Unit.DamageType.Impact, 1.00f } }, 10.0f, 10.0f, new List<Ability>(),
            new List<Unit.Tag>()));
        Kingdom.Units.Add(new Unit("Dummy2", Unit.UnitType.Infantry, "default_unit", 2.0f, 10, 10, 0.1f, 5, Acoustics, new List<Building>(),
            2.0f, true, 0.0f, 0.0f,
            10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Impact, 1.00f } }, 0.25f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            10.0f, 10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 1.00f }, { Unit.DamageType.Thrust, 1.00f }, { Unit.DamageType.Impact, 1.00f } }, 10.0f, 10.0f, new List<Ability>() {
                AbilityPrototypes.Instance.Get("urban combat bonus", 0.50f)
            }, new List<Unit.Tag>()));
        Kingdom.Units.Add(new Unit("Dummy3", Unit.UnitType.Infantry, "default_unit", 2.0f, 10, 10, 0.1f, 5, Acoustics, new List<Building>(),
            2.0f, true, 0.0f, 0.0f,
            10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Impact, 1.00f } }, 0.25f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            10.0f, 10.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 1.00f }, { Unit.DamageType.Thrust, 1.00f }, { Unit.DamageType.Impact, 1.00f } }, 10.0f, 10.0f, new List<Ability>() {
                AbilityPrototypes.Instance.Get("city attack bonus", 0.50f)
            }, new List<Unit.Tag>()));*/



        neutral_cities = new Faction("Neutral Cities", 1000, 1, new Dictionary<City.CitySize, Yields>() {
                { City.CitySize.Town, new Yields(2, 1, 2, 1, 1, 0, 0) },
                { City.CitySize.City, new Yields(1, 2, 3, 2, 1, 0, 0) },
                { City.CitySize.Metropolis, new Yields(0, 2, 5, 3, 2, 0, 0) }
            }, 3.0f, 100, 1.0f, -0.40f, 1.0f, -0.30f, 1.5f, -0.20f, 1.0f,
            new Technology("Root", 5, new List<AI.Tag>()), new Army("Garrison", "default_unit", 100), new EmpireModifiers());

        neutral_cities.Units.Add(new Unit("Town Guard", Unit.UnitType.Infantry, "town_guard", 2.0f, 200, 200, 0.25f, 2, null, null,
            2.0f, true, 10.0f, 100.0f, 100.0f,
            13.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Thrust, 0.60f }, { Unit.DamageType.Slash, 0.35f }, { Unit.DamageType.Impact, 0.05f } }, 0.15f,
            0.0f, new Dictionary<Unit.DamageType, float>(), 0, 0,
            10.0f, 6.0f, new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Slash, 0.75f }, { Unit.DamageType.Thrust, 0.90f }, { Unit.DamageType.Impact, 1.0f } }, 7.0f, 8.0f, new List<Ability>() {
                AbilityPrototypes.Instance.Get("anti cavalry", 0.15f),
                AbilityPrototypes.Instance.Get("charge resistance", 0.25f),
                AbilityPrototypes.Instance.Get("urban combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("city defence bonus", 0.35f)
            }, new List<Unit.Tag>()));


        initialized = true;
    }
}
