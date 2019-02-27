using System.Collections.Generic;
using UnityEngine;

public class Road {
    public string Type { get; private set; }
    public WorldMapHex Hex { get; private set; }
    public string North_East_Texture { get; private set; }
    public string East_Texture { get; private set; }
    public float Movement_Cost_Reduction { get; private set; }
    public GameObject Base_GameObject { get; private set; }
    public GameObject North_East_GameObject { get; private set; }
    public GameObject East_GameObject { get; private set; }
    public GameObject South_East_GameObject { get; private set; }
    public GameObject South_West_GameObject { get; private set; }
    public GameObject West_GameObject { get; private set; }
    public GameObject North_West_GameObject { get; private set; }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="type"></param>
    /// <param name="north_east_texture"></param>
    /// <param name="east_texture"></param>
    /// <param name="movement_cost_reduction"></param>
    public Road(string type, string north_east_texture, string east_texture, float movement_cost_reduction)
    {
        Type = type;
        North_East_Texture = north_east_texture;
        East_Texture = east_texture;
        Movement_Cost_Reduction = movement_cost_reduction;
    }

    public Road(WorldMapHex hex, Road prototype)
    {
        Hex = hex;
        Type = prototype.Type;
        North_East_Texture = prototype.North_East_Texture;
        East_Texture = prototype.East_Texture;
        Movement_Cost_Reduction = prototype.Movement_Cost_Reduction;

        hex.Road = this;
        Base_GameObject = new GameObject(Type);
        Base_GameObject.transform.parent = hex.GameObject.transform;
        Base_GameObject.transform.position = hex.GameObject.transform.position;

        //North east
        North_East_GameObject = new GameObject("NE-part");
        North_East_GameObject.transform.parent = Base_GameObject.transform;
        North_East_GameObject.transform.position = Base_GameObject.transform.position;
        SpriteRenderer renderer = North_East_GameObject.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = SortingLayer.ROADS;
        renderer.sprite = SpriteManager.Instance.Get_Sprite(North_East_Texture, SpriteManager.SpriteType.Improvement);

        //East
        East_GameObject = new GameObject("E-part");
        East_GameObject.transform.parent = Base_GameObject.transform;
        East_GameObject.transform.position = Base_GameObject.transform.position;
        renderer = East_GameObject.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = SortingLayer.ROADS;
        renderer.sprite = SpriteManager.Instance.Get_Sprite(East_Texture, SpriteManager.SpriteType.Improvement);

        //South east
        South_East_GameObject = new GameObject("SE-part");
        South_East_GameObject.transform.parent = Base_GameObject.transform;
        South_East_GameObject.transform.position = Base_GameObject.transform.position;
        renderer = South_East_GameObject.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = SortingLayer.ROADS;
        renderer.sprite = SpriteManager.Instance.Get_Sprite(North_East_Texture, SpriteManager.SpriteType.Improvement);
        renderer.flipY = true;

        //South west
        South_West_GameObject = new GameObject("SW-part");
        South_West_GameObject.transform.parent = Base_GameObject.transform;
        South_West_GameObject.transform.position = Base_GameObject.transform.position;
        renderer = South_West_GameObject.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = SortingLayer.ROADS;
        renderer.sprite = SpriteManager.Instance.Get_Sprite(North_East_Texture, SpriteManager.SpriteType.Improvement);
        renderer.flipX = true;
        renderer.flipY = true;

        //West
        West_GameObject = new GameObject("W-part");
        West_GameObject.transform.parent = Base_GameObject.transform;
        West_GameObject.transform.position = Base_GameObject.transform.position;
        renderer = West_GameObject.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = SortingLayer.ROADS;
        renderer.sprite = SpriteManager.Instance.Get_Sprite(East_Texture, SpriteManager.SpriteType.Improvement);
        renderer.flipX = true;

        //North west
        North_West_GameObject = new GameObject("NW-part");
        North_West_GameObject.transform.parent = Base_GameObject.transform;
        North_West_GameObject.transform.position = Base_GameObject.transform.position;
        renderer = North_West_GameObject.AddComponent<SpriteRenderer>();
        renderer.sortingLayerName = SortingLayer.ROADS;
        renderer.sprite = SpriteManager.Instance.Get_Sprite(North_East_Texture, SpriteManager.SpriteType.Improvement);
        renderer.flipX = true;
    }

    public void Update_Graphics()
    {
        North_East_GameObject.SetActive(false);
        East_GameObject.SetActive(false);
        South_East_GameObject.SetActive(false);
        South_West_GameObject.SetActive(false);
        West_GameObject.SetActive(false);
        North_West_GameObject.SetActive(false);
        List<Map.Direction> connections = new List<Map.Direction>();
        bool has_edge_connection = false;
        foreach(KeyValuePair<Map.Direction, Coordinates> adjancent_coordinates in Hex.Coordinates.Get_Adjanced_Coordinates()) {
            WorldMapHex adjancent_hex = World.Instance.Map.Get_Hex_At(adjancent_coordinates.Value);
            if((adjancent_hex == null && Hex.Is_Map_Edge_Road_Connection && !has_edge_connection) || (adjancent_hex != null && (adjancent_hex.Road != null || adjancent_hex.City != null || adjancent_hex.Village != null))) {
                connections.Add(adjancent_coordinates.Key);
                if (!has_edge_connection && adjancent_hex == null) {
                    has_edge_connection = true;
                }
            }
        }
        foreach(Map.Direction direction in connections) {
            switch (direction) {
                case Map.Direction.East:
                    East_GameObject.SetActive(true);
                    break;
                case Map.Direction.North_East:
                    North_East_GameObject.SetActive(true);
                    break;
                case Map.Direction.North_West:
                    North_West_GameObject.SetActive(true);
                    break;
                case Map.Direction.South_East:
                    South_East_GameObject.SetActive(true);
                    break;
                case Map.Direction.South_West:
                    South_West_GameObject.SetActive(true);
                    break;
                case Map.Direction.West:
                    West_GameObject.SetActive(true);
                    break;
            }
        }
        if(connections.Count == 0) {
            East_GameObject.SetActive(true);
            West_GameObject.SetActive(true);
        }
    }

    public bool Active
    {
        get {
            if(Base_GameObject == null) {
                return false;
            }
            return Base_GameObject.activeSelf;
        }
        set {
            Base_GameObject.SetActive(value);
        }
    }

    public void Destroy()
    {
        GameObject.Destroy(Base_GameObject);
    }
}
