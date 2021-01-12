using System;
using System.Collections.Generic;
using System.Diagnostics;

public class BanditAI : IConfigListener, I_AI
{
    private static readonly float SPAWN_RATE = 0.05f;
    
    public Player Player { get; private set; }
    public bool Log_Actions { get; set; }
    public List<AI.LogType> Logged_Action_Types { get; set; }
    public AI.Level AI_Level { get; private set; }
    public bool Show_Moves { get; set; }
    public bool Follow_Moves { get; set; }
    public float Time_Between_Actions { get; set; }

    public BanditAI(Player player, AI.Level level)
    {
        Log_Actions = true;
        Logged_Action_Types = new List<AI.LogType>() { AI.LogType.General, AI.LogType.Military };
        Player = player;
        AI_Level = level;
        Show_Moves = AI.Default_Show_Moves;
        ConfigManager.Instance.Register_Listener(this);
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

        Log(string.Format("---- {0}: starting turn ----", Player.Name), AI.LogType.General);
        Stopwatch stopwatch = Stopwatch.StartNew();

        //Spawn armies
        /*int max_spawns = Mathf.RoundToInt(World.Instance.Map.Hex_Count * SPAWN_RATE) - Player.WorldMapEntitys.Count + 1;
        int spawn_count = 0;
        int max_iterations = 5000 + Mathf.RoundToInt(50000 * SPAWN_RATE);
        int iteration = 0;
        while (spawn_count < max_spawns && iteration < max_iterations) {
            iteration++;
            WorldMapHex random_hex = World.Instance.Map.Random_Hex;
            if (random_hex.Entity != null) {
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
            //TODO: spawn
            spawn_count++;
        }
        Log(string.Format("{0} arm{1} spawned", spawn_count, (spawn_count == 1 ? "y" : "ies")), AI.LogType.Military);*/

        Log(string.Format("Start turn: {0} ms", stopwatch.ElapsedMilliseconds), AI.LogType.Diagnostic);
    }

    public void Act(float delta_s)
    {
        Log(string.Format("---- {0}: ending turn ----", Player.Name), AI.LogType.General);
        Main.Instance.Next_Turn();
    }

    public void Combat_Act(float delta_s)
    {
        throw new NotImplementedException();
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
}
