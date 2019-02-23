using System;
using UnityEngine;
using UnityEngine.UI;

public class HexPanelManager : MonoBehaviour
{
    private static Color Border_Color = new Color(0.70f, 0.70f, 0.70f);

    public static HexPanelManager Instance { get; private set; }

    public GameObject Panel;
    public Text Terrain_Text;
    public Image Hex_Image;
    public Text Food_Text;
    public Text Production_Text;
    public Text Cash_Text;
    public Text Science_Text;
    public Text Culture_Text;
    public Text Mana_Text;
    public Text Faith_Text;
    public Text Movement_Cost_Text;
    public Text Improvement_Text;
    public Text Owner_Text;
    public Text Mineral_Text;

    private WorldMapHex hex;

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Warning(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
        Active = false;
        hex = null;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {

    }

    public bool Active
    {
        get {
            return Panel.activeSelf;
        }
        set {
            if(value == Active) {
                return;
            }
            Panel.SetActive(value);
            if(hex != null) {
                hex.Borders = Active ? Border_Color : (Color?)null;
            }
        }
    }

    public WorldMapHex Hex
    {
        get {
            return hex;
        }
        set {
            WorldMapHex old_hex = hex;
            hex = value;
            if (hex == null) {
                Active = false;
                if (old_hex != null) {
                    old_hex.Borders = null;
                }
                return;
            }
            Active = true;
            Terrain_Text.text = hex.Terrain + " " + hex.Coordinates.X + "," + hex.Coordinates.Y;
            Hex_Image.overrideSprite = SpriteManager.Instance.Get_Sprite(hex.Texture, SpriteManager.SpriteType.Terrain);
            Food_Text.text = Helper.Float_To_String(hex.Yields.Food, 1, false, false);
            Production_Text.text = Helper.Float_To_String(hex.Yields.Production, 1, false, false);
            Cash_Text.text = Helper.Float_To_String(hex.Yields.Cash, 1, false, false);
            Science_Text.text = Helper.Float_To_String(hex.Yields.Science, 1, false, false);
            Culture_Text.text = Helper.Float_To_String(hex.Yields.Culture, 1, false, false);
            Mana_Text.text = Helper.Float_To_String(hex.Yields.Mana, 1, false, false);
            Faith_Text.text = Helper.Float_To_String(hex.Yields.Faith, 1, false, false);
            Movement_Cost_Text.text = string.Format("Movement{0}Cost: {1}", Environment.NewLine, hex.Passable ? hex.Movement_Cost.ToString("#.#") : "N/A");
            Improvement_Text.text = string.Format("Improvement:{0}{1}", Environment.NewLine, hex.Improvement == null ? "None" : hex.Improvement.Name);
            Owner_Text.text = string.Format("Owner:{0}{1}", Environment.NewLine, hex.Has_Owner ? hex.Owner.Name : "None");
            if (Hex.Can_Spawn_Minerals) {
                Mineral_Text.gameObject.SetActive(true);
                if (Hex.Is_Prospected_By(Main.Instance.Viewing_Player)) {
                    Mineral_Text.text = string.Format("Mineral:{0}{1}", Environment.NewLine, hex.Mineral != null ? hex.Mineral.Name : "None");
                } else {
                    Mineral_Text.text = string.Format("Mineral:{0}???", Environment.NewLine);
                }
            } else {
                Mineral_Text.gameObject.SetActive(false);
            }

            if (old_hex != null) {
                old_hex.Borders = null;
            }
            hex.Borders = Border_Color;
        }
    }
}