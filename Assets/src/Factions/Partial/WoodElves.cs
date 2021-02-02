using System.Collections.Generic;
using System.Linq;

public partial class Factions
{
    private static Faction Wood_Elves()
    {
        Faction faction = new Faction("Glistening Glade", 100, 1, new Dictionary<City.CitySize, Yields>() {
            { City.CitySize.Town,       new Yields(2.0f, 1.0f, 2.0f, 1.0f, 2.0f, 1.0f, 0.0f) },
            { City.CitySize.City,       new Yields(2.0f, 1.0f, 3.0f, 2.0f, 3.0f, 1.0f, 0.0f) },
            { City.CitySize.Metropolis, new Yields(2.0f, 2.0f, 4.0f, 3.0f, 4.0f, 1.0f, 0.0f) }
        }, 3.0f, 100, 2.0f, -0.50f, 1.0f, -0.10f, 1.0f, -0.15f, 1.5f, 0.50f, false, "elven city",
        new Technology(null, "Root", 5, new List<AI.Tag>()), new Army("Army", "elf_flag_bearer", "ship_2", 10, new List<string>() { "kingdom_tent_1", "kingdom_tent_2", "kingdom_tent_3" }, 5.0f), new EmpireModifiers() {
            Passive_Income = 3.0f,
            Max_Mana = 150.0f
        }) {
            Starting_Mana = 100.0f
        };

        Technology Commune_With_Nature = new Technology(faction, "Commune With Nature", 25, new List<AI.Tag>() { AI.Tag.Health, AI.Tag.Food, AI.Tag.Science, AI.Tag.Mana });
        faction.Root_Technology.Link(Commune_With_Nature, 3);

        Technology Animal_Pact = new Technology(faction, "Animal Pact", 50, new List<AI.Tag>() { AI.Tag.Military });
        Commune_With_Nature.Link(Animal_Pact, 1);

        Technology Friendly_Skies = new Technology(faction, "Friendly Skies", 100, new List<AI.Tag>() { AI.Tag.Military });
        Animal_Pact.Link(Friendly_Skies, 3);

        Technology Tree_Worship = new Technology(faction, "Tree Worship", 45, new List<AI.Tag>() { AI.Tag.Faith, AI.Tag.Culture, AI.Tag.Mana, AI.Tag.Happiness });
        Commune_With_Nature.Link(Tree_Worship, 5);

        Technology Communal_Discourse = new Technology(faction, "Communal Discourse", 85, new List<AI.Tag>() { AI.Tag.Faith, AI.Tag.Culture, AI.Tag.Mana, AI.Tag.Happiness });
        Tree_Worship.Link(Communal_Discourse, 4);

        Technology Communal_Singing = new Technology(faction, "Communal Singing", 200, new List<AI.Tag>() { AI.Tag.Faith, AI.Tag.Culture, AI.Tag.Mana, AI.Tag.Happiness });
        Communal_Discourse.Link(Communal_Singing, 3);

        Technology Awakening_Forest = new Technology(faction, "Awakening Forest", 120, new List<AI.Tag>() { AI.Tag.Military, AI.Tag.Order });
        Tree_Worship.Link(Awakening_Forest, 3);

        Technology Contact_Fairies = new Technology(faction, "Contact Fairies", 250, new List<AI.Tag>() { AI.Tag.Military, AI.Tag.Mana, AI.Tag.Faith });
        Awakening_Forest.Link(Contact_Fairies, 3);

        Technology Ecological_Crafts = new Technology(faction, "Ecological Crafts", 105, new List<AI.Tag>() { AI.Tag.Health, AI.Tag.Science, AI.Tag.Mana });
        Commune_With_Nature.Link(Ecological_Crafts, 3);
        
        Technology Arcane_Smithing = new Technology(faction, "Arcane Smithing", 220, new List<AI.Tag>() { AI.Tag.Production });
        Ecological_Crafts.Link(Arcane_Smithing, 2);

        faction.Improvements.Add(new Improvement(faction, "Grove", "grove", "grove_inactive", new Yields(0.5f, 0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 10, 0, false, HexPrototypes.Instance.Get_Names(new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Forest }, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Cursed, WorldMapHex.Tag.Structure }), null, null));
        faction.Improvements.Add(new Improvement(faction, "Garden", "garden", "garden_inactive", new Yields(1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.1f, 0.0f, 15, 0, false, HexPrototypes.Instance.Get_Names(new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Open }, new List<WorldMapHex.Tag>() { WorldMapHex.Tag.Cursed, WorldMapHex.Tag.Arid, WorldMapHex.Tag.Structure }), null, null));

