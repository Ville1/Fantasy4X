using UnityEngine;
using UnityEngine.UI;

public class CombatTopPanelManager : MonoBehaviour {
    public static CombatTopPanelManager Instance;

    public GameObject Panel;

    public Image Balance_Bar_Blue;
    public Text Attacker_Name_Text;
    public Text Attacker_Strenght_Text;
    public Text Attacker_Manpower_Text;
    public Text Attacker_Morale_Text;
    public Text Defender_Name_Text;
    public Text Defender_Strenght_Text;
    public Text Defender_Manpower_Text;
    public Text Defender_Morale_Text;

    private float bar_lenght;

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
        bar_lenght = Balance_Bar_Blue.GetComponentInChildren<RectTransform>().sizeDelta.x;
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

    public void Update_GUI()
    {
        if (!Active) {
            return;
        }
        Attacker_Name_Text.text = CombatManager.Instance.Army_1.Owner.Faction.Name;
        float attacker_strenght = CombatManager.Instance.Army_1.Get_Relative_Strenght_When_On_Hex(CombatManager.Instance.Hex, true, true);
        Attacker_Strenght_Text.text = Helper.Float_To_String(attacker_strenght, 0);
        Attacker_Manpower_Text.text = string.Format("{0}%", Helper.Float_To_String(CombatManager.Instance.Army_1.Average_Manpower * 100.0f, 0));
        Attacker_Morale_Text.text = string.Format("{0}%", Helper.Float_To_String(CombatManager.Instance.Army_1.Average_Morale * 100.0f, 0));

        Defender_Name_Text.text = CombatManager.Instance.Army_2.Owner.Faction.Name;
        float defender_strenght = CombatManager.Instance.Army_2.Get_Relative_Strenght_When_On_Hex(CombatManager.Instance.Hex, true, false);
        Defender_Strenght_Text.text = Helper.Float_To_String(defender_strenght, 0);
        Defender_Manpower_Text.text = string.Format("{0}%", Helper.Float_To_String(CombatManager.Instance.Army_2.Average_Manpower * 100.0f, 0));
        Defender_Morale_Text.text = string.Format("{0}%", Helper.Float_To_String(CombatManager.Instance.Army_2.Average_Morale * 100.0f, 0));

        float blue_strenght = attacker_strenght;
        float red_strenght = defender_strenght;
        if(CombatManager.Instance.Army_1.Owner.AI != null && CombatManager.Instance.Army_2.Owner.AI == null) {
            blue_strenght = defender_strenght;
            red_strenght = attacker_strenght;
        }
        Balance_Bar_Blue.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (blue_strenght / (blue_strenght + red_strenght)) * bar_lenght);
    }
}
