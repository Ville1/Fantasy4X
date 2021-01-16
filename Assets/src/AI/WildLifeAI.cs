using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class WildLifeAI : IConfigListener, I_AI
{
    private static readonly float SPAWN_RATE = 0.01f;//0.0 - 1.0f
    private static readonly float WOLF_AGGRESSION = 0.5f;
    private static readonly int WOLF_WORKABLE_HEX_TRUCE = 15;
    private static readonly int WOLF_CHASE_TRUCE = 35;

    public Player Player { get; private set; }
    public bool Log_Actions { get; set; }
    public List<AI.LogType> Logged_Action_Types { get; set; }
    public AI.Level AI_Level { get { return AI.Level.Inactive; } }
    public bool Show_Moves { get; set; }
    public bool Follow_Moves { get; set; }
    public float Time_Between_Actions { get; set; }

    private float act_cooldown;
    private bool last_action_was_visible;
    private Dictionary<Army, ArmyWaitData> waiting_armies;
    private int performance_heavy_actions_taken;

    public WildLifeAI(Player player)
    {
        Log_Actions = true;
        Logged_Action_Types = new List<AI.LogType>() { AI.LogType.General, AI.LogType.Military, AI.LogType.Diagnostic };
        Player = player;
        Show_Moves = AI.Default_Show_Moves;
        ConfigManager.Instance.Register_Listener(this);
        act_cooldown = 0.0f;
        waiting_armies = new Dictionary<Army, ArmyWaitData>();
    }

    public void Update_Settings()
    {
        Time_Between_Actions = ConfigManager.Instance.Current_Config.AI_Action_Delay;
        Follow_Moves = ConfigManager.Instance.Current_Config.AI_Follow_Moves;
    }

    public void Start_Turn()
    {
        //TODO: This call should not be needed
        Main.Instance.Update_Flags();
        performance_heavy_actions_taken = 0;

        Log(string.Format("---- {0}: starting turn ----", Player.Name), AI.LogType.General);
        Stopwatch stopwatch = Stopwatch.StartNew();
        act_cooldown = 0.0f;
        
        //Spawn armies
        int max_spawns = Mathf.RoundToInt(World.Instance.Map.Hex_Count * SPAWN_RATE) - Player.WorldMapEntitys.Count + 1;
        int spawn_count = 0;
        int max_iterations = 5000 + Mathf.RoundToInt(50000 * SPAWN_RATE);
        int iteration = 0;
        while (spawn_count < max_spawns && iteration < max_iterations) {
            iteration++;
            WorldMapHex random_hex = World.Instance.Map.Random_Hex;
            if (random_hex.Entity != null || random_hex.Is_Water) {
                continue;
            }
            bool explored = false;
            foreach (Player player in Main.Instance.Players) {
                if (random_hex.Is_Explored_By(player)) {
                    explored = true;
                    break;
                }
            }
            if (explored) {
                continue;
            }
            Unit wolf_prototype = Player.Faction.Units.First(x => x.Name == "Wolves") as Unit;
            Army army = new Army(random_hex, Player.Faction.Army_Prototype, Player, new Unit(wolf_prototype));
            random_hex.Entity = army;
            int wolves = 1;
            if (Main.Instance.Round > 50) {
                wolves += RNG.Instance.Next(0, 2);
            } else if (Main.Instance.Round > 25) {
                wolves += RNG.Instance.Next(0, 1);
            }
            while (army.Units.Count < wolves) {
                army.Add_Unit(new Unit(wolf_prototype));
            }
            if (Show_Moves) {
                World.Instance.Map.Update_LoS(army);
            }
            spawn_count++;
        }
        Log(string.Format("{0} arm{1} spawned", spawn_count, (spawn_count == 1 ? "y" : "ies")), AI.LogType.Military);

        //Waiting armies
        List<Army> done_waiting = new List<Army>();
        foreach(KeyValuePair<Army, ArmyWaitData> pair in waiting_armies) {
            pair.Value.Turns_Left--;
            if(pair.Value.Turns_Left <= 0 || pair.Key.Hex == null) {
                done_waiting.Add(pair.Key);
            }
        }
        foreach(Army army in done_waiting) {
            waiting_armies.Remove(army);
        }
        Log(string.Format("{0} arm{1} waiting", waiting_armies.Count, (waiting_armies.Count == 1 ? "y" : "ies")), AI.LogType.Military);

        Log(string.Format("Start turn: {0} ms", stopwatch.ElapsedMilliseconds), AI.LogType.Diagnostic);
    }

    public void Act(float delta_s)
    {
        act_cooldown -= delta_s;
        if (act_cooldown > 0.0f) {
            return;
        }
        act_cooldown += last_action_was_visible || Show_Moves ? Time_Between_Actions : 0.0f;

        Stopwatch stopwatch = Stopwatch.StartNew();

        //Move units
        foreach (WorldMapEntity entity in Player.WorldMapEntitys) {
            if (entity.Wait_Turn) {
                continue;
            }
            if(entity is Army) {
                Manage_Wolves(entity as Army);
                return;
            }
        }

        Log(string.Format("---- {0}: ending turn ----", Player.Name), AI.LogType.General);
        Main.Instance.Next_Turn();
    }

    private void Manage_Wolves(Army army)
    {
        Log("--- Managing wolves #" + army.Id + " ---", AI.LogType.Military);
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        last_action_was_visible = false;
        List<WorldMapHex> adjancent_hexes = army.Hex.Get_Adjancent_Hexes();
        foreach(WorldMapHex adjancent_hex in adjancent_hexes) {
            if (!adjancent_hex.Passable_For(army) || !Can_Enter_Hex(adjancent_hex)) {
                continue;
            }
            if(adjancent_hex.Civilian != null && !adjancent_hex.Civilian.Is_Owned_By(Player) && adjancent_hex.Army == null) {
                Log(string.Format("Attacking adjacent civilian #{0}", adjancent_hex.Civilian.Id), AI.LogType.Military);
                if (!army.Move(adjancent_hex)) {
                    CustomLogger.Instance.Error("Failed to move");
                    army.Wait_Turn = true;
                }
            }
            if(adjancent_hex.Army != null && !adjancent_hex.Army.Is_Owned_By(Player) && !adjancent_hex.Army.Owner.Is_Neutral && army.Get_Relative_Strenght_When_On_Hex(adjancent_hex, true, true) * WOLF_AGGRESSION >
                adjancent_hex.Army.Get_Relative_Strenght_When_On_Hex(adjancent_hex, true, false)) {
                Log(string.Format("Attacking adjacent army #{0}", adjancent_hex.Army.Id), AI.LogType.Military);
                army.Attack(adjancent_hex.Army);
            }
        }
        if (army.Current_Movement <= 0.0f) {
            army.Wait_Turn = true;
        }
        if (waiting_armies.ContainsKey(army)) {
            Log(string.Format("Waiting {0} turn{1} left", waiting_armies[army].Turns_Left, Helper.Plural(waiting_armies[army].Turns_Left)), AI.LogType.Military);
            army.Wait_Turn = true;
        }
        
        if (!army.Wait_Turn) {
            if (army.Has_Stored_Path) {
                Log("Moving", AI.LogType.Military);
                if (!army.Follow_Stored_Path()) {
                    army.Wait_Turn = true;
                    army.Clear_Stored_Path();
                    CustomLogger.Instance.Error("Failed to move");
                } else {
                    last_action_was_visible = army.Hex.Visible_To_Viewing_Player;
                    Update_Spectator_View_On_Move(army);
                    if (!army.Has_Stored_Path) {
                        waiting_armies.Add(army, new ArmyWaitData() { Turns_Left = 10 });
                    }
                }
            } else {
                if (performance_heavy_actions_taken > 0) {
                    army.Wait_Turn = true;
                } else {
                    List<WorldMapHex> hexes_in_los = army.Get_Hexes_In_LoS();
                    Dictionary<WorldMapHex, List<PathfindingNode>> reachable_hexes = new Dictionary<WorldMapHex, List<PathfindingNode>>();
                    foreach (WorldMapHex hex_in_los in hexes_in_los) {
                        if (!hex_in_los.Passable_For(army) || (hex_in_los.Entity != null && hex_in_los.Entity.Is_Owned_By(Player)) || !Can_Enter_Hex(hex_in_los)) {
                            continue;
                        }
                        List<PathfindingNode> path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army), hex_in_los.Get_Specific_PathfindingNode(army));
                        if (path.Count != 0) {
                            reachable_hexes.Add(hex_in_los, path);
                        }
                    }
                    performance_heavy_actions_taken++;
                    if (reachable_hexes.Count == 0) {
                        army.Wait_Turn = true;
                        Log("Can't reach any hexes", AI.LogType.Military);
                        waiting_armies.Add(army, new ArmyWaitData() { Turns_Left = 20 });
                    } else {
                        WorldMapHex target = null;
                        List<WorldMapHex> empty_reachable_hexes = new List<WorldMapHex>();

                        foreach (KeyValuePair<WorldMapHex, List<PathfindingNode>> reachable_hex in reachable_hexes) {
                            if (reachable_hex.Key.Civilian == null && reachable_hex.Key.Entity == null) {
                                empty_reachable_hexes.Add(reachable_hex.Key);
                            }
                            if (reachable_hex.Key.Civilian != null && !reachable_hex.Key.Civilian.Is_Owned_By(Player) &&
                                    reachable_hex.Key.Army == null && Main.Instance.Round > WOLF_CHASE_TRUCE) {
                                target = reachable_hex.Key;
                                Log(string.Format("Targeting civilian #{0}", target.Civilian.Id), AI.LogType.Military);
                                break;
                            }
                        }

                        if (target == null && Main.Instance.Round > WOLF_CHASE_TRUCE) {
                            foreach (KeyValuePair<WorldMapHex, List<PathfindingNode>> reachable_hex in reachable_hexes) {
                                if (reachable_hex.Key.Army != null && !reachable_hex.Key.Army.Is_Owned_By(Player) &&
                                        army.Get_Relative_Strenght_When_On_Hex(target, true, true) * WOLF_AGGRESSION > reachable_hex.Key.Army.Get_Relative_Strenght_When_On_Hex(target, true, false)) {
                                    target = reachable_hex.Key;
                                    Log(string.Format("Targeting army #{0}", target.Army.Id), AI.LogType.Military);
                                    break;
                                }
                            }
                        }

                        if (target == null && empty_reachable_hexes.Count != 0) {
                            target = empty_reachable_hexes[RNG.Instance.Next(0, empty_reachable_hexes.Count - 1)];
                            Log(string.Format("Targeting random hex {0}", target.Coordinates.ToString()), AI.LogType.Military);
                        }
                        if (target == null) {
                            army.Wait_Turn = true;
                            Log("Failed to find a target", AI.LogType.Military);
                        } else {
                            army.Create_Stored_Path(reachable_hexes[target]);
                            if (!army.Follow_Stored_Path()) {
                                army.Wait_Turn = true;
                                CustomLogger.Instance.Error("Failed to move");
                            } else {
                                last_action_was_visible = army.Hex.Visible_To_Viewing_Player;
                                Update_Spectator_View_On_Move(army);
                            }
                        }
                    }
                }
            }

            if(army.Current_Movement <= 0.0f) {
                army.Wait_Turn = true;
            }
        }

        Log(string.Format("Manage wolves: {0} ms", stopwatch.ElapsedMilliseconds), AI.LogType.Diagnostic);
    }

    /// <summary>
    /// TODO: copy-pasta
    /// </summary>
    /// <param name="entity"></param>
    private void Update_Spectator_View_On_Move(WorldMapEntity entity)
    {
        if ((Show_Moves || entity.Hex.Visible_To_Viewing_Player) && Follow_Moves) {
            BottomGUIManager.Instance.Current_Entity = entity;
            CameraManager.Instance.Set_Camera_Location(entity.Hex);
        }
    }

    private bool Can_Enter_Hex(WorldMapHex hex)
    {
        return hex.City == null && hex.Village == null && (Main.Instance.Round > WOLF_WORKABLE_HEX_TRUCE || !hex.In_Work_Range_Of.Any(x => !x.Owner.Is_Neutral));
    }

    public void Combat_Act(float delta_s)
    {
        if (!CombatManager.Instance.Active_Combat) {
            CustomLogger.Instance.Error("There is no combat going on");
            return;
        }
        act_cooldown -= delta_s;
        if (act_cooldown > 0.0f) {
            return;
        }
        act_cooldown += Time_Between_Actions;

        Unit unit = null;
        Army army = CombatManager.Instance.Army_1.Is_Owned_By(Player) ? CombatManager.Instance.Army_1 : CombatManager.Instance.Army_2;
        Army enemy_army = CombatManager.Instance.Army_1.Is_Owned_By(Player) ? CombatManager.Instance.Army_2 : CombatManager.Instance.Army_1;

        if (CombatManager.Instance.Deployment_Mode) {
            foreach (Unit u in army.Units) {
                CombatMapHex hex = CombatManager.Instance.Map.Random_Hex;
                while (!u.Deploy(hex)) {
                    hex = CombatManager.Instance.Map.Random_Hex;
                }
            }
            CombatManager.Instance.Next_Turn();
            return;
        }

        foreach (Unit u in army.Units) {
            if (u.Controllable && !u.Wait_Turn) {
                unit = u;
                break;
            }
        }

        if (unit == null) {
            CombatManager.Instance.Next_Turn();
            return;
        }

        if (CombatUIManager.Instance.Active) {
            CombatUIManager.Instance.Current_Unit = unit;
        }

        //Zombie AI
        //Attacking
        List<CombatMapHex> hexes_in_range = unit.Hex.Get_Adjancent_Hexes();
        if (unit.Can_Ranged_Attack) {
            hexes_in_range = unit.Get_Hexes_In_Attack_Range();
        }
        foreach (CombatMapHex hex in hexes_in_range) {
            if (hex.Unit != null && !hex.Unit.Army.Is_Owned_By(Player)) {
                if (unit.Attack(hex.Unit, false) != null) {
                    if (CombatUIManager.Instance.Active) {
                        CombatUIManager.Instance.Update_Current_Unit();
                    }
                    break;
                }
            }
        }
        if (!unit.Can_Attack && unit.Current_Movement <= 0.0f) {
            unit.Wait_Turn = true;
            return;
        }

        //Moving
        Unit closest_enemy = null;
        int closest_distance = -1;
        foreach (Unit enemy in enemy_army.Units) {
            if (closest_distance == -1 || (enemy.Hex.Distance(unit.Hex) < closest_distance)) {
                closest_enemy = enemy;
                closest_distance = enemy.Hex.Distance(unit.Hex);
            }
        }
        if (closest_enemy.Hex.Is_Adjancent_To(unit.Hex)) {
            unit.Wait_Turn = true;
            return;
        }
        if (!unit.Pathfind(closest_enemy.Hex, unit.Relative_Stamina >= 0.75f)) {
            //Path blocked
            unit.Wait_Turn = true;
        }
        if (CombatUIManager.Instance.Active) {
            CombatUIManager.Instance.Update_Current_Unit();
        }
    }

    public void On_Delete()
    {
        ConfigManager.Instance.Unregister_Listener(this);
    }

    private void Log(string message, AI.LogType type)
    {
        if (!Log_Actions || !Logged_Action_Types.Contains(type)) {
            return;
        }
        CustomLogger.Instance.Debug(message);
    }

    private class ArmyWaitData
    {
        public int Turns_Left { get; set; }
    }
}
