using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class NewGameGUIManager : MonoBehaviour {
    private static readonly int DEFAULT_MAX_ROUNDS = 1000;
    private static readonly int MAX_PLAYERS = 6;
    private static readonly float PLAYER_ROW_HEIGHT = 30.0f;
    private static readonly int DEFAULT_WIDTH = 30;
    private static readonly int DEFAULT_HEIGHT = 30;
    private static readonly int DEFAULT_NEUTRAL_CITIES = 5;
    private static readonly int DEFAULT_MAX_VILLAGES = 10;

    public static NewGameGUIManager Instance;

    public InputField Max_Rounds_InputField;

    public InputField Width_InputField;
    public InputField Height_InputField;
    public InputField Neutral_Cities_InputField;
    public InputField Max_Villages_InputField;

    public GameObject Panel;
    public GameObject Player_Rows_GO;
    public GameObject Player_Row_Prototype;
    public Button Add_Player_Button;

    private List<GameObject> player_rows;
    private int player_row_index;
    private int bot_name_index;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
        Player_Row_Prototype.SetActive(false);
        player_rows = new List<GameObject>();
        player_row_index = 0;
        bot_name_index = 1;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    { }


    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if(Active == value) {
                return;
            }
            Panel.SetActive(value);
            if (Active) {
                MasterUIManager.Instance.Show_UI = false;

                //Reset UI
                foreach (GameObject row in player_rows) {
                    GameObject.Destroy(row);
                }
                player_rows.Clear();
                Add_Player_Button.transform.position = new Vector3(Add_Player_Button.transform.position.x, Player_Row_Prototype.transform.position.y - PLAYER_ROW_HEIGHT,
                    Player_Row_Prototype.transform.position.z);
                bot_name_index = 1;

                Max_Rounds_InputField.text = DEFAULT_MAX_ROUNDS.ToString();
                Width_InputField.text = DEFAULT_WIDTH.ToString();
                Height_InputField.text = DEFAULT_HEIGHT.ToString();
                Neutral_Cities_InputField.text = DEFAULT_NEUTRAL_CITIES.ToString();
                Max_Villages_InputField.text = DEFAULT_MAX_VILLAGES.ToString();

                //Add default players
                Add_Player();
                Add_Player();
            }
        }
    }

    public void Add_Player()
    {
        if(player_rows.Count >= MAX_PLAYERS) {
            return;
        }

        int default_faction_index = 0;
        AI.Level default_ai_level = AI.Level.Medium;
        Player.NewPlayerData player = player_rows.Count == 0 ?
            new Player.NewPlayerData("Anonymous", null, Factions.All[default_faction_index]) :
            new Player.NewPlayerData(string.Format("Bot {0}", bot_name_index), default_ai_level, Factions.All[default_faction_index]);
        
        GameObject new_row = GameObject.Instantiate(Player_Row_Prototype, Panel.transform);
        new_row.transform.position = new Vector3(new_row.transform.position.x, new_row.transform.position.y - (PLAYER_ROW_HEIGHT * player_rows.Count),
            new_row.transform.position.z);
        Add_Player_Button.transform.position = new Vector3(Add_Player_Button.transform.position.x, new_row.transform.position.y - PLAYER_ROW_HEIGHT,
            new_row.transform.position.z);
        new_row.SetActive(true);
        new_row.transform.SetParent(Player_Rows_GO.transform);
        new_row.name = string.Format("player_row_{0}", player_row_index);

        Get_NameInputField(new_row).text = player.Name;
        Get_FactionDropdown(new_row).options = Factions.All.Select(x => new Dropdown.OptionData(x.Name)).ToList();
        Get_FactionDropdown(new_row).value = default_faction_index;
        List<Dropdown.OptionData> ai_options = new List<Dropdown.OptionData>();
        ai_options.Add(new Dropdown.OptionData("Human"));
        foreach(string option_s in Enum.GetNames(typeof(AI.Level))) {
            ai_options.Add(new Dropdown.OptionData(option_s));
        }
        Get_AIDropdown(new_row).options = ai_options;
        Get_AIDropdown(new_row).value = player.AI == null ? 0 : ((int)default_ai_level + 1);

        Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
        on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
            Delete_Player(new_row);
        }));
        Get_RemoveButton(new_row).onClick = on_click_event;

        player_rows.Add(new_row);
        player_row_index++;
        if(player.AI != null) {
            bot_name_index++;
        }
    }

    private void Delete_Player(GameObject row)
    {
        for(int i = player_rows.IndexOf(row) + 1; i < player_rows.Count; i++) {
            player_rows[i].transform.position = new Vector3(player_rows[i].transform.position.x, player_rows[i].transform.position.y + PLAYER_ROW_HEIGHT,
                player_rows[i].transform.position.z);
        }
        Add_Player_Button.transform.position = new Vector3(Add_Player_Button.transform.position.x, Add_Player_Button.transform.position.y + PLAYER_ROW_HEIGHT,
            Add_Player_Button.transform.position.z);
        player_rows.Remove(row);
        GameObject.Destroy(row);
    }

    public void Start_New_Game()
    {
        int max_rounds, widht, height, neutral_cities, max_villages;
        if(!int.TryParse(Max_Rounds_InputField.text, out max_rounds) || !int.TryParse(Width_InputField.text, out widht)
            || !int.TryParse(Height_InputField.text, out height) || !int.TryParse(Neutral_Cities_InputField.text, out neutral_cities) ||
            !int.TryParse(Max_Villages_InputField.text, out max_villages)) {
            return;
        }
        if(max_rounds < 1 || widht < 10 || height < 10 || neutral_cities < 0 || max_villages < 0) {
            return;
        }

        List<Player.NewPlayerData> players = new List<Player.NewPlayerData>();
        foreach(GameObject row in player_rows) {
            Player.NewPlayerData player = new Player.NewPlayerData();
            player.Name = Get_NameInputField(row).text;
            if (string.IsNullOrEmpty(player.Name)) {
                return;
            }
            player.Faction = Factions.All[Get_FactionDropdown(row).value];
            player.AI = Get_AIDropdown(row).value == 0 ? (AI.Level?)null : (AI.Level)(Get_AIDropdown(row).value - 1);
            players.Add(player);
        }
        Active = false;
        World.Instance.Generate_New_Map(widht, height);
        Main.Instance.Start_New_Game(players, max_rounds, neutral_cities, max_villages);
    }

    private InputField Get_NameInputField(GameObject row)
    {
        return GameObject.Find(row.name + "/NameInputField").GetComponent<InputField>();
    }

    private Dropdown Get_FactionDropdown(GameObject row)
    {
        return GameObject.Find(row.name + "/FactionDropdown").GetComponent<Dropdown>();
    }

    private Dropdown Get_AIDropdown(GameObject row)
    {
        return GameObject.Find(row.name + "/AIDropdown").GetComponent<Dropdown>();
    }

    private Button Get_RemoveButton(GameObject row)
    {
        return GameObject.Find(row.name + "/RemoveButton").GetComponent<Button>();
    }
}