        faction.Buildings.Add(new Building("Great tree", "placeholder", 200, 25, 0.0f, new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, false, null, null) {
            Recruitment_Limits = new Dictionary<string, int>() { { "Woodland Sentries", 2 } }
        });
        faction.Starting_Buildings.Add(faction.Buildings.First(x => x.Name == "Great tree"));

        faction.Buildings.Add(new Building("Market", "placeholder", 90, 55, 1.0f, new Yields(0, 0, 0, 0, 1, 0, 0), 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, null, null) {
            Percentage_Yield_Bonuses = new Yields(10, 0, 5, 0, 0, 0, 0),
            Trade_Value = 1.0f
        });
        faction.Buildings.Add(new Building("Treetop Watch", "placeholder", 75, 35, 0.5f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, true, null, null) {
            Recruitment_Limits = new Dictionary<string, int>() { { "Woodland Sentries", 1 } },
            City_LoS_Bonus = 1
        });
        faction.Buildings.Add(new Building("Communal Garden", "placeholder", 150, 75, 1.0f, new Yields(2, 0, 0, 0, 1, 0, 0), 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, null, null) {
            Pop_Growth_Additive_Bonus = 0.05f,
            Food_Storage = 25
        });
        faction.Buildings.Add(new Building("Arcane Forge", "placeholder", 130, 100, 2.0f, new Yields(0, 1, 1, 0, 0, -0.25f, 0), 0.0f, -0.25f, 0.0f, 0.0f, 0.0f, 0.0f, true, Arcane_Smithing,
            delegate (Building building) {
                int bonus_minerals = 0;
                foreach (WorldMapHex hex in building.City.Worked_Hexes) {
                    if (hex.Improvement != null && hex.Improvement.Extracts_Minerals && hex.Mineral != null &&
                        (hex.Mineral.Name == "Copper" || hex.Mineral.Name == "Iron" || hex.Mineral.Name == "Adamantine")) {
                        bonus_minerals++;
                    }
                }
                if (bonus_minerals != 0) {
                    building.Yield_Delta = new Yields(0, bonus_minerals, 0, 0, 0, 0, 0);
                } else {
                    building.Yield_Delta = null;
                }
            }
        ));
        faction.Buildings.Add(new Building("Herbalist", "placeholder", 100, 125, 1.0f, new Yields(0.5f, 0, 0, 0.5f, 0, 0.5f, 0), 0.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Commune_With_Nature, null) {
            Pop_Growth_Additive_Bonus = 0.05f
        });
        faction.Buildings.Add(new Building("Potion Maker", "placeholder", 150, 150, 2.0f, new Yields(0, 0, 0.5f, 1.0f, 0, 0.5f, 0), 0.0f, 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Ecological_Crafts,
            delegate(Building building) {
                if(building.City.Buildings.Exists(x => x.Name == "Herbalist") && !building.City.Buildings.First(x => x.Name == "Herbalist").Paused) {
                    building.Yield_Delta = new Yields(0.0f, 0.0f, 0.5f, 0.5f, 0.0f, 0.5f, 0.0f);
                } else {
                    building.Yield_Delta = null;
                }
            }) {
            Pop_Growth_Additive_Bonus = 0.05f,
            Trade_Value = 0.5f
        });
        faction.Buildings.Add(new Building("Shrine", "placeholder", 250, 210, 2.0f, new Yields(0, 0, 0, 0, 1.0f, 1.0f, 1.0f), 2.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Tree_Worship,
            delegate (Building building) {
                int forests_worked = 0;
                foreach(WorldMapHex hex in building.City.Worked_Hexes) {
                    if(hex.Tags.Contains(WorldMapHex.Tag.Forest) && !hex.Tags.Contains(WorldMapHex.Tag.Cursed) && !hex.Tags.Contains(WorldMapHex.Tag.Structure)) {
                        forests_worked++;
                    }
                }
                if(forests_worked == 0) {
                    building.Yield_Delta = null;
                } else {
                    building.Yield_Delta = new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.1f * forests_worked);
                }
            }));
        faction.Buildings.Add(new Building("Watchful Trees", "placeholder", 125, 75, 0.5f, new Yields(0, 0, 0, 0, 0, 0, 0), 0.0f, 0.0f, 5.0f, 0.0f, 0.0f, 0.0f, true, Awakening_Forest, null));
        faction.Buildings.Add(new Building("Fairy Circle", "placeholder", 25, 100, 0.5f, new Yields(0, 0, 0, 0, 0, 1.0f, 1.0f), 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Contact_Fairies, null) {
            Unit_Summoning_Speed_Bonus = 1.00f,
            Tags = new Dictionary<AI.Tag, float>() { { AI.Tag.Military, 5.0f } }
        });

        faction.Buildings.Add(new Building("Podium", "placeholder", 115, 175, 1.0f, new Yields(0, 0, 0, 1, 1, 0, 0), 1.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f, true, Communal_Discourse, null) {
            Percentage_Yield_Bonuses = new Yields(0, 0, 0, 5, 0, 0, 0)
        });
        faction.Buildings.Add(new Building("Choir Hall", "placeholder", 290, 275, 2.0f, new Yields(0, 0, 0, 0, 3, 0, 0), 3.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, true, Communal_Singing, null) {
            Percentage_Yield_Bonuses = new Yields(0, 0, 0, 0, 10, 0, 0)
        });

        faction.Units.Add(new Worker("Worker", 2.0f, Map.MovementType.Land, 2, "elf_worker", new List<string>() { "elf_working_1", "elf_working_2" }, 4.0f, new List<Improvement>()
            { faction.Improvements.First(x => x.Name == "Grove"), faction.Improvements.First(x => x.Name == "Garden")},
            1.0f, 25, 25, 0.5f, null, 0.0f));
        faction.Units.Add(new Unit("Woodland Sentries", Unit.UnitType.Infantry, "elf_archer", 3.0f, 175, 260, 1.0f, 0, 0.0f, 3, null, new List<Building>() { },
            2.0f, true, 7.5f, 125.0f, 125.0f,
            new Damage(16.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.75f }, { Damage.Type.Thrust, 0.25f } }), 0.25f,
            new Damage(12.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Thrust, 1.00f } }), 8, 20, null, null,
            17.0f, 11.0f, new Dictionary<Damage.Type, float>() {
                { Damage.Type.Slash, 0.50f },
                { Damage.Type.Thrust, 0.75f },
                { Damage.Type.Impact, 1.0f },
                { Damage.Type.Acid, 0.90f },
                { Damage.Type.Cold, 0.90f },
                { Damage.Type.Fire, 0.90f },
                { Damage.Type.Earth, 1.10f },
                { Damage.Type.Light, 1.10f },
                { Damage.Type.Dark, 0.90f }
            },
            12.0f, 10.0f, Unit.ArmorType.Unarmoured, new List<Ability>() {
                AbilityPrototypes.Instance.Get("forest combat bonus", 0.25f),
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("city defence bonus", 0.05f),
                AbilityPrototypes.Instance.Get("village defence bonus", 0.05f),
                AbilityPrototypes.Instance.Get("forest stealth", 2.0f),
                AbilityPrototypes.Instance.Get("vegetation stealth", 1.0f),
                AbilityPrototypes.Instance.Get("detection", 1.0f),
                AbilityPrototypes.Instance.Get("vegetation detection", 3.0f),
                AbilityPrototypes.Instance.Get("skirmisher", 1.0f)
            },
            new List<Unit.Tag>() { Unit.Tag.Limited_Recruitment }));

        faction.Units.Add(new Unit("Wolves", Unit.UnitType.Animal, "elf_wolf", 3.0f, 10, 0, 0.0f, 25, 0.25f, 3, null, null,
            3.0f, true, 5.0f, 75.0f, 150.0f,
            new Damage(7.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.85f }, { Damage.Type.Thrust, 0.15f } }), 0.25f,
            null, 0, 0, null, null,
            6.0f, 3.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Slash, 0.50f }, { Damage.Type.Thrust, 0.50f }, { Damage.Type.Fire, 0.9f }, { Damage.Type.Cold, 1.1f } },
            4.0f, 6.0f, Unit.ArmorType.Unarmoured, new List<Ability>() {
                AbilityPrototypes.Instance.Get("forest combat bonus", 0.35f),
                AbilityPrototypes.Instance.Get("hill combat bonus", 0.10f),
                AbilityPrototypes.Instance.Get("skirmisher", 1.0f),
                AbilityPrototypes.Instance.Get("forest stealth", 2.0f),
                AbilityPrototypes.Instance.Get("vegetation stealth", 2.0f),
                AbilityPrototypes.Instance.Get("detection", 3.0f)
            }, new List<Unit.Tag>()));

        faction.Units.Add(new Unit("Treant Guards", Unit.UnitType.Monster, "treant", 1.0f, 25, 0, 0.0f, 50, 0.25f, 2, null, new List<Building>() { },
            1.0f, true, 12.5f, 110.0f, 150.0f,
            new Damage(18, new Dictionary<Damage.Type, float>() { { Damage.Type.Impact, 1.0f }, { Damage.Type.Earth, 0.10f } }), 0.25f,
            null, 0, 0, null, null,
            25.0f, 28.0f, new Dictionary<Damage.Type, float>() {
                { Damage.Type.Slash, 0.95f },
                { Damage.Type.Thrust, 1.10f },
                { Damage.Type.Impact, 0.95f },
                { Damage.Type.Acid, 0.75f },
                { Damage.Type.Cold, 0.65f },
                { Damage.Type.Fire, 0.50f },
                { Damage.Type.Earth, 1.25f }
            }, 11, 9.0f, Unit.ArmorType.Light, new List<Ability>() {
                AbilityPrototypes.Instance.Get("forest combat bonus", 0.35f),
                AbilityPrototypes.Instance.Get("city defence bonus", 0.10f),
                AbilityPrototypes.Instance.Get("village defence bonus", 0.10f),
                AbilityPrototypes.Instance.Get("forest stealth", 2.0f),
                AbilityPrototypes.Instance.Get("forest detection", 2.0f)
            }, new List<Unit.Tag>() { Unit.Tag.Large, Unit.Tag.Wooden }) {
            Actions = new List<UnitAction>() {
                new UnitAction("Entangling Roots", "entangling roots", 5, 10, 0, 3, false, UnitAction.TargetingType.Enemy, 0.0f, 0.01f, "debuff", SpriteManager.SpriteType.Skill,
                "bind", null, delegate(Unit unit, UnitAction action, CombatMapHex hex, bool is_preview, out AttackResult[] result, out string message) {
                    result = action.Melee_Attack(unit, hex.Unit, new Damage(5.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Impact, 1.0f }, { Damage.Type.Earth, 0.35f } }), is_preview);
                    UnitStatusEffect effect = new UnitStatusEffect(action.Internal_Name, action.Name, 1, UnitStatusEffect.EffectType.Debuff, false, "debuff", SpriteManager.SpriteType.Skill);
                    effect.Effects.Disables_Stealth = true;
                    effect.Effects.Movement_Delta = -1.0f;
                    effect.Effects.Melee_Attack_Delta_Flat = -1.0f;
                    effect.Effects.Melee_Defence_Delta_Flat = -1.0f;
                    action.Apply_Debuff(unit, hex.Unit, effect, is_preview);
                    message = null;
                    return true;
                })
            }
        });



        faction.Transports.Add(new Unit("Transport", Unit.UnitType.Ship, "ship_2", 1.0f, 300, 100, 2.0f, 0, 0.0f, 2, null, new List<Building>(),
            2.0f, false, 10.0f, 85.0f, -1.0f,
            new Damage(6.0f, new Dictionary<Damage.Type, float> { { Damage.Type.Thrust, 1.0f } }), 0.0f,
            new Damage(5.0f, new Dictionary<Damage.Type, float> { { Damage.Type.Thrust, 1.0f } }), 6, -1, null, null,
            10.0f, 13.0f, new Dictionary<Damage.Type, float>() { { Damage.Type.Impact, 0.75f }, { Damage.Type.Fire, 0.50f } },
            8.0f, 6.0f, Unit.ArmorType.Light, new List<Ability>() {
            }, new List<Unit.Tag>() { Unit.Tag.Naval, Unit.Tag.Crewed_Single_Entity, Unit.Tag.Embark_Transport }));

        faction.Spells.Add(new Spell("Whispering Trees", 10.0f, 5, Commune_With_Nature, false, delegate (Spell spell, Player caster, WorldMapHex hex) {

            return new Spell.SpellResult();
        }));

        faction.Spells.Add(new Spell("Talking Trees", 50.0f, 25, Awakening_Forest, true, delegate (Spell spell, Player caster, WorldMapHex hex) {

            return new Spell.SpellResult();
        }));

        faction.Blessings.Add(new Blessing("Sacred Festivals", 1, 25, 5, Tree_Worship, null, null, delegate (Blessing blessing, Player caster, int turns_left) {
            Blessing.BlessingResult result = new Blessing.BlessingResult(true, null, caster.Cities.Select(x => x.Hex).ToList());
            float potency = 1.0f + (caster.Faith_Income * 0.33f);
            foreach (WorldMapHex hex in result.Affected_Hexes) {
                hex.City.Apply_Status_Effect(new CityStatusEffect(blessing.Name, 1) {
                    Parent_Duration = turns_left,
                    Yield_Delta = new Yields(0.0f, -0.1f * potency, 0.0f, 0.0f, potency, 0.0f, 0.0f),
                    Happiness = potency
                }, false);
            }
            return result;
        }) {
            AI_Casting_Guidance = new Blessing.AIBlessingCastingGuidance(Blessing.AIBlessingCastingGuidance.TargetType.Caster, null, new Dictionary<AI.Tag, float>() { { AI.Tag.Culture, 1.0f }, { AI.Tag.Happiness, 1.0f } })
        });

        return faction;
    }
}
