using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoGUIManager : MonoBehaviour
{
    public static UnitInfoGUIManager Instance;
    public GameObject Panel;

    public Text Name_Text;
    public Image Image;
    public Image Armor_Image;
    public Text Type_Text;
    public Text Tags_Text;
    public Text Relative_Strenght_Text;
    public Image Manpower_Image;
    public Text Manpower_Text;
    public Text Cost_Text;
    public Text Production_Text;
    public Text Melee_Attack_Text;
    public Text Charge_Bonus_Text;
    public Text Melee_Damage_Types_Text;

    public GameObject Ranged_Attack_Container;
    public Text Ranged_Attack_Text;
    public Text Range_Text;
    public Text Ammo_Text;
    public Text Ranged_Damage_Types_Text;

    public GameObject Bottom_Container;
    public Text Melee_Defence_Text;
    public Text Ranged_Defence_Text;
    public GameObject Resistances_Content;
    public GameObject Resistances_Row_Prototype;

    public Text Morale_Text;
    public Text Stamina_Text;
    public Text Movement_Text;
    public Text LoS_Text;
    public Text Discipline_Text;
    public GameObject Abilities_Content;
    public GameObject Abilities_Row_Prototype;

    public Button Train_Button;
    public Button Close_Button;

    public Unit Unit { get; private set; }
    public bool Is_Preview { get; private set; }

    private Color default_text_color;
    private float bottom_position_y;
    private RowScrollView<Damage.Type> resistances_scroll_view;
    private RowScrollView<Ability> abilities_scroll_view;
    private float panel_position_x;
    private bool train_gui;

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
        default_text_color = Relative_Strenght_Text.color;
        bottom_position_y = Bottom_Container.gameObject.transform.position.y;
        resistances_scroll_view = new RowScrollView<Damage.Type>("resistances_scroll_view", Resistances_Content, Resistances_Row_Prototype, 15.0f);
        abilities_scroll_view = new RowScrollView<Ability>("abilities_scroll_view", Abilities_Content, Abilities_Row_Prototype, 15.0f);
        panel_position_x = Panel.gameObject.transform.position.x;
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
            if (value == Active || Main.Instance.Other_Players_Turn) {
                return;
            }
            Panel.SetActive(value);
        }
    }

    public void Open(Unit unit, bool is_preview, bool train_gui = false)
    {
        Unit = unit;
        Is_Preview = is_preview;
        this.train_gui = train_gui;
        Active = true;
        Update_Info();
    }

    public void Close()
    {
        Active = false;
    }

    public void Train()
    {
        if(CityGUIManager.Instance.Current_City == null) {
            CustomLogger.Instance.Error("No city selected");
            return;
        }
        CityGUIManager.Instance.Select_Unit(Unit);
        Active = false;
    }

    public void Update_Info()
    {
        Panel.gameObject.transform.position = new Vector3(
            panel_position_x + (train_gui ? 150.0f : 0.0f),
            Panel.gameObject.transform.position.y,
            Panel.gameObject.transform.position.z
        );

        Name_Text.text = Unit.Name;
        Image.sprite = SpriteManager.Instance.Get(Unit.Texture, SpriteManager.SpriteType.Unit);

        Train_Button.gameObject.SetActive(train_gui);
        Train_Button.interactable = CityGUIManager.Instance.Current_City != null ? CityGUIManager.Instance.Current_City.Can_Train(Unit) : false;
        Close_Button.GetComponentInChildren<Text>().text = train_gui ? "Cancel" : "Close";

        Armor_Image.gameObject.SetActive(true);
        switch (Unit.Armor) {
            case Unit.ArmorType.Light:
                Armor_Image.sprite = SpriteManager.Instance.Get("light_armor", SpriteManager.SpriteType.UI);
                break;
            case Unit.ArmorType.Medium:
                Armor_Image.sprite = SpriteManager.Instance.Get("medium_armor", SpriteManager.SpriteType.UI);
                break;
            case Unit.ArmorType.Heavy:
                Armor_Image.sprite = SpriteManager.Instance.Get("heavy_armor", SpriteManager.SpriteType.UI);
                break;
            default:
                Armor_Image.gameObject.SetActive(false);
                break;
        }
        Type_Text.text = string.Format("{0} {1} {2}", Helper.Snake_Case_To_UI(Unit.Armor.ToString(), true), Unit.Can_Ranged_Attack ? "ranged" : "melee", Unit.Type == Unit.UnitType.Undefined ? "unit" : Helper.Snake_Case_To_UI(Unit.Type.ToString()));
        Tags_Text.text = string.Join(", ", Unit.Tags.Select(x => Helper.Snake_Case_To_UI(x.ToString())).ToArray());
        Relative_Strenght_Text.text = Helper.Float_To_String(Is_Preview ? Unit.Relative_Strenght : Unit.Current_Relative_Strenght, 0);
        Manpower_Image.gameObject.SetActive(!Is_Preview);
        Manpower_Text.gameObject.SetActive(!Is_Preview);
        Manpower_Text.text = Helper.Float_To_String(100.0f * Unit.Manpower, 0) + "%";
        Manpower_Text.color = Unit.Manpower <= 0.5f ? Color.red : (Unit.Manpower == 1.0f ? default_text_color : Color.yellow);
        Relative_Strenght_Text.color = Is_Preview ? default_text_color : Manpower_Text.color;
        Cost_Text.text = string.Format("{0} ({1}/turn)", Unit.Cost, Helper.Float_To_String(Unit.Upkeep, 2));
        Production_Text.text = Unit.Production_Required.ToString();

        Melee_Attack_Text.text = Helper.Float_To_String(Unit.Melee_Attack.Total, 0);
        Charge_Bonus_Text.text = string.Format("{0}%", Helper.Float_To_String(Unit.Charge * 100.0f, 0, true));
        Melee_Damage_Types_Text.text = string.Join(", ", Unit.Melee_Attack.Type_Weights.OrderByDescending(x => x.Value).Select(x => string.Format("{0} {1}%", Helper.Snake_Case_To_UI(x.Key.ToString()), Helper.Float_To_String(x.Value * 100.0f, 0))).ToArray());

        Ranged_Attack_Container.SetActive(Unit.Can_Ranged_Attack);
        if(Unit.Can_Ranged_Attack) {
            Ranged_Attack_Text.text = Helper.Float_To_String(Unit.Ranged_Attack.Total, 0);
            Range_Text.text = Unit.Range.ToString();
            Ammo_Text.text = Unit.Max_Ammo.ToString();
            Ranged_Damage_Types_Text.text = string.Join(", ", Unit.Ranged_Attack.Type_Weights.OrderByDescending(x => x.Value).Select(x => string.Format("{0} {1}%", Helper.Snake_Case_To_UI(x.Key.ToString()), Helper.Float_To_String(x.Value * 100.0f, 0))).ToArray());
        }
        Bottom_Container.gameObject.transform.position = new Vector3(
            Bottom_Container.gameObject.transform.position.x,
            bottom_position_y + (Unit.Can_Ranged_Attack ? 0.0f : 25.0f),
            Bottom_Container.gameObject.transform.position.z
        );

        Melee_Defence_Text.text = Helper.Float_To_String(Unit.Melee_Defence, 0);
        Ranged_Defence_Text.text = Helper.Float_To_String(Unit.Ranged_Defence, 0);
        resistances_scroll_view.Clear();
        foreach(KeyValuePair<Damage.Type, float> pair in Unit.Resistances.OrderBy(x => (int)x.Key)) {
            resistances_scroll_view.Add(pair.Key, new List<UIElementData>() {
                new UIElementData("TypeText", Helper.Snake_Case_To_UI(pair.Key.ToString(), true)),
                new UIElementData("ValueText", string.Format("{0}%", Helper.Float_To_String(pair.Value * 100.0f, 0)), pair.Value < 1.0f ? Color.red : (pair.Value > 1.0f ? Color.blue : default_text_color))
            });
        }

        Morale_Text.text = Helper.Float_To_String(Unit.Max_Morale, 0);
        Stamina_Text.text = Helper.Float_To_String(Unit.Max_Stamina, 0);
        Movement_Text.text = string.Format("{0} / {1}", Helper.Float_To_String(Unit.Max_Movement, 0), Helper.Float_To_String(Unit.Max_Campaing_Map_Movement, 0));
        LoS_Text.text = Unit.LoS.ToString();
        Discipline_Text.text = Helper.Float_To_String(Unit.Discipline, 0);

        abilities_scroll_view.Clear();
        foreach(Ability ability in Unit.Abilities) {
            abilities_scroll_view.Add(ability, new List<UIElementData>() {
                new UIElementData("NameText", ability.Name),
                new UIElementData("ValueText", ability.Uses_Potency ? (ability.Potency_As_Percent ? string.Format("{0}%", Helper.Float_To_String(ability.Potency * 100.0f, 0)) : Helper.Float_To_String(ability.Potency, 2)) : string.Empty)
            });
        }
    }
}
