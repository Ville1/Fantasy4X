using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    public Button Menu_Button;

    private bool in_combat_position;
    private Vector3 normal_position;

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
        in_combat_position = false;
        normal_position = new Vector3(
            Menu_Button.transform.position.x,
            Menu_Button.transform.position.y,
            Menu_Button.transform.position.z
        );
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    { }

    public bool Active
    {
        get {
            return Menu_Button.gameObject.activeSelf;
        }
        set {
            Menu_Button.gameObject.SetActive(value);
        }
    }

    public bool Interactable
    {
        get {
            return Menu_Button.interactable;
        }
        set {
            Menu_Button.interactable = value;
        }
    }

    public bool Combat_Position
    {
        get {
            return in_combat_position;
        }
        set {
            in_combat_position = value;
            Menu_Button.transform.position = new Vector3(
                normal_position.x - (in_combat_position ? 450.0f : 0.0f),
                normal_position.y,
                normal_position.z
            );
        }
    }

    public void Menu_On_Click()
    {
        MainMenuManager.Instance.Toggle();
    }
}
