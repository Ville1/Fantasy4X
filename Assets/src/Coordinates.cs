using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Coordinates
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coordinates(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Coordinates(int x, int y, Map.Direction delta)
    {
        X = x;
        Y = y;
        Shift(delta);
    }

    public Coordinates(Coordinates coordinates)
    {
        X = coordinates.X;
        Y = coordinates.Y;
    }

    public Vector2 Vector
    {
        get {
            return new Vector2(X, Y);
        }
        set {
            X = (int)value.x;
            Y = (int)value.y;
        }
    }

    /// <summary>
    /// Shifts coordinate point to specified direction
    /// </summary>
    /// <param name="direction"></param>
    public void Shift(Map.Direction direction)
    {
        switch (direction) {
            case Map.Direction.North_East:
                Y++;
                break;
            case Map.Direction.East:
                X++;
                break;
            case Map.Direction.South_East:
                Y--;
                X++;
                break;
            case Map.Direction.South_West:
                Y--;
                break;
            case Map.Direction.West:
                X--;
                break;
            case Map.Direction.North_West:
                Y++;
                X--;
                break;
        }
    }

    public void Shift(Coordinates coordinates)
    {
        X += coordinates.X;
        Y += coordinates.Y;
    }

    public static Coordinates Shift_Delta(Map.Direction direction)
    {
        Coordinates delta = new Coordinates(0, 0);
        delta.Shift(direction);
        return delta;
    }

    public override string ToString()
    {
        return "Coordinates(X: " + X + ", Y: " + Y + ")";
    }

    public override bool Equals(object obj)
    {
        if (obj is Coordinates) {
            return ((Coordinates)obj).X == X && ((Coordinates)obj).Y == Y;
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

    public Dictionary<Map.Direction, Coordinates> Get_Adjanced_Coordinates()
    {
        Dictionary<Map.Direction, Coordinates> coordinates = new Dictionary<Map.Direction, Coordinates>();
        foreach (Map.Direction direction in Enum.GetValues(typeof(Map.Direction))) {
            Coordinates new_coordinates = new Coordinates(this);
            new_coordinates.Shift(direction);
            coordinates.Add(direction, new_coordinates);
        }
        return coordinates;
    }
    
    public bool Is_Adjancent_To(Coordinates coordinates)
    {
        return Get_Adjanced_Coordinates().Select(x => x.Value).ToArray().Contains(coordinates);
    }

    public Map.Direction? Direction(Coordinates coordinates)
    {
        if (!Is_Adjancent_To(coordinates)) {
            return null;
        }
        if (X == coordinates.X && Y == coordinates.Y + 1) {
            return Map.Direction.South_West;
        }
        if (X == coordinates.X + 1 && Y == coordinates.Y) {
            return Map.Direction.West;
        }
        if (X == coordinates.X + 1 && Y == coordinates.Y - 1) {
            return Map.Direction.North_West;
        }
        if (X == coordinates.X && Y == coordinates.Y - 1) {
            return Map.Direction.North_East;
        }
        if (X == coordinates.X - 1 && Y == coordinates.Y) {
            return Map.Direction.East;
        }
        return Map.Direction.South_East;
    }
}
