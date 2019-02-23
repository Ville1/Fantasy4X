using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CombatLogManager : MonoBehaviour {
    private static readonly int TEXT_ROWS = 15;
    public enum LogLevel { Basic = 0, Verbose = 1 }

    public static CombatLogManager Instance;

    public GameObject Panel;
    public Text Text;
    public Button Up_Button;
    public Button Down_Button;

    public LogLevel Max_Log_Level { get; set; }

    private List<string> log;
    private int scroll_position;

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

        log = new List<string>();
        Panel.SetActive(false);
        Clear_Log();
        Max_Log_Level = LogLevel.Verbose;
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
            Panel.SetActive(value);
        }
    }

    public void Clear_Log()
    {
        log.Clear();
        Text.text = "";
        scroll_position = 0;
    }

    public void Print_Log(string line, LogLevel level = LogLevel.Basic)
    {
        if((int)level > (int)Max_Log_Level) {
            return;
        }
        log.Add(line);
        Update_Text();
    }

    public void Scroll_Up()
    {
        scroll_position++;
        if (log.Count - TEXT_ROWS - 2 - scroll_position < 0) {
            scroll_position--;
        }
        Update_Text();
    }

    public void Scroll_Down()
    {
        scroll_position--;
        if (scroll_position < 0) {
            scroll_position = 0;
        }
        Update_Text();
    }

    private void Update_Text()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < TEXT_ROWS; i++) {
            if (log.Count > i - scroll_position) {
                builder.Insert(0, log[log.Count - i - 1 - scroll_position] + Environment.NewLine);
            }
        }
        Text.text = builder.ToString();
    }
}
