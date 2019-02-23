using UnityEngine;

public class World : MonoBehaviour
{
    public enum Direction { North, North_East, East, South_East, South, South_West, West, North_West }

    public static World Instance { get; private set; }
    public Map Map { get; private set; }

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
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if(Map != null) {
            Map.Update(Time.deltaTime);
        }
    }

    public void Generate_New_Map(int width, int height)
    {
        if(Map != null) {
            Map.Delete();
        }
        Map = new Map(width, height, 0.35f);
    }
}
