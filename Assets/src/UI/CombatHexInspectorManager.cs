using UnityEngine;
using UnityEngine.UI;

public class CombatHexInspectorManager : MonoBehaviour
{
    public static CombatHexInspectorManager Instance;

    public GameObject Panel;

    public Text Name_Text;
    public Image Hex_Image;
    public Text Movement_Cost_Text;
    public Text Cover_Text;
    public Text Elevation_Text;
    public Text Height_Text;

    public GameObject Unit_Panel;
    public Image Unit_Image;
    public Image Routing_Icon_Image;
    public Text Unit_Name_Text;
    public Image Unit_Manpower_Bar_Image;
    public Text Unit_Manpower_Text;
    public Image Unit_Morale_Bar_Image;
    public Text Unit_Morale_Text;
    public Image Unit_Stamina_Bar_Image;
    public Text Unit_Stamina_Text;

    private float bar_max_lenght;

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
        bar_max_lenght = Unit_Manpower_Bar_Image.GetComponentInChildren<RectTransform>().rect.width;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!CombatManager.Instance.Active_Combat) {
            if (Active) {
                Active = false;
            }
            return;
        }
        if(MouseManager.Instance.Hex_Under_Cursor != null && MouseManager.Instance.Hex_Under_Cursor is CombatMapHex) {
            Panel.SetActive(true);
            CombatMapHex hex = MouseManager.Instance.Hex_Under_Cursor as CombatMapHex;
            Name_Text.text = hex.Terrain;
            Hex_Image.sprite = SpriteManager.Instance.Get(hex.Sprite, SpriteManager.SpriteType.Terrain);
            Hex_Image.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Hex_Image.sprite.rect.height > Hex_Image.sprite.rect.width ? 100.0f : 50.0f);
            Movement_Cost_Text.text = Helper.Float_To_String(hex.Movement_Cost, 1);
            Cover_Text.text = string.Format("{0}%", Helper.Float_To_String(hex.Cover * 100.0f, 0));
            Elevation_Text.text = Helper.Float_To_String(hex.Elevation, 1);
            Height_Text.text = Helper.Float_To_String(hex.Height, 1);
            if(hex.Unit != null) {
                Unit_Panel.SetActive(true);
                Unit_Image.sprite = SpriteManager.Instance.Get(hex.Unit.Texture, SpriteManager.SpriteType.Unit);
                Unit_Name_Text.text = hex.Unit.Name;
                Routing_Icon_Image.gameObject.SetActive(hex.Unit.Is_Routed);
                Unit_Manpower_Text.text = string.Format("{0}%", Helper.Float_To_String(hex.Unit.Manpower * 100.0f, 0));
                Unit_Manpower_Bar_Image.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bar_max_lenght * hex.Unit.Manpower);
                Unit_Morale_Text.text = string.Format("{0}%", Helper.Float_To_String(hex.Unit.Relative_Morale * 100.0f, 0));
                Unit_Morale_Bar_Image.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bar_max_lenght * hex.Unit.Relative_Morale);
                Unit_Stamina_Text.text = string.Format("{0}%", Helper.Float_To_String(hex.Unit.Relative_Stamina * 100.0f, 0));
                Unit_Stamina_Bar_Image.GetComponentInChildren<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bar_max_lenght * hex.Unit.Relative_Stamina);
            } else {
                Unit_Panel.SetActive(false);
            }
        } else {
            Panel.SetActive(false);
        }
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
}
