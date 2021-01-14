using System.Collections.Generic;
using UnityEngine;

public class SelectionCircle {
    private static readonly float ANIMATION_FPS = 10.0f;

    private static SelectionCircle instance;

    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject.GetComponent<SpriteRenderer>(); } }

    private WorldMapEntity entity;
    private List<Sprite> sprites;
    private float animation_frame_time_left;
    private int animation_index;

    private SelectionCircle()
    {
        sprites = new List<Sprite>() {
            SpriteManager.Instance.Get("selection_circle_1", SpriteManager.SpriteType.UI),
            SpriteManager.Instance.Get("selection_circle_2", SpriteManager.SpriteType.UI),
            SpriteManager.Instance.Get("selection_circle_3", SpriteManager.SpriteType.UI)
        };

        GameObject = new GameObject();
        GameObject.name = ToString();
        GameObject.AddComponent<SpriteRenderer>();
        SpriteRenderer.sortingLayerName = SortingLayer.BORDERS;
        animation_frame_time_left = 1.0f / ANIMATION_FPS;
        animation_index = 0;
        SpriteRenderer.sprite = sprites[0];
        GameObject.SetActive(false);
    }

    public static SelectionCircle Instance
    {
        get {
            if(instance == null) {
                instance = new SelectionCircle();
            }
            return instance;
        }
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

    public void Update(float delta_s)
    {
        if (!Active) {
            return;
        }
        GameObject.transform.position = new Vector3(
            entity.Hex.GameObject.transform.position.x,
            entity.Hex.GameObject.transform.position.y,
            entity.Hex.GameObject.transform.position.z
        );
        animation_frame_time_left -= delta_s;
        if (animation_frame_time_left <= 0.0f) {
            animation_frame_time_left += (1.0f / ANIMATION_FPS);
            animation_index++;
            if (animation_index >= sprites.Count) {
                animation_index = 0;
            }
            SpriteRenderer.sprite = sprites[animation_index];
        }
    }

    public WorldMapEntity Entity
    {
        get {
            return entity;
        }
        set {
            if(Entity == value) {
                return;
            }
            entity = value;
            if(entity == null || entity.Hex == null) {
                Active = false;
            } else {
                Active = true;
                animation_frame_time_left = 1.0f / ANIMATION_FPS;
                animation_index = 0;
                GameObject.transform.position = new Vector3(
                    entity.Hex.GameObject.transform.position.x,
                    entity.Hex.GameObject.transform.position.y,
                    entity.Hex.GameObject.transform.position.z
                );
                GameObject.transform.SetParent(World.Instance.Map.GameObject.transform);
            }
        }
    }
}
