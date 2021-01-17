using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour {
    public static MainMenuManager Instance { get; private set; }

    public GameObject Panel;
    public Button Custom_Battle_Button;

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
        Panel.SetActive(false);
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
            if((value && Panel.activeSelf) || (!value && !Panel.activeSelf)) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_All();
                Custom_Battle_Button.interactable = World.Instance.State == World.GameState.Menu;
            }
            Panel.SetActive(value);
        }
    }

    public void Toggle()
    {
        if (!ProgressBarManager.Instance.Active) {
            Active = !Active;
        }
    }

    public void New_Game_Button_On_Click()
    {
        NewGameGUIManager.Instance.Active = true;
        Active = false;
    }

    public void Exit_Button_On_Click()
    {
        Main.Quit();
    }

    public void Save_Button_On_Click()
    {
        SaveGUIManager.Instance.Active = true;
        Active = false;
    }

    public void Load_Button_On_Click()
    {
        LoadGUIManager.Instance.Active = true;
        Active = false;
    }

    public void Custom_Battle_Button_On_Click()
    {
        CustomBattleGUIManager.Instance.Active = true;
        Active = false;
    }

    public void Options_Button_On_Click()
    {
        MessageManager.Instance.Show_Message("Not implemented");
    }
}
