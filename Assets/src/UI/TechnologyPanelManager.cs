using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TechnologyPanelManager : MonoBehaviour {
    public static TechnologyPanelManager Instance;

    private static readonly int COLUMN_WIDTH = 150;
    private static readonly int ROW_HEIGHT = 40;
    private static readonly int LINE_WIDTH = 5;

    public GameObject Panel;
    public GameObject Root_Technology_GameObject;
    public GameObject Line_Panel;
    public GameObject Buttons_GameObject;
    public GameObject Lines_GameObject;
    public GameObject Scroll_View_Content;

    private Dictionary<Technology, GameObject> technology_gameobjects;
    private List<GameObject> lines;
    private Color cant_be_researched_color;
    private Color researched_color;
    private Color can_be_researched_color;
    private List<Technology> update_leads_to_links;
    private long current_item_id;
    private float max_y;
    private float min_y;
    private int max_column;

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
        Root_Technology_GameObject.gameObject.SetActive(false);
        cant_be_researched_color = new Color(1.0f, 0.0f, 0.0f);
        researched_color = new Color(0.0f, 0.0f, 1.0f);
        can_be_researched_color = new Color(0.0f, 1.0f, 0.0f);
        technology_gameobjects = new Dictionary<Technology, GameObject>();
        lines = new List<GameObject>();
        current_item_id = 0;
        max_y = 0.0f;
        min_y = float.MaxValue;
        max_column = 0;
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
        Helper.Delete_All(technology_gameobjects);
        TooltipManager.Instance.Unregister_Tooltips_By_Owner(gameObject);
        technology_gameobjects.Clear();
        Helper.Delete_All(lines);
        lines.Clear();
        update_leads_to_links = new List<Technology>();
        int column = 0;
        max_y = 0.0f;
        min_y = float.MaxValue;
        max_column = 0;
        foreach (KeyValuePair<int, Technology> pair in Player.Root_Technology.Leads_To) {
            Update_Technologies_Recursive(pair.Value, column, pair.Key, Root_Technology_GameObject.transform.position.y);
        }
        foreach(Technology update_tech in update_leads_to_links) {
            foreach (KeyValuePair<int, Technology> pair in update_tech.Leads_To) {
                GameObject line = GameObject.Instantiate(Line_Panel);
                line.SetActive(true);
                line.name = string.Format("Line{0}#{1}", pair.Value.Name.Replace(" ", ""), current_item_id);
                current_item_id = current_item_id == long.MaxValue ? 0 : current_item_id + 1;
                line.transform.SetParent(Lines_GameObject.transform, false);

                Vector3 differenceVector = technology_gameobjects[update_tech].transform.position - technology_gameobjects[pair.Value].transform.position;

                line.GetComponent<RectTransform>().sizeDelta = new Vector2(differenceVector.magnitude, LINE_WIDTH);
                line.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                line.GetComponent<RectTransform>().position = technology_gameobjects[update_tech].transform.position;
                float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
                line.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, (angle + 180.0f));
            }
        }
        
        Scroll_View_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (max_column + 1) * COLUMN_WIDTH);
        Scroll_View_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, (2 * (int)Mathf.Max(max_y, -min_y)) + (2 * ROW_HEIGHT));
    }

    private void Update_Technologies_Recursive(Technology tech, int column, int position, float last_y)
    {
        float new_y = last_y;
        GameObject button_go = null;
        max_column = column > max_column ? column : max_column;

        if (!technology_gameobjects.ContainsKey(tech) && position < 7) {
            int estimated_turns = tech.Turns_Left_Estimate;
            GameObject tech_gameobject = GameObject.Instantiate(
                Root_Technology_GameObject,
                new Vector3(
                    Root_Technology_GameObject.transform.position.x + (COLUMN_WIDTH * column),
                    last_y + (-1.0f * (ROW_HEIGHT * (position - 3))),
                    Root_Technology_GameObject.transform.position.z
                ),
                Quaternion.identity,
                Buttons_GameObject.transform
            );
            tech_gameobject.gameObject.SetActive(true);
            tech_gameobject.name = string.Format("{0}Item#{1}", tech.Name.Replace(" ", ""), current_item_id);
            current_item_id = current_item_id == long.MaxValue ? 0 : current_item_id + 1;

            tech_gameobject.GetComponentInChildren<Text>().text = tech.Is_Researched ? tech.Name : string.Format("{0} ({1} turn{2})", tech.Name,
                estimated_turns, Helper.Plural(estimated_turns));
            Button button = tech_gameobject.GetComponentInChildren<Button>();
            Image background_image = GameObject.Find(string.Format("{0}/BackgroundImage", tech_gameobject.name)).GetComponentInChildren<Image>();
            button.interactable = tech.Can_Be_Researched;
            
            TooltipManager.Instance.Register_Tooltip(button.gameObject, tech.Tooltip, gameObject);

            new_y = tech_gameobject.transform.position.y;
            min_y = tech_gameobject.transform.localPosition.y < min_y ? tech_gameobject.transform.localPosition.y : min_y;
            max_y = tech_gameobject.transform.localPosition.y > max_y ? tech_gameobject.transform.localPosition.y : max_y;
            
            if (tech.Is_Researched) {
                background_image.color = researched_color;
            } else if (tech.Can_Be_Researched) {
                background_image.color = can_be_researched_color;
            } else {
                background_image.color = cant_be_researched_color;
            }
            Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
            Technology t = tech;
            on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                Player.Current_Technology = t;
                Active = false;
                TopGUIManager.Instance.Update_GUI();
                BottomGUIManager.Instance.Update_Next_Turn_Button_Text();
            }));
            tech_gameobject.GetComponentInChildren<Button>().onClick = on_click_event;
            
            GameObject icon_prototype = GameObject.Find(string.Format("{0}/Icons/IconPrototype", tech_gameobject.name));
            int index = 0;
            foreach(Technology.IconData icon_data in tech.UI_Icons(Player)) {
                GameObject new_icon = GameObject.Instantiate(
                    icon_prototype,
                    new Vector3(
                        icon_prototype.gameObject.transform.position.x + (index * 15.0f),
                        icon_prototype.gameObject.transform.position.y,
                        icon_prototype.gameObject.transform.position.z
                    ),
                    Quaternion.identity,
                    GameObject.Find(string.Format("{0}/Icons", tech_gameobject.name)).transform
                );
                new_icon.name = string.Format("Icon#{0}", current_item_id);
                current_item_id = current_item_id == long.MaxValue ? 0 : current_item_id + 1;
                new_icon.GetComponentInChildren<Image>().sprite = SpriteManager.Instance.Get(icon_data.Sprite, icon_data.Sprite_Type);
                TooltipManager.Instance.Register_Tooltip(new_icon, icon_data.Tooltip, gameObject);
                if(icon_data.On_Click != null) {
                    Helper.Set_Button_On_Click(new_icon.name, "SelectButton", delegate() { icon_data.On_Click(); });
                }
                index++;
            }
            icon_prototype.gameObject.SetActive(false);

            technology_gameobjects.Add(tech, tech_gameobject);
            button_go = tech_gameobject;
        } else {
            if (technology_gameobjects.ContainsKey(tech)) {
                button_go = technology_gameobjects[tech];
            } else {
                update_leads_to_links.Add(tech);
            }
        }

        if(button_go != null && button_go.gameObject.activeSelf) {
            foreach (Technology prerequisite_tech in tech.Prequisites) {
                if (!technology_gameobjects.ContainsKey(prerequisite_tech)) {
                    continue;
                }
                GameObject line = GameObject.Instantiate(Line_Panel);
                line.SetActive(true);
                line.name = string.Format("Line_{0}_{1}", prerequisite_tech.Name.Replace(" ", ""), tech.Name.Replace(" ", ""));
                line.transform.SetParent(Lines_GameObject.transform, false);

                Vector3 differenceVector = technology_gameobjects[prerequisite_tech].transform.position - button_go.transform.position;

                line.GetComponent<RectTransform>().sizeDelta = new Vector2(differenceVector.magnitude, LINE_WIDTH);
                line.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
                line.GetComponent<RectTransform>().position = technology_gameobjects[prerequisite_tech].transform.position;
                float angle = Mathf.Atan2(differenceVector.y, differenceVector.x) * Mathf.Rad2Deg;
                line.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, (angle + 180.0f));

                lines.Add(line);
            }
        }
        
        foreach (KeyValuePair<int, Technology> next_pair in tech.Leads_To) {
            Update_Technologies_Recursive(next_pair.Value, column + 1, next_pair.Key, new_y);
        }
    }
}
