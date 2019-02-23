using UnityEngine;

public class KeyboardManager : MonoBehaviour
{
    public static KeyboardManager Instance { get; private set; }

    /// <summary>
    /// Initialization
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Update is called once per frame
    /// TODO: MouseManager stuff
    /// </summary>
    private void Update()
    {
        if (Input.anyKey) {
            MessageManager.Instance.Active = false;
        }

        if (Input.GetButtonDown("Console")) {
            ConsoleManager.Instance.Toggle_Console();
        }

        /*if (Input.anyKeyDown && !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2)) {
            UIManager.Instance.Hide_Message();
        }*/

        if (!MasterUIManager.Instance.Intercept_Keyboard_Input) {
            //Move camera
            if (Input.GetAxis("Vertical") > 0.0f) {
                CameraManager.Instance.Move_Camera(World.Direction.North);
            }
            if (Input.GetAxis("Horizontal") < 0.0f) {
                CameraManager.Instance.Move_Camera(World.Direction.West);
            }
            if (Input.GetAxis("Vertical") < 0.0f) {
                CameraManager.Instance.Move_Camera(World.Direction.South);
            }
            if (Input.GetAxis("Horizontal") > 0.0f) {
                CameraManager.Instance.Move_Camera(World.Direction.East);
            }
            
            if (Input.GetButtonDown("Next turn") && !Main.Instance.Other_Players_Turn) {
                if (CombatManager.Instance.Active_Combat) {
                    CombatManager.Instance.Next_Turn();
                } else {
                    BottomGUIManager.Instance.Next_Turn_On_Click();
                }
            }
            if(Input.GetButtonDown("Close all windows")) {
                MasterUIManager.Instance.Close_All();
            }
            if (Input.GetButtonDown("Next unit")) {
                BottomGUIManager.Instance.Next_Unit();
            }
            if (Input.GetButtonDown("Wait turn")) {
                BottomGUIManager.Instance.Wait_Turn();
            }
            if (Input.GetButtonDown("Sleep")) {
                BottomGUIManager.Instance.Sleep();
            }

            if(Input.GetButton("Deploy unit")) {
                CombatUIManager.Instance.Deploy_Button_On_Click();
            }
        } else {
            MasterUIManager.Instance.Read_Keyboard_Input();
        }
    }

    public bool Shift_Down
    {
        get {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }
    }
}
