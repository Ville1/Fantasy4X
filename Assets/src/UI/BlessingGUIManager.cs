using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlessingGUIManager : MonoBehaviour {
    public static BlessingGUIManager Instance;

    public GameObject Panel;
    public GameObject Blessing_GameObject;

    private List<GameObject> blessing_game_objects;
    private int current_row_id;

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
        Blessing_GameObject.SetActive(false);
        blessing_game_objects = new List<GameObject>();
        current_row_id = 0;
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
            if (Main.Instance.Other_Players_Turn || Active == value) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_All();
            }
            Panel.SetActive(value);
            if (Active) {
                Update_List();
            } else {
                Clear_List();
                current_row_id = 0;
            }
        }
    }

    public void Toggle()
    {
        Active = !Active;
    }

    private void Update_List()
    {
        Clear_List();
        //Weird stuff happens if Spell-GO is destroyed and then immediately recreated with same name
        List<Blessing> available_blessings = Main.Instance.Current_Player.Available_Blessings;
        float row_height = 20.0f;
        for (int i = 0; i < available_blessings.Count; i++) {
            Blessing blessing = available_blessings[i];
            GameObject go = GameObject.Instantiate(Blessing_GameObject);
            go.SetActive(true);
            go.transform.SetParent(Panel.transform, false);
            go.name = "Blessing" + current_row_id;
            current_row_id++;
            go.transform.position = new Vector3(Blessing_GameObject.transform.position.x,
                Blessing_GameObject.transform.position.y - (i * row_height), Blessing_GameObject.transform.position.z);
            Get_Text(go.name, "Name").text = blessing.Name;
            Get_Text(go.name, "FaithRequired").text = Helper.Float_To_String(blessing.Faith_Required, 1);
            Get_Text(go.name, "CastButton").text = Main.Instance.Current_Player.Blessing_Cooldown(blessing) > 0 ? string.Format("cd: {0}",
                Main.Instance.Current_Player.Blessing_Cooldown(blessing)) : "Cast";
            go.GetComponentInChildren<Button>().interactable = Main.Instance.Current_Player.Can_Cast(blessing);
            Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
            int index = i;
            on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                Blessing.BlessingResult result = blessing.Cast(Main.Instance.Current_Player);
                if (!string.IsNullOrEmpty(result.Message)) {
                    MessageManager.Instance.Show_Message(result.Message);
                }
                if (result.Success) {
                    Update_List();
                }
            }));
            go.GetComponentInChildren<Button>().onClick = on_click_event;
            blessing_game_objects.Add(go);
            TooltipManager.Instance.Register_Tooltip(Get_Text(go.name, "Name").gameObject, available_blessings[i].Tooltip, gameObject);
        }
    }

    private void Clear_List()
    {
        TooltipManager.Instance.Unregister_Tooltip(gameObject);
        foreach (GameObject go in blessing_game_objects) {
            GameObject.Destroy(go);
        }
        blessing_game_objects.Clear();
    }

    private Text Get_Text(string parent_game_object_name, string name)
    {
        return GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, name)).GetComponentInChildren<Text>();
    }
}
