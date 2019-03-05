using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellGUIManager : MonoBehaviour {
    public static SpellGUIManager Instance;

    public GameObject Panel;
    public GameObject Spell_GameObject;

    private List<GameObject> spell_game_objects;
    private int current_row_id;

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
        Spell_GameObject.SetActive(false);
        spell_game_objects = new List<GameObject>();
        current_row_id = 0;
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
            if (Main.Instance.Other_Players_Turn || Active == value) {
                return;
            }
            if (value) {
                MasterUIManager.Instance.Close_All();
            }
            Panel.SetActive(value);
            if (Active) {
                Update_List();
            } else {
                Clear_List();
                current_row_id = 0;
            }
        }
    }

    public void Toggle()
    {
        Active = !Active;
    }

    private void Update_List()
    {
        Clear_List();
        //Weird stuff happens if Spell-GO is destroyed and then immediately recreated with same name
        List<Spell> available_spells = Main.Instance.Current_Player.Available_Spells;
        float row_height = 20.0f;
        for(int i = 0; i < available_spells.Count; i++) {
            Spell spell = available_spells[i];
            GameObject go = GameObject.Instantiate(Spell_GameObject);
            go.SetActive(true);
            go.transform.SetParent(Panel.transform, false);
            go.name = "Spell" + current_row_id;
            current_row_id++;
            go.transform.position = new Vector3(Spell_GameObject.transform.position.x,
                Spell_GameObject.transform.position.y - (i * row_height), Spell_GameObject.transform.position.z);
            Get_Text(go.name, "Name").text = spell.Name;
            Get_Text(go.name, "ManaCost").text = Mathf.FloorToInt(spell.Mana_Cost).ToString();
            Get_Text(go.name, "CastButton").text = Main.Instance.Current_Player.Spell_Cooldown(spell) > 0 ? string.Format("cd: {0}",
                Main.Instance.Current_Player.Spell_Cooldown(spell)) : "Cast";
            go.GetComponentInChildren<Button>().interactable = Main.Instance.Current_Player.Can_Cast(spell);
            Button.ButtonClickedEvent on_click_event = new Button.ButtonClickedEvent();
            int index = i;
            on_click_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                if (!spell.Requires_Target) {
                    Spell.SpellResult result = spell.Cast(Main.Instance.Current_Player, null);
                    if (result.Has_Message) {
                        MessageManager.Instance.Show_Message(result.Message);
                    }
                    if (result.Success) {
                        Update_List();
                    }
                } else {
                    Active = false;
                    MouseManager.Instance.Set_Select_Hex_Mode(true, delegate(Hex hex) {
                        if (!(hex is WorldMapHex)) {
                            CustomLogger.Instance.Warning("Hex is not WorldMapHex");
                        } else {
                            Spell.SpellResult result = spell.Cast(Main.Instance.Current_Player, hex as WorldMapHex);
                            if (result.Has_Message) {
                                MessageManager.Instance.Show_Message(result.Message);
                            }
                        }
                    });
                }
            }));
            go.GetComponentInChildren<Button>().onClick = on_click_event;
            spell_game_objects.Add(go);
            TooltipManager.Instance.Register_Tooltip(Get_Text(go.name, "Name").gameObject, available_spells[i].Tooltip, gameObject);
        }
    }

    private void Clear_List()
    {
        TooltipManager.Instance.Unregister_Tooltip(gameObject);
        foreach (GameObject go in spell_game_objects) {
            GameObject.Destroy(go);
        }
        spell_game_objects.Clear();
    }

    private Text Get_Text(string parent_game_object_name, string name)
    {
        return GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, name)).GetComponentInChildren<Text>();
    }
}
