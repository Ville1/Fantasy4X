using System.Collections.Generic;

public class AbilityPrototypes {
    private static AbilityPrototypes instance;
    private Dictionary<string, Ability> prototypes;

    private AbilityPrototypes()
    {
        prototypes = new Dictionary<string, Ability>();

        prototypes.Add("anti cavalry", new Ability("Anti Cavalry (Melee)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate(Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Type == Unit.UnitType.Cavalry ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti cavalry ranged", new Ability("Anti Cavalry (Ranged)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Type == Unit.UnitType.Cavalry ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti infantry", new Ability("Anti Infantry (Melee)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Type == Unit.UnitType.Infantry ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti infantry ranged", new Ability("Anti Infantry (Ranged)", true, true) {
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Type == Unit.UnitType.Infantry ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti large", new Ability("Anti Large (Melee)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Tags.Contains(Unit.Tag.Large) ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti large ranged", new Ability("Anti Large (Ranged)", true, true) {
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Tags.Contains(Unit.Tag.Large) ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("armor piercing", new Ability("Armor Piercing (Melee)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                float multiplier = 0.0f;
                switch (target.Armor) {
                    case Unit.ArmorType.Light:
                        multiplier = ability.Potency * 0.25f;
                        break;
                    case Unit.ArmorType.Medium:
                        multiplier = ability.Potency * 0.50f;
                        break;
                    case Unit.ArmorType.Heavy:
                        multiplier = ability.Potency;
                        break;
                }
                return new Ability.DamageData() {
                    Attack_Multiplier = multiplier
                };
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.25f;
            }
        });

        prototypes.Add("shield piercing", new Ability("Shield Piercing (Melee)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                float multiplier = 0.0f;
                if (target.Tags.Contains(Unit.Tag.Large_Shields)) {
                    multiplier = ability.Potency;
                } else if (target.Tags.Contains(Unit.Tag.Medium_Shields)) {
                    multiplier = ability.Potency * 0.50f;
                } else if(target.Tags.Contains(Unit.Tag.Small_Shields)) {
                    multiplier = ability.Potency * 0.25f;
                } 
                return new Ability.DamageData() {
                    Attack_Multiplier = multiplier
                };
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.25f;
            }
        });

        prototypes.Add("armor piercing ranged", new Ability("Armor Piercing (Ranged)", true, true) {
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                float multiplier = 0.0f;
                switch (target.Armor) {
                    case Unit.ArmorType.Light:
                        multiplier = ability.Potency * 0.25f;
                        break;
                    case Unit.ArmorType.Medium:
                        multiplier = ability.Potency * 0.50f;
                        break;
                    case Unit.ArmorType.Heavy:
                        multiplier = ability.Potency;
                        break;
                }
                return new Ability.DamageData() {
                    Attack_Multiplier = multiplier
                };
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.25f;
            }
        });

        prototypes.Add("shield piercing ranged", new Ability("Shield Piercing (Ranged)", true, true) {
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                float multiplier = 0.0f;
                if (target.Tags.Contains(Unit.Tag.Large_Shields)) {
                    multiplier = ability.Potency;
                } else if (target.Tags.Contains(Unit.Tag.Medium_Shields)) {
                    multiplier = ability.Potency * 0.50f;
                } else if (target.Tags.Contains(Unit.Tag.Small_Shields)) {
                    multiplier = ability.Potency * 0.25f;
                }
                return new Ability.DamageData() {
                    Attack_Multiplier = multiplier
                };
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.25f;
            }
        });

        prototypes.Add("straight shot bonus", new Ability("Straight Shot Bonus", true, true) {
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = attacker.Get_Attack_Arch(target.Hex) == Unit.AttackArch.None ? ability.Potency : 0.0f
                };
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.25f;
            }
        });

        prototypes.Add("straight shots only", new Ability("Straight Shots Only", false, false) {
            On_Allow_Ranged_Attack = delegate (Ability ability, Unit unit, CombatMapHex target) {
                return unit.Get_Attack_Arch(target) == Unit.AttackArch.None;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return -0.035f;
            }
        });

        prototypes.Add("no high shots", new Ability("No High Shots", true, false) {
            On_Allow_Ranged_Attack = delegate (Ability ability, Unit unit, CombatMapHex target) {
                return unit.Get_Attack_Arch(target) != Unit.AttackArch.High;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return -0.025f;
            }
        });

