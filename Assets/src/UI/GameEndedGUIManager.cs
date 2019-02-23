using UnityEngine;
using UnityEngine.UI;

public class GameEndedGUIManager : MonoBehaviour {
    public static GameEndedGUIManager Instance;

    public GameObject Panel;
    public Text Player_Text;
    public Text Victory_Text;

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
            Panel.SetActive(value);
        }
    }

    public void Show(Player winner, string victory_type)
    {
        Active = true;
        Player_Text.text = string.Format("{0} is WINNER!", winner.Name);
        Victory_Text.text = string.Format("{0} Victory", victory_type);
    }

    public void New_Game_On_Click()
    {
        Active = false;
        NewGameGUIManager.Instance.Active = true;
    }

    public void Quit_On_Click()
    {
        Main.Quit();
    }
}
