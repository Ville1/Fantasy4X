using System;
using System.Collections.Generic;
using UnityEngine;

public class Helper {
    /// <summary>
    /// input < min
    /// min * multiplier_1
    /// 
    /// input <= break_point_1
    /// input * multiplier_1
    /// 
    /// input > break_point && input <= max
    /// break_point * multiplier_1 + (input - break_point) * multiplier_2
    /// 
    /// input > max
    /// break_point * multiplier_1 + (max - break_point) * multiplier_2
    /// </summary>
    /// <param name="min"></param>
    /// <param name="break_point_1"></param>
    /// <param name="max"></param>
    /// <param name="multiplier_1"></param>
    /// <param name="multiplier_2"></param>
    /// <returns></returns>
    public static float Break_Point_Multiply(float input, float min, float break_point, float max, float multiplier_1, float multiplier_2)
    {
        if (break_point <= min || break_point >= max) {
            CustomLogger.Instance.Warning("Invalid input!");
            return -1.0f;
        }
        if (input < min) {
            return min * multiplier_1;
        }
        if (input <= break_point) {
            return input * multiplier_1;
        }
        if (input > break_point && input <= max) {
            return (break_point * multiplier_1) + ((input - break_point) * multiplier_2);
        }
        return (break_point * multiplier_1) + ((max - break_point) * multiplier_2);
    }

    public static float Break_Point_Bonus(float input, float break_point, float max, float bonus_at_break_point, float bonus_at_max)
    {
        if(break_point >= max || bonus_at_break_point >= bonus_at_max) {
            CustomLogger.Instance.Warning("Invalid input!");
            return -1.0f;
        }
        if (input <= break_point) {
            return bonus_at_break_point * (input / break_point);
        } else if (input <= max) {
            return bonus_at_break_point + ((bonus_at_max - bonus_at_break_point) * ((input - break_point) / (max - break_point)));
        } else {
            return bonus_at_max;
        }
    }

    public static float Break_Point_Bonus(float input, float break_point, float bonus_at_break_point, float bonus_per_input_after_break_point)
    {
        if (input <= break_point) {
            return bonus_at_break_point * (input / break_point);
        }
        return bonus_at_break_point + ((input - break_point) * bonus_per_input_after_break_point);
    }

    public static string Float_To_String(float f, int digits, bool show_plus_sign = false, bool show_zeros = true)
    {
        double rounded = Math.Round(f, digits);
        if(!show_zeros && rounded == 0.0d) {
            return string.Empty;
        }
        if (f < 0.0f) {
            return rounded.ToString();
        }
        return string.Format("{0}{1}", (show_plus_sign ? "+" : string.Empty), rounded);
    }

    public static List<T> Copy_List<T>(List<T> original)
    {
        List<T> copy = new List<T>();
        foreach(T item in original) {
            copy.Add(item);
        }
        return copy;
    }

    public static Dictionary<T, float> Copy_Dictionary<T>(Dictionary<T, float> dictionary)
    {
        Dictionary<T, float> copy = new Dictionary<T, float>();
        foreach(KeyValuePair<T, float> pair in dictionary) {
            copy.Add(pair.Key, pair.Value);
        }
        return copy;
    }

    public static string Plural(int i)
    {
        if(i == 1) {
            return "";
        }
        return "s";
    }

    public static string Parse_To_Human_Readable(string s)
    {
        return s.Replace('_', ' ');
    }

    public static Map.Direction Rotate(Map.Direction direction, int rotation)
    {
        int index = (int)direction + rotation;
        Map.Direction[] direction_array = (Map.Direction[])Enum.GetValues(typeof(Map.Direction));
        while (index < 0) {
            index += direction_array.Length;
        }
        while (index >= direction_array.Length) {
            index -= direction_array.Length;
        }
        return direction_array[index];
    }

    public static void Destroy_GameObjects(ref List<GameObject> list)
    {
        foreach(GameObject go in list) {
            GameObject.Destroy(go);
        }
        list.Clear();
    }
}
