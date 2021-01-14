using System.Collections.Generic;
using UnityEngine;

public class PathRenderer {
    private static readonly string GAME_OBJECT_NAME_PREFIX = "PathNode";
    private static readonly string GAME_OBJECT_PARENT_NAME = "Path";
    private static readonly string SPRITE_NAME = "path_circle";
    private static PathRenderer instance;

    private List<GameObject> current_path_gos;
    private GameObject parent;

    private PathRenderer()
    {
        current_path_gos = new List<GameObject>();
        parent = GameObject.Find(GAME_OBJECT_PARENT_NAME);
    }

    public static PathRenderer Instance
    {
        get {
            if(instance == null) {
                instance = new PathRenderer();
            }
            return instance;
        }
    }

    public void Render_Path(Coordinates start, List<WorldMapHex> path)
    {
        List<PathfindingNode> nodes = new List<PathfindingNode>();
        foreach(WorldMapHex hex in path) {
            nodes.Add(hex.PathfindingNode);
        }
        Render_Path(start, nodes);
    }

    public void Render_Path(Coordinates start, List<PathfindingNode> path)
    {
        if (!Main.Instance.Game_Is_Running) {
            return;
        }
        Clear_Path();
        bool start_found = false;
        foreach(PathfindingNode node in path) {
            if (!start_found) {
                if (node.Coordinates.Equals(start)) {
                    start_found = true;
                } else {
                    continue;
                }
            }
            WorldMapHex hex = World.Instance.Map.Get_Hex_At(node.Coordinates);
            GameObject game_object = new GameObject();
            game_object.name = GAME_OBJECT_NAME_PREFIX + "(" + hex.Q + "," + hex.R + ")";
            game_object.transform.position = hex.GameObject.transform.position;
            game_object.transform.parent = parent.transform;
            game_object.AddComponent<BoxCollider>();
            SpriteRenderer renderer = game_object.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteManager.Instance.Get(SPRITE_NAME, SpriteManager.SpriteType.UI);
            current_path_gos.Add(game_object);
        }
    }

    public void Clear_Path()
    {
        foreach(GameObject go in current_path_gos) {
            GameObject.Destroy(go);
        }
        current_path_gos.Clear();
    }
}
