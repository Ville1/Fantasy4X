using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleManager : MonoBehaviour
{
    private static int max_lines = 10;
    private static char echo_command_start = '[';
    private static char echo_command_end = ']';
    private static char argument_variable_prefix = '$';

    public static ConsoleManager Instance;
    public GameObject Panel;
    public Text Output;
    public InputField Input;

    private List<string> output_log;
    private int scroll_position;
    private delegate string Command(string[] arguments);
    private Dictionary<string, Command> commands;
    private List<string> command_history;
    private int command_history_index;
    private Dictionary<string, string> variables;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;

        output_log = new List<string>();
        output_log.Add("Console!");
        scroll_position = 0;
        command_history = new List<string>();
        command_history_index = 0;
        variables = new Dictionary<string, string>();

        commands = new Dictionary<string, Command>();
        commands.Add("exit", (string[] arguments) => {
            Close_Console();
            return "";
        });
        commands.Add("echo", (string[] arguments) => {
            if (arguments.Length == 1) {
                return "Missing argument";
            }
            StringBuilder builder = new StringBuilder();
            for (int i = 1; i < arguments.Length; i++) {
                builder.Append(arguments[i]);
                builder.Append(" ");
            }
            string full_input = builder.ToString();
            builder = new StringBuilder();
            StringBuilder command_builder = new StringBuilder();
            bool in_command = false;
            for (int i = 0; i < full_input.Length; i++) {
                if (full_input[i] == echo_command_start && !in_command) {
                    in_command = true;
                } else if (full_input[i] == echo_command_end && in_command) {
                    in_command = false;
                    if (command_builder.Length != 0) {
                        string[] parts = command_builder.ToString().Split(' ');
                        if (commands.ContainsKey(parts[0])) {
                            builder.Append(commands[parts[0]](parts));
                        } else if (variables.ContainsKey(parts[0])) {
                            builder.Append(variables[parts[0]]);
                        } else {
                            builder.Append("Invalid command!");
                        }
                    }
                    command_builder = new StringBuilder();
                } else {
                    if (in_command) {
                        command_builder.Append(full_input[i]);
                    } else {
                        builder.Append(full_input[i]);
                    }
                }
            }

            return builder.ToString();
        });
        commands.Add("version", (string[] arguments) => {
            return ("Game: " + Main.VERSION + " Unity: " + Application.unityVersion);
        });
        commands.Add("kill", (string[] arguments) => {
            Application.Quit();
            return "";
        });
        commands.Add("list", (string[] arguments) => {
            StringBuilder list = new StringBuilder();
            foreach(KeyValuePair<string, Command> command in commands) {
                list.Append(command.Key).Append(", ");
            }
            list.Remove(list.Length - 2, 2);
            return list.ToString();
        });

        commands.Add("set_variable", (string[] arguments) => {
            if (arguments.Length != 3) {
                return "Invalid number of arguments";
            }
            if(commands.ContainsKey(arguments[1]) || ConsoleScriptManager.Instance.Script_Exists(arguments[1])) {
                return "Reserved name";
            }
            if (variables.ContainsKey(arguments[1])) {
                variables[arguments[1]] = arguments[2];
            } else {
                variables.Add(arguments[1], arguments[2]);
            }
            return string.Format("{0} = {1}", arguments[1], arguments[2]);
        });
        commands.Add("remove_variable", (string[] arguments) => {
            if (arguments.Length != 2) {
                return "Invalid number of arguments";
            }
            if (!variables.ContainsKey(arguments[1])) {
                return "Variable does not exist";
            }
            variables.Remove(arguments[1]);
            return "Variable removed";
        });

        commands.Add("instant_research", (string[] arguments) => {
            if(Main.Instance.Current_Player.Current_Technology == null) {
                return "No technology selected";
            }
            Main.Instance.Current_Player.Current_Technology.Research_Acquired = Main.Instance.Current_Player.Current_Technology.Research_Required;
            TopGUIManager.Instance.Update_GUI();
            return string.Format("{0} tech finished", Main.Instance.Current_Player.Current_Technology.Name);
        });
        commands.Add("instant_build", (string[] arguments) => {
            int builds = 0;
            foreach(City city in Main.Instance.Current_Player.Cities) {
                if(city.Building_Under_Production != null) {
                    city.Building_Production_Acquired = city.Building_Under_Production.Production_Required;
                    builds++;
                }
            }
            return string.Format("{0} building{1} finished", builds, Helper.Plural(builds));
        });
        commands.Add("instant_train", (string[] arguments) => {
            int trains = 0;
            foreach (City city in Main.Instance.Current_Player.Cities) {
                if (city.Unit_Under_Production != null) {
                    city.Unit_Production_Acquired = city.Unit_Under_Production.Production_Required;
                    trains++;
                }
            }
            return string.Format("{0} unit{1} finished", trains, Helper.Plural(trains));
        });
        commands.Add("give_cash", (string[] arguments) => {
            if(arguments.Length != 2) {
                return "Invalid number of arguments";
            }
            float amount;
            if(!float.TryParse(arguments[1], out amount)) {
                return "Invalid argument";
            }
            Main.Instance.Current_Player.Cash += amount;
            return string.Format("+{0} cash", amount);
        });

        commands.Add("spawn_army", (string[] arguments) => {
            if (arguments.Length != 3) {
                return "Invalid number of arguments";
            }
            int player_index;
            string[] unit_strings = arguments[2].Split(',');
            if (!int.TryParse(arguments[1], out player_index)) {
                return "Invalid argument";
            }
            if((player_index < 0 || player_index >= Main.Instance.Players.Count) || unit_strings.Length == 0) {
                return "Invalid argument";
            }
            if(HexPanelManager.Instance.Hex == null) {
                return "Select a hex";
            }
            if(HexPanelManager.Instance.Hex.Entity != null) {
                return "Hex already contains an entity";
            }
            Player player = Main.Instance.Players[player_index];
            Dictionary<Unit, int> units = new Dictionary<Unit, int>();
            foreach(string unit_string in unit_strings) {
                int count = 1;
                string[] split = unit_string.Split(':');
                string name = split[0].Replace('_', ' ');
                if(split.Length > 1) {
                    if (!int.TryParse(split[1], out count)) {
                        return "Invalid argument";
                    }
                }
                Trainable trainable = player.Faction.Units.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
                if(trainable == null || !(trainable is Unit) || units.ContainsKey(trainable as Unit)) {
                    return "Invalid argument";
                }
                units.Add(trainable as Unit, count);
            }
            bool out_of_space = false;
            Army army = new Army(HexPanelManager.Instance.Hex, player.Faction.Army_Prototype, player, null);
            foreach(KeyValuePair<Unit, int> unit_data in units) {
                for(int i = 0; i < unit_data.Value; i++) {
                    if(!army.Add_Unit(new Unit(unit_data.Key))) {
                        out_of_space = true;
                    }
                }
            }
            HexPanelManager.Instance.Hex.Entity = army;
            if (army.Is_Owned_By_Current_Player) {
                army.Hex.Current_LoS = WorldMapHex.LoS_Status.Visible;
                TopGUIManager.Instance.Update_GUI();
            }
            return out_of_space ? "Army spawned (not enough space for all units)" : "Army spawned";
        });

        commands.Add("show_ai_moves", (string[] arguments) => {
            if (arguments.Length != 2 && arguments.Length != 3) {
                return "Invalid number of arguments";
            }
            bool b;
            if(!bool.TryParse(arguments[1], out b)) {
                return "Invalid argument";
            }
            int index = -1;
            if(arguments.Length == 3) {
                if (!int.TryParse(arguments[2], out index)) {
                    return "Invalid argument";
                } else {
                    if(index < 0 || index >= Main.Instance.Players.Count) {
                        return "Invalid argument";
                    }
                    Player p = Main.Instance.Players[index];
                    if(p.AI == null) {
                        return string.Format("Player {0} is not an AI", index);
                    }
                    p.AI.Show_Moves = b;
                    return string.Format("Player {0}: Show_Moves = {1}", index, b);
                }
            }
            foreach(Player p in Main.Instance.Players) {
                if(p.AI != null) {
                    p.AI.Show_Moves = b;
                }
            }
            AI.Default_Show_Moves = b;
            return string.Format("AI-Players: Show_Moves = {0}", b);
        });

        commands.Add("pause_ai", (string[] arguments) => {
            if (arguments.Length != 1 && arguments.Length != 2) {
                return "Invalid number of arguments";
            }
            bool b = !Main.Instance.Pause_AI;
            if (arguments.Length == 2 && !bool.TryParse(arguments[1], out b)) {
                return "Invalid argument";
            }
            Main.Instance.Pause_AI = b;
            return string.Format("AI {0}", Main.Instance.Pause_AI ? "paused" : "unpaused");
        });

        commands.Add("log_ai_actions", (string[] arguments) => {
            if (arguments.Length != 2 && arguments.Length != 3) {
                return "Invalid number of arguments";
            }
            bool b;
            if (!bool.TryParse(arguments[1], out b)) {
                return "Invalid argument";
            }
            int index = -1;
            if (arguments.Length == 3) {
                if (!int.TryParse(arguments[2], out index)) {
                    return "Invalid argument";
                } else {
                    if (index < 0 || index >= Main.Instance.Players.Count) {
                        return "Invalid argument";
                    }
                    Player p = Main.Instance.Players[index];
                    if (p.AI == null) {
                        return string.Format("Player {0} is not an AI", index);
                    }
                    p.AI.Log_Actions = b;
                    return string.Format("Player {0}: Log_Actions = {1}", index, b);
                }
            }
            foreach (Player p in Main.Instance.Players) {
                if (p.AI != null) {
                    p.AI.Log_Actions = b;
                }
            }
            return string.Format("AI-Players: Log_Actions = {0}", b);
        });

        Update_Output();
        Panel.SetActive(false);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {

    }

    /// <summary>
    /// Opens console
    /// </summary>
    public void Open_Console()
    {
        if (Panel.activeSelf) {
            return;
        }
        MasterUIManager.Instance.Close_All();
        Panel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
        Input.OnPointerClick(new PointerEventData(EventSystem.current));
    }

    /// <summary>
    /// Close console
    /// </summary>
    public void Close_Console()
    {
        if (Panel.activeSelf == false) {
            return;
        }
        Panel.SetActive(false);
    }

    /// <summary>
    /// Open / close console
    /// </summary>
    public void Toggle_Console()
    {
        if (Panel.activeSelf) {
            Close_Console();
        } else {
            Open_Console();
        }
    }

    /// <summary>
    /// Run command from program
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public bool Run_Command(string command)
    {
        Input.text = command;
        return Run_Command(false);
    }

    /// <summary>
    /// Runs a command currently typed into Input
    /// </summary>
    /// <param name="add_to_history"></param>
    /// <returns></returns>
    public bool Run_Command(bool add_to_history = true)
    {
        if (Input.text == string.Empty) {
            return false;
        }
        string[] parts = Input.text.Split(' ');
        //Read arguments from variables
        for(int i = 1; i < parts.Length; i++) {
            if(parts[i][0] == argument_variable_prefix && parts[i].Length > 1) {
                if (variables.ContainsKey(parts[i].Substring(1))) {
                    parts[i] = variables[parts[i].Substring(1)];
                }
            }
        }
        if (!commands.ContainsKey(parts[0])) {
            if (variables.ContainsKey(parts[0])) {
                //Read variable
                output_log.Add(variables[parts[0]]);
                Input.text = string.Empty;
            } else if (!ConsoleScriptManager.Instance.Run_Script(parts[0])) { //Run script
                output_log.Add("Invalid command!");
            }
        } else {
            //Run command
            if (add_to_history) {
                command_history.Insert(0, Input.text);
            }
            string log = commands[parts[0]](parts);
            if (!string.IsNullOrEmpty(log)) {
                output_log.Add(log);
            }
            Input.text = string.Empty;
        }
        Update_Output();
        EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
        Input.OnPointerClick(new PointerEventData(EventSystem.current));
        return true;
    }

    /// <summary>
    /// Scroll up
    /// </summary>
    /// <returns></returns>
    public bool Scroll_Up()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        scroll_position++;
        if (output_log.Count - max_lines - 2 - scroll_position < 0) {
            scroll_position--;
        }
        Update_Output();
        return true;
    }

    /// <summary>
    /// Scroll down
    /// </summary>
    /// <returns></returns>
    public bool Scroll_Down()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        scroll_position--;
        if (scroll_position < 0) {
            scroll_position = 0;
        }
        Update_Output();
        return true;
    }

    /// <summary>
    /// Scroll command history up
    /// </summary>
    /// <returns></returns>
    public bool Command_History_Up()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if (command_history.Count == 0) {
            return true;
        }
        command_history_index++;
        if (command_history_index > command_history.Count + 1) {
            command_history_index = command_history.Count + 1;
        }
        if (command_history.Count > command_history_index - 1) {
            Input.text = command_history[command_history_index - 1];
        } else {
            Input.text = "";
        }
        return true;
    }

    /// <summary>
    /// Scroll command history down
    /// </summary>
    /// <returns></returns>
    public bool Command_History_Down()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if (command_history.Count == 0) {
            return true;
        }
        command_history_index--;
        if (command_history_index < 0) {
            command_history_index = 0;
        }
        if (command_history.Count > command_history_index - 1 && command_history_index > 0) {
            Input.text = command_history[command_history_index - 1];
        } else {
            Input.text = "";
        }
        return true;
    }

    /// <summary>
    /// Try to autocomplete command name
    /// </summary>
    /// <returns></returns>
    public bool Auto_Complete()
    {
        if (!Panel.activeSelf) {
            return false;
        }
        if (Input.text == "") {
            return true;
        }

        string complete = "";
        foreach (KeyValuePair<string, Command> command in commands) {
            if (command.Key.StartsWith(Input.text)) {
                complete = command.Key;
                break;
            }
        }

        if (complete != "") {
            Input.text = complete;
        }

        return true;
    }

    /// <summary>
    /// Is console open?
    /// </summary>
    public bool Is_Open()
    {
        return Panel.activeSelf;
    }

    /// <summary>
    /// Updates console output field
    /// </summary>
    private void Update_Output()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < max_lines; i++) {
            if (output_log.Count > i - scroll_position) {
                builder.Insert(0, output_log[output_log.Count - i - 1 - scroll_position] + "\n");
            }
        }
        Output.text = builder.ToString();
        command_history_index = 0;
    }
}
