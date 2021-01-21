using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectTechnologyPanelManager : MonoBehaviour {
    public static SelectTechnologyPanelManager Instance;

    public GameObject Panel;
    public Text New_Technology_Name_Text;
    public GameObject Icon_List_Container;
    public GameObject Icon_Prototype;
    public GameObject Options_Scroll_View_Content;
    public GameObject Options_Scroll_View_Row_Prototype;

    private long current_item_id;
    private List<GameObject> icon_gameobjects;
    private RowScrollView<Technology> options_scroll_view;

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
        current_item_id = 0;
        icon_gameobjects = new List<GameObject>();
        Icon_Prototype.SetActive(false);
        options_scroll_view = new RowScrollView<Technology>("options_scroll_view", Options_Scroll_View_Content, Options_Scroll_View_Row_Prototype, 40.0f);
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

    private Technology Tech
    {
        get {
            return Player.Last_Technology_Researched;
        }
    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if (value && Tech == null) {
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

            New_Technology_Name_Text.text = Tech.Name;
            Helper.Delete_All(icon_gameobjects);
            List<Technology.IconData> icons = Tech.UI_Icons(Player);
            for (int i = 0; i < icons.Count; i++) {
                GameObject icon_gameobject = GameObject.Instantiate(
                    Icon_Prototype,
                    new Vector3(
                        Icon_Prototype.transform.position.x + (i * 20.0f),
                        Icon_Prototype.transform.position.y,
                        Icon_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Icon_List_Container.transform
                );
                icon_gameobject.name = string.Format("Icon{0}", current_item_id);
                current_item_id = current_item_id == long.MaxValue ? 0 : current_item_id + 1;
                icon_gameobject.SetActive(true);
                Helper.Set_Image(icon_gameobject.name, "Image", icons[i].Sprite, icons[i].Sprite_Type);
                if(icons[i].On_Click != null) {
                    Technology.IconData d = icons[i];
                    Helper.Set_Button_On_Click(icon_gameobject.name, "SelectButton", delegate() {
                        d.On_Click();
                    });
                }
                if (icons[i].Sprite_Type == SpriteManager.SpriteType.Improvement) {
                    icon_gameobject.GetComponentInChildren<Image>().GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40.0f);
                }
                TooltipManager.Instance.Register_Tooltip(icon_gameobject.GetComponentInChildren<Image>().gameObject, icons[i].Tooltip, gameObject);
                icon_gameobjects.Add(icon_gameobject);
            }

            List<Technology> available_technologies = new List<Technology>();
            foreach(Technology technology in Player.Root_Technology.All_Techs_This_Leads_To) {
                if (technology.Can_Be_Researched) {
                    available_technologies.Add(technology);
                }
            }
            options_scroll_view.Clear();
            foreach(Technology technology in available_technologies) {
                options_scroll_view.Add(technology, new List<UIElementData>() {
                    new UIElementData("NameText", string.Format("{0} ({1} turn{2})", technology.Name, (new Technology(technology, Player)).Turns_Left_Estimate, Helper.Plural((new Technology(technology, Player)).Turns_Left_Estimate))),
                    new UIElementData("SelectButton", null, delegate() {
                        Player.Current_Technology = technology;
                        Active = false;
                        TopGUIManager.Instance.Update_GUI();
                    })
                });
                GameObject row_gameobject = options_scroll_view.Get(technology);
                GameObject icon_container = GameObject.Find(string.Format("{0}/Icons", row_gameobject.name));
                GameObject icon_prototype = GameObject.Find(string.Format("{0}/{1}/IconPrototype", row_gameobject.name, icon_container.name));
                int index = 0;
                foreach(Technology.IconData icon_data in technology.UI_Icons(Player)) {
                    GameObject icon_gameobject = GameObject.Instantiate(
                        icon_prototype,
                        new Vector3(
                            icon_prototype.transform.position.x + (index * 17.0f),
                            icon_prototype.transform.position.y,
                            icon_prototype.transform.position.z
                        ),
                        Quaternion.identity,
                        icon_container.transform
                    );
                    icon_gameobject.name = string.Format("Icon{0}", current_item_id);
                    current_item_id = current_item_id == long.MaxValue ? 0 : current_item_id + 1;
                    icon_gameobject.SetActive(true);
                    Helper.Set_Image(icon_gameobject.name, "Image", icon_data.Sprite, icon_data.Sprite_Type);
                    if (icon_data.On_Click != null) {
                        Helper.Set_Button_On_Click(icon_gameobject.name, "SelectButton", delegate () {
                            icon_data.On_Click();
                        });
                    }
                    if (icon_data.Sprite_Type == SpriteManager.SpriteType.Improvement) {
                        icon_gameobject.GetComponentInChildren<Image>().GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 34.0f);
                    }
                    TooltipManager.Instance.Register_Tooltip(icon_gameobject.GetComponentInChildren<Image>().gameObject, icon_data.Tooltip, gameObject);
                    index++;
                }
                icon_prototype.SetActive(false);
            }
        }
    }

    public void Close()
    {
        Active = false;
    }
}