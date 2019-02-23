﻿using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TODO: This propably does not need to be MonoBehaviour
/// </summary>
public class Main : MonoBehaviour {
    public static readonly float VERSION = 0.1f;
    private static readonly string MENU_MUSIC = "music_m";
    private static readonly string CAMPAIGN_MAP_MUSIC = "music_b";

    public static Main Instance;

    public List<Player> Players { get; private set; }
    public Player Current_Player { get; private set; }
    public Player Viewing_Player { get; private set; }
    public Player Neutral_Player { get; private set; }
    public bool Game_Is_Running { get; private set; }
    public bool Pause_AI { get; set; }
    public int Round { get; private set; }
    public int Max_Rounds { get; private set; }
    private int player_index;

	/// <summary>
    /// Initializiation
    /// </summary>
	private void Start () {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
        Players = new List<Player>();
        Game_Is_Running = false;
        AudioManager.Instance.Play_Music(MENU_MUSIC);
    }
	
	/// <summary>
    /// Per frame update
    /// </summary>
	private void Update () {
        if (!Game_Is_Running) {
            return;
        }
        if (Current_Player != null && Current_Player.AI != null && !CombatManager.Instance.Active_Combat && !Pause_AI) {
            Current_Player.AI.Act(Time.deltaTime);
        }
        EffectManager.Instance.Update(Time.deltaTime);
	}

    /// <summary>
    /// Normal players + neutral player
    /// </summary>
    public List<Player> All_Players
    {
        get {
            List<Player> list = new List<Player>() { Neutral_Player };
            foreach(Player player in Players) {
                list.Add(player);
            }
            return list;
        }
    }

    /// <summary>
    /// Exits the game
    /// </summary>
    public static void Quit()
    {
        Application.Quit();
    }

    /// <summary>
    /// Starts a new game
    /// </summary>
    public void Start_New_Game(List<Player.NewPlayerData> player_data, int max_rounds, int neutral_cities, int max_villages)
    {
        Game_Is_Running = true;
        MasterUIManager.Instance.Show_UI = true;
        GameEndedGUIManager.Instance.Active = false;
        foreach(Player p in Players) {
            if(p.AI != null) {
                p.AI.On_Delete();
            }
        }
        Players.Clear();
        foreach(Player.NewPlayerData data in player_data) {
            Players.Add(new Player(data.Name, data.AI, data.Faction));
        }
        Neutral_Player = new Player("Neutral Cities", (!Average_AI_Level.HasValue || Average_AI_Level == AI.Level.Inactive) ? AI.Level.Easy : Average_AI_Level,
            Factions.Neutral_Cities, true);
        Current_Player = Players[0];
        Viewing_Player = Current_Player;
        player_index = 0;
        Round = 0;
        Max_Rounds = max_rounds;
        World.Instance.Map.Spawn_Cities_And_Villages(neutral_cities, max_villages);
        EffectManager.Instance.Update_Target_Map();
        Turn_Start_Update_GUI();
        World.Instance.Map.Update_LoS();
        Update_Flags();
        AudioManager.Instance.Play_Music(CAMPAIGN_MAP_MUSIC);
        foreach(Player player in Players) {
            player.Update_Score();
        }
    }

    public void Next_Turn()
    {
        if (!Showning_AI_Moves) {
            MasterUIManager.Instance.Close_All();
        }
        PathRenderer.Instance.Clear_Path();
        BottomGUIManager.Instance.Current_Entity = null;
        Current_Player.End_Turn();
        foreach(Village village in World.Instance.Map.Villages) {
            village.Update_Owner();
        }
        player_index++;
        if(player_index >= Players.Count) {
            player_index = 0;
            Round++;
        }

        if(Round == Max_Rounds) {
            //Score victory
            Player winner = null;
            foreach(Player player in Players) {
                if (player.Defeated) {
                    continue;
                }
                if(winner == null || winner.Score < player.Score) {
                    winner = player;
                }
            }
            Victory(winner, "Score");
            return;
        }

        Current_Player = Players[player_index];
        while (Current_Player.Defeated) {
            player_index++;
            if (player_index >= Players.Count) {
                player_index = 0;
                Round++;
            }
            Current_Player = Players[player_index];
        }
        Current_Player.Start_Turn();
        if (!Other_Players_Turn) {
            Viewing_Player = Current_Player;
        }

        WaitingForPlayerGUIManager.Instance.Active = Other_Players_Turn;
        if (WaitingForPlayerGUIManager.Instance.Active) {
            WaitingForPlayerGUIManager.Instance.Player = Current_Player;
        }
        if(!Other_Players_Turn || Showning_AI_Moves) {
            Turn_Start_Update_GUI();
        }
        World.Instance.Map.Update_LoS();
        Update_Flags();
        if (Current_Player.AI != null) {
            Current_Player.AI.Start_Turn();
        }
    }

    /// <summary>
    /// TODO: Multiplayer, better name?
    /// </summary>
    public bool Other_Players_Turn
    {
        get {
            if(Current_Player == null) {
                return false;
            }
            return Current_Player.AI != null;
        }
    }

    public bool Showning_AI_Moves
    {
        get {
            if(Current_Player == null || Current_Player.AI == null) {
                return false;
            }
            return Current_Player.AI.Show_Moves || AIs_Only;
        }
    }

    public bool AIs_Only
    {
        get {
            foreach(Player p in Players) {
                if(p.AI == null) {
                    return false;
                }
            }
            return true;
        }
    }

    public AI.Level? Average_AI_Level
    {
        get {
            int total_level = 0;
            int ai_count = 0;
            foreach(Player player in Players) {
                if(player.AI != null) {
                    total_level += (int)player.AI.AI_Level;
                    ai_count++;
                }
            }
            if(ai_count == 0) {
                return null;
            }
            return (AI.Level)(Mathf.RoundToInt(total_level / (float)ai_count));
        }
    }

    /// <summary>
    /// Used for conquest victory
    /// </summary>
    /// <returns></returns>
    public bool Check_If_Game_Ended()
    {
        int active_players = 0;
        Player winner = null;
        foreach(Player player in Players) {
            if (!player.Defeated) {
                active_players++;
                winner = player;
            }
        }
        bool ended = active_players == 1;
        if (ended) {
            Victory(winner, "Conquest");
        }
        return ended;
    }

    private void Victory(Player winner, string victory_type)
    {
        Game_Is_Running = false;
        MasterUIManager.Instance.Close_All();
        MasterUIManager.Instance.Show_UI = false;
        GameEndedGUIManager.Instance.Show(winner, victory_type);
        WaitingForPlayerGUIManager.Instance.Active = false;
    }

    private void Turn_Start_Update_GUI()
    {
        BottomGUIManager.Instance.Start_Turn();
        CameraManager.Instance.Set_Camera_Location(BottomGUIManager.Instance.Current_Entity != null ?
            BottomGUIManager.Instance.Current_Entity.Hex : Current_Player.Capital.Hex);
        TopGUIManager.Instance.Update_GUI();
    }

    public void Update_Flags()
    {
        if(Current_Player.AI != null && !AIs_Only) {
            return;
        }
        foreach(Player player in All_Players) {
            foreach (WorldMapEntity entity in player.WorldMapEntitys) {
                entity.Flag.Update_Type();
            }
            foreach(City city in player.Cities) {
                city.Flag.Update_Type();
            }
            foreach (Village village in player.Villages) {
                village.Flag.Update_Type();
            }
        }
    }
}
