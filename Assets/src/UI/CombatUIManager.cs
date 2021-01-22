using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CombatUIManager : MonoBehaviour {
    private static readonly bool SHOW_ATTACK_PREVIEW_DETAILS = true;
    public static CombatUIManager Instance { get; private set; }

    public GameObject Panel;

    public Button Next_Turn_Button;

    public GameObject Unit_Container;
    public Image Unit_Image;
    public Text Unit_Name_Text;
    public Text Unit_Movement_Text;
    public Image Can_Attack_Image;
    public Button Deploy_Button;
    public Button Next_Unit_Button;
    public Button Previous_Unit_Button;
    public Button Toggle_Run_Button;

    public GameObject Manpower_GameObject;
    public Image Manpower_Bar;
    public Text Manpower_Text;
    public GameObject Morale_GameObject;
    public Image Morale_Bar;
    public Text Morale_Text;
    public GameObject Stamina_GameObject;
    public Image Stamina_Bar;
    public Text Stamina_Text;

    public GameObject Damage_Output_Preview_Panel;
    public Text Damage_Output_Preview_Damage_Text;
    public Text Damage_Output_Preview_Attack_Text;
    public Text Damage_Output_Preview_Base_Attack_Text;
    public Text Damage_Output_Preview_Attack_Multiplier_Text;
    public Text Damage_Output_Preview_Defence_Text;
    public Text Damage_Output_Preview_Base_Defence_Text;
    public Text Damage_Output_Preview_Defence_Multiplier_Text;
    public GameObject Damage_Output_Preview_Detail_Row;
    public GameObject Damage_Taken_Preview_Panel;
    public Text Damage_Taken_Preview_Damage_Text;
    public Text Damage_Taken_Preview_Attack_Text;
    public Text Damage_Taken_Preview_Base_Attack_Text;
    public Text Damage_Taken_Preview_Attack_Multiplier_Text;
    public Text Damage_Taken_Preview_Defence_Text;
    public Text Damage_Taken_Preview_Base_Defence_Text;
    public Text Damage_Taken_Preview_Defence_Multiplier_Text;
    public GameObject Damage_Taken_Preview_Detail_Row;

    public GameObject Unit_List_Content;
    public GameObject Unit_List_Prototype;

    private Unit current_unit;
    private Color default_text_color;
    private Color enemy_name_text_color;
    private Color penalized_damage_color;
    private Color buffed_damage_color;
    private Color penalized_stat_color;
    private Color buffed_stat_color;
    private Color movement_range_highlight;
    private float penalty_buff_color_change_threshold;
    private bool run;
    private float bar_height;
    private float bar_max_lenght;
    private CombatMapHex last_hex_under_cursor;
    private List<CombatMapHex> highlighted_movement_range_hexes;
    private List<CombatMapHex> marked_attack_range_hexes;
    private List<GameObject> damage_output_details;
    private List<GameObject> damage_taken_details;
    private Dictionary<Unit, GameObject[]> unit_list_items;
    private long unit_list_index;

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
        Damage_Output_Preview_Detail_Row.SetActive(false);
        Damage_Taken_Preview_Detail_Row.SetActive(false);
        Damage_Output_Preview_Panel.SetActive(false);
        Damage_Taken_Preview_Panel.SetActive(false);
        Unit_List_Prototype.SetActive(false);

        default_text_color = Unit_Name_Text.color;

        enemy_name_text_color = new Color(1.0f, 0.0f, 0.0f);
        penalized_damage_color = new Color(1.0f, 0.0f, 0.0f);
        buffed_damage_color = new Color(0.0f, 1.0f, 0.0f);
        penalized_stat_color = new Color(1.0f, 0.0f, 0.0f);
        buffed_stat_color = new Color(0.0f, 1.0f, 0.0f);
        movement_range_highlight = new Color(0.0f, 1.75f, 0.25f, 1.0f);

        penalty_buff_color_change_threshold = 0.10f;
        bar_height = Manpower_Bar.rectTransform.rect.height;
        bar_max_lenght = Manpower_Bar.rectTransform.rect.width;
        last_hex_under_cursor = null;
        highlighted_movement_range_hexes = new List<CombatMapHex>();
        marked_attack_range_hexes = new List<CombatMapHex>();
        damage_output_details = new List<GameObject>();
        damage_taken_details = new List<GameObject>();
        unit_list_items = new Dictionary<Unit, GameObject[]>();
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!CombatManager.Instance.Active_Combat) {
            return;
        }
        if(!CombatManager.Instance.Deployment_Mode && !CombatManager.Instance.Other_Players_Turn && !CombatManager.Instance.Retreat_Phase && Current_Unit != null && Current_Unit.Is_Owned_By_Current_Player) {
            CombatMapHex h = (CombatMapHex)MouseManager.Instance.Hex_Under_Cursor;
            if(h == null || h.Unit == null || h.Unit.Is_Owned_By_Current_Player) {
                Close_Preview_Panels();
                last_hex_under_cursor = null;
            } else if(h != last_hex_under_cursor) {
                last_hex_under_cursor = h;
                AttackResult[] preview = Current_Unit.Attack(h.Unit, true);
                if (preview != null) {
                    //Output
                    Damage_Output_Preview_Panel.SetActive(true);
                    Damage_Output_Preview_Damage_Text.text = string.Format("{0} / {1}", Mathf.RoundToInt(preview[1].Manpower_Delta * -100.0f), Mathf.RoundToInt(preview[1].Morale_Delta * -1.0f));
                    Damage_Output_Preview_Base_Attack_Text.text = Mathf.RoundToInt(preview[1].Base_Attack).ToString();
                    Damage_Output_Preview_Attack_Multiplier_Text.text = string.Format("{0}%", Helper.Float_To_String((preview[1].Final_Attack / preview[1].Base_Attack) * 100.0f, 0));
                    if (preview[1].Damage_Effectiveness <= 1.0f - penalty_buff_color_change_threshold) {
                        Damage_Output_Preview_Damage_Text.color = penalized_damage_color;
                    } else if(preview[1].Damage_Effectiveness >= 1.0f + penalty_buff_color_change_threshold) {
                        Damage_Output_Preview_Damage_Text.color = buffed_damage_color;
                    } else {
                        Damage_Output_Preview_Damage_Text.color = default_text_color;
                    }
                    Damage_Output_Preview_Attack_Text.text = Math.Round(preview[1].Final_Attack, 1).ToString();
                    if (preview[1].Attack_Effectiveness <= 1.0f - penalty_buff_color_change_threshold) {
                        Damage_Output_Preview_Attack_Text.color = penalized_stat_color;
                    } else if (preview[1].Attack_Effectiveness >= 1.0f + penalty_buff_color_change_threshold) {
                        Damage_Output_Preview_Attack_Text.color = buffed_stat_color;
                    } else {
                        Damage_Output_Preview_Attack_Text.color = default_text_color;
                    }
                    if (preview[0].Final_Defence != 0.0f) {
                        Damage_Output_Preview_Defence_Text.text = Math.Round(preview[0].Final_Defence, 1).ToString();
                        Damage_Output_Preview_Base_Defence_Text.text = Mathf.RoundToInt(preview[0].Base_Defence).ToString();
                        Damage_Output_Preview_Defence_Multiplier_Text.text = string.Format("{0}%", Helper.Float_To_String((preview[0].Final_Defence / preview[0].Base_Defence) * 100.0f, 0));
                        if (preview[0].Defence_Effectiveness <= 1.0f - penalty_buff_color_change_threshold) {
                            Damage_Output_Preview_Defence_Text.color = penalized_stat_color;
                        } else if (preview[0].Defence_Effectiveness >= 1.0f + penalty_buff_color_change_threshold) {
                            Damage_Output_Preview_Defence_Text.color = buffed_stat_color;
                        } else {
                            Damage_Output_Preview_Defence_Text.color = default_text_color;
                        }
                    } else {
                        //TODO: Right def stat
                        Damage_Output_Preview_Defence_Text.text = Math.Round(Current_Unit.Melee_Defence, 1).ToString();
                        Damage_Output_Preview_Base_Defence_Text.text = Mathf.RoundToInt(Current_Unit.Melee_Defence).ToString();
                        Damage_Output_Preview_Defence_Multiplier_Text.text = "100%";
                        Damage_Output_Preview_Defence_Text.color = default_text_color;
                    }
                    Show_Details(preview[1], preview[0], true);

                    //Taken
                    Damage_Taken_Preview_Panel.SetActive(true);
                    Damage_Taken_Preview_Damage_Text.text = string.Format("{0} / {1}", Mathf.RoundToInt(preview[0].Manpower_Delta * -100.0f), Mathf.RoundToInt(preview[0].Morale_Delta * -1.0f));
                    Damage_Taken_Preview_Base_Attack_Text.text = Mathf.RoundToInt(preview[0].Base_Attack).ToString();
                    Damage_Taken_Preview_Attack_Multiplier_Text.text = string.Format("{0}%", Helper.Float_To_String((preview[0].Final_Attack / preview[0].Base_Attack) * 100.0f, 0));
                    if (preview[0].Damage_Effectiveness <= 1.0f - penalty_buff_color_change_threshold) {
                        Damage_Taken_Preview_Damage_Text.color = penalized_damage_color;
                    } else if (preview[0].Damage_Effectiveness >= 1.0f + penalty_buff_color_change_threshold) {
                        Damage_Taken_Preview_Damage_Text.color = buffed_damage_color;
                    } else {
                        Damage_Taken_Preview_Damage_Text.color = default_text_color;
                    }
                    Damage_Taken_Preview_Attack_Text.text = Math.Round(preview[0].Final_Attack, 1).ToString();
                    if (preview[0].Attack_Effectiveness <= 1.0f - penalty_buff_color_change_threshold) {
                        Damage_Taken_Preview_Attack_Text.color = penalized_stat_color;
                    } else if (preview[0].Attack_Effectiveness >= 1.0f + penalty_buff_color_change_threshold) {
                        Damage_Taken_Preview_Attack_Text.color = buffed_stat_color;
                    } else {
                        Damage_Taken_Preview_Attack_Text.color = default_text_color;
                    }
                    if (preview[1].Final_Defence != 0.0f) {
                        Damage_Taken_Preview_Defence_Text.text = Math.Round(preview[1].Final_Defence, 1).ToString();
                        Damage_Taken_Preview_Base_Defence_Text.text = Mathf.RoundToInt(preview[1].Base_Defence).ToString();
                        Damage_Taken_Preview_Defence_Multiplier_Text.text = string.Format("{0}%", Helper.Float_To_String((preview[1].Final_Defence / preview[1].Base_Defence) * 100.0f, 0));
                        if (preview[1].Defence_Effectiveness <= 1.0f - penalty_buff_color_change_threshold) {
                            Damage_Taken_Preview_Defence_Text.color = penalized_stat_color;
                        } else if (preview[1].Defence_Effectiveness >= 1.0f + penalty_buff_color_change_threshold) {
                            Damage_Taken_Preview_Defence_Text.color = buffed_stat_color;
                        } else {
                            Damage_Taken_Preview_Defence_Text.color = default_text_color;
                        }
                    } else {
                        //TODO: Right def stat
                        Damage_Taken_Preview_Defence_Text.text = Math.Round(h.Unit.Melee_Defence, 1).ToString();
                        Damage_Taken_Preview_Base_Defence_Text.text = Mathf.RoundToInt(h.Unit.Melee_Defence).ToString();
                        Damage_Taken_Preview_Defence_Multiplier_Text.text = "100%";
                        Damage_Taken_Preview_Defence_Text.color = default_text_color;
                    }
                    Show_Details(preview[1], preview[0], false);
                } else {
                    Damage_Output_Preview_Panel.SetActive(false);
                    Damage_Taken_Preview_Panel.SetActive(false);
                }
            }
        } else {
            Close_Preview_Panels();
            last_hex_under_cursor = null;
        }
    }

    private void Close_Preview_Panels()
    {
        if (Damage_Output_Preview_Panel.activeSelf) {
            Damage_Output_Preview_Panel.SetActive(false);
        }
        if (Damage_Taken_Preview_Panel.activeSelf) {
            Damage_Taken_Preview_Panel.SetActive(false);
        }
    }

    public bool Active
    {
        get {
            return Panel.gameObject.activeSelf;
        }
        set {
            Panel.gameObject.SetActive(value);
            if (value) {
                unit_list_index = 0;
            }
        }
    }

    public void End_Turn_On_Click()
    {
        CombatManager.Instance.Next_Turn();
        Run = false;
        Update_GUI();
    }

    public void Next_Unit_On_Click()
    {
        CombatManager.Instance.Next_Unit();
        Update_Current_Unit();
        if(Current_Unit.Hex != null) {
            CameraManager.Instance.Set_Camera_Location(Current_Unit.Hex);
        }
    }

    public void Previous_Unit_On_Click()
    {
        CombatManager.Instance.Previous_Unit();
        Update_Current_Unit();
        if (Current_Unit.Hex != null) {
            CameraManager.Instance.Set_Camera_Location(Current_Unit.Hex);
        }
    }

    public Unit Current_Unit
    {
        get {
            return current_unit;
        }
        set {
            if(value == null && CombatManager.Instance.Deployment_Mode) {
                return;
            }
            if (current_unit != null && current_unit.Hex != null) {
                current_unit.Hex.Borders = current_unit.Is_Owned_By_Current_Player ? CombatMapHex.Owned_Unit_Color : CombatMapHex.Enemy_Unit_Color;
            }
            current_unit = value;
            Update_Current_Unit();
        }
    }

    public void Update_GUI()
    {
        Next_Turn_Button.interactable = !CombatManager.Instance.Other_Players_Turn && !CombatManager.Instance.Retreat_Phase;
        Helper.Delete_All(unit_list_items.Select(x => x.Value[0]).ToList());
        unit_list_items.Clear();
        bool top_row = true;
        int column = 0;
        if (!CombatManager.Instance.Other_Players_Turn) {
            foreach(Unit unit in CombatManager.Instance.Current_Army.Units.Where(x => x.Hex != null || CombatManager.Instance.Deployment_Mode).ToList()) {
                GameObject item = GameObject.Instantiate(
                    Unit_List_Prototype,
                    new Vector3(
                        Unit_List_Prototype.transform.position.x + (column * 45.0f),
                        Unit_List_Prototype.transform.position.y - (top_row ? 0.0f : 45.0f),
                        Unit_List_Prototype.transform.position.z
                    ),
                    Quaternion.identity,
                    Unit_List_Content.transform
                );
                item.name = string.Format("{0}Item{1}", unit.Name, unit_list_index);
                item.SetActive(true);
                unit_list_index++;
                if (!top_row) {
                    column++;
                }
                top_row = !top_row;
                Helper.Set_Image(item.name, "Image", unit.Texture, SpriteManager.SpriteType.Unit);
                GameObject can_attack_image = GameObject.Find(string.Format("{0}/{1}", item.name, "CanAttackImage"));
                can_attack_image.gameObject.SetActive(unit.Can_Attack);
                GameObject can_move_image = GameObject.Find(string.Format("{0}/{1}", item.name, "CanMoveImage"));
                can_move_image.gameObject.SetActive(unit.Current_Movement > 0.0f);
                GameObject routing_image = GameObject.Find(string.Format("{0}/{1}", item.name, "RoutingImage"));
                routing_image.gameObject.SetActive(unit.Is_Routed);
                GameObject selection_image = GameObject.Find(string.Format("{0}/{1}", item.name, "SelectionImage"));
                selection_image.gameObject.SetActive(Current_Unit != null && unit.Id == Current_Unit.Id);
                Helper.Set_Button_On_Click(item.name, "SelectButton", delegate () {
                    Current_Unit = unit;
                });
                unit_list_items.Add(unit, new GameObject[5] {
                    item,
                    can_attack_image,
                    can_move_image,
                    routing_image,
                    selection_image
                });
            }
            Unit_List_Content.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, (column + 1) * 45.0f);
        }
    }
    
    public void Update_Current_Unit()
    {
        MouseManager.Instance.Set_Select_Hex_Mode(false);

        Unit_Container.SetActive(Current_Unit != null);
        if (Current_Unit == null) {
            //No unit selected
            Deploy_Button.gameObject.SetActive(false);
            Clear_Movement_And_Attack_Range();
            TooltipManager.Instance.Unregister_Tooltip(Unit_Image.gameObject);
            return;
        }

        //Update UI
        Unit_Image.gameObject.SetActive(true);
        Deploy_Button.gameObject.SetActive(CombatManager.Instance.Deployment_Mode);
        Deploy_Button.GetComponentInChildren<Text>().text = Current_Unit.Hex == null ? "Deploy" : "Undeploy";
        Unit_Image.overrideSprite = SpriteManager.Instance.Get(Current_Unit.Texture, SpriteManager.SpriteType.Unit);
        Unit_Name_Text.text = Current_Unit.Name;
        Unit_Name_Text.color = Current_Unit.Is_Owned_By_Current_Player ? default_text_color : enemy_name_text_color;
        Unit_Movement_Text.text = string.Format("{0} / {1}", Math.Round(Current_Unit.Current_Movement, 1), Current_Unit.Max_Movement);
        Can_Attack_Image.gameObject.SetActive(Current_Unit.Can_Attack);

        Manpower_Bar.rectTransform.sizeDelta = new Vector2(bar_max_lenght * Current_Unit.Manpower, bar_height);
        Manpower_Text.text = Mathf.RoundToInt(Current_Unit.Manpower * 100.0f).ToString() + "%";

        if (Current_Unit.Uses_Morale) {
            Morale_Bar.rectTransform.sizeDelta = new Vector2(bar_max_lenght * Current_Unit.Relative_Morale, bar_height);
            Morale_Text.text = Mathf.RoundToInt(Current_Unit.Relative_Morale * 100.0f).ToString() + "%";
        } else {
            Morale_Bar.rectTransform.sizeDelta = new Vector2(bar_max_lenght, bar_height);
            Morale_Text.text = "N/A";
        }

        if (Current_Unit.Uses_Stamina) {
            Stamina_Bar.rectTransform.sizeDelta = new Vector2(bar_max_lenght * Current_Unit.Relative_Stamina, bar_height);
            Stamina_Text.text = Mathf.RoundToInt(Current_Unit.Relative_Stamina * 100.0f).ToString() + "%";
        } else {
            Stamina_Bar.rectTransform.sizeDelta = new Vector2(bar_max_lenght, bar_height);
            Stamina_Text.text = "N/A";
        }

        Next_Unit_Button.interactable = true;
        Previous_Unit_Button.interactable = true;
        Toggle_Run_Button.interactable = Current_Unit.Can_Run && !CombatManager.Instance.Deployment_Mode && !CombatManager.Instance.Other_Players_Turn && !Current_Unit.Hex.Is_Adjancent_To_Enemy(Current_Unit.Owner);
        if (!Current_Unit.Can_Run) {
            run = false;
            Toggle_Run_Button.GetComponentInChildren<Text>().text = "Run";
        }

        TooltipManager.Instance.Register_Tooltip(Unit_Image.gameObject, Current_Unit.Tooltip, gameObject);

        //Update highlights
        Update_Movement_And_Attack_Range();

        //Update unit list
        foreach(KeyValuePair<Unit, GameObject[]> pair in unit_list_items) {
            pair.Value[4].gameObject.SetActive(pair.Key.Id == Current_Unit.Id);
            if (pair.Key.Id == Current_Unit.Id) {
                pair.Value[1].gameObject.SetActive(Current_Unit.Can_Attack);
                pair.Value[2].gameObject.SetActive(Current_Unit.Current_Movement > 0.0f);
                pair.Value[3].gameObject.SetActive(Current_Unit.Is_Routed);
            }
        }
    }

    public void Deploy_Button_On_Click()
    {
        if (!Active) {
            return;
        }
        if(Current_Unit.Hex != null) {
            Current_Unit.Undeploy();
            Update_Current_Unit();
            return;
        }
        MessageManager.Instance.Show_Message("Select hex to deploy this unit");
        MouseManager.Instance.Set_Select_Hex_Mode(true, delegate(Hex selected_hex) {
            if(!(selected_hex is CombatMapHex)) {
                CustomLogger.Instance.Error("Selected is not CombatMapHex");
                return;
            }
            if(!Current_Unit.Deploy(selected_hex as CombatMapHex)) {
                MessageManager.Instance.Show_Message("Unit can't be deployed here");
                return;
            }
            foreach(Unit u in CombatManager.Instance.Current_Army.Units) {
                if(u.Hex == null) {
                    Current_Unit = u;
                    break;
                }
            }
            Update_Current_Unit();
        });
    }

    public bool Run
    {
        get {
            return run;
        }
        set {
            if(run == value) {
                return;
            }
            run = value;
            Toggle_Run_Button.GetComponentInChildren<Text>().text = run ? "Run" : "Walk";
            Update_Current_Unit();
        }
    }

    public void Toggle_Running_On_Click()
    {
        Run = !Run;
    }

    public void Info_On_Click()
    {
        if(Current_Unit != null) {
            UnitInfoGUIManager.Instance.Open(Current_Unit, false);
        }
    }

    private void Clear_Movement_And_Attack_Range()
    {
        foreach (CombatMapHex old_movement_range_hex in highlighted_movement_range_hexes) {
            old_movement_range_hex.Clear_Highlight();
        }
        highlighted_movement_range_hexes.Clear();
        foreach (CombatMapHex old_attack_range_hex in marked_attack_range_hexes) {
            old_attack_range_hex.In_Attack_Range_Mark = false;
        }
        marked_attack_range_hexes.Clear();
    }

    private void Update_Movement_And_Attack_Range()
    {
        if (current_unit == null || current_unit.Hex == null) {
            return;
        }
        Clear_Movement_And_Attack_Range();
        current_unit.Hex.Borders = current_unit.Is_Owned_By_Current_Player ? CombatMapHex.Current_Owned_Unit_Color : CombatMapHex.Current_Enemy_Unit_Color;
        if (!CombatManager.Instance.Deployment_Mode) {
            foreach (CombatMapHex new_movement_range_hex in current_unit.Get_Hexes_In_Movement_Range(Run)) {
                new_movement_range_hex.Highlight = movement_range_highlight;
                highlighted_movement_range_hexes.Add(new_movement_range_hex);
            }
            foreach (CombatMapHex new_attack_range_hex in current_unit.Get_Hexes_In_Attack_Range()) {
                new_attack_range_hex.In_Attack_Range_Mark = true;
                marked_attack_range_hexes.Add(new_attack_range_hex);
            }
        }
    }

    private void Show_Details(AttackResult damage_output_result, AttackResult damage_taken_result, bool output)
    {
        List<GameObject> detail_list = output ? damage_output_details : damage_taken_details;
        GameObject detail_row = output ? Damage_Output_Preview_Detail_Row : Damage_Taken_Preview_Detail_Row;
        Helper.Destroy_GameObjects(ref detail_list);

        if (!SHOW_ATTACK_PREVIEW_DETAILS) {
            return;
        }

        Dictionary<string, AttackResult.Detail> details = new Dictionary<string, AttackResult.Detail>();
        foreach(AttackResult.Detail output_detail in damage_output_result.Details) {
            if((output && output_detail.Has_Attack_Data) || (!output && output_detail.Has_Defence_Data)) {
                if (details.ContainsKey(output_detail.Description)) {
                    details[output_detail.Description].Add(output_detail);
                } else {
                    details.Add(output_detail.Description, output_detail.Clone());
                }
            }
        }
        foreach (AttackResult.Detail taken_detail in damage_taken_result.Details) {
            if ((output && taken_detail.Has_Defence_Data) || (!output && taken_detail.Has_Attack_Data)) {
                if (details.ContainsKey(taken_detail.Description)) {
                    details[taken_detail.Description].Add(taken_detail);
                } else {
                    details.Add(taken_detail.Description, taken_detail.Clone());
                }
            }
        }
        float row_height = 20.0f;
        int i = 0;
        foreach (KeyValuePair<string, AttackResult.Detail> detail in details) {
            GameObject go = GameObject.Instantiate(detail_row);
            go.SetActive(true);
            go.transform.SetParent(detail_row.gameObject.transform.parent, false);
            go.name = "Detail" + i;
            go.transform.position = new Vector3(detail_row.transform.position.x,
                detail_row.transform.position.y + (i * row_height), detail_row.transform.position.z);
            go.GetComponentInChildren<Text>().text = detail.Value.ToString();
            detail_list.Add(go);
            i++;
        }
    }
}
