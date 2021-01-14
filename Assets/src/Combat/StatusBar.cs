using System;
using System.Collections.Generic;
using UnityEngine;

public class StatusBar {
    private static readonly int FILLED = 0;
    private static readonly int EMPTY = 1;
    private static readonly float SCALE_X = 1.35f;
    private static readonly float SCALE_Y = 0.125f;
    private static readonly float SCALE_Z = 1.0f;
    private static readonly Dictionary<BarType, string[]> TEXTURES = new Dictionary<BarType, string[]>() {
        { BarType.Manpower, new string[2] { "manpower_bar_filled", "manpower_bar_empty" } },
        { BarType.Morale, new string[2] { "morale_bar_filled", "morale_bar_empty" } },
        { BarType.Stamina, new string[2] { "stamina_bar_filled", "stamina_bar_empty" } }
    };
    private static readonly Dictionary<BarType, Vector3> POSITION_DELTA = new Dictionary<BarType, Vector3>() {
        { BarType.Manpower, new Vector3(0.0f, -0.53f, 0.0f) },
        { BarType.Morale, new Vector3(0.0f, -0.6f, 0.0f) },
        { BarType.Stamina, new Vector3(0.0f, -0.67f, 0.0f) }
    };

    public enum BarType { Manpower, Morale, Stamina }

    public Unit Unit { get; private set; }
    public BarType Type { get; private set; }
    public GameObject GameObject_Filled { get; private set; }
    public SpriteRenderer SpriteRenderer_Filled { get { return GameObject_Filled.GetComponent<SpriteRenderer>(); } }
    public GameObject GameObject_Empty { get; private set; }
    public SpriteRenderer SpriteRenderer_Empty { get { return GameObject_Empty.GetComponent<SpriteRenderer>(); } }

    public StatusBar(Unit unit, BarType type)
    {
        Unit = unit;
        Type = type;
        GameObject_Empty = Initialize_GameObject(EMPTY, SortingLayer.UNIT_GUI_1);
        GameObject_Filled = Initialize_GameObject(FILLED, SortingLayer.UNIT_GUI_2);
    }

    public void Update()
    {
        switch (Type) {
            case BarType.Manpower:
                GameObject_Filled.transform.localScale = new Vector3(SCALE_X * Unit.Manpower, SCALE_Y, SCALE_Z);
                break;
            case BarType.Morale:
                GameObject_Filled.transform.localScale = new Vector3(SCALE_X * Unit.Relative_Morale, SCALE_Y, SCALE_Z);
                break;
            case BarType.Stamina:
                GameObject_Filled.transform.localScale = new Vector3(SCALE_X * Unit.Relative_Stamina, SCALE_Y, SCALE_Z);
                break;
        }
    }

    public void Destroy()
    {
        GameObject.Destroy(GameObject_Empty);
        GameObject.Destroy(GameObject_Filled);
    }

    private GameObject Initialize_GameObject(int texture_index, string sorting_layer)
    {
        GameObject gameObject = new GameObject();
        gameObject.name = string.Format("{0}{1}#{2}", Type.ToString(), texture_index, Unit.Id);
        gameObject.transform.position = new Vector3(
            Unit.GameObject.transform.position.x + POSITION_DELTA[Type].x,
            Unit.GameObject.transform.position.y + POSITION_DELTA[Type].y,
            Unit.GameObject.transform.position.z + POSITION_DELTA[Type].z
        );
        gameObject.transform.parent = Unit.GameObject.transform.transform;
        gameObject.transform.localScale = new Vector3(SCALE_X, SCALE_Y, SCALE_Z);
        gameObject.AddComponent<SpriteRenderer>();
        gameObject.GetComponent<SpriteRenderer>().sprite = SpriteManager.Instance.Get(TEXTURES[Type][texture_index], SpriteManager.SpriteType.UI);
        gameObject.GetComponent<SpriteRenderer>().sortingLayerName = sorting_layer;
        return gameObject;
    }

    public static void Create_Bars(Unit unit)
    {
        foreach (BarType type in Enum.GetValues(typeof(BarType))) {
            unit.Bars.Add(new StatusBar(unit, type));
        }
    }

    public static void Update_Bars(Unit unit)
    {
        if(unit == null || unit.GameObject == null) {
            return;
        }
        if(unit.Bars.Count == 0) {
            Create_Bars(unit);
        }
        foreach (StatusBar bar in unit.Bars) {
            bar.Update();
        }
    }

    public static void Destroy_Bars(Unit unit)
    {
        foreach (StatusBar bar in unit.Bars) {
            bar.Destroy();
        }
        unit.Bars.Clear();
    }
}
