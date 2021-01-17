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
    public GameObject Content;
    public GameObject Row_Prototype;
    public Scrollbar Scrollbar;

    public LogLevel Max_Log_Level { get; set; }

    private RowScrollView<long> log_scroll_view;
    private long current_index;
    private bool new_log_message;
    
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
        Max_Log_Level = LogLevel.Verbose;
        log_scroll_view = new RowScrollView<long>("log_scroll_view", Content, Row_Prototype, 15.0f);
        current_index = 0;
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    {
        if (Scrollbar.value != 0.0f && new_log_message) {
            Scrollbar.value = 0.0f;
            new_log_message = false;
        }
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
        log_scroll_view.Clear();
        current_index = 0;
    }

    public void Print_Log(string line, LogLevel level = LogLevel.Basic)
    {
        if((int)level > (int)Max_Log_Level) {
            return;
        }
        log_scroll_view.Add(current_index, new List<UIElementData>() { new UIElementData("Text", line) });
        current_index++;
        new_log_message = true;
    }
}
