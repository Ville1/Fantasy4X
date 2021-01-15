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
        Close_Others(string.Empty);
    }

    public void Close_Others(string type_name)
    {
        if (ConsoleManager.Instance != null && typeof(ConsoleManager).Name != type_name) {
            ConsoleManager.Instance.Close_Console();
        }
        if (MainMenuManager.Instance != null && typeof(MainMenuManager).Name != type_name) {
            MainMenuManager.Instance.Active = false;
        }
        if (SaveGUIManager.Instance != null && typeof(SaveGUIManager).Name != type_name) {
            SaveGUIManager.Instance.Active = false;
        }
        if (LoadGUIManager.Instance != null && typeof(LoadGUIManager).Name != type_name) {
            LoadGUIManager.Instance.Active = false;
        }
        if (HexPanelManager.Instance != null && typeof(HexPanelManager).Name != type_name) {
            HexPanelManager.Instance.Active = false;
        }
        if (CityGUIManager.Instance != null && typeof(CityGUIManager).Name != type_name) {
            CityGUIManager.Instance.Active = false;
        }
        if (TechnologyPanelManager.Instance != null && typeof(TechnologyPanelManager).Name != type_name) {
            TechnologyPanelManager.Instance.Active = false;
        }
        if (SelectTechnologyPanelManager.Instance != null && typeof(SelectTechnologyPanelManager).Name != type_name) {
            SelectTechnologyPanelManager.Instance.Active = false;
        }
        if (NewGameGUIManager.Instance != null && typeof(NewGameGUIManager).Name != type_name) {
            NewGameGUIManager.Instance.Active = false;
        }
        if (SpellGUIManager.Instance != null && typeof(SpellGUIManager).Name != type_name) {
            SpellGUIManager.Instance.Active = false;
        }
        if (BlessingGUIManager.Instance != null && typeof(BlessingGUIManager).Name != type_name) {
            BlessingGUIManager.Instance.Active = false;
        }
        MouseManager.Instance.Set_Select_Hex_Mode(false);
    }

    public bool Intercept_Keyboard_Input
    {
        get {
            return ConsoleManager.Instance.Is_Open() || SaveGUIManager.Instance.Active || LoadGUIManager.Instance.Active || ProgressBarManager.Instance.Active;
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
            if (combat_ui) {
                WaitingForPlayerGUIManager.Instance.Active = false;
            } else if (!combat_ui && Main.Instance.Other_Players_Turn) {
                WaitingForPlayerGUIManager.Instance.Active = true;
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
