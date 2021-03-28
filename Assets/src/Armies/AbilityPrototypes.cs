using System;
using System.Collections.Generic;
using System.Linq;

public class AbilityPrototypes {
    private static AbilityPrototypes instance;
    private Dictionary<string, Ability> prototypes;

    private AbilityPrototypes()
    {
        prototypes = new Dictionary<string, Ability>();

        prototypes.Add("magic resistance", new Ability("Magic Resistance", true, true, true) {
            Get_Magic_Resistance = delegate (Ability ability, Unit unit) {
                return ability.Potency;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.05f;
            }
        });

        prototypes.Add("psionic resistance", new Ability("Psionic Resistance", true, true, true) {
            Get_Psionic_Resistance = delegate (Ability ability, Unit unit) {
                return ability.Potency;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.05f;
            }
        });

        prototypes.Add("combat mana max", new Ability("Combat Mana Max", false, true) {
            Get_Combat_Mana_Max = delegate (Ability ability, Unit unit) {
                return (int)ability.Potency;
            }
        });

        prototypes.Add("combat mana regen", new Ability("Combat Mana Regen", false, true) {
            Get_Combat_Mana_Regen = delegate (Ability ability, Unit unit) {
                return ability.Potency;
            }
        });

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

        prototypes.Add("anti monster", new Ability("Monster Slayer (Melee)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Type == Unit.UnitType.Monster ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti monster ranged", new Ability("Monster Slayer (Ranged)", true, true) {
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Type == Unit.UnitType.Monster ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti animal", new Ability("Animal Slayer (Melee)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Type == Unit.UnitType.Animal ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti animal ranged", new Ability("Animal Slayer (Ranged)", true, true) {
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Type == Unit.UnitType.Animal ? ability.Potency : 0.0f
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

        prototypes.Add("anti wooden", new Ability("Anti Wooden (Melee)", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Tags.Contains(Unit.Tag.Wooden) ? ability.Potency : 0.0f
                };
            }
        });

        prototypes.Add("anti wooden ranged", new Ability("Anti Wooden (Ranged)", true, true) {
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Tags.Contains(Unit.Tag.Wooden) ? ability.Potency : 0.0f
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
        prototypes.Add("underground combat bonus", new Ability("Underground Combat Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Underground) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Underground) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Underground) ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.Tags.Contains(CombatMapHex.Tag.Underground) ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Underground) ? ability.Potency : 0.0f;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.1f;
            }
        });
        prototypes.Add("city defence bonus", new Ability("City Defence Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.City && CombatManager.Instance.Army_2.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.City && CombatManager.Instance.Army_2.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.City && CombatManager.Instance.Army_2.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.City && CombatManager.Instance.Army_2.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Urban) && !attacker ? ability.Potency : 0.0f;
            }
        });
        prototypes.Add("city attack bonus", new Ability("City Attack Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.City && CombatManager.Instance.Army_1.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.City && CombatManager.Instance.Army_1.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = target.Hex.City && CombatManager.Instance.Army_1.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = target.Hex.City && CombatManager.Instance.Army_1.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Urban) && attacker ? ability.Potency : 0.0f;
            }
        });
        prototypes.Add("village defence bonus", new Ability("Village Defence Bonus", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = CombatManager.Instance.Map.WorldMapHex.Village != null && target.Hex.City &&
                        CombatManager.Instance.Army_2.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = CombatManager.Instance.Map.WorldMapHex.Village != null && target.Hex.City &&
                        CombatManager.Instance.Army_2.Id == target.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Attack_Multiplier = CombatManager.Instance.Map.WorldMapHex.Village != null && target.Hex.City &&
                        CombatManager.Instance.Army_2.Id == attacker.Army.Id ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Ranged_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                return new Ability.DamageData() {
                    Defence_Multiplier = CombatManager.Instance.Map.WorldMapHex.Village != null && target.Hex.City &&
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
                    New_Attack_Types = attacker.Last_Move_This_Turn_Was_Running ? new Dictionary<Damage.Type, float>() { { Damage.Type.Thrust, 0.90f }, { Damage.Type.Impact, 0.10f } } : null
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
        prototypes.Add("thief", new Ability("Thief", false, true) {
            On_Worked_Hex_Raid = delegate (Ability ability, Unit unit, WorldMapHex hex) {
                float cash_stolen = Math.Max(Math.Min(ability.Potency, hex.Yields.Cash), 0.0f);
                hex.Apply_Status_Effect(new HexStatusEffect("Thieves", 1) { Yield_Delta = new Yields(0.0f, 0.0f, -cash_stolen, 0.0f, 0.0f, 0.0f, 0.0f),
                    Happiness = -ability.Potency, Order = -ability.Potency }, true);
                return new Yields(0.0f, 0.0f, cash_stolen, 0.0f, 0.0f, 0.0f, 0.0f);
            }
        });
        prototypes.Add("marauder", new Ability("Marauder", false, true) {
            On_Worked_Hex_Raid = delegate (Ability ability, Unit unit, WorldMapHex hex) {
                float food_destroyed = Math.Max(Math.Min(ability.Potency, hex.Yields.Food), 0.0f);
                float production_destroyed = Math.Max(Math.Min(ability.Potency, hex.Yields.Production), 0.0f);
                float cash_stolen = Math.Max(Math.Min(ability.Potency, hex.Yields.Cash), 0.0f);
                float science_destroyed = Math.Max(Math.Min(ability.Potency, hex.Yields.Science), 0.0f);
                float culture_destroyed = Math.Max(Math.Min(ability.Potency, hex.Yields.Culture), 0.0f);
                float mana_destroyed = Math.Max(Math.Min(ability.Potency, hex.Yields.Mana), 0.0f);
                float faith_destroyed = Math.Max(Math.Min(ability.Potency, hex.Yields.Faith), 0.0f);
                hex.Apply_Status_Effect(new HexStatusEffect("Marauders", 1) {
                    Yield_Delta = new Yields(-food_destroyed, -production_destroyed, -cash_stolen, -science_destroyed, -culture_destroyed, -mana_destroyed, -faith_destroyed),
                    Happiness = -2.0f * ability.Potency,
                    Order = -2.0f * ability.Potency
                }, true);
                return new Yields(0.0f, 0.0f, cash_stolen * 0.5f, 0.0f, 0.0f, 0.0f, 0.0f);
            }
        });
        prototypes.Add("wild animal", new Ability("Wild Animal", false, true) {
            On_Worked_Hex_Raid = delegate (Ability ability, Unit unit, WorldMapHex hex) {
                float food_destroyed = Math.Max(Math.Min(ability.Potency * 0.1f, hex.Yields.Food), 0.0f);
                float production_destroyed = Math.Max(Math.Min(ability.Potency * 0.1f, hex.Yields.Production), 0.0f);
                float cash_destroyed = Math.Max(Math.Min(ability.Potency * 0.1f, hex.Yields.Cash), 0.0f);
                hex.Apply_Status_Effect(new HexStatusEffect("Wild animals", 1) {
                    Yield_Delta = new Yields(-food_destroyed, -production_destroyed, -cash_destroyed, 0.0f, 0.0f, 0.0f, 0.0f),
                    Happiness = -ability.Potency
                }, true);
                return new Yields(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
            }
        });
        prototypes.Add("village influence", new Ability("Village Influence", false, true) {
            On_Campaign_Turn_End = delegate (Ability ability, Unit unit) {
                if(unit.Army.Hex.Village != null) {
                    if (unit.Army.Hex.Village.Cultural_Influence.ContainsKey(unit.Owner)) {
                        unit.Army.Hex.Village.Cultural_Influence[unit.Owner] += ability.Potency;
                    } else {
                        unit.Army.Hex.Village.Cultural_Influence.Add(unit.Owner, ability.Potency);
                    }
                }
            }
        });
        prototypes.Add("village yield bonus", new Ability("Village Yield Bonus", false, true) {
            On_Campaign_Turn_End = delegate (Ability ability, Unit unit) {
                if (unit.Army.Hex.Village != null) {
                    unit.Army.Hex.Apply_Status_Effect(new HexStatusEffect("Village yield bonus ability", 2) {//2 turns, because Army.End_Turn gets called before WorldMapHex.End_Turn
                        Yield_Delta = new Yields(ability.Potency, ability.Potency, ability.Potency, 0.0f, 0.0f, 0.0f, 0.0f)
                    }, true);
                }
            }
        });

        prototypes.Add("knight upkeep reduction", new Ability("Knight Upkeep Reduction", true, true));
        prototypes.Add("knight upkeep", new Ability("Knight", false, false) {
            Get_Upkeep_Multiplier = delegate (Ability ability, WorldMapHex hex) {
                float squire_reduction = 0.0f;
                float min_reduction = float.MaxValue;
                int knights = 0;
                foreach(Unit unit in hex.Army.Units) {
                    if(unit.Abilities.Exists(x => x.Name == "Knight Upkeep Reduction")) {
                        float squire = unit.Abilities.First(x => x.Name == "Knight Upkeep Reduction").Potency;
                        squire_reduction += squire;
                        min_reduction = squire_reduction < min_reduction ? squire_reduction : min_reduction;
                    }
                    if(unit.Abilities.Exists(x => x.Name == "Knight")) {
                        knights++;
                    }
                }
                if(squire_reduction == 0.0f) {
                    return 0.0f;
                }
                if(knights == 0) {
                    CustomLogger.Instance.Error("Army has no knights, but unit with this ability should be there");
                    return 0.0f;
                }
                return -Math.Min(min_reduction, squire_reduction / knights);
            }
        });

        prototypes.Add("skirmisher", new Ability("Skirmisher", true, true) {
            Get_Disengagement_Movement_Cost = delegate (Ability ability, Unit unit) {
                return ability.Potency;
            }
        });

        prototypes.Add("inspiring presence", new Ability("Inspiring Presence", false, true) {
            On_Combat_Turn_Start = delegate (Ability ability, Unit unit) {
                foreach(Unit adjacent_unit in unit.Hex.Get_Adjancent_Hexes().Where(x => x.Unit != null && x.Unit.Owner.Id == unit.Owner.Id && !x.Unit.Abilities.Exists(y => y.Name == ability.Name)).
                        Select(x => x.Unit).ToArray()) {
                    float missing_relative_morale = Math.Max(0.0f, adjacent_unit.Manpower - adjacent_unit.Relative_Morale);
                    if(missing_relative_morale != 0.0f) {
                        float amount_restored = Math.Min(ability.Potency, missing_relative_morale * adjacent_unit.Max_Morale);
                        adjacent_unit.Alter_Morale(amount_restored);
                    }
                }
            }
        });

        prototypes.Add("duelist", new Ability("Duelist", true, true) {
            On_Calculate_Melee_Damage_As_Attacker = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                int adjacent_enemies = attacker.Hex.Get_Adjancent_Hexes().Where(x => x.Unit != null && x.Unit.Owner.Id != attacker.Owner.Id && !x.Unit.Is_Routed).Select(x => x.Unit).ToArray().Length;
                return new Ability.DamageData() {
                    Attack_Multiplier = adjacent_enemies == 1 ? ability.Potency : 0.0f
                };
            },
            On_Calculate_Melee_Damage_As_Defender = delegate (Ability ability, Unit attacker, Unit target, AttackResult result) {
                int adjacent_enemies = target.Hex.Get_Adjancent_Hexes().Where(x => x.Unit != null && x.Unit.Owner.Id != target.Owner.Id && !x.Unit.Is_Routed).Select(x => x.Unit).ToArray().Length;
                return new Ability.DamageData() {
                    Defence_Multiplier = adjacent_enemies == 1 ? ability.Potency : 0.0f
                };
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate (Ability ability) {
                return ability.Potency * 0.5f;
            }
        });

        prototypes.Add("forest stealth", new Ability("Forest Stealth", false, true) {
            Get_Stealth = delegate(Ability ability, Unit unit) {
                return unit.Hex != null && unit.Hex.Tags.Contains(CombatMapHex.Tag.Forest) ? ability.Potency : 0.0f;
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Forest) ? 0.01f * ability.Potency : 0.0f;
            }
        });

        prototypes.Add("vegetation stealth", new Ability("Vegetation Stealth", false, true) {
            Get_Stealth = delegate (Ability ability, Unit unit) {
                return unit.Hex != null && unit.Hex.Tags.Contains(CombatMapHex.Tag.Vegetation) ? ability.Potency : 0.0f;
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Forest) ? 0.01f * ability.Potency : 0.0f;
            }
        });

        prototypes.Add("urban stealth", new Ability("Urban Stealth", false, true) {
            Get_Stealth = delegate (Ability ability, Unit unit) {
                return unit.Hex != null && unit.Hex.Tags.Contains(CombatMapHex.Tag.Urban) ? ability.Potency : 0.0f;
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Urban) ? 0.01f * ability.Potency : (hex.Tags.Contains(WorldMapHex.Tag.Structure) ? 0.005f * ability.Potency : 0.0f);
            }
        });

        prototypes.Add("underground stealth", new Ability("Underground Stealth", false, true) {
            Get_Stealth = delegate (Ability ability, Unit unit) {
                return unit.Hex != null && unit.Hex.Tags.Contains(CombatMapHex.Tag.Underground) ? ability.Potency : 0.0f;
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Underground) ? 0.01f * ability.Potency : 0.0f;
            }
        });

        prototypes.Add("detection", new Ability("Detection", false, true) {
            Get_Detection = delegate (Ability ability, Unit unit, CombatMapHex hex) {
                return ability.Potency;
            },
            Get_Relative_Strength_Multiplier_Bonus = delegate(Ability ability) {
                return 0.005f * ability.Potency;
            }
        });

        prototypes.Add("forest detection", new Ability("Forest Detection", false, true) {
            Get_Detection = delegate (Ability ability, Unit unit, CombatMapHex hex) {
                return hex.Tags.Contains(CombatMapHex.Tag.Forest) ? ability.Potency : 0.0f;
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Forest) ? 0.005f * ability.Potency : 0.0f;
            }
        });