        prototypes.Add("charge resistance", new Ability("Charge Resistance", true, true) {
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Charge_Multiplier = -ability.Potency
                };
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate(Ability ability) {
                return ability.Potency * 0.1f;
            }
        });
        prototypes.Add("urban combat bonus", new Ability("Urban Combat Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Urban) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Urban) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Urban) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Urban) ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate(Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Urban) ? ability.Potency : 0.0f;
            }
        });
        prototypes.Add("forest combat bonus", new Ability("Forest Combat Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Forest) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Forest) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Forest) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Forest) ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Forest) ? ability.Potency : 0.0f;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.1f;
            }
        });
        prototypes.Add("hill combat bonus", new Ability("Hill Combat Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Hill) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Hill) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Hill) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Hill) ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Hill) ? ability.Potency : 0.0f;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.1f;
            }
        });
        prototypes.Add("city defence bonus", new Ability("City Defence Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = CombatManager.Instance.Map.WorldMapHex.Tags.Contains(WorldMapHex.Tag.Urban) &&
                        CombatManager.Instance.Army_2.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = CombatManager.Instance.Map.WorldMapHex.Tags.Contains(WorldMapHex.Tag.Urban) &&
                        CombatManager.Instance.Army_2.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = CombatManager.Instance.Map.WorldMapHex.Tags.Contains(WorldMapHex.Tag.Urban) &&
                        CombatManager.Instance.Army_2.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = CombatManager.Instance.Map.WorldMapHex.Tags.Contains(WorldMapHex.Tag.Urban) &&
                        CombatManager.Instance.Army_2.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Urban) && !attacker ? ability.Potency : 0.0f;
            }
        });
        prototypes.Add("city attack bonus", new Ability("City Attack Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = CombatManager.Instance.Map.WorldMapHex.Tags.Contains(WorldMapHex.Tag.Urban) &&
                        CombatManager.Instance.Army_1.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = CombatManager.Instance.Map.WorldMapHex.Tags.Contains(WorldMapHex.Tag.Urban) &&
                        CombatManager.Instance.Army_1.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = CombatManager.Instance.Map.WorldMapHex.Tags.Contains(WorldMapHex.Tag.Urban) &&
                        CombatManager.Instance.Army_1.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = CombatManager.Instance.Map.WorldMapHex.Tags.Contains(WorldMapHex.Tag.Urban) &&
                        CombatManager.Instance.Army_1.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Urban) && attacker ? ability.Potency : 0.0f;
            }
        });
        prototypes.Add("village defence bonus", new Ability("Village Defence Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = CombatManager.Instance.Map.WorldMapHex.Village != null &&
                        CombatManager.Instance.Army_2.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = CombatManager.Instance.Map.WorldMapHex.Village != null &&
                        CombatManager.Instance.Army_2.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = CombatManager.Instance.Map.WorldMapHex.Village != null &&
                        CombatManager.Instance.Army_2.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = CombatManager.Instance.Map.WorldMapHex.Village != null &&
                        CombatManager.Instance.Army_2.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Village != null && !attacker ? ability.Potency : 0.0f;
            }
        });
        prototypes.Add("lance charge", new Ability("Lance Charge", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = attacker.Last_Move_This_Turn_Was_Running ? ability.Potency : 0.0f,
                    New_Attack_Types = attacker.Last_Move_This_Turn_Was_Running ? new Dictionary<Unit.DamageType, float>() { { Unit.DamageType.Thrust, 0.90f }, { Unit.DamageType.Impact, 0.10f } } : null
                };
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.1f;
            }
        });
        prototypes.Add("rough terrain penalty", new Ability("Rough Terrain Penalty", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = !target.Hex.Tags.Contains(CombatMapHex.Tag.Open) ? -ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = !target.Hex.Tags.Contains(CombatMapHex.Tag.Open) ? -ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = !target.Hex.Tags.Contains(CombatMapHex.Tag.Open) ? -ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = !target.Hex.Tags.Contains(CombatMapHex.Tag.Open) ? -ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return !hex.Tags.Contains(WorldMapHex.Tag.Open) ? -ability.Potency : 0.0f;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * -0.1f;
            }
        });
        prototypes.Add("upkeep reduction on village", new Ability("Upkeep Reduction On Village", true, true) {
            Get_Upkeep_Multiplier = delegate (Ability ability, WorldMapHex hex) {
                return hex.Village == null ? 0.0f : -ability.Potency;
            }
        });
        prototypes.Add("increases happiness", new Ability("Increases Happiness", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Happiness = ability.Potency };
            }
        });
        prototypes.Add("increases health", new Ability("Increases Health", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Health = ability.Potency };
            }
        });
        prototypes.Add("increases order", new Ability("Increases Order", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Order = ability.Potency };
            }
        });
        prototypes.Add("decreases happiness", new Ability("Decreases Happiness", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Happiness = -ability.Potency };
            }
        });
        prototypes.Add("decreases health", new Ability("Decreases Health", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Health = -ability.Potency };
            }
        });
        prototypes.Add("decreases order", new Ability("Decreases Order", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Order = -ability.Potency };
            }
        });
        prototypes.Add("city food", new Ability("City Food", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Yields = new Yields(ability.Potency, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f) };
            }
        });
        prototypes.Add("city production", new Ability("City Production", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Yields = new Yields(0.0f, ability.Potency, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f) };
            }
        });
        prototypes.Add("city cash", new Ability("City Cash", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Yields = new Yields(0.0f, 0.0f, ability.Potency, 0.0f, 0.0f, 0.0f, 0.0f) };
            }
        });
        prototypes.Add("city science", new Ability("City Science", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Yields = new Yields(0.0f, 0.0f, 0.0f, ability.Potency, 0.0f, 0.0f, 0.0f) };
            }
        });
        prototypes.Add("city culture", new Ability("City Culture", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Yields = new Yields(0.0f, 0.0f, 0.0f, 0.0f, ability.Potency, 0.0f, 0.0f) };
            }
        });
        prototypes.Add("city mana", new Ability("City Mana", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Yields = new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, ability.Potency, 0.0f) };
            }
        });
        prototypes.Add("city faith", new Ability("City Faith", false, true) {
            Get_City_Effects = delegate (Ability ability, City city) {
                return new Ability.CityEffects() { Yields = new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, ability.Potency) };
            }
        });
        //TODO: rough terrain penalty & ranged attacks
        //TODO: lance charge in urban? impassable houses?
        //TODO: stealth? Does not get destroyed when losing city defence?

    }

    public static AbilityPrototypes Instance
    {
        get {
            if (instance == null) {
                instance = new AbilityPrototypes();
            }
            return instance;
        }
    }

    public Ability Get(string name)
    {
        return Get(name, -1.0f);
    }

    public Ability Get(string name, float potency = -1.0f)
    {
        if (!prototypes.ContainsKey(name.ToLower())) {
            CustomLogger.Instance.Error("Ability prototype does not exist: " + name.ToLower());
            return null;
        }
        Ability ability = prototypes[name.ToLower()].Clone();
        ability.Potency = potency;
        return ability;
    }
}
