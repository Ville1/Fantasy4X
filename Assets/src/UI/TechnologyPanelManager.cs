using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyPanelManager : MonoBehaviour {
    private static Vector2 position_movement_speed = new Vector2(10.0f, 10.0f);
    public static TechnologyPanelManager Instance;

    private static int line_width = 5;

    public GameObject Panel;
    public Button Root_Technology_Button;
    public GameObject Line_Panel;
    public GameObject Buttons_GameObject;
    public GameObject Lines_GameObject;

    private Dictionary<Technology, Button> technology_buttons;
    private List<GameObject> lines;
    private Color cant_be_researched_color;
    private Color researched_color;
    private Color can_be_researched_color;
    private List<Technology> update_leads_to_links;
    private Vector2 position_delta;
    private Vector3 original_root_position;

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
        Line_Panel.SetActive(false);
        Root_Technology_Button.gameObject.SetActive(false);
        cant_be_researched_color = new Color(1.0f, 0.0f, 0.0f);
        researched_color = new Color(0.0f, 0.0f, 1.0f);
        can_be_researched_color = new Color(0.0f, 1.0f, 0.0f);
        technology_buttons = new Dictionary<Technology, Button>();
        lines = new List<GameObject>();
        position_delta = new Vector2(0.0f, 0.0f);
        original_root_position = new Vector3(Root_Technology_Button.transform.position.x, Root_Technology_Button.transform.position.y,
            Root_Technology_Button.transform.position.z);
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
            if(Main.Instance.Other_Players_Turn || Active == value) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_All();
            }
            Panel.SetActive(value);
            if (Active) {
                position_delta = new Vector2(0.0f, 0.0f);
                Update_Technologies();
            }
        }
    }

    public void Toggle()
    {
        Active = !Active;
    }

    public void Update_Technologies()
    {
        foreach(KeyValuePair<Technology, Button> pair in technology_buttons) {
            GameObject.Destroy(pair.Value.gameObject);
        }
        TooltipManager.Instance.Unregister_Tooltips_By_Owner(gameObject);
        technology_buttons.Clear();
        foreach(GameObject line in lines) {
            GameObject.Destroy(line);
        }
        lines.Clear();
        update_leads_to_links = new List<Technology>();
        int column = 0;
        Root_Technology_Button.transform.position = new Vector3(
            original_root_position.x + position_delta.x,
            original_root_position.y + position_delta.y,
            original_root_position.z
        );
        foreach (KeyValuePair<int, Technology> pair in Player.Root_Technology.Leads_To) {
            Update_Technologies_Recursive(pair.Value, column, pair.Key, Root_Technology_Button.transform.position.y);
        }
        foreach(Technology update_tech in update_leads_to_links) {
            foreach (KeyValuePair<int, Technology> pair in update_tech.Leads_To) {
                GameObject line = GameObject.Instantiate(Line_Panel);
                line.SetActive(true);
                line.name = "Line" + pair.Value.Name.Replace(" ", "");
                line.transform.SetParent(Lines_GameObject.transform, false);

                Vector3 differenceVector = technology_buttons[update_tech].transform.position - technology_buttons[pair.Value].transform.position;

                line.GetComponent<RectTransform>().sizeDelta = new Vector2(differenceVector.magnitude, line_width);
                line.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                line.GetComponent<RectTransform>().position = technology_buttons[update_tech].transform.position;
                float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
                line.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, (angle + 180.0f));
            }
        }
    }

    private void Update_Technologies_Recursive(Technology tech, int column, int position, float last_y)
    {
        int column_widht = 150;
        int row_height = 40;
        float new_y = last_y;
        Button button_go = null;

        if (!technology_buttons.ContainsKey(tech) && position < 7) {
            int estimated_turns = tech.Turns_Left_Estimate;
            Button new_button = GameObject.Instantiate<Button>(Root_Technology_Button);
            new_button.gameObject.SetActive(true);
            new_button.transform.SetParent(Buttons_GameObject.transform, false);
            new_button.name = string.Format("{0}Button", tech.Name.Replace(" ", ""));
            new_button.transform.position = new Vector3(
                Root_Technology_Button.transform.position.x + (column_widht * column),
                last_y + (-1.0f * (row_height * (position - 3))),
                Root_Technology_Button.transform.position.z
            );
            new_button.GetComponentInChildren<Text>().text = tech.Is_Researched ? tech.Name : string.Format("{0} ({1} turn{2})", tech.Name,
                estimated_turns, Helper.Plural(estimated_turns));
            new_button.interactable = tech.Can_Be_Researched;
            
            if(new_button.transform.position.x < 120.0f /*button width*/ * 1.35f || new_button.transform.position.x > 900.0f /*panel width*/ * 1.05f ||
                new_button.transform.position.y < 120.0f || new_button.transform.position.y > 550.0f) {//<- idk where these numers come from :P
                new_button.gameObject.SetActive(false);//Off screen
            }

            TooltipManager.Instance.Register_Tooltip(new_button.gameObject, tech.Tooltip, gameObject);

            new_y = new_button.transform.position.y;

            if (tech.Is_Researched) {
                new_button.image.color = researched_color;
            } else if (tech.Can_Be_Researched) {
                new_button.image.color = can_be_researched_color;
            } else {
                new_button.image.color = cant_be_researched_color;
            }
            Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
            Technology t = tech;
            on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                Player.Current_Technology = t;
                Active = false;
                TopGUIManager.Instance.Update_GUI();
                BottomGUIManager.Instance.Update_Next_Turn_Button_Text();
            }));
            new_button.GetComponentInChildren<Button>().onClick = on_click_event;

            technology_buttons.Add(tech, new_button);
            button_go = new_button;
        } else {
            if (technology_buttons.ContainsKey(tech)) {
                button_go = technology_buttons[tech];
            } else {
                update_leads_to_links.Add(tech);
            }
        }

        if(button_go != null && button_go.gameObject.activeSelf) {
            foreach (Technology prerequisite_tech in tech.Prequisites) {
                if (!technology_buttons.ContainsKey(prerequisite_tech)) {
                    continue;
                }
                GameObject line = GameObject.Instantiate(Line_Panel);
                line.SetActive(true);
                line.name = string.Format("Line_{0}_{1}", prerequisite_tech.Name.Replace(" ", ""), tech.Name.Replace(" ", ""));
                line.transform.SetParent(Lines_GameObject.transform, false);

                Vector3 differenceVector = technology_buttons[prerequisite_tech].transform.position - button_go.transform.position;

                line.GetComponent<RectTransform>().sizeDelta = new Vector2(differenceVector.magnitude, line_width);
                line.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                line.GetComponent<RectTransform>().position = technology_buttons[prerequisite_tech].transform.position;
                float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
                line.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, (angle + 180.0f));

                lines.Add(line);
            }
        }
        
        foreach (KeyValuePair<int, Technology> next_pair in tech.Leads_To) {
            Update_Technologies_Recursive(next_pair.Value, column + 1, next_pair.Key, new_y);
        }
    }

    public void Move_Up()
    {
        position_delta.y -= position_movement_speed.y;
        Update_Technologies();
    }

    public void Move_Down()
    {
        position_delta.y += position_movement_speed.y;
        Update_Technologies();
    }

    public void Move_Left()
    {
        position_delta.x += position_movement_speed.x;
        Update_Technologies();
    }

    public void Move_Right()
    {
        position_delta.x -= position_movement_speed.x;
        Update_Technologies();
    }
}