        prototypes.Add("vegetation detection", new Ability("Vegetation Detection", false, true) {
            Get_Detection = delegate (Ability ability, Unit unit, CombatMapHex hex) {
                return hex.Tags.Contains(CombatMapHex.Tag.Vegetation) ? ability.Potency : 0.0f;
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Forest) ? 0.005f * ability.Potency : 0.0f;
            }
        });

        prototypes.Add("urban detection", new Ability("Urban Detection", false, true) {
            Get_Detection = delegate (Ability ability, Unit unit, CombatMapHex hex) {
                return hex.Tags.Contains(CombatMapHex.Tag.Urban) ? ability.Potency : 0.0f;
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Urban) ? 0.005f * ability.Potency : (hex.Tags.Contains(WorldMapHex.Tag.Structure) ? 0.0025f * ability.Potency : 0.0f);
            }
        });

        prototypes.Add("underground detection", new Ability("Underground Detection", false, true) {
            Get_Detection = delegate (Ability ability, Unit unit, CombatMapHex hex) {
                return hex.Tags.Contains(CombatMapHex.Tag.Underground) ? ability.Potency : 0.0f;
            },
            Get_Fluctuating_Relative_Strength_Multiplier_Bonus = delegate (Ability ability, WorldMapHex hex, bool attacker) {
                return hex.Tags.Contains(WorldMapHex.Tag.Underground) ? 0.005f * ability.Potency : 0.0f;
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
