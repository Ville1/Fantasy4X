using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ScoresGUIManager : MonoBehaviour {
    public static ScoresGUIManager Instance;

    public GameObject Panel;
    private List<GameObject> gameobjects;

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
            Panel.SetActive(value);
            if(Active && gameobjects == null) {
                gameobjects = new List<GameObject>();
                int index = 1;
                while (true) {
                    GameObject go = GameObject.Find(Panel.name + "/Player" + index);
                    if (go == null) {
                        break;
                    }
                    go.SetActive(false);
                    gameobjects.Add(go);
                    index++;
                }
            }
        }
    }

    public void Update_Scores()
    {
        if (!Active) {
            return;
        }

        foreach(GameObject go in gameobjects) {
            go.SetActive(false);
        }

        Dictionary<Player, float> players_and_scores = new Dictionary<Player, float>();
        foreach(Player player in Main.Instance.Players) {
            if (!player.Defeated) {
                players_and_scores.Add(player, player.Score);
            }
        }
        List<Player> players_by_score = players_and_scores.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
        for(int i = 0; i < players_by_score.Count && i < gameobjects.Count; i++) {
            gameobjects[i].SetActive(true);
            GameObject.Find(gameobjects[i].name + "/NameText").GetComponent<Text>().text = players_by_score[i].Name;
            GameObject.Find(gameobjects[i].name + "/ScoreText").GetComponent<Text>().text = Mathf.RoundToInt(players_by_score[i].Score).ToString();
        }
    }
}
