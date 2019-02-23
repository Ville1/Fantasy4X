using UnityEngine;

public class PrefabManager : MonoBehaviour {
    public static PrefabManager Instance { get; private set; }

    public GameObject World_Map_Hex;
    public GameObject Floating_Text;
    public GameObject Floating_Text_Morale;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Warning(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update () {
		
	}
}
