using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatAI
{
    private static readonly int TURNS_TO_SEARCH_WHOLE_MAP = 20;

    public enum DeploymentRole { Slow_Melee, Slow_Ranged, Fast_Melee, Fast_Ranged }
    public enum Strategy { Basic }

    private I_AI ai;

    private AI.Level AI_Level { get { return ai.AI_Level; } }
    private Player Player { get { return ai.Player; } }

    private List<Unit> unit_order;
    private Unit unit;
    private Strategy strategy;
    private Army army;
    private Army enemy_army;
    private UnitTarget target;
    private float strategy_aggressiveness;
    private bool is_attacker;
    private bool searching_for_hidden;

    public CombatAI(I_AI ai)
    {
        this.ai = ai;
    }

    public void Start_Combat()
    {
        army = CombatManager.Instance.Army_1.Is_Owned_By(Player) ? CombatManager.Instance.Army_1 : CombatManager.Instance.Army_2;
        enemy_army = CombatManager.Instance.Army_1.Is_Owned_By(Player) ? CombatManager.Instance.Army_2 : CombatManager.Instance.Army_1;
        strategy = Strategy.Basic;
        is_attacker = CombatManager.Instance.Army_1.Is_Owned_By(Player);

        strategy_aggressiveness = 0.5f;//0.0f - 1.0f
        float own_strenght = army.Get_Relative_Strenght_When_On_Hex(CombatManager.Instance.Hex, true, is_attacker);
        float enemy_strenght = enemy_army.Get_Relative_Strenght_When_On_Hex(CombatManager.Instance.Hex, true, !is_attacker);
        strategy_aggressiveness +=
            own_strenght > enemy_strenght ?
            Math.Min(0.25f, 0.25f * ((own_strenght - enemy_strenght) / enemy_strenght)) :
            -1.0f * Math.Min(0.25f, 0.25f * ((enemy_strenght - own_strenght) / own_strenght));
        if((int)AI_Level <= (int)AI.Level.Easy) {
            strategy_aggressiveness += 0.01f * RNG.Instance.Next(0, 50);
        }
        strategy_aggressiveness = Mathf.Clamp01(strategy_aggressiveness);
    }

    public void Start_Combat_Turn()
    {
        Dictionary<Unit, float> distances = new Dictionary<Unit, float>();
        foreach (Unit unit in (CombatManager.Instance.Army_1.Is_Owned_By(Player) ? CombatManager.Instance.Army_1 : CombatManager.Instance.Army_2).Units) {
            if (!unit.Controllable) {
                continue;
            }
            float min_distance = float.MaxValue;
            foreach (Unit enemy_unit in (CombatManager.Instance.Army_1.Is_Owned_By(Player) ? CombatManager.Instance.Army_2 : CombatManager.Instance.Army_1).Units) {
                if (enemy_unit.Hex != null) {
                    float distance = unit.Hex.Coordinates.Distance(enemy_unit.Hex.Coordinates);
                    if (distance < min_distance) {
                        min_distance = distance;
                    }
                }
            }
            distances.Add(unit, min_distance);
        }
        unit_order = distances.OrderBy(x => x.Value).Select(x => x.Key).ToList();
        unit = null;
    }
    
    public void Combat_Act(float delta_s)
    {
        if (CombatManager.Instance.Deployment_Mode) {
            Deploy(AI_Level, army);
            CombatManager.Instance.Next_Turn();
            return;
        }

        if(unit == null) {
            unit = unit_order.Where(x => x.Controllable && !x.Wait_Turn).FirstOrDefault();
            searching_for_hidden = false;
            if (unit == null) {
                CombatManager.Instance.Next_Turn();
                return;
            }
        }
        
        if (CombatUIManager.Instance.Active) {
            CombatUIManager.Instance.Current_Unit = unit;
        }
        
        if(searching_for_hidden && enemy_army.Units.Exists(x => x.Is_Visible)) {
            searching_for_hidden = false;
            target = null;
        }

        if(target != null && target.Is_Move && !target.Is_Attack && !target.Is_Action && target.Path != null && target.Path.Count == 0) {
            target = null;
        }

        if(target == null) {
            List<CombatMapHex> reachable_hexes = unit.Get_Hexes_In_Movement_Range(false);
            List<CombatMapHex> reachable_hexes_run = unit.Get_Hexes_In_Movement_Range(true);

            List<Unit> melee_reachable_enemies = new List<Unit>();
            List<Unit> melee_reachable_enemies_run = new List<Unit>();
            List<Unit> ranged_reachable_enemies = new List<Unit>();
            List<Unit> ranged_reachable_enemies_run = new List<Unit>();

            bool is_in_melee = unit.Hex.Is_Adjancent_To_Enemy(Player);

            if (enemy_army.Units.Exists(x => x.Is_Visible)) {
                //Find all enemies that can be reached
                if (is_in_melee) {
                    foreach(CombatMapHex hex in unit.Hex.Get_Adjancent_Hexes()) {
                        if (hex.Is_Adjancent_To_Enemy(Player)) {
                            if(hex.Unit != null && hex.Unit.Army.Owner.Id != unit.Army.Owner.Id && hex.Unit.Is_Visible && !melee_reachable_enemies.Contains(hex.Unit)) {
                                melee_reachable_enemies.Add(hex.Unit);
                            }
                            foreach(CombatMapHex adjacent_hex in hex.Get_Adjancent_Hexes()) {
                                if (adjacent_hex.Unit != null && adjacent_hex.Unit.Army.Owner.Id != unit.Army.Owner.Id && adjacent_hex.Unit.Is_Visible && !melee_reachable_enemies.Contains(adjacent_hex.Unit)) {
                                    melee_reachable_enemies.Add(adjacent_hex.Unit);
                                }
                            }
                        }
                    }
                } else {
                    foreach (CombatMapHex hex in reachable_hexes) {
                        foreach (CombatMapHex adjacent_hex in hex.Get_Adjancent_Hexes()) {
                            if (adjacent_hex.Unit != null && adjacent_hex.Unit.Army.Owner.Id != unit.Army.Owner.Id && adjacent_hex.Unit.Is_Visible && !melee_reachable_enemies.Contains(adjacent_hex.Unit)) {
                                melee_reachable_enemies.Add(adjacent_hex.Unit);
                            }
                        }
                    }
                    foreach (CombatMapHex hex in reachable_hexes_run.Where(x => !reachable_hexes.Contains(x))) {
                        foreach (CombatMapHex adjacent_hex in hex.Get_Adjancent_Hexes()) {
                            if (adjacent_hex.Unit != null && adjacent_hex.Unit.Army.Owner.Id != unit.Army.Owner.Id && adjacent_hex.Unit.Is_Visible && !melee_reachable_enemies.Contains(adjacent_hex.Unit) &&
                                !melee_reachable_enemies_run.Contains(adjacent_hex.Unit)) {
                                melee_reachable_enemies_run.Add(adjacent_hex.Unit);
                            }
                        }
                    }
                }
                if (unit.Can_Ranged_Attack) {
                    foreach (CombatMapHex hex in unit.Get_Hexes_In_Attack_Range()) {
                        if (!hex.Is_Adjancent_To(unit.Hex) && hex.Unit != null && hex.Unit.Army.Owner.Id != unit.Army.Owner.Id && hex.Unit.Is_Visible && !ranged_reachable_enemies.Contains(hex.Unit)) {
                            ranged_reachable_enemies.Add(hex.Unit);
                        }
                    }
                    foreach (CombatMapHex hex in reachable_hexes) {
                        foreach (CombatMapHex range_hex in unit.Get_Hexes_In_Attack_Range(-1, hex)) {
                            if (range_hex.Unit != null && range_hex.Unit.Army.Owner.Id != unit.Army.Owner.Id && range_hex.Unit.Is_Visible && !ranged_reachable_enemies.Contains(range_hex.Unit)) {
                                ranged_reachable_enemies.Add(range_hex.Unit);
                            }
                        }
                    }
                    foreach (CombatMapHex hex in reachable_hexes_run.Where(x => !reachable_hexes.Contains(x))) {
                        foreach (CombatMapHex range_hex in unit.Get_Hexes_In_Attack_Range(-1, hex)) {
                            if (range_hex.Unit != null && range_hex.Unit.Army.Owner.Id != unit.Army.Owner.Id && range_hex.Unit.Is_Visible && !ranged_reachable_enemies.Contains(range_hex.Unit) &&
                                !ranged_reachable_enemies_run.Contains(range_hex.Unit)) {
                                ranged_reachable_enemies_run.Add(range_hex.Unit);
                            }
                        }
                    }
                }
            }

            //Calculate preferences for enemies
            if(melee_reachable_enemies.Count != 0 || melee_reachable_enemies_run.Count != 0 || ranged_reachable_enemies.Count != 0 || ranged_reachable_enemies_run.Count != 0) {
                Dictionary<Unit, object[]> melee_preferences = new Dictionary<Unit, object[]>();
                float routed_multiplier = 0.001f;
                foreach(Unit enemy in melee_reachable_enemies) {
                    AttackResult[] preview = unit.Attack(enemy, true, null, true);
                    float damage_dealt_value = (-1.0f * preview[1].Manpower_Delta * enemy.Relative_Strenght) * (enemy.Is_Routed ? routed_multiplier : 1.0f);
                    float damage_taken_value = 0.5f * (1.0f / (1.0f + strategy_aggressiveness)) * (preview[0].Manpower_Delta * unit.Relative_Strenght);
                    float total_value = damage_dealt_value + damage_taken_value;
                    melee_preferences.Add(enemy, new object[2] { Randomize_Value(total_value), false });
                }
                foreach (Unit enemy in melee_reachable_enemies_run) {
                    AttackResult[] preview = unit.Attack(enemy, true, null, true);
                    float damage_dealt_value = (0.1f + (strategy_aggressiveness * 0.25f)) * (-1.0f * preview[1].Manpower_Delta * enemy.Relative_Strenght) * (enemy.Is_Routed ? routed_multiplier : 1.0f);
                    float damage_taken_value = 0.5f * (1.0f / (1.0f + strategy_aggressiveness)) * (preview[0].Manpower_Delta * unit.Relative_Strenght);
                    float total_value = damage_dealt_value + damage_taken_value;
                    melee_preferences.Add(enemy, new object[2] { Randomize_Value(total_value), true });
                }
                Dictionary<Unit, object[]> ranged_preferences = new Dictionary<Unit, object[]>();
                foreach (Unit enemy in ranged_reachable_enemies) {
                    AttackResult[] preview = unit.Attack(enemy, true, null, false);
                    float total_value = (-1.0f * preview[1].Manpower_Delta * enemy.Relative_Strenght) * (enemy.Is_Routed ? routed_multiplier : 1.0f);
                    ranged_preferences.Add(enemy, new object[2] { Randomize_Value(total_value), false });
                }
                foreach (Unit enemy in ranged_reachable_enemies_run) {
                    AttackResult[] preview = unit.Attack(enemy, true, null, false);
                    float total_value = (0.1f + (strategy_aggressiveness * 0.25f)) * (-1.0f * preview[1].Manpower_Delta * enemy.Relative_Strenght) * (enemy.Is_Routed ? routed_multiplier : 1.0f);
                    total_value -= 25.0f;//TODO: static readonly
                    ranged_preferences.Add(enemy, new object[2] { Randomize_Value(total_value), true });
                }
                float top_melee_value = melee_preferences.Count != 0 ? melee_preferences.Select(x => (float)x.Value[0]).OrderByDescending(x => x).First() : -1.0f;
                float top_ranged_value = ranged_preferences.Count != 0 ? ranged_preferences.Select(x => (float)x.Value[0]).OrderByDescending(x => x).First() : -1.0f;
                if(top_melee_value > 0.0f && top_melee_value > top_ranged_value) {
                    //Melee attack
                    target = new UnitTarget(melee_preferences.OrderByDescending(x => (float)x.Value[0]).First().Key, true, (bool)melee_preferences.OrderByDescending(x => (float)x.Value[0]).First().Value[1]);
                    target.Path = CombatManager.Instance.Map.Path(unit, target.Unit.Hex);
                } else if(top_ranged_value > 0.0f && top_ranged_value > top_melee_value) {
                    //Ranged attack
                    target = new UnitTarget(ranged_preferences.OrderByDescending(x => (float)x.Value[0]).First().Key, false, (bool)ranged_preferences.OrderByDescending(x => (float)x.Value[0]).First().Value[1]);
                    target.Path = CombatManager.Instance.Map.Path(unit, target.Unit.Hex);
                } else {
                    //No good options
                    target = null;
                }
            } else {
                //Cant reach any enemies this turn
                if((is_attacker ? 0.5f : 0.0f) + strategy_aggressiveness >= 0.75f) {
                    //Advance
                    if(enemy_army.Units.Exists(x => x.Is_Visible && x.Hex != null)) {
                        float shortest_distance = float.MaxValue;
                        Unit closest_enemy_unit = null;
                        foreach(Unit enemy_unit in enemy_army.Units.Where(x => x.Is_Visible && x.Hex != null)) {
                            float distance = unit.Hex.Coordinates.Distance(enemy_unit.Hex.Coordinates);
                            if(distance < shortest_distance) {
                                shortest_distance = distance;
                                closest_enemy_unit = enemy_unit;
                            }
                        }
                        if(closest_enemy_unit == null) {
                            //This should not happen
                            CustomLogger.Instance.Error("closest_enemy_unit == null");
                            target = null;
                        } else {
                            target = new UnitTarget(closest_enemy_unit.Hex, false);
                            target.Path = CombatManager.Instance.Map.Path(unit, closest_enemy_unit.Hex);
                        }
                    } else {
                        //No visible enemies, seacrh for hidden ones
                        CombatMapHex scout_hex = null;
                        if(CombatManager.Instance.Turn > TURNS_TO_SEARCH_WHOLE_MAP) {
                            scout_hex = CombatManager.Instance.Map.Random_Hex;
                        } else {
                            scout_hex = CombatManager.Instance.Army_1.Id == army.Id ? RNG.Instance.Random_Item(CombatManager.Instance.Map.Deployment_Zone_2) :
                                RNG.Instance.Random_Item(CombatManager.Instance.Map.Deployment_Zone_1);
                        }
                        target = new UnitTarget(scout_hex, false);
                        target.Path = CombatManager.Instance.Map.Path(unit, scout_hex);
                        searching_for_hidden = true;
                    }
                } else {
                    //Stand ground
                    target = null;
                }
            }
        } else {
            AttackResult[] result;
            string message;
            if (target.Is_Attack && ((target.Melee_Attack.Value && unit.Hex.Is_Adjancent_To(target.Unit.Hex)) || (!target.Melee_Attack.Value && unit.In_Attack_Range(target.Unit.Hex)))) {
                result = unit.Attack(target.Unit, false);
                target = null;
            } else if (target.Is_Action && unit.Actions.First(x => x.Internal_Name == target.Action).Activate(unit, target.Unit == null ? target.Hex : target.Unit.Hex, true, out result, out message)) {
                unit.Actions.First(x => x.Internal_Name == target.Action).Activate(unit, target.Unit == null ? target.Hex : target.Unit.Hex, false, out result, out message);
                target = null;
            } else if (target.Path != null && target.Path.Count != 0) {
                if (!unit.Move(target.Path[0], target.Run || (target.Melee_Attack == true && unit.Relative_Stamina >= 0.125f / ((strategy_aggressiveness + 0.5f) / 2.0f) && target.Path.Count == 1))) {
                    target = null;
                } else {
                    target.Path.RemoveAt(0);
                }
            }
        }

        if(target == null) {
            unit.Wait_Turn = true;
            unit = null;
        }

        if (CombatUIManager.Instance.Active) {
            CombatUIManager.Instance.Update_Current_Unit();
        }
    }

    private float Randomize_Value(float value)
    {
        if((int)AI_Level >= (int)AI.Level.Medium) {
            return value;
        }
        return value * (0.9f + 0.2f * (RNG.Instance.Next(0, 100) * 0.01f));
    }

    public static void Deploy(AI.Level level, Army army)
    {
        List<List<CombatMapHex>> rows = new List<List<CombatMapHex>>();
        Dictionary<int, List<CombatMapHex>> row_help = new Dictionary<int, List<CombatMapHex>>();

        //Deployment rows
        for (int x = 0; x < CombatManager.Instance.Map.Width; x++) {
            for (int y = 0; y < CombatManager.Instance.Map.Height; y++) {
                CombatMapHex hex = CombatManager.Instance.Map.Get_Hex_At(x, y);
                if (hex == null || hex.Hidden) {
                    continue;
                }
                if (army.Id == CombatManager.Instance.Army_2.Id) {
                    if (!row_help.ContainsKey(y)) {
                        row_help.Add(y, new List<CombatMapHex>() { hex });
                    } else {
                        row_help[y].Add(hex);
                    }
                } else {
                    if (!row_help.ContainsKey(CombatManager.Instance.Map.Height - y)) {
                        row_help.Add(CombatManager.Instance.Map.Height - y, new List<CombatMapHex>() { hex });
                    } else {
                        row_help[CombatManager.Instance.Map.Height - y].Add(hex);
                    }
                }
            }
        }
        foreach (int i in row_help.Select(x => x.Key).OrderBy(x => x).ToList()) {
            rows.Add(row_help[i]);
        }

        Dictionary<Unit, DeploymentRole> roles = Assign_Deployment_Roles(army);
        if ((int)level <= (int)AI.Level.Easy) {
            Deploy_Default(rows, army, roles);
            return;
        }
        Dictionary<DeploymentRole, float> role_distribution_help = Helper.Instantiate_Dictionary<DeploymentRole>(1.0f);
        foreach (KeyValuePair<Unit, DeploymentRole> pair in roles) {
            role_distribution_help[pair.Value] += 1.0f;
        }
        Dictionary<DeploymentRole, float> role_distribution = new Dictionary<DeploymentRole, float>();
        foreach (KeyValuePair<DeploymentRole, float> pair in role_distribution_help) {
            role_distribution.Add(pair.Key, pair.Value / army.Units.Count);
        }

        if (role_distribution[DeploymentRole.Fast_Melee] >= 0.5f) {
            Deploy_Alpha_Strike(rows, army, roles);
        } else if (role_distribution[DeploymentRole.Slow_Ranged] + role_distribution[DeploymentRole.Fast_Ranged] >= 0.5f) {
            Deploy_Ranged_Offensive(rows, army, roles);
        } else {
            Deploy_Default(rows, army, roles);
        }
    }

    public static void Deploy_Default(List<List<CombatMapHex>> rows, Army army, Dictionary<Unit, DeploymentRole> roles)
    {
        int index = 0;
        int row_index = 0;
        bool left = false;
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Slow_Melee).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Slow melee
            int row_column = (rows[row_index].Count / 2) + (left ? -index : index);
            if (row_column < 0 || row_column >= rows[row_index].Count) {
                index = 0;
                left = false;
                row_index++;
            }
            if (!unit.Deploy(rows[row_index][(rows[row_index].Count / 2) + (left ? -index : index)])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            index = !left ? index + 1 : index;
            left = !left;
        }

        if (row_index == 0 && roles.Where(x => x.Value == DeploymentRole.Slow_Ranged).Select(x => x.Key).ToList().Count != 0) {
            row_index++;
            index = 0;
            left = false;
        }
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Slow_Ranged).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Slow ranged
            int row_column = (rows[row_index].Count / 2) + (left ? -index : index);
            if (row_column < 0 || row_column >= rows[row_index].Count) {
                index = 0;
                left = false;
                row_index++;
            }
            if (!unit.Deploy(rows[row_index][(rows[row_index].Count / 2) + (left ? -index : index)])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            index = !left ? index + 1 : index;
            left = !left;
        }

        row_index++;
        index = 0;
        left = true;
        int formation_width = 0;
        foreach (List<CombatMapHex> row in rows) {
            int unit_count = row.Where(x => x.Unit != null).ToList().Count;
            if (unit_count > formation_width) {
                formation_width = unit_count;
            }
        }
        if (rows[row_index].Count < formation_width) {
            formation_width = rows[row_index].Count;
        }
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Fast_Melee || x.Value == DeploymentRole.Fast_Ranged).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Fast units
            int left_position = ((rows[row_index].Count - formation_width) / 2) + index;
            int right_position = formation_width + ((rows[row_index].Count - formation_width) / 2) - index;
            int position = left ? left_position : right_position;
            if (position < 0 || position >= rows[row_index].Count || rows[row_index][position].Unit != null) {
                row_index++;
                index = 0;
                left = true;
                if (rows[row_index].Count < formation_width) {
                    formation_width = rows[row_index].Count;
                }
            }
            if (row_index >= rows.Count) {
                break;
            }
            if (!unit.Deploy(rows[row_index][position])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            left = !left;
            index = left ? index + 1 : index;
        }

        foreach (Unit unit in army.Units.Where(x => x.Hex == null).ToList()) {
            while (!unit.Deploy(CombatManager.Instance.Map.Random_Hex)) { }
        }
    }

    public static void Deploy_Alpha_Strike(List<List<CombatMapHex>> rows, Army army, Dictionary<Unit, DeploymentRole> roles)
    {
        int index = 0;
        int row_index = 0;
        bool left = false;
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Fast_Melee).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Fast melee
            int row_column = (rows[row_index].Count / 2) + (left ? -index : index);
            if (row_column < 0 || row_column >= rows[row_index].Count) {
                index = 0;
                left = false;
                row_index++;
            }
            if (!unit.Deploy(rows[row_index][(rows[row_index].Count / 2) + (left ? -index : index)])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            index = !left ? index + 1 : index;
            left = !left;
        }

        if (row_index == 0 && roles.Where(x => x.Value == DeploymentRole.Slow_Ranged || x.Value == DeploymentRole.Fast_Ranged).Select(x => x.Key).ToList().Count != 0) {
            row_index++;
            index = 0;
            left = false;
        }
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Slow_Ranged || x.Value == DeploymentRole.Fast_Ranged).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Ranged
            int row_column = (rows[row_index].Count / 2) + (left ? -index : index);
            if (row_column < 0 || row_column >= rows[row_index].Count) {
                index = 0;
                left = false;
                row_index++;
            }
            if (!unit.Deploy(rows[row_index][(rows[row_index].Count / 2) + (left ? -index : index)])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            index = !left ? index + 1 : index;
            left = !left;
        }

        row_index++;
        index = 0;
        left = false;
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Slow_Melee).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Slow melee
            int row_column = (rows[row_index].Count / 2) + (left ? -index : index);
            if (row_column < 0 || row_column >= rows[row_index].Count) {
                index = 0;
                left = false;
                row_index++;
            }
            if (!unit.Deploy(rows[row_index][(rows[row_index].Count / 2) + (left ? -index : index)])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            index = !left ? index + 1 : index;
            left = !left;
        }

        foreach (Unit unit in army.Units.Where(x => x.Hex == null).ToList()) {
            while (!unit.Deploy(CombatManager.Instance.Map.Random_Hex)) { }
        }
    }

    public static void Deploy_Ranged_Offensive(List<List<CombatMapHex>> rows, Army army, Dictionary<Unit, DeploymentRole> roles)
    {
        int index = 0;
        int row_index = 0;
        bool left = false;
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Slow_Ranged || x.Value == DeploymentRole.Fast_Ranged).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Ranged
            int row_column = (rows[row_index].Count / 2) + (left ? -index : index);
            if (row_column < 0 || row_column >= rows[row_index].Count) {
                index = 0;
                left = false;
                row_index++;
            }
            if (!unit.Deploy(rows[row_index][(rows[row_index].Count / 2) + (left ? -index : index)])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            index = !left ? index + 1 : index;
            left = !left;
        }

        if (row_index == 0 && roles.Where(x => x.Value == DeploymentRole.Slow_Melee).Select(x => x.Key).ToList().Count != 0) {
            row_index++;
            index = 0;
            left = false;
        }
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Slow_Melee).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Slow melee
            int row_column = (rows[row_index].Count / 2) + (left ? -index : index);
            if (row_column < 0 || row_column >= rows[row_index].Count) {
                index = 0;
                left = false;
                row_index++;
            }
            if (!unit.Deploy(rows[row_index][(rows[row_index].Count / 2) + (left ? -index : index)])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            index = !left ? index + 1 : index;
            left = !left;
        }

        row_index++;
        index = 0;
        left = true;
        int formation_width = 0;
        foreach (List<CombatMapHex> row in rows) {
            int unit_count = row.Where(x => x.Unit != null).ToList().Count;
            if (unit_count > formation_width) {
                formation_width = unit_count;
            }
        }
        if (rows[row_index].Count < formation_width) {
            formation_width = rows[row_index].Count;
        }
        foreach (Unit unit in roles.Where(x => x.Value == DeploymentRole.Fast_Melee).Select(x => x.Key).OrderByDescending(x => x.Relative_Strenght).ToList()) {
            //Fast melee
            int left_position = ((rows[row_index].Count - formation_width) / 2) + index;
            int right_position = formation_width + ((rows[row_index].Count - formation_width) / 2) - index;
            int position = left ? left_position : right_position;
            if (position < 0 || position >= rows[row_index].Count || rows[row_index][position].Unit != null) {
                row_index++;
                index = 0;
                left = true;
                if (rows[row_index].Count < formation_width) {
                    formation_width = rows[row_index].Count;
                }
            }
            if (row_index >= rows.Count) {
                break;
            }
            if (!unit.Deploy(rows[row_index][position])) {
                CustomLogger.Instance.Error("Failed to deploy unit {0} #{1}", unit.Name, unit.Id.ToString());
            }

            left = !left;
            index = left ? index + 1 : index;
        }

        foreach (Unit unit in army.Units.Where(x => x.Hex == null).ToList()) {
            while (!unit.Deploy(CombatManager.Instance.Map.Random_Hex)) { }
        }
    }

    private static Dictionary<Unit, DeploymentRole> Assign_Deployment_Roles(Army army)
    {
        Dictionary<Unit, DeploymentRole> roles = new Dictionary<Unit, DeploymentRole>();
        Dictionary<float, int> speed_tiers = new Dictionary<float, int>();
        foreach (Unit unit in army.Owner.Faction.Units.Where(x => x is Unit).Select(x => x as Unit).ToList()) {
            if (!speed_tiers.ContainsKey(unit.Max_Movement)) {
                speed_tiers.Add(unit.Can_Ranged_Attack && unit.Max_Movement < 2.0f ? 2.0f : unit.Max_Movement, 0);
            }
        }
        foreach (Unit unit in army.Units) {
            if (!speed_tiers.ContainsKey(unit.Max_Movement)) {
                speed_tiers.Add(unit.Can_Ranged_Attack && unit.Max_Movement < 2.0f ? 2.0f : unit.Max_Movement, 1);
            } else {
                speed_tiers[unit.Can_Ranged_Attack && unit.Max_Movement < 2.0f ? 2.0f : unit.Max_Movement]++;
            }
        }
        float base_speed = speed_tiers.OrderBy(x => x.Key).Select(x => x.Key).ToArray()[0];
        foreach (Unit unit in army.Units) {
            roles.Add(unit, unit.Can_Ranged_Attack ?
                (unit.Max_Movement <= base_speed ? DeploymentRole.Slow_Ranged : DeploymentRole.Fast_Ranged) :
                (unit.Max_Movement <= base_speed ? DeploymentRole.Slow_Melee : DeploymentRole.Fast_Melee));
        }
        return roles;
    }

    private class UnitTarget
    {
        public CombatMapHex Hex { get; set; }
        public Unit Unit { get; set; }
        public bool? Melee_Attack { get; set; }
        public string Action { get; set; }
        public bool Is_Move { get { return Hex != null; } }
        public bool Is_Attack { get { return Melee_Attack.HasValue; } }
        public bool Is_Action { get { return !string.IsNullOrEmpty(Action); } }
        public List<CombatMapHex> Path { get; set; }
        public bool Run { get; set; }

        public UnitTarget(CombatMapHex hex, bool run)
        {
            Hex = hex;
            Unit = null;
            Melee_Attack = null;
            Action = null;
            Run = run;
        }

        public UnitTarget(Unit unit, bool melee_attack, bool run)
        {
            Hex = null;
            Unit = unit;
            Melee_Attack = melee_attack;
            Action = null;
            Path = null;
            Run = run;
        }

        public UnitTarget(Unit unit, string action, bool run)
        {
            Hex = null;
            Unit = unit;
            Melee_Attack = null;
            Action = action;
            Path = null;
            Run = run;
        }
    }
}
