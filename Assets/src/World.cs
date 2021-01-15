using UnityEngine;

public class World : MonoBehaviour
{
    public enum Direction { North, North_East, East, South_East, South, South_West, West, North_West }
    public enum GameState { Normal, Saving, Loading }

    public static World Instance { get; private set; }
    public Map Map { get; private set; }
    public GameState State { get; private set; }

    private bool map_saved;
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
        State = GameState.Normal;
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
    }

    public void Start_Saving(string path)
    {
        map_saved = false;
        progress = 0.0f;
        State = GameState.Saving;
        ProgressBarManager.Instance.Active = true;
        SaveManager.Instance.Start_Saving(path);
        Map.Start_Saving();
        Update_Progress();
    }

    public void Process_Saving()
    {
        if (!map_saved) {
            float map_progress = Map.Process_Saving();
            progress = map_progress;
            if(map_progress == 1.0f) {
                map_saved = true;
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

    public void Start_Loading(string path)
    {
        progress = 0.0f;
        SaveManager.Instance.Start_Loading(path);
        State = GameState.Loading;
        ProgressBarManager.Instance.Active = true;
        Update_Progress();
    }

    public void Process_Loading()
    {
        Update_Progress();
    }

    private void Finish_Loading()
    {
        SaveManager.Instance.Finish_Loading();
        State = GameState.Normal;
        ProgressBarManager.Instance.Active = false;
    }

    private void Update_Progress()
    {
        ProgressBarManager.Instance.Show(State == GameState.Saving ? "Saving..." : "Loading...", progress);
    }
}
