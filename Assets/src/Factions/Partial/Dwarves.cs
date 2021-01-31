using System.Collections.Generic;
using System.Linq;

public partial class Factions
{
    private static Faction Dwarves()
    {
        Faction faction = new Faction("Underearth Realm", 200, 1, new Dictionary<City.CitySize, Yields>() {
            { City.CitySize.Town,       new Yields(1.0f, 2.0f, 2.0f, 1.0f, 0.5f, 0.0f, 0.0f) },
            { City.CitySize.City,       new Yields(0.0f, 3.0f, 3.0f, 2.0f, 0.5f, 0.0f, 0.0f) },
            { City.CitySize.Metropolis, new Yields(0.0f, 4.0f, 5.0f, 3.0f, 1.0f, 0.0f, 0.0f) }
        }, 3.0f, 100, 2.0f, -0.40f, 1.0f, -0.15f, 1.5f, -0.40f, 1.0f, false, "dwarven city",
        new Technology(null, "Root", 5, new List<AI.Tag>()), new Army("Army", "dwarf_flag_bearer", "ship_2", 10, new List<string>() { "kingdom_tent_1", "kingdom_tent_2", "kingdom_tent_3" }, 5.0f), new EmpireModifiers() {
            Passive_Income = 3.0f,
            Max_Mana = 100.0f
        });

        Technology Advanced_Tunneling = new Technology(faction, "Advanced Tunneling", 30, new List<AI.Tag>() { });
        faction.Root_Technology.Link(Advanced_Tunneling, 1);

        Technology Reinforced_Shafts = new Technology(faction, "Reinforced Shafts", 45, new List<AI.Tag>() { });
        Advanced_Tunneling.Link(Reinforced_Shafts, 3);

        Technology Mechanization = new Technology(faction, "Mechanization", 105, new List<AI.Tag>() { });
        Reinforced_Shafts.Link(Mechanization, 3);

        Technology Siege_Engineering = new Technology(faction, "Siege Engineering", 175, new List<AI.Tag>() { });
        Mechanization.Link(Siege_Engineering, 4);

        Technology Minecarts = new Technology(faction, "Minecarts", 220, new List<AI.Tag>() { });
        Mechanization.Link(Minecarts, 3);

        Technology Elevators = new Technology(faction, "Elevators", 420, new List<AI.Tag>() { });
        Minecarts.Link(Elevators, 3);

        Technology Mass_Smelting = new Technology(faction, "Mass Smelting", 400, new List<AI.Tag>() { });
        Minecarts.Link(Mass_Smelting, 4);

        Technology Magma_Industry = new Technology(faction, "Magma Industry", 800, new List<AI.Tag>() { });
        Elevators.Link(Magma_Industry, 3);
        Mass_Smelting.Link(Magma_Industry);

        Technology Sophisticated_Masonry = new Technology(faction, "Sophisticated Masonry", 60, new List<AI.Tag>() { });
        Sophisticated_Masonry.EmpireModifiers = new EmpireModifiers() { Building_Constuction_Speed_Bonus = 0.05f };
        Advanced_Tunneling.Link(Sophisticated_Masonry, 5);

        Technology Aboveground_Construction = new Technology(faction, "Aboveground Construction", 90, new List<AI.Tag>() { });
        Sophisticated_Masonry.Link(Aboveground_Construction, 3);
        Aboveground_Construction.EmpireModifiers = new EmpireModifiers() { Improvement_Constuction_Speed_Bonus = 0.1f };

        Technology Great_Halls = new Technology(faction, "Great Halls", 200, new List<AI.Tag>() { });
        Aboveground_Construction.Link(Great_Halls, 3);
        //Reinforced_Shafts.Link(Great_Halls);TODO: Tech tree bugs out with this connection

        Technology Artifact_Reverence = new Technology(faction, "Artifact Reverence", 450, new List<AI.Tag>() { });
        Great_Halls.Link(Artifact_Reverence, 3);

        Technology Deep_Mines = new Technology(faction, "Deep Mines", 400, new List<AI.Tag>() { });
        Minecarts.Link(Deep_Mines, 2);
        Deep_Mines.Link(Magma_Industry);

        Technology Reservoirs = new Technology(faction, "Reservoirs", 55, new List<AI.Tag>() { });
        Advanced_Tunneling.Link(Reservoirs, 1);

        Technology Advanced_Waterworks = new Technology(faction, "Advanced Waterworks", 100, new List<AI.Tag>() { });
        Reservoirs.Link(Advanced_Waterworks, 3);

        Technology Ventilation = new Technology(faction, "Ventilation", 105, new List<AI.Tag>() { });
        Reinforced_Shafts.Link(Ventilation, 2);

        Technology Hospitalization = new Technology(faction, "Hospitalization", 175, new List<AI.Tag>() { });
        Advanced_Waterworks.Link(Hospitalization, 3);
        Ventilation.Link(Hospitalization);

        Technology Noble_Traditions = new Technology(faction, "Noble Traditions", 35, new List<AI.Tag>() { });
        faction.Root_Technology.Link(Noble_Traditions, 4);

        Technology Sheriffs = new Technology(faction, "Sheriffs", 50, new List<AI.Tag>() { });
        Noble_Traditions.Link(Sheriffs, 3);

        Technology Mayors = new Technology(faction, "Mayors", 60, new List<AI.Tag>() { });
        Noble_Traditions.Link(Mayors, 5);

        Technology Fortress_Guard = new Technology(faction, "Fortress Guard", 115, new List<AI.Tag>() { });
        Sheriffs.Link(Fortress_Guard, 3);
        Mayors.Link(Fortress_Guard);

        Technology Hammerers = new Technology(faction, "Hammerers", 200, new List<AI.Tag>() { });
        Fortress_Guard.Link(Hammerers, 3);

        Technology Extensive_Militia_Forces = new Technology(faction, "Extensive Militia Forces", 70, new List<AI.Tag>() { });
        Sheriffs.Link(Extensive_Militia_Forces, 4);

        Technology Champions = new Technology(faction, "Champions", 100, new List<AI.Tag>() { });
        Extensive_Militia_Forces.Link(Champions, 3);

        Technology Barony = new Technology(faction, "Barony", 100, new List<AI.Tag>() { });
        Mayors.Link(Barony, 4);

        Technology County = new Technology(faction, "County", 200, new List<AI.Tag>() { });
        Barony.Link(County, 3);

        Technology Dukedom = new Technology(faction, "Dukedom", 400, new List<AI.Tag>() { });
        County.Link(Dukedom, 3);

        faction.Improvements.Add(new Improvement(faction, "Aboveground Farm", "dwarven_farm_2", "dwarven_farm_inactive_2", new Yields(1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 11, 0, false, new List<string>() { "Grassland", "Plains", "Flower Field", "Hill" }, null, null));
        faction.Improvements.Add(new Improvement(faction, "Logging Camp", "logging_camp_2", "logging_camp_inactive_2", new Yields(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 9, 0, false, HexPrototypes.Instance.Get_Names(new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Timber }), null, null));
        faction.Improvements.Add(new Improvement(faction, "Tunnel", "tunnel_2", "tunnel_inactive_2", new Yields(0.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, -0.5f, 19, 0, true, HexPrototypes.Instance.All_Non_Structures.Concat(new List<string>() { "Mountain", "Volcano" }).ToList(), null, null));
        faction.Improvements.Add(new Improvement(faction, "Deep Mine", "deep_mine", "deep_mine_inactive", new Yields(-1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, -1.0f, 0.0f, 25, 0, true, HexPrototypes.Instance.Get_Names((WorldMapHex.Tag?)null, null, true), Deep_Mines, delegate (Improvement improvement) {
            if (improvement.Hex.Mineral != null) {
                improvement.Special_Yield_Delta = new Yields(0.0f, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f);
            } else {
                improvement.Special_Yield_Delta = new Yields();
            }
        }));


        faction.Improvements.Add(new Improvement(faction, "Lumber Mill", "large_hut_2", "large_hut_inactive_2", new Yields(-1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 13, 0, false, HexPrototypes.Instance.All_Non_Structures, Mechanization,
            delegate (Improvement improvement) {
                int adjancent_lumber_camps = 0;
                foreach (WorldMapHex hex in improvement.Hex.Get_Adjancent_Hexes()) {
                    if (hex.Improvement != null && hex.Owner != null && hex.Improvement.Name == "Logging Camp") {
                        adjancent_lumber_camps++;
                    }
                }
                improvement.Special_Yield_Delta = new Yields();
                if (adjancent_lumber_camps >= 1) {
                    improvement.Special_Yield_Delta.Production += 2.0f;
                }
                if (adjancent_lumber_camps >= 2) {
                    improvement.Special_Yield_Delta.Production += 1.0f;
                }
                if (adjancent_lumber_camps >= 3) {
                    improvement.Special_Yield_Delta.Production += 1.0f;
                    improvement.Special_Yield_Delta.Cash += 1.0f;
                }
            }
        ));

        faction.Improvements.Add(new Improvement(faction, "Magma Extractor", "tunnel_2_stone", "tunnel_2_stone_inactive", new Yields(0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f), 0.0f, -1.0f, 0.0f, 50, 0, false, new List<string>() { "Volcano" }, Magma_Industry, null));

        faction.Buildings.Add(new Building("Mushroom Farm", "placeholder", 120, 50, 0.5f, new Yields(2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, null, delegate (Building building) {
            building.Yield_Delta = new Yields();
            if (building.City.Buildings.Any(x => x.Name == "Irrigation Tunnels")) {
                building.Yield_Delta.Food = 1;
            }
        }));
        faction.Buildings.Add(new Building("Workshop", "placeholder", 110, 85, 1.0f, new Yields(0.0f, 2.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, null, null));
        faction.Buildings.Add(new Building("Mushroom Distillery", "placeholder", 80, 95, 1.0f, new Yields(-1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 2.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, true, null, null));

        faction.Buildings.Add(new Building("Barracks", "placeholder", 65, 80, 1.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, false, null, null));
        faction.Buildings.Add(new Building("Siege Workshop", "placeholder", 150, 160, 2.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Mechanization, null));

        faction.Buildings.Add(new Building("Irrigation Tunnels", "placeholder", 200, 50, 0.25f, new Yields(1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Advanced_Tunneling, delegate (Building building) {
            building.Yield_Delta = new Yields();
            if (building.City.Is_Coastal || building.City.Buildings.Any(x => x.Name == "Reservoir")) {
                building.Yield_Delta.Food = 1;
            }
        }));
        faction.Buildings.Add(new Building("Dining Hall", "placeholder", 250, 100, 1.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f), 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Advanced_Tunneling, null) {
            Percentage_Yield_Bonuses = new Yields(10.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f),
            Food_Storage = 100
        });
        faction.Buildings.Add(new Building("Reservoir", "placeholder", 250, 75, 0.5f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Reservoirs, null) {
            Percentage_Yield_Bonuses = new Yields(10.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)
        });
        faction.Buildings.Add(new Building("Baths", "placeholder", 210, 230, 2.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 2.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Advanced_Waterworks, null));
        faction.Buildings.Add(new Building("Hospital", "placeholder", 375, 390, 3.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 3.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Hospitalization, null) {
            Pop_Growth_Additive_Bonus = 0.10f,
            Pop_Growth_Multiplier_Bonus = 0.10f
        });
        faction.Buildings.Add(new Building("Temple", "placeholder", 190, 255, 3.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 2.0f), 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Sophisticated_Masonry, null));
        faction.Buildings.Add(new Building("Ventilation System", "placeholder", 225, 100, 0.5f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Ventilation, null) {
            Health_Penalty_From_Buildings_Multiplier = -0.1f
        });

        faction.Buildings.Add(new Building("Mechanist's Foundry", "placeholder", 235, 255, 3.0f, new Yields(0.0f, 4.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f), 0.0f, -2.0f, 0.0f, 0.0f, 0.05f, 0.05f, true, Mechanization,
            delegate (Building b) {
                int bonus_industrial_minerals = b.City.Worked_Improvements.Count(x => x.Extracts_Minerals && x.Hex.Has_Mineral(Mineral.Tag.Smeltable, Mineral.Tag.Industrial));
                Building charcoal_kiln = b.City.Buildings.FirstOrDefault(x => x.Name == "Charcoal Kiln");
                float charcoal_prod_bonus = 0.0f;
                if (charcoal_kiln != null && !charcoal_kiln.Yield_Delta.Empty) {
                    charcoal_prod_bonus = charcoal_kiln.Yield_Delta.Production;
                }
                b.Yield_Delta = new Yields(0.0f, bonus_industrial_minerals + (0.5f * charcoal_prod_bonus), (charcoal_prod_bonus == 0.0f ? 0.0f : 1.0f), 1.0f, 0.0f, 0.0f, 0.0f);
            }
        ));

        faction.Buildings.Add(new Building("Charcoal Kiln", "placeholder", 130, 60, 1.0f, new Yields(0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, -3.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Reinforced_Shafts,
            delegate (Building b) {
                int logging_camps = b.City.Worked_Improvements.Count(x => x.Name == "Logging Camp");
                b.Yield_Delta = new Yields(0.0f, 0.5f * logging_camps, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
            }
        ) { Update_Priority = 1 });

        faction.Buildings.Add(new Building("Centralized Stairs", "placeholder", 165, 95, 0.5f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Reinforced_Shafts, null) {
            Percentage_Yield_Bonuses = new Yields(5.0f, 5.0f, 5.0f, 0.0f, 0.0f, 0.0f, 0.0f),
            Building_Upkeep_Reduction = 0.05f
        });
        faction.Buildings.Add(new Building("Minecart Network", "placeholder", 265, 250, 3.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.25f, true, Minecarts, null) {
            Percentage_Yield_Bonuses = new Yields(0.0f, 15.0f, 10.0f, 0.0f, 0.0f, 0.0f, 0.0f)
        });
        faction.Buildings.Add(new Building("Elevators", "placeholder", 225, 350, 3.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Elevators, null) {
            Percentage_Yield_Bonuses = new Yields(5.0f, 10.0f, 10.0f, 5.0f, 0.0f, 0.0f, 0.0f),
            Building_Upkeep_Reduction = 0.1f
        });

        faction.Buildings.Add(new Building("Aboveground Dwellings", "placeholder", 150, 160, 1.0f, new Yields(0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.0f, 0.0f), 0.0f, 0.0f, -1.0f, 0.0f, 0.0f, 0.0f, false, Aboveground_Construction, null) {
            Pop_Growth_Additive_Bonus = 0.1f
        });

        //TODO: Def bonus?
        faction.Buildings.Add(new Building("Aboveground Towers", "placeholder", 200, 165, 1.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.05f, false, Aboveground_Construction, null) {
            Garrison_Upkeep_Reduction = 0.1f,
            Trade_Value = 0.5f
        });

        faction.Buildings.Add(new Building("Great Meeting Hall", "placeholder", 330, 310, 1.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f), 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Great_Halls, null) {
            Cultural_Influence_Range = 1.0f,
            Trade_Value = 1.0f
        });

        faction.Buildings.Add(new Building("Catacombs", "placeholder", 290, 270, 1.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f), 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, Great_Halls, null));
        faction.Buildings.Add(new Building("Library", "placeholder", 300, 290, 2.0f, new Yields(0.0f, 0.0f, 0.0f, 3.0f, 0.0f, 1.0f, 0.0f), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Great_Halls, null) {
            Percentage_Yield_Bonuses = new Yields(0.0f, 0.0f, 0.0f, 10.0f, 0.0f, 0.0f, 0.0f)
        });
        faction.Buildings.Add(new Building("Museum", "placeholder", 410, 520, 3.0f, new Yields(0.0f, 0.0f, 0.0f, 2.0f, 1.0f, 2.0f, 1.0f), 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Artifact_Reverence, null) {
            Percentage_Yield_Bonuses = new Yields(0.0f, 0.0f, 0.0f, 5.0f, 5.0f, 0.0f, 0.0f),
            Max_Mana = 200.0f
        });

        faction.Buildings.Add(new Building("Great Foundry", "placeholder", 400, 400, 3.0f, new Yields(0.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, -3.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Mass_Smelting,
            delegate (Building b) {
                int bonus_industrial_minerals = b.City.Worked_Improvements.Count(x => x.Extracts_Minerals && x.Hex.Has_Mineral(Mineral.Tag.Smeltable, Mineral.Tag.Industrial));
                int bonus_precious_minerals = b.City.Worked_Improvements.Count(x => x.Extracts_Minerals && x.Hex.Has_Mineral(Mineral.Tag.Smeltable, Mineral.Tag.Precious));
                Building charcoal_kiln = b.City.Buildings.FirstOrDefault(x => x.Name == "Charcoal Kiln");
                float charcoal_prod_bonus = 0.0f;
                if (charcoal_kiln != null && !charcoal_kiln.Yield_Delta.Empty) {
                    charcoal_prod_bonus = charcoal_kiln.Yield_Delta.Production;
                }
                b.Yield_Delta = new Yields(0.0f, (2.0f * bonus_industrial_minerals) + (0.5f * charcoal_prod_bonus), bonus_precious_minerals + (charcoal_prod_bonus == 0.0f ? 0.0f : 1.0f), 0.0f, 0.0f, 0.0f, 0.0f);
            }
        ));

        faction.Buildings.Add(new Building("Magma Foundry", "placeholder", 700, 600, 5.0f, new Yields(0.0f, 2.0f, 0.0f, 2.0f, 0.0f, 0.0f, 0.0f), 0.0f, -4.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Magma_Industry,
            delegate (Building b) {
                int bonus_minerals = b.City.Worked_Improvements.Count(x => x.Extracts_Minerals && x.Hex.Has_Mineral(Mineral.Tag.Smeltable, Mineral.Tag.Industrial));
                int bonus_precious_minerals = b.City.Worked_Improvements.Count(x => x.Extracts_Minerals && x.Hex.Has_Mineral(Mineral.Tag.Smeltable, Mineral.Tag.Precious));
                int bonus_volcanos = b.City.Worked_Improvements.Count(x => x.Name == "Magma Extractor");
                b.Yield_Delta = new Yields(0.0f, bonus_minerals + bonus_volcanos, bonus_precious_minerals, bonus_volcanos, 0.0f, 0.0f, 0.0f);
            }
        ));

        faction.Buildings.Add(new Building("Sheriff's Office", "placeholder", 90, 150, 1.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 2.0f, 0.0f, 0.0f, 0.0f, true, Sheriffs, null));
        faction.Buildings.Add(new Building("Mayor's Office", "placeholder", 130, 200, 2.0f, new Yields(0.0f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f), -1.0f, 0.0f, 1.0f, 0.0f, 0.1f, 0.0f, true, Mayors, null));
        faction.Buildings.Add(new Building("Captain of the Guard's Office", "placeholder", 140, 175, 2.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 3.0f, 0.0f, 0.0f, 0.0f, true, Fortress_Guard, null));
        faction.Buildings.Add(new Building("Baron's Office", "placeholder", 200, 250, 3.0f, new Yields(0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f), -2.0f, 0.0f, 2.0f, 0.0f, 0.0f, 0.0f, true, Barony, null) {
            Village_Cultural_Influence = 2.0f
        });
        faction.Buildings.Add(new Building("Count's Office", "placeholder", 350, 450, 4.0f, new Yields(0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f), -3.0f, 0.0f, 3.0f, 0.05f, 0.05f, 0.05f, true, County, null) {
            Garrison_Upkeep_Reduction = 0.05f
        });
        faction.Buildings.Add(new Building("Duke's Office", "placeholder", 350, 450, 4.0f, new Yields(0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f, 0.0f), -4.0f, 0.0f, 4.0f, 0.0f, 0.0f, 0.0f, true, Dukedom, null) {
            Trade_Value = 2.0f,
            Percentage_Yield_Bonuses = new Yields(0.0f, 0.0f, 10.0f, 0.0f, 10.0f, 0.0f, 10.0f)
        });

        //Spells
        //Whispers of earth
        //Reveals random nearby hill, mountains, volcanoes Prospects?
        faction.Spells.Add(new Spell("Whispers of Earth", 30.0f, 10, null, false, delegate (Spell spell, Player caster, WorldMapHex hex) {

            return new Spell.SpellResult();
        }) {
            Advanced_AI_Casting_Guidance = delegate (Spell spell, Player caster, Dictionary<AI.Tag, float> priorities) {
                if (Main.Instance.Round > 100) {
                    return null;
                }
                AI.SpellPreference preference = new AI.SpellPreference();
                preference.Spell = spell;
                preference.Preference = 15.0f + (50.0f / (Main.Instance.Round + 10.0f));
                preference.Target = null;
                return preference;
            }
        });

        //Raise hills
        //Can only be used on own city's radious
        //Can't be used next to hill / forested hill
        //Turns hex into hill / forested hill

        //Eruption
        //Turns montain into a volcano
        //can't be used next to another volcano
        //Can only be used on own city's radious

        //Blessings
        //Inspire artisan, after random amount of turns +production on citys building (and unit?)
        //On random city?
        faction.Blessings.Add(new Blessing("Inspire Artisan", 1.0f, 16, 5, null, delegate (Blessing blessing, Player caster) {
            caster.Store_Temp_Data(blessing.ToString(), 0.0f);
            return new Blessing.BlessingResult();
        }, delegate (Blessing blessing, Player caster, bool interrupted) {
            float potency = caster.Get_Temp_Data<float>(blessing.ToString(), true);
            if (!interrupted) {
                City random_city = RNG.Instance.Random_Item(caster.Cities);
                float production_multiplier = 0.1f * RNG.Instance.Next(75, 125);
                float culture_multiplier = 0.01f * RNG.Instance.Next(75, 125);
                float mana_multiplier = 0.01f * RNG.Instance.Next(75, 125);
                if (random_city.Building_Under_Production != null) {
                    random_city.Building_Production_Acquired += potency * production_multiplier;
                }
                random_city.Apply_Status_Effect(new CityStatusEffect(blessing.Name, 1) {
                    Yield_Delta = new Yields(0.0f, 0.0f, 0.0f, 0.0f, potency * culture_multiplier, potency * mana_multiplier, 0.0f)
                }, true);
                return new Blessing.BlessingResult(true, null, random_city.Hex);
            }
            return new Blessing.BlessingResult();
        }, delegate (Blessing blessing, Player caster, int turns_left) {
            float current_potency = caster.Get_Temp_Data<float>(blessing.ToString());
            current_potency += caster.Faith_Income;
            caster.Store_Temp_Data(blessing.ToString(), current_potency);
            return new Blessing.BlessingResult();
        }) {
            AI_Casting_Guidance = new Blessing.AIBlessingCastingGuidance(Blessing.AIBlessingCastingGuidance.TargetType.Caster, null, new Dictionary<AI.Tag, float>() { { AI.Tag.Production, 10.0f }, { AI.Tag.Culture, 5.0f }, { AI.Tag.Mana, 5.0f } })
        });


        faction.Units.Add(new Worker("Miner", 2.0f, Map.MovementType.Land, 2, "dwarf_miner", new List<string>() { "dwarf_working_1", "dwarf_working_2", "dwarf_working_3" }, 5.0f, new List<Improvement>()
            { faction.Improvements.First(x => x.Name == "Aboveground Farm"), faction.Improvements.First(x => x.Name == "Logging Camp"), faction.Improvements.First(x => x.Name == "Tunnel"),
            faction.Improvements.First(x => x.Name == "Deep Mine")},
            1.0f, 35, 20, 0.75f, null, 0.25f));
        faction.Units.Add(new Prospector("Wanderer", 2.0f, 3, "dwarf_wanderer", new List<string>() { "prospecting_1", "prospecting_2", "prospecting_3", "prospecting_4" }, 6.0f,
            10, 20, 0.5f, null, 3, 0.25f));
        faction.Units.Add(new Unit("Rabble", Unit.UnitType.Infantry, "dwarf_rabble", 2.0f, 45, 25, 0.5f, 0, 0.0f, 2, null, null, 2.0f, true, 10.0f, 75.0f, 150.0f,
            new Damage(7.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Thrust, 0.95f }, { Damage.Type.Impact, 0.05f } }), 0.25f,
            new Damage(3.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Impact, 1.0f } }), 3, 5, "stones", null,
            11.0f, 7.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.5f }, { Damage.Type.Thrust, 0.75f }, { Damage.Type.Earth, 1.25f }, { Damage.Type.Electric, 1.05f }, { Damage.Type.Fire, 1.10f } },
            4.0f, 4.0f, Unit.ArmorType.Unarmoured, new List<Ability>() {
                AbilityPrototypes.Instance.Get("urban combat bonus", 0.05f),
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.15f),
                AbilityPrototypes.Instance.Get("underground combat bonus", 0.25f),
                AbilityPrototypes.Instance.Get("city attack bonus", 0.10f),
                AbilityPrototypes.Instance.Get("armor piercing", 0.25f),
                AbilityPrototypes.Instance.Get("shield piercing", 0.10f),
                AbilityPrototypes.Instance.Get("no high shots")
            }, new List<Unit.Tag>()));
        faction.Units.Add(new Unit("Militia", Unit.UnitType.Infantry, "dwarf_militia_2", 2.0f, 80, 70, 1.0f, 0, 0.0f, 2, null, null, 2.0f, true, 10.0f, 85.0f, 150.0f,
            new Damage(6.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.5f }, { Damage.Type.Thrust, 0.5f } }), 0.1f,
            null, 0, 0, null, null,
            14.0f, 16.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.9f }, { Damage.Type.Thrust, 1.0f }, { Damage.Type.Earth, 1.25f }, { Damage.Type.Electric, 1.05f }, { Damage.Type.Fire, 1.10f } },
            8.0f, 7.0f, Unit.ArmorType.Light, new List<Ability>() {
                AbilityPrototypes.Instance.Get("urban combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("underground combat bonus", 0.25f),
                AbilityPrototypes.Instance.Get("city defence bonus", 0.25f),
                AbilityPrototypes.Instance.Get("increases order", 0.25f)
            }, new List<Unit.Tag>() { Unit.Tag.Medium_Shields }));
        faction.Units.Add(new Unit("Warrior", Unit.UnitType.Infantry, "dwarf_warrior_2", 2.0f, 300, 225, 1.75f, 0, 0.0f, 2, Extensive_Militia_Forces, new List<Building>() { faction.Buildings.First(x => x.Name == "Barracks") },
            2.0f, true, 10.0f, 150.0f, 200.0f,
            new Damage(16.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.9f }, { Damage.Type.Impact, 0.1f } }), 0.25f,
            null, 0, 0, null, null,
            27.0f, 27.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 1.1f }, { Damage.Type.Thrust, 0.9f }, { Damage.Type.Earth, 1.25f }, { Damage.Type.Electric, 1.05f }, { Damage.Type.Fire, 1.10f } },
            11.0f, 8.0f, Unit.ArmorType.Medium, new List<Ability>() {
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("underground combat bonus", 0.25f),
                AbilityPrototypes.Instance.Get("armor piercing", 0.10f),
                AbilityPrototypes.Instance.Get("shield piercing", 0.20f),
                AbilityPrototypes.Instance.Get("anti wooden", 0.25f)
            }, new List<Unit.Tag>() { Unit.Tag.Medium_Shields }));
        faction.Units.Add(new Unit("Marksdwarf", Unit.UnitType.Infantry, "marksdwarf", 2.0f, 285, 240, 1.5f, 0, 0.0f, 2, Extensive_Militia_Forces, new List<Building>() { faction.Buildings.First(x => x.Name == "Barracks") },
            2.0f, true, 10.0f, 125.0f, 150.0f,
            new Damage(11.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.9f }, { Damage.Type.Impact, 0.1f } }), 0.25f,
            new Damage(14.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Thrust, 1.0f } }), 6, 20, null, null,
            15.0f, 12.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 1.1f }, { Damage.Type.Thrust, 0.85f }, { Damage.Type.Earth, 1.25f }, { Damage.Type.Electric, 1.05f }, { Damage.Type.Fire, 1.10f } },
            11.0f, 7.0f, Unit.ArmorType.Medium, new List<Ability>() {
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("underground combat bonus", 0.25f),
                AbilityPrototypes.Instance.Get("armor piercing", 0.05f),
                AbilityPrototypes.Instance.Get("shield piercing", 0.10f),
                AbilityPrototypes.Instance.Get("armor piercing ranged", 0.25f),
                AbilityPrototypes.Instance.Get("straight shot bonus", 0.25f),
                AbilityPrototypes.Instance.Get("anti wooden", 0.10f)
            }, new List<Unit.Tag>()));
        faction.Units.Add(new Unit("Fortress Guard", Unit.UnitType.Infantry, "fortress_guard", 2.0f, 290, 265, 1.75f, 0, 0.0f, 2, Fortress_Guard, new List<Building>() { faction.Buildings.First(x => x.Name == "Captain of the Guard's Office") },
            2.0f, true, 10.0f, 150.0f, 200.0f,
            new Damage(12.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Impact, 1.0f } }), 0.25f,
            new Damage(8.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Thrust, 1.0f } }), 5, 20, null, null,
            25.0f, 27.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 1.1f }, { Damage.Type.Thrust, 0.9f }, { Damage.Type.Earth, 1.25f }, { Damage.Type.Electric, 1.05f }, { Damage.Type.Fire, 1.10f } },
            11.0f, 12.0f, Unit.ArmorType.Medium, new List<Ability>() {
                AbilityPrototypes.Instance.Get("urban combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("underground combat bonus", 0.25f),
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("city defence bonus", 0.25f),
                AbilityPrototypes.Instance.Get("armor piercing", 0.35f),
                AbilityPrototypes.Instance.Get("armor piercing ranged", 0.20f),
                AbilityPrototypes.Instance.Get("straight shot bonus", 0.25f),
                AbilityPrototypes.Instance.Get("increases order", 1.00f)
            }, new List<Unit.Tag>() { Unit.Tag.Medium_Shields }));

        faction.Units.Add(new Unit("Catapult", Unit.UnitType.Siege_Weapon, "catapult", 2.0f, 500, 350, 1.50f, 0, 0.0f, 2, Mechanization, new List<Building>() { faction.Buildings.First(x => x.Name == "Siege Workshop") },
            1.0f, false, 10.0f, 125.0f, 150.0f,
            new Damage(11.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.9f }, { Damage.Type.Impact, 0.1f } }), 0.25f,
            new Damage(18.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Impact, 1.0f } }), 14, 30, "big_stones", null,
            14.0f, 12.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 1.1f }, { Damage.Type.Thrust, 0.85f }, { Damage.Type.Earth, 1.25f }, { Damage.Type.Electric, 1.05f }, { Damage.Type.Fire, 0.75f } },
            15.0f, 7.0f, Unit.ArmorType.Medium, new List<Ability>() {
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.05f),
                AbilityPrototypes.Instance.Get("underground combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("armor piercing", 0.05f),
                AbilityPrototypes.Instance.Get("shield piercing", 0.10f),
                AbilityPrototypes.Instance.Get("anti infantry ranged", 0.10f),
                AbilityPrototypes.Instance.Get("armor piercing ranged", 0.35f),
                AbilityPrototypes.Instance.Get("city attack bonus", 0.35f),
                AbilityPrototypes.Instance.Get("anti wooden", 0.10f)
            }, new List<Unit.Tag>() { Unit.Tag.Crewed_Single_Entity, Unit.Tag.Mechanical_Ranged, Unit.Tag.No_Move_Attack }));

        faction.Units.Add(new Unit("Field Catapult", Unit.UnitType.Siege_Weapon, "field_catapult", 2.0f, 380, 310, 1.75f, 0, 0.0f, 2, Siege_Engineering, new List<Building>() { faction.Buildings.First(x => x.Name == "Siege Workshop") },
            2.0f, false, 10.0f, 150.0f, 150.0f,
            new Damage(11.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.9f }, { Damage.Type.Impact, 0.1f } }), 0.25f,
            new Damage(10.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Impact, 1.0f } }), 12, 30, "stones", null,
            15.0f, 12.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 1.1f }, { Damage.Type.Thrust, 0.85f }, { Damage.Type.Earth, 1.25f }, { Damage.Type.Electric, 1.05f }, { Damage.Type.Fire, 0.75f } },
            15.0f, 7.0f, Unit.ArmorType.Medium, new List<Ability>() {
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.05f),
                AbilityPrototypes.Instance.Get("underground combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("armor piercing", 0.05f),
                AbilityPrototypes.Instance.Get("shield piercing", 0.10f),
                AbilityPrototypes.Instance.Get("anti infantry ranged", 0.25f),
                AbilityPrototypes.Instance.Get("armor piercing ranged", 0.10f),
                AbilityPrototypes.Instance.Get("city attack bonus", 0.10f),
                AbilityPrototypes.Instance.Get("anti wooden", 0.10f)
            }, new List<Unit.Tag>() { Unit.Tag.Crewed_Single_Entity, Unit.Tag.Mechanical_Ranged }));

        faction.Units.Add(new Unit("Ballista", Unit.UnitType.Siege_Weapon, "dwarf_wanderer", 2.0f, 500, 410, 1.75f, 0, 0.0f, 2, Siege_Engineering, new List<Building>() { faction.Buildings.First(x => x.Name == "Siege Workshop") },
            1.0f, false, 10.0f, 125.0f, 150.0f,
            new Damage(11.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.9f }, { Damage.Type.Impact, 0.1f } }), 0.25f,
            new Damage(16.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Thrust, 0.85f }, { Damage.Type.Impact, 0.15f } }), 14, 20, null, null,
            14.0f, 12.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 1.1f }, { Damage.Type.Thrust, 0.85f }, { Damage.Type.Earth, 1.25f }, { Damage.Type.Electric, 1.05f }, { Damage.Type.Fire, 0.75f } },
            16.0f, 7.0f, Unit.ArmorType.Medium, new List<Ability>() {
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.05f),
                AbilityPrototypes.Instance.Get("underground combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("armor piercing", 0.05f),
                AbilityPrototypes.Instance.Get("shield piercing", 0.10f),
                AbilityPrototypes.Instance.Get("shield piercing ranged", 0.25f),
                AbilityPrototypes.Instance.Get("anti large ranged", 0.35f),
                AbilityPrototypes.Instance.Get("armor piercing ranged", 0.35f),
                AbilityPrototypes.Instance.Get("city attack bonus", 0.25f),
                AbilityPrototypes.Instance.Get("anti wooden", 0.10f)
            }, new List<Unit.Tag>() { Unit.Tag.Crewed_Single_Entity, Unit.Tag.Mechanical_Ranged, Unit.Tag.No_Move_Attack }));

        faction.Transports.Add(new Unit("Transport", Unit.UnitType.Ship, "ship_2", 1.0f, 300, 100, 2.0f, 0, 0.0f, 2, null, new List<Building>(),
            2.0f, false, 10.0f, 75.0f, -1.0f,
            new Damage(4.0f, new Dictionary<Damage.Type, float> { { Damage.Type.Thrust, 1.0f } }), 0.0f,
            new Damage(3.0f, new Dictionary<Damage.Type, float> { { Damage.Type.Thrust, 1.0f } }), 6, -1, null, null,
            12.0f, 14.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Impact, 0.75f }, { Damage.Type.Fire, 0.50f } },
            8.0f, 6.0f, Unit.ArmorType.Light, new List<Ability>() {
            }, new List<Unit.Tag>() { Unit.Tag.Naval, Unit.Tag.Crewed_Single_Entity, Unit.Tag.Embark_Transport }));

        return faction;
    }
}
