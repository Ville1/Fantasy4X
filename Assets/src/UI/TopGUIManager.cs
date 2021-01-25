using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TopGUIManager : MonoBehaviour {
    public static TopGUIManager Instance;

    public GameObject Panel;
    public Text Cash_Text;
    public Button Technology_Button;
    public Button Spell_Button;
    public Button Blessing_Button;
    public Text Rounds_Text;

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

    private Player Player
    {
        get {
            return Main.Instance.Current_Player;
        }
    }

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
            Active = true;
        }
        Cash_Text.text = string.Format("{0} {1}", Mathf.RoundToInt(Player.Cash), Helper.Float_To_String(Player.Income, 1, true));
        Technology_Button.GetComponentInChildren<Text>().text = Player.Current_Technology != null ? string.Format("{0} ({1} turn{2})",
            Player.Current_Technology.Name, Player.Current_Technology.Turns_Left_Estimate,
            Helper.Plural(Player.Current_Technology.Turns_Left_Estimate)) : "Nothing";
        Spell_Button.GetComponentInChildren<Text>().text = string.Format("{0} ({1}) / {2}", Mathf.RoundToInt(Player.Mana), Helper.Float_To_String(Player.Mana_Income, 1, true),
            Mathf.RoundToInt(Player.Max_Mana));
        Dictionary<Blessing, int> active_blessings = Player.Active_Blessings;
        string faith_income_string = Helper.Float_To_String(Player.Faith_Income, 1, true);
        if (active_blessings.Count == 0) {
            Blessing_Button.GetComponentInChildren<Text>().text = string.Format("Nothing ({0})", faith_income_string);
        } else if(active_blessings.Count == 1) {
            Blessing blessing = active_blessings.First().Key;
            int duration = active_blessings.First().Value;
            Blessing_Button.GetComponentInChildren<Text>().text = string.Format("{0} ({1} turn{2}, {3})", blessing.Name, duration, Helper.Plural(duration), faith_income_string);
        } else {
            Blessing_Button.GetComponentInChildren<Text>().text = string.Format("Multiple: {0} ({1})", active_blessings.Count, faith_income_string);
        }

        Rounds_Text.text = string.Format("Round: {0}", Main.Instance.Round);
        TooltipManager.Instance.Register_Tooltip(Rounds_Text.gameObject, string.Format("Max: {0}", Main.Instance.Max_Rounds), gameObject);
    }

    public void Technology_Button_On_Click()
    {
        TechnologyPanelManager.Instance.Toggle();
    }

    public void Spell_Button_On_Click()
    {
        SpellGUIManager.Instance.Toggle();
    }

    public void Blessing_Button_On_Click()
    {
        BlessingGUIManager.Instance.Toggle();
    }
}
