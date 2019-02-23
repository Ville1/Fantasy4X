using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathfindingNode
{
    public Coordinates Coordinates { get; set; }
    public float Cost { get; set; }
    public bool Passable { get { return Cost > 0.0f; } }
    public int X { get { return Coordinates.X; } set { Coordinates.X = value; } }
    public int Y { get { return Coordinates.Y; } set { Coordinates.Y = value; } }
    public float GameObject_X { get; set; }
    public float GameObject_Y { get; set; }

    public PathfindingNode(Coordinates coordinates, float GameObject_X, float GameObject_Y, float cost)
    {
        Coordinates = new Coordinates(coordinates);
        Cost = cost;
        this.GameObject_X = GameObject_X;
        this.GameObject_Y = GameObject_Y;
    }

    public override bool Equals(object obj)
    {
        if (obj is PathfindingNode) {
            return ((PathfindingNode)obj).X == X && ((PathfindingNode)obj).Y == Y;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Int32.Parse(X + "" + Y);
    }

    public float Distance(Coordinates coordinates)
    {
        return Mathf.Sqrt((X - coordinates.X) * (X - coordinates.X) + (Y - coordinates.Y) * (Y - coordinates.Y));
    }

    public float GameObject_Distance(float x, float y)
    {
        return Mathf.Sqrt((GameObject_X - x) * (GameObject_X - x) + (GameObject_Y - y) * (GameObject_Y - y));
    }

    public override string ToString()
    {
        return "PathfindingNode(X: " + X + ", Y: " + Y + ")";
    }

    public Dictionary<Map.Direction, PathfindingNode> Get_Adjanced_Nodes(List<PathfindingNode> other_nodes)
    {
        Dictionary<Map.Direction, PathfindingNode> nodes = new Dictionary<Map.Direction, PathfindingNode>();
        foreach (Map.Direction direction in Enum.GetValues(typeof(Map.Direction))) {
            Coordinates new_coordinates = new Coordinates(Coordinates);
            new_coordinates.Shift(direction);
            PathfindingNode node = other_nodes.FirstOrDefault(x => x.Coordinates.X == new_coordinates.X && x.Coordinates.Y == new_coordinates.Y);
            if (node != null) {
                nodes.Add(direction, node);
            }
        }
        return nodes;
    }
}
