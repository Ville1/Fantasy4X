using System.Collections.Generic;

public interface I_AI {
    void Start_Turn();
    void On_Delete();
    void Update_Settings();
    void Act(float delta_s);
    void Start_Combat();
    void Start_Combat_Turn();
    void Combat_Act(float delta_s);
    bool Show_Moves { get; set; }
    bool Log_Actions { get; set; }
    List<AI.LogType> Logged_Action_Types { get; set; }
    bool Follow_Moves { get; set; }
    float Time_Between_Actions { get; set; }
    Player Player { get; }
    AI.Level AI_Level { get; }
}
