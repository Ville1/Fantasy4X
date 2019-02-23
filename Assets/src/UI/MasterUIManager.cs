using UnityEngine;

public class MasterUIManager : MonoBehaviour {
    public static MasterUIManager Instance { get; private set; }

    private bool show_ui;
    private bool combat_ui;
    private bool show_scores;

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
        Show_UI = false;
        show_scores = true;
        MenuManager.Instance.Active = true;
        TopGUIManager.Instance.Active = false;
        NewGameGUIManager.Instance.Active = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    { }

    /// <summary>
    /// Closes all menus and UI elements
    /// </summary>
    public void Close_All()
    {
        ConsoleManager.Instance.Close_Console();
        MainMenuManager.Instance.Active = false;
        HexPanelManager.Instance.Active = false;
        CityGUIManager.Instance.Active = false;
        TechnologyPanelManager.Instance.Active = false;
        SelectTechnologyPanelManager.Instance.Active = false;
        NewGameGUIManager.Instance.Active = false;
        MouseManager.Instance.Set_Select_Hex_Mode(false);
    }

    public bool Intercept_Keyboard_Input
    {
        get {
            return ConsoleManager.Instance.Is_Open();
        }
    }

    public bool Show_UI
    {
        get {
            return show_ui;
        }
        set {
            show_ui = value;
            BottomGUIManager.Instance.Active = show_ui;
            TopGUIManager.Instance.Active = show_ui;
            NotificationManager.Instance.Active = show_ui;
            ScoresGUIManager.Instance.Active = Show_Scores && show_ui;
        }
    }

    public bool Combat_UI
    {
        get {
            return combat_ui;
        }
        set {
            combat_ui = value;
            CombatUIManager.Instance.Active = combat_ui;
            CombatLogManager.Instance.Active = combat_ui;
            BottomGUIManager.Instance.Active = !combat_ui;
            TopGUIManager.Instance.Active = !combat_ui;
            NotificationManager.Instance.Active = !combat_ui;
            ScoresGUIManager.Instance.Active = show_scores && !combat_ui;
            if(combat_ui && HexPanelManager.Instance.Active) {
                HexPanelManager.Instance.Active = false;
            }
            if(BottomGUIManager.Instance.Current_Entity != null) {
                SelectionCircle.Instance.Active = !combat_ui;
            }
        }
    }

    public bool Show_Scores
    {
        get {
            return show_scores;
        }
        set {
            show_scores = value;
            if (Show_UI) {
                ScoresGUIManager.Instance.Active = show_scores;
            }
        }
    }

    public void Read_Keyboard_Input()
    {
        if (ConsoleManager.Instance.Is_Open()) {
            if (Input.GetButtonDown("Submit")) {
                ConsoleManager.Instance.Run_Command();
            }
            if (Input.GetButtonDown("Console scroll down")) {
                ConsoleManager.Instance.Scroll_Down();
            }
            if (Input.GetButtonDown("Console scroll up")) {
                ConsoleManager.Instance.Scroll_Up();
            }
            if (Input.GetButtonDown("Auto complete")) {
                ConsoleManager.Instance.Auto_Complete();
            }
            if (Input.GetButtonDown("Console history up")) {
                ConsoleManager.Instance.Command_History_Up();
            }
            if (Input.GetButtonDown("Console history down")) {
                ConsoleManager.Instance.Command_History_Down();
            }
        }
    }
}
