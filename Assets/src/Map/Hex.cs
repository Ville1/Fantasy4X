using UnityEngine;

public class Hex : Ownable {
    public static readonly string GAME_OBJECT_NAME_PREFIX = "Hex";
    static readonly float WIDTH_MULTIPLIER = 0.86f;// Mathf.Sqrt(3.0f) / 2.0f;
    static readonly float HEIGHT_MULTIPLIER = 0.595f;

    /// <summary>
    /// Column
    /// </summary>
    public int Q { get; private set; }
    /// <summary>
    /// Row
    /// </summary>
    public int R { get; private set; }
    /// <summary>
    /// Q + R + S = 0
    /// </summary>
    public int S { get; private set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject.GetComponent<SpriteRenderer>(); } }
    public bool Destroyed { get; private set; }
    
    protected Color highlight_color;
    protected bool show_coordinates;
    protected GameObject text_game_object;
    protected TextMesh TextMesh { get { return text_game_object.GetComponent<TextMesh>(); } }
    protected Color? border_color;
    protected GameObject border_game_object;

    public Hex(int q, int r, GameObject parent, int map_height)
    {
        Q = q;
        R = r;
        S = -(q + r);

        GameObject = GameObject.Instantiate(
            PrefabManager.Instance.World_Map_Hex,
            Position,
            Quaternion.identity,
            parent.transform
        );
        GameObject.name = string.Format("{0}({1},{2})", GAME_OBJECT_NAME_PREFIX, Q, R);
        GameObject.AddComponent<SphereCollider>();
        SpriteRenderer.sortingOrder = (3 * map_height) - (R * 3);

        highlight_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        Destroyed = false;
        show_coordinates = false;
        text_game_object = GameObject.GetComponentInChildren<TextMesh>().gameObject;
        text_game_object.SetActive(false);
    }

    /// <summary>
    /// Prototype contructor
    /// </summary>
    public Hex()
    { }

    public Vector3 Position
    {
        get {
            float radius = 1.0f;
            float height = radius * 2.0f;
            float width = WIDTH_MULTIPLIER * height;
            float vertical = height * HEIGHT_MULTIPLIER;
            float horizontal = width;
            return new Vector3(
                horizontal * (Q + R / 2.0f),
                vertical * R,
                Map.Z_LEVEL
            );
        }
    }

    public Coordinates Coordinates
    {
        get {
            return new Coordinates(Q, R);
        }
    }

    public virtual PathfindingNode PathfindingNode
    {
        get {
            return new PathfindingNode(Coordinates, GameObject.transform.position.x, GameObject.transform.position.y, 1.0f);
        }
    }

    public override string ToString()
    {
        return string.Format("Hex({0},{1})", Q, R);
    }

    public bool Active
    {
        get {
            return GameObject.activeSelf;
        }
        set {
            GameObject.SetActive(value);
        }
    }

    /// <summary>
    /// Color used to highlight this tile
    /// </summary>
    public Color Highlight
    {
        get {
            return highlight_color;
        }
        set {
            if (value != highlight_color) {
                highlight_color = value;
            } else {
                highlight_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            GameObject.GetComponent<SpriteRenderer>().color = highlight_color;
        }
    }

    public void Clear_Highlight()
    {
        highlight_color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        GameObject.GetComponent<SpriteRenderer>().color = highlight_color;
    }

    public Color? Borders
    {
        get {
            return border_color;
        }
        set {
            border_color = value;
            if(border_color == null) {
                if(border_game_object != null) {
                    GameObject.Destroy(border_game_object);
                    border_game_object = null;
                }
                return;
            } else {
                if(border_game_object == null) {
                    border_game_object = new GameObject();
                    border_game_object.name = "HexBorder";
                    border_game_object.transform.position = GameObject.transform.position;
                    border_game_object.transform.parent = GameObject.transform.transform;
                    SpriteRenderer renderer = border_game_object.AddComponent<SpriteRenderer>();
                    renderer.sprite = SpriteManager.Instance.Get("hex_borders", SpriteManager.SpriteType.UI);
                    renderer.sortingLayerName = "Borders";
                }
                border_game_object.GetComponent<SpriteRenderer>().color = border_color.Value;
            }
        }
    }

    /// <summary>
    /// Deletes tile's GameObject
    /// </summary>
    public void Delete()
    {
        GameObject.Destroy(GameObject);
        Destroyed = true;
    }

    public bool Show_Coordinates
    {
        get {
            if (!text_game_object.activeSelf) {
                return false;
            }
            return show_coordinates;
        }
        set {
            show_coordinates = value;
            if (!show_coordinates) {
                text_game_object.SetActive(false);
                return;
            }
            text_game_object.SetActive(true);
            TextMesh.text = string.Format("{0},{1}", Q, R);
        }
    }

    public bool Is_Adjancent_To(Hex hex)
    {
        return Coordinates.Is_Adjancent_To(hex.Coordinates);
    }
}
