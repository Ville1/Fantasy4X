using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BottomGUIManager : MonoBehaviour
{
    public static BottomGUIManager Instance;

    public GameObject Panel;
    public GameObject Current_Entity_GameObject;
    public Image Current_Entity_Image;
    public Text Current_Entity_Name_Text;
    public Text Current_Entity_Movement_Text;
    public Text Current_Entity_Info_Text;
    public GameObject Current_Entity_Transport_Container;
    public Text Current_Entity_Transport_Text;
    public Button Next_Button;
    public Button Wait_Button;
    public Button Sleep_Button;
    public Button Info_Button;

    public GameObject Action_GameObject;

    public GameObject Units_GameObject;
    public GameObject Unit_GameObject;

    public Button Next_Turn_Button;

    public List<Unit> Selected_Units { get; private set; }

    private WorldMapEntity current_entity;
    private Color default_text_color;
    private Color enemy_name_text_color;
    private List<GameObject> actions;
    private List<GameObject> units;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        default_text_color = Current_Entity_Name_Text.color;
        enemy_name_text_color = new Color(1.0f, 0.0f, 0.0f);
        Action_GameObject.SetActive(false);
        Unit_GameObject.SetActive(false);
        Instance = this;
        Current_Entity = null;

        Selected_Units = new List<Unit>();
        actions = new List<GameObject>();
        units = new List<GameObject>();
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        SelectionCircle.Instance.Update(Time.deltaTime);
    }

    public void Next_Turn_On_Click()
    {
        if (KeyboardManager.Instance.Shift_Down) {
            Main.Instance.Next_Turn();
            return;
        }
        if (Has_Things_Todo(true)) {
            return;
        }
        Main.Instance.Next_Turn();
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
    
    public WorldMapEntity Current_Entity
    {
        get {
            return current_entity;
        }
        set {
            current_entity = value;
            if (Selected_Units != null) {
                Selected_Units.Clear();
            }
            if (current_entity == null) {
                SelectionCircle.Instance.Entity = null;
                Current_Entity_GameObject.SetActive(false);
                if(actions != null) {
                    foreach (GameObject go in actions) {
                        GameObject.Destroy(go);
                    }
                    actions.Clear();
                }
                if(units != null) {
                    foreach (GameObject go in units) {
                        GameObject.Destroy(go);
                    }
                    units.Clear();
                }
                if(World.Instance.Map != null) {
                    World.Instance.Map.Map_Mode = WorldMapHex.InfoText.None;
                    PathRenderer.Instance.Clear_Path();
                }
            } else {
                SelectionCircle.Instance.Entity = Current_Entity;
                Current_Entity_GameObject.SetActive(true);
                Update_Current_Entity();
                if(Current_Entity is Prospector && !Main.Instance.Other_Players_Turn) {
                    World.Instance.Map.Map_Mode = WorldMapHex.InfoText.Minerals;
                } else {
                    World.Instance.Map.Map_Mode = WorldMapHex.InfoText.None;
                }
                if (!Main.Instance.Other_Players_Turn && Current_Entity.Stored_Path != null) {
                    PathRenderer.Instance.Render_Path(Current_Entity.Hex.Coordinates, Current_Entity.Stored_Path);
                }
            }
        }
    }

    public void Start_Turn()
    {
        Update_Buttons();
        Next_Unit();
    }

    public void Update_Current_Entity()
    {
        if(Current_Entity == null) {
            CustomLogger.Instance.Warning("Current entity is not set");
            return;
        }

        if (!Main.Instance.Game_Is_Running) {
            //Happens sometimes on AI turns, when ai wins the game
            return;
        }
        if (CombatManager.Instance.Active_Combat) {
            return;
        }

        Update_Entity_Info();
        Update_Actions();
        Update_Buttons();
        
        foreach (GameObject go in units) {
            TooltipManager.Instance.Unregister_Tooltip(go.GetComponentInChildren<Button>().gameObject);
            GameObject.Destroy(go);
        }
        units.Clear();

        if (Current_Entity is Army) {
            Units_GameObject.transform.position = new Vector3(Action_GameObject.transform.position.x + (Action_GameObject.GetComponent<RectTransform>().rect.width * actions.Count),
                Units_GameObject.transform.position.y, Units_GameObject.transform.position.z);

            Army army = Current_Entity as Army;
            for (int i = 0; i < army.Units.Count; i++) {
                GameObject go = GameObject.Instantiate(Unit_GameObject);
                go.SetActive(true);
                go.name = string.Format("{0}Unit(ID:{1})", army.Units[i].Name, army.Units[i].Id);
                go.transform.SetParent(Units_GameObject.transform, false);
                go.transform.position = new Vector3(Unit_GameObject.transform.position.x + (go.GetComponent<RectTransform>().rect.width * i),
                    Unit_GameObject.transform.position.y, Unit_GameObject.transform.position.z);

                Image selected_image = GameObject.Find(go.name + "/SelectedImage").GetComponent<Image>();
                selected_image.gameObject.SetActive(Selected_Units.Contains(army.Units[i]));

                Button button = go.GetComponentInChildren<Button>();
                button.image.overrideSprite = SpriteManager.Instance.Get(army.Units[i].Texture, SpriteManager.SpriteType.Unit);

                Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
                Unit u = army.Units[i];
                on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                    Toggle_Unit_Selection(selected_image, u);
                }));
                button.onClick = on_click_event;

                TooltipManager.Instance.Register_Tooltip(button.gameObject, army.Units[i].Tooltip, gameObject);

                units.Add(go);
            }
        }
    }

    public void Update_Entity_Info()
    {
        if (Current_Entity == null) {
            CustomLogger.Instance.Warning("Current entity is not set");
            return;
        }
        TooltipManager.Instance.Unregister_Tooltips_By_Owner(gameObject);

        Update_Next_Turn_Button_Text();

        Current_Entity_Image.overrideSprite = SpriteManager.Instance.Get(current_entity.Texture, SpriteManager.SpriteType.Unit);
        Current_Entity_Name_Text.text = Current_Entity.Is_Owned_By_Current_Player ? current_entity.Name : string.Format("Enemy {0}", current_entity.Name);
        Current_Entity_Name_Text.color = Current_Entity.Is_Owned_By_Current_Player ? default_text_color : enemy_name_text_color;
        Current_Entity_Movement_Text.text = string.Format("{0} / {1}", Helper.Float_To_String(current_entity.Current_Movement, 1), current_entity.Max_Movement);
        
        Current_Entity_Info_Text.text = current_entity.Wait_Turn ? "wait" : (current_entity.Sleep ? "sleep" : string.Empty);
        if (Current_Entity is Worker) {
            Update_Actions();
            if((Current_Entity as Worker).Improvement_Under_Construction != null) {
                Current_Entity_Info_Text.text = string.Format("Building {0}, {1} turn{2} left", (Current_Entity as Worker).Improvement_Under_Construction.Name,
                    (Current_Entity as Worker).Turns_Left, Helper.Plural((Current_Entity as Worker).Turns_Left));
            }
        } else if (Current_Entity is Prospector) {
            Update_Actions();
            if ((Current_Entity as Prospector).Prospecting) {
                Current_Entity_Info_Text.text = string.Format("Prospecting {0} turn{1} left", ((Current_Entity as Prospector).Prospect_Turns - (Current_Entity as Prospector).Prospect_Progress),
                    Helper.Plural((Current_Entity as Prospector).Prospect_Turns - (Current_Entity as Prospector).Prospect_Progress));
            }
        }
    }

    public void Next_Unit()
    {
        if(!Active || Main.Instance.Other_Players_Turn || Main.Instance.Current_Player.World_Map_Entities.Count == 0) {
            return;
        }

        List<WorldMapEntity> idle_entitys = new List<WorldMapEntity>();
        int start_index = 0;
        for (int i = 0; i < Main.Instance.Current_Player.World_Map_Entities.Count; i++) {
            if (!Main.Instance.Current_Player.World_Map_Entities[i].Is_Idle) {
                continue;
            }
            idle_entitys.Add(Main.Instance.Current_Player.World_Map_Entities[i]);
            if (Current_Entity != null && Current_Entity.Id == Main.Instance.Current_Player.World_Map_Entities[i].Id) {
                start_index = idle_entitys.Count - 1;
            }
        }

        if(idle_entitys.Count == 0) {
            for(int i = 0; i < Main.Instance.Current_Player.World_Map_Entities.Count; i++) {
                if(Current_Entity != null && Current_Entity.Id == Main.Instance.Current_Player.World_Map_Entities[i].Id) {
                    start_index = i;
                    break;
                }
            }
        }

        List<WorldMapEntity> list = idle_entitys.Count != 0 ? idle_entitys : Main.Instance.Current_Player.World_Map_Entities;

        int index = start_index;
        int entitys_cheched = 0;
        while (true) {
            WorldMapEntity current = list[index];
            if(Current_Entity == null || current.Id != Current_Entity.Id) {
                Current_Entity = current;
                CameraManager.Instance.Set_Camera_Location(Current_Entity.Hex);
                break;
            }
            index++;
            if(index >= list.Count) {
                index = 0;
            }
            entitys_cheched++;
            if(entitys_cheched >= list.Count) {
                break;
            }
        }
    }

    public void Wait_Turn()
    {
        if (!Active || Main.Instance.Other_Players_Turn || Current_Entity == null || !Current_Entity.Is_Owned_By_Current_Player) {
            return;
        }
        Current_Entity.Wait_Turn = !Current_Entity.Wait_Turn;
        Update_Entity_Info();
    }

    public void Sleep()
    {
        if (!Active || Main.Instance.Other_Players_Turn || Current_Entity == null || !Current_Entity.Is_Owned_By_Current_Player) {
            return;
        }
        Current_Entity.Sleep = !Current_Entity.Sleep;
        Update_Entity_Info();
    }

    public void Open_Info()
    {
        if(!Active || Main.Instance.Other_Players_Turn || Current_Entity == null || !(Current_Entity is Army)) {
            return;
        }
        UnitInfoGUIManager.Instance.Open(Selected_Units.Count != 0 ? Selected_Units[0] : (Current_Entity as Army).Units[0], false);
    }

    public void Update_Next_Turn_Button_Text()
    {
        if (Next_Turn_Button.interactable) {
            Next_Turn_Button.GetComponentInChildren<Text>().text = Has_Things_Todo(false) ? "!" : ">";
        }
    }

    private void Update_Actions()
    {
        if (Current_Entity == null) {
            CustomLogger.Instance.Warning("Current entity is not set");
            return;
        }

        foreach (GameObject go in actions) {
            GameObject.Destroy(go);
        }
        actions.Clear();

        for (int i = 0; i < Current_Entity.Actions.Count; i++) {
            GameObject action_gameobject = GameObject.Instantiate(Action_GameObject);
            action_gameobject.SetActive(true);
            action_gameobject.transform.SetParent(Panel.transform, false);
            action_gameobject.name = string.Format("Action{0}", i);
            action_gameobject.transform.position = new Vector3(
                Action_GameObject.transform.position.x + (action_gameobject.GetComponent<RectTransform>().rect.width * i),
                Action_GameObject.transform.position.y,
                Action_GameObject.transform.position.z
            );
            action_gameobject.GetComponentInChildren<Image>().sprite = SpriteManager.Instance.Get(Current_Entity.Actions[i].Texture, Current_Entity.Actions[i].Texture_Type);
            if(Current_Entity.Actions[i].Texture_Type == SpriteManager.SpriteType.Improvement) {
                action_gameobject.GetComponentInChildren<Image>().GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 160.0f);
            }
            action_gameobject.GetComponentInChildren<Text>().text = Current_Entity.Actions[i].Name;
            action_gameobject.GetComponentInChildren<Button>().interactable = Current_Entity.Actions[i].Can_Be_Activated(Current_Entity) && Current_Entity.Is_Owned_By(Main.Instance.Viewing_Player);
            if (!Current_Entity.Actions[i].Can_Be_Activated(Current_Entity) || !Current_Entity.Is_Owned_By(Main.Instance.Viewing_Player)) {
                action_gameobject.GetComponentInChildren<Image>().color = Color.gray;
            }
            Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
            int index = i;
            on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                Current_Entity.Actions[index].Activate(Current_Entity);
                Update_Current_Entity();
            }));
            action_gameobject.GetComponentInChildren<Button>().onClick = on_click_event;
            actions.Add(action_gameobject);
            if (!string.IsNullOrEmpty(Current_Entity.Actions[i].Tooltip)) {
                TooltipManager.Instance.Register_Tooltip(action_gameobject.GetComponentInChildren<Image>().gameObject, Current_Entity.Actions[i].Tooltip, gameObject);
            }
        }
    }
    
    private void Toggle_Unit_Selection(Image image, Unit unit)
    {
        if (Selected_Units.Contains(unit)) {
            Selected_Units.Remove(unit);
        } else {
            Selected_Units.Add(unit);
        }
        MouseManager.Instance.Set_Select_Hex_Mode(false);
        Update_Actions();
        //Update_Current_Entity();
        if(image != null) {
            image.gameObject.SetActive(Selected_Units.Contains(unit));
        }
    }

    private void Update_Buttons()
    {
        bool interactable = !Main.Instance.Other_Players_Turn;
        Next_Button.interactable = interactable;
        Wait_Button.interactable = interactable;
        Sleep_Button.interactable = interactable;
        Info_Button.interactable = interactable && Current_Entity != null && Current_Entity is Army;
        Next_Turn_Button.interactable = interactable;
        Update_Next_Turn_Button_Text();
    }

    private bool Has_Things_Todo(bool activate_ui)
    {
        if (Main.Instance.Current_Player.Current_Technology == null) {
            if (activate_ui) {
                TechnologyPanelManager.Instance.Active = true;
            }
            return true;
        }
        bool has_idle_entitys = false;
        foreach (WorldMapEntity entity in Main.Instance.Current_Player.World_Map_Entities) {
            if (entity.Is_Idle) {
                has_idle_entitys = true;
                break;
            }
        }
        if (has_idle_entitys) {
            if (activate_ui) {
                Next_Unit();
            }
            return true;
        }
        return false;
    }
}
