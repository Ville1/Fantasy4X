using UnityEngine;
using UnityEngine.UI;

public class WaitingForPlayerGUIManager : MonoBehaviour {
    public static WaitingForPlayerGUIManager Instance;

    private static readonly float y_delta_when_spectating = 40.0f;

    public GameObject Panel;
    public Text Title_Text;
    public Text Name_Text;

    private Player player;
    private float original_y;

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
        original_y = Panel.transform.position.y;
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

    public Player Player
    {
        get {
            return player;
        }
        set {
            player = value;
            Name_Text.text = player.Name;
            Title_Text.text = Main.Instance.Showning_AI_Moves ? "Spectating" : "Waiting For";
            Panel.transform.position = new Vector3(
                Panel.transform.position.x,
                Main.Instance.Showning_AI_Moves ? (original_y + y_delta_when_spectating) : original_y,
                Panel.transform.position.z
            );
        }
    }
}
