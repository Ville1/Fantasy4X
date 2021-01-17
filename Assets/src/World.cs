using UnityEngine;

public class World : MonoBehaviour
{
    public enum Direction { North, North_East, East, South_East, South, South_West, West, North_West }
    public enum GameState { Menu, Normal, Saving, Loading }

    public static World Instance { get; private set; }
    public Map Map { get; private set; }
    public GameState State { get; private set; }

    private bool map_processed;
    private float progress;

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
        State = GameState.Menu;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        switch (State) {
            case GameState.Normal:
                if (Map != null) {
                    Map.Update(Time.deltaTime);
                }
                break;
            case GameState.Saving:
                Process_Saving();
                break;
            case GameState.Loading:
                Process_Loading();
                break;
        }
    }

    public void Generate_New_Map(int width, int height)
    {
        if(Map != null) {
            Map.Delete();
        }
        Map = new Map(width, height, 0.35f);
        State = GameState.Normal;
    }

    public void Generate_Placeholder_Map()
    {
        if (Map != null) {
            Map.Delete();
        }
        State = GameState.Normal;
        Map = new Map(3, 3, 0.35f);
    }

    public void Start_Saving()
    {
        map_processed = false;
        progress = 0.0f;
        State = GameState.Saving;
        Map.Start_Saving();
        Update_Progress();
    }

    public void Process_Saving()
    {
        if (!map_processed) {
            float map_progress = Map.Process_Saving();
            if(map_progress == -1.0f) {
                map_processed = true;
                progress = 1.0f;
            } else {
                progress = map_progress;
            }
            Update_Progress();
        } else {
            Finish_Saving();
        }
    }

    private void Finish_Saving()
    {
        SaveManager.Instance.Finish_Saving();
        State = GameState.Normal;
        ProgressBarManager.Instance.Active = false;
    }

    public void Start_Loading()
    {
        map_processed = false;
        progress = 0.0f;
        State = GameState.Loading;
        if (Map != null) {
            Map.Delete();
        }
        Map = new Map(SaveManager.Instance.Data.Map);
        EffectManager.Instance.Update_Target_Map();
        Update_Progress();
    }

    public void Process_Loading()
    {
        if (!map_processed) {
            float map_progress = Map.Process_Loading();
            if (map_progress == -1.0f) {
                map_processed = true;
                progress = 1.0f;
            } else {
                progress = map_progress;
            }
            Update_Progress();
        } else {
            Finish_Loading();
        }
    }

    private void Finish_Loading()
    {
        State = GameState.Normal;
        ProgressBarManager.Instance.Active = false;
        Main.Instance.Finish_Loading();
        SaveManager.Instance.Finish_Loading();
    }

    private void Update_Progress()
    {
        ProgressBarManager.Instance.Show(State == GameState.Saving ? "Saving..." : "Loading...", progress);
    }
}
