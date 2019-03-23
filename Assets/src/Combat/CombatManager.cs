using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatManager : MonoBehaviour {
    private static readonly int map_width = 20;
    private static readonly int map_height = 20;

    public static CombatManager Instance;

    public CombatMap Map { get; private set; }
    public Army Current_Army { get; private set; }
    public Army Other_Army { get; private set; }
    public bool Deployment_Mode { get; private set; }
    public Army Army_1 { get; private set; }
    public Army Army_2 { get; private set; }
    public bool Retreat_Phase { get; private set; }
    public float Retreat_Move_Cooldown { get; set; }

    private float retreat_timer;
    private bool end_retreat_phase;
    private bool active_combat;

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
        Retreat_Move_Cooldown = 0.5f;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (!Active_Combat) {
            return;
        }
        foreach(Unit unit in Army_1.Units) {
            unit.Update(Time.deltaTime);
        }
        foreach (Unit unit in Army_2.Units) {
            unit.Update(Time.deltaTime);
        }
        if (Retreat_Phase) {
            if (retreat_timer > 0.0f) {
                retreat_timer -= Time.deltaTime;
                return;
            }

            bool retreating_units = false;
            foreach (Unit u in Current_Army.Units) {
                if (u.Retreat()) {
                    retreating_units = true;
                    if (u == CombatUIManager.Instance.Current_Unit) {
                        CombatUIManager.Instance.Update_Current_Unit();
                    }
                    break;
                }
            }

            retreat_timer += Retreat_Move_Cooldown;
            if (!retreating_units) {
                end_retreat_phase = true;
                Next_Turn();
            }
        } else if(Current_Player.AI != null) {
            Current_Player.AI.Combat_Act(Time.deltaTime);
        }
    }

    public bool Active_Combat
    {
        get {
            return active_combat;
        }
    }

    public void Start_Combat(Army army_1, Army army_2, WorldMapHex hex)
    {
        World.Instance.Map.Active = false;
        active_combat = true;
        MasterUIManager.Instance.Combat_UI = true;
        Current_Army = army_1;
        Other_Army = army_2;
        Army_1 = army_1;
        Army_2 = army_2;
        Army_1.Start_Combat();
        Army_2.Start_Combat();
        Map = new CombatMap(map_width, map_height, hex);
        CameraManager.Instance.Set_Camera_Location(Map.Center_Of_Deployment_1);
        Deployment_Mode = true;
        CombatUIManager.Instance.Current_Unit = Current_Army.Units[0];
        Map.Set_Deployment_Mode(Current_Army);
        EffectManager.Instance.Update_Target_Map();
        Retreat_Phase = false;
        retreat_timer = 0.0f;
        end_retreat_phase = false;
        CombatLogManager.Instance.Clear_Log();
        CombatLogManager.Instance.Print_Log(string.Format("Combat starts: {0} ({1}) vs {2} ({3})", army_1.Owner.Name, army_1.Owner.Faction.Name, army_2.Owner.Name, army_2.Owner.Faction.Name));
    }

    public void End_Combat(bool victory)
    {
        active_combat = false;
        World.Instance.Map.Active = true;
        MasterUIManager.Instance.Combat_UI = false;
        WorldMapHex hex = Map.WorldMapHex;
        Map.Delete();
        Map = null;
        Army_1.End_Combat();
        Army_2.End_Combat();
        CameraManager.Instance.Set_Camera_Location(Army_1.Hex);
        EffectManager.Instance.Update_Target_Map();
        MessageManager.Instance.Show_Message(victory ? "Victory" : "Defeat");
        if (victory) {
            Army_1.Push_Into(hex);
            if(hex.City != null) {//Villages?
                Army_2.Delete();
            }
        }
        Army_1.Update_Text();
        Army_2.Update_Text();
        BottomGUIManager.Instance.Update_Current_Entity();
    }

    public void Next_Turn()
    {
        if (Deployment_Mode) {
            foreach(Unit u in Current_Army.Units) {
                if(u.Hex == null) {
                    MessageManager.Instance.Show_Message("You still have undeployed units remaining");
                    return;
                }
            }
            if(Current_Army == Army_2) {
                Deployment_Mode = false;
                Map.Set_Deployment_Mode(null);
            } else {
                CameraManager.Instance.Set_Camera_Location(Map.Center_Of_Deployment_2);
            }
        } else {
            Current_Army.End_Combat_Turn();
        }

        if(!Retreat_Phase && !end_retreat_phase && !Deployment_Mode) {
            Retreat_Phase = true;
            retreat_timer = 0.0f;
            CombatUIManager.Instance.Update_GUI();
            return;
        }
        if (end_retreat_phase) {
            Retreat_Phase = false;
            end_retreat_phase = false;
        }

        bool has_units = false;
        foreach(Unit u in Current_Army.Units) {
            if(u.Hex != null) {
                has_units = true;
                break;
            }
        }
        if (!has_units) {
            End_Combat(Current_Army == Army_2);
            return;
        }


        if(Current_Army.Owner.Id == Army_1.Owner.Id) {
            Current_Army = Army_2;
            Other_Army = Army_1;
        } else {
            Current_Army = Army_1;
            Other_Army = Army_2;
        }
        if (Deployment_Mode) {
            Map.Set_Deployment_Mode(Current_Army);
        }

        if (!Deployment_Mode) {
            foreach (Unit u in Current_Army.Units) {
                if(u.Hex == null) {
                    continue;
                }
                u.Hex.Borders = CombatMapHex.Owned_Unit_Color;
            }
            CombatUIManager.Instance.Current_Unit = Current_Army.Units.First(x => x.Hex != null);
        } else {
            CombatUIManager.Instance.Current_Unit = Current_Army.Units[0];
        }
        
        foreach (Unit u in Other_Army.Units) {
            if (Deployment_Mode) {
                u.Hex.Borders = null;
            } else if(u.Hex != null) {
                u.Hex.Borders = CombatMapHex.Enemy_Unit_Color;
            }
        }
        CombatUIManager.Instance.Update_GUI();
        if(CombatUIManager.Instance.Current_Unit.Hex != null) {
            CameraManager.Instance.Set_Camera_Location(CombatUIManager.Instance.Current_Unit.Hex);
        }
    }

    public Player Current_Player
    {
        get {
            return Current_Army.Owner;
        }
    }

    public Player Other_Player
    {
        get {
            return Other_Army.Owner;
        }
    }

    public bool Other_Players_Turn
    {
        get {
            return Current_Player.AI != null;
        }
    }

    public void Next_Unit()
    {
        List<Unit> units = Deployment_Mode ? Current_Army.Units : Current_Army.Units.Where(x => x.Hex != null).ToList();
        int current_index = units.IndexOf(CombatUIManager.Instance.Current_Unit);
        current_index++;
        if(current_index >= units.Count) {
            current_index = 0;
        }
        CombatUIManager.Instance.Current_Unit = units[current_index];
    }

    public void Previous_Unit()
    {
        List<Unit> units = Deployment_Mode ? Current_Army.Units : Current_Army.Units.Where(x => x.Hex != null).ToList();
        int current_index = units.IndexOf(CombatUIManager.Instance.Current_Unit);
        current_index--;
        if (current_index < 0) {
            current_index = units.Count - 1;
        }
        CombatUIManager.Instance.Current_Unit = units[current_index];
    }
}
