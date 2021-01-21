using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CustomBattleGUIManager : MonoBehaviour {
    private static readonly int DEFAULT_CASH = 1000;
    private static readonly int DEFAULT_PRODUCTION = 1200;
    private static readonly int DEFAULT_UNIT_MAX = 20;

    public static CustomBattleGUIManager Instance { get; private set; }

    public GameObject Panel;
    public Button Start_Button;

    public InputField Cash_Input_Field;
    public InputField Production_Input_Field;
    public Image Hex_Image;
    public Dropdown Hex_Dropdown;
    public InputField Max_Units_Input_Field;

    public Text Left_Name_Text;
    public Dropdown Left_Faction_Dropdown;
    public Dropdown Left_AI_Dropdown;
    public Button Left_Position_Button;
    public Text Left_Cash_Text;
    public Text Left_Production_Text;
    public GameObject Left_Unit_Selection_Content;
    public GameObject Left_Unit_Selection_Row_Prototype;
    public Text Left_Unit_Count_Text;
    public Text Left_Strenght_Text;
    public GameObject Left_Army_Content;
    public GameObject Left_Army_Row_Prototype;

    public Text Right_Name_Text;
    public Dropdown Right_Faction_Dropdown;
    public Dropdown Right_AI_Dropdown;
    public Button Right_Position_Button;
    public Text Right_Cash_Text;
    public Text Right_Production_Text;
    public GameObject Right_Unit_Selection_Content;
    public GameObject Right_Unit_Selection_Row_Prototype;
    public Text Right_Unit_Count_Text;
    public Text Right_Strenght_Text;
    public GameObject Right_Army_Content;
    public GameObject Right_Army_Row_Prototype;

    private Player.NewPlayerData left_player;
    private Player.NewPlayerData right_player;
    private bool left_is_attacker;
    private List<string> ai_labels;
    private int available_cash;
    private int available_production;
    private WorldMapHex selected_hex;
    private RowScrollView<Unit> left_unit_selection_scroll_view;
    private RowScrollView<Unit> left_army_scroll_view;
    private RowScrollView<Unit> right_unit_selection_scroll_view;
    private RowScrollView<Unit> right_army_scroll_view;
    private int left_cash_left;
    private int left_production_left;
    private int right_cash_left;
    private int right_production_left;
    private List<Unit> left_units;
    private List<Unit> right_units;
    private WorldMapHex placeholder_hex;
    private int max_units;
    private Color default_text_color;
    private List<Faction> factions;

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
        Active = false;
        ai_labels = new List<string>() { "Human" };
        foreach(AI.Level level in Enum.GetValues(typeof(AI.Level))) {
            ai_labels.Add(string.Format("{0} AI", level.ToString()));
        }
        left_unit_selection_scroll_view = new RowScrollView<Unit>("left_unit_selection_scroll_view", Left_Unit_Selection_Content, Left_Unit_Selection_Row_Prototype, 20.0f);
        left_army_scroll_view = new RowScrollView<Unit>("left_army_scroll_view", Left_Army_Content, Left_Army_Row_Prototype, 20.0f);
        right_unit_selection_scroll_view = new RowScrollView<Unit>("right_unit_selection_scroll_view", Right_Unit_Selection_Content, Right_Unit_Selection_Row_Prototype, 20.0f);
        right_army_scroll_view = new RowScrollView<Unit>("right_army_scroll_view", Right_Army_Content, Right_Army_Row_Prototype, 20.0f);
        default_text_color = Left_Cash_Text.color;
        factions = Factions.Custom_Battle_Options;
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
            if (Active) {
                MasterUIManager.Instance.Close_Others(GetType().Name);
                Initialize_GUI();
            }
        }
    }

    public void Back()
    {
        Active = false;
        MainMenuManager.Instance.Active = true;
    }

    public void Initialize_GUI()
    {
        left_player = new Player.NewPlayerData("Player 1", null, factions[0]);
        right_player = new Player.NewPlayerData("Player 2", AI.Level.Medium, factions[0]);
        left_is_attacker = true;
        available_cash = DEFAULT_CASH;
        available_production = DEFAULT_PRODUCTION;
        selected_hex = HexPrototypes.Instance.Get_World_Map_Hex("grassland");
        left_cash_left = available_cash;
        left_production_left = available_production;
        right_cash_left = available_cash;
        right_production_left = available_production;
        left_units = new List<Unit>();
        right_units = new List<Unit>();
        max_units = DEFAULT_UNIT_MAX;
        Update_GUI();
    }

    public void Start_Battle()
    {
        Main.Instance.Start_Custom_Battle(
            left_is_attacker ? left_player : right_player,
            left_is_attacker ? left_units : right_units,
            left_is_attacker ? right_player : left_player,
            left_is_attacker ? right_units : left_units,
            selected_hex
        );
        Active = false;
    }

    public void Switch_Positions()
    {
        left_is_attacker = !left_is_attacker;
        Update_Positions();
    }

    private void Update_Positions()
    {
        Left_Position_Button.GetComponentInChildren<Text>().text = left_is_attacker ? "Attacker" : "Defender";
        Right_Position_Button.GetComponentInChildren<Text>().text = !left_is_attacker ? "Attacker" : "Defender";
    }

    public void Edit_Cash()
    {
        if(!int.TryParse(Cash_Input_Field.text, out available_cash)) {
            available_cash = DEFAULT_CASH;
        }
        Update_GUI();
    }

    public void Edit_Production()
    {
        if (!int.TryParse(Production_Input_Field.text, out available_production)) {
            available_production = DEFAULT_PRODUCTION;
        }
        Update_GUI();
    }

    public void Edit_Max_Units()
    {
        if (!int.TryParse(Max_Units_Input_Field.text, out max_units)) {
            max_units = DEFAULT_UNIT_MAX;
        }
        Update_GUI();
    }

    public void Select_Hex()
    {
        selected_hex = HexPrototypes.Instance.Get_World_Map_Hex(HexPrototypes.Instance.All_Internal_Names[Hex_Dropdown.value]);
        Update_GUI();
    }

    public void Select_Left_Faction()
    {
        left_player.Faction = factions[Left_Faction_Dropdown.value];
        left_units.Clear();
        Update_GUI();
    }

    public void Select_Right_Faction()
    {
        right_player.Faction = factions[Right_Faction_Dropdown.value];
        right_units.Clear();
        Update_GUI();
    }

    public void Select_Left_AI()
    {
        left_player.AI = Left_AI_Dropdown.value == 0 ? (AI.Level?)null : (AI.Level)(Left_AI_Dropdown.value - 1);
        Update_GUI();
    }

    public void Select_Right_AI()
    {
        right_player.AI = Right_AI_Dropdown.value == 0 ? (AI.Level?)null : (AI.Level)(Right_AI_Dropdown.value - 1);
        Update_GUI();
    }

    private void Update_GUI()
    {
        //Middle
        Cash_Input_Field.text = available_cash.ToString();
        Production_Input_Field.text = available_production.ToString();
        Hex_Image.sprite = SpriteManager.Instance.Get(selected_hex.Sprite, SpriteManager.SpriteType.Terrain);
        Helper.Set_Dropdown_Options(Hex_Dropdown, HexPrototypes.Instance.All_Internal_Names.Select(x => HexPrototypes.Instance.Get_World_Map_Hex(x).Terrain).ToList(), selected_hex.Terrain);
        Max_Units_Input_Field.text = max_units.ToString();

        //Left side
        Left_Name_Text.text = left_player.Faction.Name;
        Helper.Set_Dropdown_Options(Left_Faction_Dropdown, factions.Select(x => x.Name).ToList(), left_player.Faction.Name);
        Helper.Set_Dropdown_Options(Left_AI_Dropdown, ai_labels, left_player.AI.HasValue ? ai_labels[((int)left_player.AI.Value) + 1] : ai_labels[0]);
        Left_Cash_Text.text = left_cash_left.ToString();
        Left_Production_Text.text = left_production_left.ToString();
        left_unit_selection_scroll_view.Clear();
        foreach(Unit unit in left_player.Faction.Units.Where(x => x is Unit).Select(x => x as Unit).ToList()) {
            left_unit_selection_scroll_view.Add(unit, Create_Unit_Row(unit, true, true));
        }
        left_army_scroll_view.Clear();
        float strenght = 0.0f;
        int cash_used = 0;
        int production_used = 0;
        bool valid_resource_use = true;
        foreach (Unit unit in left_units) {
            left_army_scroll_view.Add(unit, Create_Unit_Row(unit, false, true));
            strenght += unit.Relative_Strenght;
            cash_used += unit.Cost;
            production_used += unit.Production_Required;
        }
        Left_Unit_Count_Text.text = string.Format("{0} / {1}", left_units.Count, max_units);
        Left_Unit_Count_Text.color = left_units.Count > max_units ? Color.red : default_text_color;
        Left_Strenght_Text.text = Helper.Float_To_String(strenght, 0);
        Left_Cash_Text.text = (available_cash - cash_used).ToString();
        Left_Cash_Text.color = cash_used > available_cash ? Color.red : default_text_color;
        Left_Production_Text.text = (available_production - production_used).ToString();
        Left_Production_Text.color = production_used > available_production ? Color.red : default_text_color;
        valid_resource_use = !valid_resource_use ? false : (cash_used <= available_cash && production_used <= available_production);

        //Right side
        Right_Name_Text.text = right_player.Faction.Name;
        Helper.Set_Dropdown_Options(Right_Faction_Dropdown, factions.Select(x => x.Name).ToList(), right_player.Faction.Name);
        Helper.Set_Dropdown_Options(Right_AI_Dropdown, ai_labels, right_player.AI.HasValue ? ai_labels[((int)right_player.AI.Value) + 1] : ai_labels[0]);
        Right_Cash_Text.text = right_cash_left.ToString();
        Right_Production_Text.text = right_production_left.ToString();
        right_unit_selection_scroll_view.Clear();
        foreach (Unit unit in right_player.Faction.Units.Where(x => x is Unit).Select(x => x as Unit).ToList()) {
            right_unit_selection_scroll_view.Add(unit, Create_Unit_Row(unit, true, false));
        }
        right_army_scroll_view.Clear();
        strenght = 0.0f;
        cash_used = 0;
        production_used = 0;
        valid_resource_use = true;
        foreach (Unit unit in right_units) {
            right_army_scroll_view.Add(unit, Create_Unit_Row(unit, false, false));
            strenght += unit.Relative_Strenght;
            cash_used += unit.Cost;
            production_used += unit.Production_Required;
        }
        Right_Unit_Count_Text.text = string.Format("{0} / {1}", right_units.Count, max_units);
        Right_Unit_Count_Text.color = right_units.Count > max_units ? Color.red : default_text_color;
        Right_Strenght_Text.text = Helper.Float_To_String(strenght, 0);
        Right_Cash_Text.text = (available_cash - cash_used).ToString();
        Right_Cash_Text.color = cash_used > available_cash ? Color.red : default_text_color;
        Right_Production_Text.text = (available_production - production_used).ToString();
        Right_Production_Text.color = production_used > available_production ? Color.red : default_text_color;
        valid_resource_use = !valid_resource_use ? false : (cash_used <= available_cash && production_used <= available_production);

        Update_Positions();
        Start_Button.interactable = left_units.Count != 0 && right_units.Count != 0 && valid_resource_use && left_units.Count <= max_units && right_units.Count <= max_units;
    }
    
    private List<UIElementData> Create_Unit_Row(Unit unit, bool selection, bool left)
    {
        return new List<UIElementData>() {
            new UIElementData("Image", unit.Texture, SpriteManager.SpriteType.Unit),
            new UIElementData("NameText", unit.Name),
            new UIElementData("CostText", string.Format("{0}/{1}", unit.Cost, unit.Production_Required)),
            new UIElementData("InfoButton", "i", delegate() {
                UnitInfoGUIManager.Instance.Open(unit, true);
            }),
            new UIElementData("SelectButton", null, delegate () {
                if(selection) {
                    if(left) {
                        left_units.Add(new Unit(unit));
                    } else {
                        right_units.Add(new Unit(unit));
                    }
                } else {
                    if(left) {
                        left_units.Remove(unit);
                    } else {
                        right_units.Remove(unit);
                    }
                }
                Update_GUI();
            })
        };
    }
}
