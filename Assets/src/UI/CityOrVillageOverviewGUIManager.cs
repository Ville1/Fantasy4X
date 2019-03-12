using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CityOrVillageOverviewGUIManager : MonoBehaviour {
    public static CityOrVillageOverviewGUIManager Instance;
    
    public GameObject Panel;
    public Text Name_Text;
    public Text Influence_Text;
    public Text Village_Yields_Label_Text;
    public Text Village_Yields_Text;

    private TradePartner current;

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
        Active = false;
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
            if (value == Active) {
                return;
            }
            Panel.SetActive(value);
        }
    }

    public TradePartner Current
    {
        get {
            return current;
        }
        set {
            current = value;
            Active = current != null;
            if (Active) {
                Update_Contents();
            }
        }
    }

    private void Update_Contents()
    {
        if(Current == null) {
            return;
        }
        Name_Text.text = Current.Name;
        Influencable current_i = Current as Influencable;
        float influence_percent = 0.0f;
        if (current_i.Cultural_Influence.ContainsKey(Main.Instance.Viewing_Player)) {
            float total_influence = 0.0f;
            foreach (KeyValuePair<Player, float> influence_data in current_i.Cultural_Influence) {
                total_influence += influence_data.Value;
            }
            influence_percent = 100.0f * (current_i.Cultural_Influence[Main.Instance.Viewing_Player] / total_influence);
        }
        Influence_Text.text = string.Format("{0}%", Helper.Float_To_String(influence_percent, 1));

        if(Current is Village) {
            Village_Yields_Visible = true;
            Village current_v = Current as Village;
            Village_Yields_Text.text = current_v.Yields.Generate_String(false);
        } else {
            Village_Yields_Visible = false;
        }
    }

    private bool Village_Yields_Visible
    {
        get {
            return Village_Yields_Label_Text.gameObject.activeSelf;
        }
        set {
            Village_Yields_Label_Text.gameObject.SetActive(value);
            Village_Yields_Text.gameObject.SetActive(value);
        }
    }
}
