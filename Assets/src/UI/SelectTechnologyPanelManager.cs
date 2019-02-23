using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectTechnologyPanelManager : MonoBehaviour {
    public static SelectTechnologyPanelManager Instance;

    public GameObject Panel;
    public Text New_Technology_Name_Text;
    public Text New_Technology_Info_Text;
    public Button Next_Technology_Button;

    private float normal_height;
    private float normal_width;
    private List<Button> button_GameObjects;

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
        normal_height = Panel.GetComponent<RectTransform>().rect.height;
        normal_width = Panel.GetComponent<RectTransform>().rect.width;
        button_GameObjects = new List<Button>();
        Panel.SetActive(false);
        Next_Technology_Button.gameObject.SetActive(false);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {

    }

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
            if (value && Player.Last_Technology_Researched == null) {
                CustomLogger.Instance.Warning("Failed to open SelectTechnologyPanelManager: Player.Last_Technology_Researched is null");
                return;
            }
            if(value && Main.Instance.Other_Players_Turn) {
                return;
            }
            Panel.SetActive(value);
            if (!Active) {
                TooltipManager.Instance.Unregister_Tooltips_By_Owner(gameObject);
                return;
            }
            New_Technology_Name_Text.text = Player.Last_Technology_Researched.Name;
            New_Technology_Info_Text.text = string.Join(Environment.NewLine, Player.Last_Technology_Researched.Get_Unlocks().ToArray());
            List<Technology> available_technologies = new List<Technology>();
            foreach(Technology technology in Player.Root_Technology.All_Techs_This_Leads_To) {
                if (technology.Can_Be_Researched) {
                    available_technologies.Add(technology);
                }
            }
            foreach(Button button in button_GameObjects) {
                GameObject.Destroy(button.gameObject);
            }
            float button_height = Next_Technology_Button.GetComponent<RectTransform>().rect.height;
            for(int i = 0; i < available_technologies.Count; i++) {
                Button new_button = GameObject.Instantiate<Button>(Next_Technology_Button);
                new_button.gameObject.SetActive(true);
                new_button.transform.SetParent(Panel.transform, false);
                new_button.name = string.Format("{0}Button", available_technologies[i].Name.Replace(" ", ""));
                new_button.transform.position = new Vector3(Next_Technology_Button.transform.position.x, Next_Technology_Button.transform.position.y - (i * button_height),
                    Next_Technology_Button.transform.position.z);
                new_button.GetComponentInChildren<Text>().text = string.Format("{0} ({1} turn{2})", available_technologies[i].Name,
                    available_technologies[i].Turns_Left_Estimate, Helper.Plural(available_technologies[i].Turns_Left_Estimate));
                
                TooltipManager.Instance.Register_Tooltip(new_button.gameObject, available_technologies[i].Tooltip, gameObject);

                Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
                Technology t = available_technologies[i];
                on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                    Player.Current_Technology = t;
                    Active = false;
                    TopGUIManager.Instance.Update_GUI();
                }));
                new_button.GetComponentInChildren<Button>().onClick = on_click_event;
            }

            Panel.GetComponent<RectTransform>().sizeDelta = new Vector2(normal_width, normal_height + (available_technologies.Count * button_height));
        }
    }
}
