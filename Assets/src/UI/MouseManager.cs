using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    public static MouseManager Instance { get; private set; }
    public delegate void Select_Hex_Delegate(Hex hex);

    public bool Select_Hex_Mode { get; private set; }

    private Vector3 last_position;
    private Select_Hex_Delegate select_hex_delegate;

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
        Select_Hex_Mode = false;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        Vector3 current_position = CameraManager.Instance.Camera.ScreenToWorldPoint(Mouse_Position_Relative_To_Camera);
        if (last_position != null && Input.GetMouseButton(2)) {
            //Move camera
            Vector3 difference = last_position - current_position;
            CameraManager.Instance.Move_Camera(-1.0f * difference);
            //Close stuff
            MasterUIManager.Instance.Close_All();
        }

        //Buttons
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            MessageManager.Instance.Active = false;
        }
        if (!EventSystem.current.IsPointerOverGameObject()) {
            if (Input.GetMouseButtonDown(0)) {
                if (Hex_Under_Cursor != null && Hex_Under_Cursor is WorldMapHex) {
                    WorldMapHex hex = Hex_Under_Cursor as WorldMapHex;
                    if (!Select_Hex_Mode) {
                        if (hex.City != null && !Main.Instance.Other_Players_Turn && hex.City.Is_Owned_By_Current_Player && ((hex.Entity == null && hex.Civilian == null)
                             || (hex.Entity != null && hex.Civilian == null && BottomGUIManager.Instance.Current_Entity == hex.Entity) 
                             || (hex.Civilian != null && BottomGUIManager.Instance.Current_Entity == hex.Civilian))) {
                            //Open city GUI
                            CityGUIManager.Instance.Current_City = hex.City;
                        } else {
                            if (CityGUIManager.Instance.Active) {
                                //City manager
                                CityGUIManager.Instance.Hex_On_Click(hex);
                            } else {
                                //Select entity and/or hex
                                //TODO: messy code
                                if(hex.Visible_To_Viewing_Player && (hex.Entity != null || hex.Civilian != null)) {
                                    if(hex.Entity != null && hex.Civilian != null) {
                                        if(BottomGUIManager.Instance.Current_Entity == hex.Entity) {
                                            BottomGUIManager.Instance.Current_Entity = hex.Civilian;
                                        } else if(BottomGUIManager.Instance.Current_Entity == hex.Civilian) {
                                            BottomGUIManager.Instance.Current_Entity = hex.Entity;
                                        } else {
                                            BottomGUIManager.Instance.Current_Entity = hex.Entity;
                                        }
                                    } else if(hex.Entity != null) {
                                        BottomGUIManager.Instance.Current_Entity = hex.Entity;
                                    } else {
                                        BottomGUIManager.Instance.Current_Entity = hex.Civilian;
                                    }
                                } else {
                                    BottomGUIManager.Instance.Current_Entity = null;
                                }
                                HexPanelManager.Instance.Hex = hex.Is_Explored_By(Main.Instance.Viewing_Player) ? hex : null;
                            }
                        }
                    } else {
                        //Select hex mode
                        select_hex_delegate(hex);
                        Set_Select_Hex_Mode(false);
                    }
                } else if (Hex_Under_Cursor != null && Hex_Under_Cursor is CombatMapHex) {
                    CombatMapHex hex = Hex_Under_Cursor as CombatMapHex;
                    if (!Select_Hex_Mode) {
                        if(hex.Unit == null) {
                            CombatUIManager.Instance.Current_Unit = null;
                        } else {
                            CombatUIManager.Instance.Current_Unit = hex.Unit;
                        }
                    } else {
                        //Select hex mode
                        select_hex_delegate(hex);
                        Set_Select_Hex_Mode(false);
                    }
                } else {
                    //Unselect entity and/or hex
                    HexPanelManager.Instance.Hex = null;
                    BottomGUIManager.Instance.Current_Entity = null;
                    //Stop Select Hexh Mode
                    Set_Select_Hex_Mode(false);
                }

                //Close stuff
                Close_Panels();
                ConsoleManager.Instance.Close_Console();
            } else if (Input.GetMouseButtonDown(1) && Hex_Under_Cursor != null) {
                if (Hex_Under_Cursor is WorldMapHex && BottomGUIManager.Instance.Current_Entity != null && BottomGUIManager.Instance.Current_Entity.Is_Owned_By_Current_Player && !Main.Instance.Other_Players_Turn) {
                    //Move entity
                    if(BottomGUIManager.Instance.Current_Entity.Move(Hex_Under_Cursor as WorldMapHex)) {
                        if (BottomGUIManager.Instance.Current_Entity is Worker) {
                            (BottomGUIManager.Instance.Current_Entity as Worker).Update_Actions_List();
                            BottomGUIManager.Instance.Update_Entity_Info();
                        } else {
                            BottomGUIManager.Instance.Update_Current_Entity();
                        }
                        BottomGUIManager.Instance.Current_Entity.Clear_Stored_Path();
                        PathRenderer.Instance.Clear_Path();
                    } else {
                        List<PathfindingNode> path = World.Instance.Map.Path(BottomGUIManager.Instance.Current_Entity.Hex, Hex_Under_Cursor as WorldMapHex, BottomGUIManager.Instance.Current_Entity, true,
                            true);
                        if (path.Count != 0) {
                            BottomGUIManager.Instance.Current_Entity.Create_Stored_Path(path);
                            while(BottomGUIManager.Instance.Current_Entity.Current_Movement > 0.0f) {
                                if (!BottomGUIManager.Instance.Current_Entity.Follow_Stored_Path()) {
                                    break;
                                }
                            }
                            BottomGUIManager.Instance.Update_Entity_Info();
                            if (BottomGUIManager.Instance.Current_Entity.Stored_Path != null) {
                                PathRenderer.Instance.Render_Path(BottomGUIManager.Instance.Current_Entity.Hex.Coordinates, path);
                            }
                        } else {
                            BottomGUIManager.Instance.Current_Entity.Clear_Stored_Path();
                            PathRenderer.Instance.Clear_Path();
                        }
                    }
                    //Close stuff
                    Close_Panels();
                    //Stop Select Hexh Mode
                    Set_Select_Hex_Mode(false);
                } else if(Hex_Under_Cursor is CombatMapHex && CombatUIManager.Instance.Current_Unit != null && CombatUIManager.Instance.Current_Unit.Is_Owned_By_Current_Player &&
                        CombatUIManager.Instance.Current_Unit.Controllable && !CombatManager.Instance.Retreat_Phase) {
                    if((Hex_Under_Cursor as CombatMapHex).Unit != null) {
                        CombatUIManager.Instance.Current_Unit.Attack((Hex_Under_Cursor as CombatMapHex).Unit, false);
                    } else {
                        CombatUIManager.Instance.Current_Unit.Pathfind(Hex_Under_Cursor as CombatMapHex, CombatUIManager.Instance.Run);
                    }
                    CombatUIManager.Instance.Update_Current_Unit();
                }
            } 
        }
        //Zooming
        if (Input.GetAxis("Mouse ScrollWheel") > 0.0f) {
            CameraManager.Instance.Zoom_Camera(CameraManager.Zoom.Out);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0.0f) {
            CameraManager.Instance.Zoom_Camera(CameraManager.Zoom.In);
        }

        last_position = CameraManager.Instance.Camera.ScreenToWorldPoint(Mouse_Position_Relative_To_Camera);
    }

    private void Close_Panels()
    {
        TechnologyPanelManager.Instance.Active = false;
        SpellGUIManager.Instance.Active = false;
        BlessingGUIManager.Instance.Active = false;
        SelectTechnologyPanelManager.Instance.Active = false;
        MainMenuManager.Instance.Active = false;
    }

    public Vector3 Mouse_Position_Relative_To_Camera
    {
        get {
            Vector3 position = Input.mousePosition;
            position.z = CameraManager.Instance.Camera.transform.position.z;
            return position;
        }
    }

    public Hex Hex_Under_Cursor
    {
        get {
            if(World.Instance.Map == null) {
                return null;
            }
            RaycastHit hit;
            if (Physics.Raycast(CameraManager.Instance.Camera.ScreenPointToRay(Input.mousePosition), out hit) && hit.transform.gameObject.name.StartsWith(Hex.GAME_OBJECT_NAME_PREFIX)) {
                string name = hit.transform.gameObject.name;
                int x, y;
                if (!int.TryParse(name.Substring(name.IndexOf('(') + 1, name.IndexOf(',') - name.IndexOf('(') - 1), out x) ||
                    !int.TryParse(name.Substring(name.IndexOf(',') + 1, name.IndexOf(')') - name.IndexOf(',') - 1), out y)) {
                    CustomLogger.Instance.Error("String parsing error");
                    return null;
                } else {
                    if (CombatManager.Instance.Active_Combat) {
                        return CombatManager.Instance.Map.Get_Hex_At(x, y);
                    }
                    return World.Instance.Map.Get_Hex_At(x, y);
                }
            }
            return null;
        }
    }

    public void Set_Select_Hex_Mode(bool on, Select_Hex_Delegate d = null)
    {
        Select_Hex_Mode = on;
        select_hex_delegate = d;
    }
}
