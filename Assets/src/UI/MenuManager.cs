using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour {
    public static MenuManager Instance { get; private set; }

    public Button Menu_Button;

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

    public void Menu_On_Click()
    {
        MainMenuManager.Instance.Toggle();
    }
}
