﻿using System;
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
            On_Turn_End = delegate (Ability ability, Unit unit) {
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
            On_Turn_End = delegate (Ability ability, Unit unit) {
                if (unit.Army.Hex.Village != null) {
                    unit.Army.Hex.Apply_Status_Effect(new HexStatusEffect("Village yield bonus ability", 2) {//2 turns, because Army.End_Turn gets called before WorldMapHex.End_Turn
                        Yield_Delta = new Yields(ability.Potency, ability.Potency, ability.Potency, 0.0f, 0.0f, 0.0f, 0.0f)
                    }, true);
                }
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
