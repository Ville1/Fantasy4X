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
    public GameObject Production_Container_1;
    public Text Production_Text_1;
    public GameObject Production_Container_2;
    public Text Production_Text_2;
    public GameObject Mana_Container;
    public Text Mana_Text;

    public Text Melee_Attack_Text;
    public Text Charge_Bonus_Text;
    public Text Melee_Damage_Types_Text;
    public GameObject Melee_Magic_Attack_Container;
    public Text Melee_Magic_Attack_Text;
    public GameObject Melee_Psionic_Attack_Container;
    public Text Melee_Psionic_Attack_Text;

    public GameObject Ranged_Attack_Container;
    public Text Ranged_Attack_Text;
    public Text Range_Text;
    public Text Ammo_Text;
    public GameObject Ranged_Magic_Attack_Container;
    public Text Ranged_Magic_Attack_Text;
    public GameObject Ranged_Psionic_Attack_Container;
    public Text Ranged_Psionic_Attack_Text;
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
    private RowScrollView<string> resistances_scroll_view;
    private RowScrollView<object> abilities_scroll_view;
    private float panel_position_x;
    private bool train_gui;
    private Vector3 melee_psionic_default_position;
    private Vector3 ranged_psionic_default_position;

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
        resistances_scroll_view = new RowScrollView<string>("resistances_scroll_view", Resistances_Content, Resistances_Row_Prototype, 15.0f);
        abilities_scroll_view = new RowScrollView<object>("abilities_scroll_view", Abilities_Content, Abilities_Row_Prototype, 15.0f);
        panel_position_x = Panel.gameObject.transform.position.x;
        melee_psionic_default_position = Melee_Psionic_Attack_Container.transform.position;
        ranged_psionic_default_position = Ranged_Psionic_Attack_Container.transform.position;
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
            panel_position_x + (train_gui ? 200.0f : 0.0f),
            Panel.gameObject.transform.position.y,
            Panel.gameObject.transform.position.z
        );
        TooltipManager.Instance.Unregister_Tooltips_By_Owner(gameObject);
        Name_Text.text = Unit.Name;
        Image.sprite = SpriteManager.Instance.Get(Unit.Texture, SpriteManager.SpriteType.Unit);

        Train_Button.gameObject.SetActive(train_gui);
        if (train_gui) {
            if(CityGUIManager.Instance.Current_City == null) {
                CustomLogger.Instance.Error("CityGUIManager.Instance.Current_City == null");
                Active = false;
                return;
            }
            Train_Button.GetComponentInChildren<Text>().text = Unit.Is_Summon ? "Summon" : "Train";
            string message = null;
            Train_Button.interactable = CityGUIManager.Instance.Current_City.Can_Train(Unit, out message);
            if (!string.IsNullOrEmpty(message)) {
                TooltipManager.Instance.Register_Tooltip(Train_Button.gameObject, message, gameObject);
            }
        }
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
        Tags_Text.text = string.Join(", ", Unit.Tags.Where(x => !Unit.HIDDEN_TAGS.Contains(x)).Select(x => Helper.Snake_Case_To_UI(x.ToString())).ToArray());
        Relative_Strenght_Text.text = Helper.Float_To_String(Is_Preview ? Unit.Relative_Strenght : Unit.Current_Relative_Strenght, 0);
        Manpower_Image.gameObject.SetActive(!Is_Preview);
        Manpower_Text.gameObject.SetActive(!Is_Preview);
        Manpower_Text.text = Helper.Float_To_String(100.0f * Unit.Manpower, 0) + "%";
        Manpower_Text.color = Unit.Manpower <= 0.5f ? Color.red : (Unit.Manpower == 1.0f ? default_text_color : Color.yellow);
        Relative_Strenght_Text.color = Is_Preview ? default_text_color : Manpower_Text.color;
        Cost_Text.text = string.Format("{0} ({1}/turn)", Unit.Cost, Helper.Float_To_String(Unit.Upkeep, 2));
        if(Unit.Mana_Cost == 0.0f && Unit.Mana_Upkeep == 0.0f) {
            Mana_Container.SetActive(false);
            Production_Container_1.SetActive(true);
            Production_Container_2.SetActive(false);
            Production_Text_1.text = Unit.Production_Required.ToString();
        } else {
            Mana_Container.SetActive(true);
            Production_Container_1.SetActive(false);
            Production_Container_2.SetActive(true);
            Production_Text_2.text = Unit.Production_Required.ToString();
            Mana_Text.text = string.Format("{0} ({1}/turn)", Unit.Mana_Cost, Helper.Float_To_String(Unit.Mana_Upkeep, 2));
        }

        Melee_Attack_Text.text = Helper.Float_To_String(Unit.Melee_Attack.Total, 0);
        Charge_Bonus_Text.text = string.Format("{0}%", Helper.Float_To_String(Unit.Charge * 100.0f, 0, true));

        decimal magic_attack = Unit.Melee_Attack.Nature_Proportions.ContainsKey(Damage.Nature.Magical) ? Unit.Melee_Attack.Nature_Proportions[Damage.Nature.Magical] : 0.0m;
        Melee_Magic_Attack_Container.SetActive(magic_attack != 0.0m);
        if (Melee_Magic_Attack_Container.activeSelf) {
            Melee_Magic_Attack_Text.text = string.Format("{0}%", Helper.Float_To_String((float)magic_attack * 100.0f, 0));
            Melee_Psionic_Attack_Container.transform.position = melee_psionic_default_position;
        } else {
            Melee_Psionic_Attack_Container.transform.position = Melee_Magic_Attack_Container.transform.position;
        }
        decimal psionic_attack = Unit.Melee_Attack.Nature_Proportions.ContainsKey(Damage.Nature.Psionic) ? Unit.Melee_Attack.Nature_Proportions[Damage.Nature.Psionic] : 0.0m;
        Melee_Psionic_Attack_Container.SetActive(psionic_attack != 0.0m);
        if (Melee_Psionic_Attack_Container.activeSelf) {
            Melee_Psionic_Attack_Text.text = string.Format("{0}%", Helper.Float_To_String((float)psionic_attack * 100.0f, 0));
        }

        Melee_Damage_Types_Text.text = string.Join(", ", Unit.Melee_Attack.Type_Weights.OrderByDescending(x => x.Value).Select(x => string.Format("{0} {1}%", Helper.Snake_Case_To_UI(x.Key.ToString()), Helper.Float_To_String(x.Value * 100.0f, 0))).ToArray());

        Ranged_Attack_Container.SetActive(Unit.Can_Ranged_Attack);
        if(Unit.Can_Ranged_Attack) {
            Ranged_Attack_Text.text = Helper.Float_To_String(Unit.Ranged_Attack.Total, 0);
            Range_Text.text = Unit.Range.ToString();
            Ammo_Text.text = Unit.Max_Ammo > 0 ? Unit.Max_Ammo.ToString() : "N/A";

            magic_attack = Unit.Ranged_Attack.Nature_Proportions.ContainsKey(Damage.Nature.Magical) ? Unit.Ranged_Attack.Nature_Proportions[Damage.Nature.Magical] : 0.0m;
            Ranged_Magic_Attack_Container.SetActive(magic_attack != 0.0m);
            if (Ranged_Magic_Attack_Container.activeSelf) {
                Ranged_Magic_Attack_Text.text = string.Format("{0}%", Helper.Float_To_String((float)magic_attack * 100.0f, 0));
                Ranged_Psionic_Attack_Container.transform.position = ranged_psionic_default_position;
            } else {
                Ranged_Psionic_Attack_Container.transform.position = Ranged_Magic_Attack_Container.transform.position;
            }
            psionic_attack = Unit.Ranged_Attack.Nature_Proportions.ContainsKey(Damage.Nature.Psionic) ? Unit.Ranged_Attack.Nature_Proportions[Damage.Nature.Psionic] : 0.0m;
            Ranged_Psionic_Attack_Container.SetActive(psionic_attack != 0.0m);
            if (Ranged_Psionic_Attack_Container.activeSelf) {
                Ranged_Psionic_Attack_Text.text = string.Format("{0}%", Helper.Float_To_String((float)psionic_attack * 100.0f, 0));
            }

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
        if(Unit.Magic_Resistance != 1.0f) {
            resistances_scroll_view.Add("Damage.Nature.Magical", new List<UIElementData>() {
                new UIElementData("TypeText", "<i>Magic</i>"),
                new UIElementData("ValueText", string.Format("<i>{0}%</i>", Helper.Float_To_String(Unit.Magic_Resistance * 100.0f, 0)), Unit.Magic_Resistance < 1.0f ? Color.red : (Unit.Magic_Resistance > 1.0f ? Color.blue : default_text_color))
            });
        }
        if (Unit.Psionic_Resistance != 1.0f) {
            resistances_scroll_view.Add("Damage.Nature.Psionic", new List<UIElementData>() {
                new UIElementData("TypeText", "<i>Psionic</i>"),
                new UIElementData("ValueText", string.Format("<i>{0}%</i>", Helper.Float_To_String(Unit.Psionic_Resistance * 100.0f, 0)), Unit.Psionic_Resistance < 1.0f ? Color.red : (Unit.Psionic_Resistance > 1.0f ? Color.blue : default_text_color))
            });
        }

        foreach (KeyValuePair<Damage.Type, float> pair in Unit.Resistances.Where(x => x.Value != 1.0f).OrderBy(x => (int)x.Key)) {
            resistances_scroll_view.Add(string.Format("Damage.Type.{0}", pair.Key.ToString()), new List<UIElementData>() {
                new UIElementData("TypeText", Helper.Snake_Case_To_UI(pair.Key.ToString(), true)),
                new UIElementData("ValueText", string.Format("{0}%", Helper.Float_To_String(pair.Value * 100.0f, 0)), pair.Value < 1.0f ? Color.red : (pair.Value > 1.0f ? Color.blue : default_text_color))
            });
        }

        Morale_Text.text = Unit.Uses_Morale ? Helper.Float_To_String(Unit.Max_Morale, 0) : "N/A";
        Stamina_Text.text = Unit.Uses_Stamina ? Helper.Float_To_String(Unit.Max_Stamina, 0) : "N/A";
        Movement_Text.text = string.Format("{0} / {1}", Helper.Float_To_String(Unit.Max_Movement, 0), Helper.Float_To_String(Unit.Max_Campaing_Map_Movement, 0));
        LoS_Text.text = Unit.LoS.ToString();
        Discipline_Text.text = Helper.Float_To_String(Unit.Discipline, 0);

        abilities_scroll_view.Clear();
        foreach(UnitAction action in Unit.Actions) {
            abilities_scroll_view.Add(action, new List<UIElementData>() {
                new UIElementData("NameText", string.Format("     {0}", action.Name)),
                new UIElementData("ValueText", string.Empty),
                new UIElementData("IconImage", action.Sprite_Name, action.Sprite_Type)
            });
        }
        foreach(Ability ability in Unit.Abilities.Where(x => !x.Is_Hidden).ToList()) {
            abilities_scroll_view.Add(ability, new List<UIElementData>() {
                new UIElementData("NameText", ability.Name),
                new UIElementData("ValueText", ability.Uses_Potency ? (ability.Potency_As_Percent ? string.Format("{0}%", Helper.Float_To_String(ability.Potency * 100.0f, 0)) : Helper.Float_To_String(ability.Potency, 2)) : string.Empty),
                new UIElementData("IconImage", "empty", SpriteManager.SpriteType.UI)
            });
        }
    }
}
