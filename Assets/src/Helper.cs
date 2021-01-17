using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Helper {
    public delegate void ButtonClickDelegate();

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
        double rounded = Math.Round(double.Parse(f.ToString()), digits);
        if (!show_zeros && rounded == 0.0d) {
            return string.Empty;
        }
        string rounded_s = rounded.ToString();
        if (show_zeros && digits > 0) {
            StringBuilder builder = new StringBuilder(rounded_s);
            int current_digits = 0;
            if (!rounded_s.Contains(".")) {
                builder.Append(".0");
                current_digits = 1;
            } else {
                current_digits = rounded_s.Substring(rounded_s.IndexOf(".")).Length - 1;
            }
            while (current_digits < digits) {
                builder.Append("0");
                current_digits++;
            }
            rounded_s = builder.ToString();
        }
        return string.Format("{0}{1}", (show_plus_sign && f >= 0.0f ? "+" : string.Empty), rounded_s);
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

    public static Dictionary<T, int> Copy_Dictionary<T>(Dictionary<T, int> dictionary)
    {
        Dictionary<T, int> copy = new Dictionary<T, int>();
        foreach (KeyValuePair<T, int> pair in dictionary) {
            copy.Add(pair.Key, pair.Value);
        }
        return copy;
    }

    public static Dictionary<T, decimal> Copy_Dictionary<T>(Dictionary<T, decimal> dictionary)
    {
        Dictionary<T, decimal> copy = new Dictionary<T, decimal>();
        foreach (KeyValuePair<T, decimal> pair in dictionary) {
            copy.Add(pair.Key, pair.Value);
        }
        return copy;
    }

    public static Dictionary<T, float> Add_Dictionary<T>(Dictionary<T, float> dictionary_1, Dictionary<T, float> dictionary_2)
    {
        Dictionary<T, float> dictionary_3 = new Dictionary<T, float>();
        foreach (KeyValuePair<T, float> pair in dictionary_1) {
            dictionary_3.Add(pair.Key, pair.Value);
        }
        foreach (KeyValuePair<T, float> pair in dictionary_2) {
            if (!dictionary_3.ContainsKey(pair.Key)) {
                dictionary_3.Add(pair.Key, pair.Value);
            } else {
                dictionary_3[pair.Key] += pair.Value;
            }
        }
        return dictionary_3;
    }

    public static Dictionary<T, float> Clamp_Dictionary<T>(Dictionary<T, float> dictionary, float min, float max)
    {
        Dictionary<T, float> new_dictionary = new Dictionary<T, float>();
        foreach (KeyValuePair<T, float> pair in dictionary) {
            new_dictionary.Add(pair.Key, Mathf.Clamp(pair.Value, min, max));
        }
        return new_dictionary;
    }

    public static Dictionary<T, float> Instantiate_Dictionary<T>(float value)
    {
        Dictionary<T, float> dictionary = new Dictionary<T, float>();
        foreach (T type in Enum.GetValues(typeof(T))) {
            dictionary.Add(type, value);
        }
        return dictionary;
    }

    public static Dictionary<T, int> Instantiate_Dictionary<T>(int value)
    {
        Dictionary<T, int> dictionary = new Dictionary<T, int>();
        foreach (T type in Enum.GetValues(typeof(T))) {
            dictionary.Add(type, value);
        }
        return dictionary;
    }

    public static string Plural(int i)
    {
        if(i == 1) {
            return "";
        }
        return "s";
    }

    public static string Snake_Case_To_UI(string snake_case, bool capitalize = false)
    {
        if (string.IsNullOrEmpty(snake_case)) {
            return string.Empty;
        }
        snake_case = snake_case.ToLower().Replace('_', ' ');
        if (capitalize) {
            snake_case = snake_case[0].ToString().ToUpper() + snake_case.Substring(1);
        }
        return snake_case;
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

    public static void Delete_All(List<GameObject> list)
    {
        foreach (GameObject obj in list) {
            GameObject.Destroy(obj);
        }
        list.Clear();
    }

    public static void Delete_All<T>(Dictionary<T, GameObject> dictionary)
    {
        foreach (KeyValuePair<T, GameObject> pair in dictionary) {
            GameObject.Destroy(pair.Value);
        }
        dictionary.Clear();
    }

    public static void Set_Text(string parent_game_object_name, string text_game_object_name, string text, Color? color = null)
    {
        GameObject text_game_object = GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, text_game_object_name));
        text_game_object.GetComponentInChildren<Text>().text = text;
        if (color.HasValue) {
            text_game_object.GetComponentInChildren<Text>().color = color.Value;
        }
    }
    
    public static void Set_Image(string parent_game_object_name, string text_game_object_name, string sprite_name, SpriteManager.SpriteType sprite_type)
    {
        GameObject image_game_object = GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, text_game_object_name));
        image_game_object.GetComponentInChildren<Image>().sprite = SpriteManager.Instance.Get(sprite_name, sprite_type);
    }
    
    public static void Set_Image(string parent_game_object_name, string text_game_object_name, string sprite_name, SpriteManager.SpriteType sprite_type, Color color)
    {
        GameObject image_game_object = GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, text_game_object_name));
        Image image = image_game_object.GetComponentInChildren<Image>();
        image.sprite = SpriteManager.Instance.Get(sprite_name, sprite_type);
        image.color = color;
    }

    public static void Set_Button_On_Click(string parent_game_object_name, string button_game_object_name, ButtonClickDelegate delegate_p)
    {
        GameObject button_game_object = GameObject.Find(string.Format("{0}/{1}", parent_game_object_name, button_game_object_name));
        Button.ButtonClickedEvent on_click = new Button.ButtonClickedEvent();
        on_click.AddListener(delegate () {
            delegate_p();
        });
        button_game_object.GetComponentInChildren<Button>().onClick = on_click;
    }

    public static decimal Round(decimal d, int decimals)
    {
        int multiplier = (int)Mathf.Pow(10, decimals);
        d *= multiplier;
        int rounded = (int)Math.Round(d);
        return (decimal)rounded / multiplier;
    }

    public static void Set_Dropdown_Options(Dropdown dropdown, List<string> options, string selected = null)
    {
        dropdown.options = options.Select(x => new Dropdown.OptionData(x)).ToList();
        dropdown.value = options.Contains(selected) ? options.IndexOf(selected) : 0;
    }
}
