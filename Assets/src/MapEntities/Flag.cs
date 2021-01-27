using System.Collections.Generic;
using UnityEngine;

public class Flag {
    public enum OwnerType { Own, Enemy, Neutral }

    private static readonly float X_OFFSET = 0.5f;
    private static readonly float Y_OFFSET = 0.5f;

    private static Dictionary<OwnerType, string> textures = new Dictionary<OwnerType, string>() {
        { OwnerType.Own, "flag_own" },
        { OwnerType.Enemy, "flag_enemy" },
        { OwnerType.Neutral, "flag_neutral" }
    };
    private static int current_id = 0;

    public OwnerType Type { get; private set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject.GetComponent<SpriteRenderer>(); } }
    public WorldMapHex Hex { get; private set; }

    private bool is_deleted;
    private bool has_spawned;

    public Flag(WorldMapHex hex)
    {
        Hex = hex;
        GameObject = new GameObject();
        GameObject.name = string.Format("Flag #{0}", current_id);
        current_id++;
        GameObject.AddComponent<SpriteRenderer>();
        has_spawned = false;
        is_deleted = false;
        Update_Position();
        Update_Type();
    }

    public void Move(WorldMapHex new_hex)
    {
        if (is_deleted) {
            return;
        }
        Hex = new_hex;
        GameObject.SetActive(Hex.Visible_To_Viewing_Player);
        Update_Position();
        GameObject.transform.parent = Hex.GameObject.transform.transform;
    }

    public void Update_Type()
    {
        if (is_deleted) {
            return;
        }
        GameObject.SetActive(Hex.Visible_To_Viewing_Player);
        OwnerType new_type = OwnerType.Neutral;
        if(Hex.Entity != null) {
            new_type = Hex.Entity.Is_Owned_By_Current_Player ? OwnerType.Own : (Hex.Entity.Owner.Is_Neutral ? OwnerType.Neutral : OwnerType.Enemy);
        } else if(Hex.Civilian != null) {
            new_type = Hex.Civilian.Is_Owned_By_Current_Player ? OwnerType.Own : (Hex.Civilian.Owner.Is_Neutral ? OwnerType.Neutral : OwnerType.Enemy);
        } else if(Hex.City != null) {
            new_type = Hex.City.Is_Owned_By_Current_Player ? OwnerType.Own : (Hex.City.Owner.Is_Neutral ? OwnerType.Neutral : OwnerType.Enemy);
        } else if (Hex.Village != null) {
            new_type = Hex.Village.Is_Owned_By_Current_Player ? OwnerType.Own : (Hex.Village.Owner.Is_Neutral ? OwnerType.Neutral : OwnerType.Enemy);
        }
        if (Type == new_type && has_spawned) {
            return;
        }
        has_spawned = true;
        Type = new_type;
        SpriteRenderer.sprite = SpriteManager.Instance.Get(textures[Type], SpriteManager.SpriteType.UI);
    }

    public void Delete()
    {
        GameObject.Destroy(GameObject);
        is_deleted = true;
    }

    private void Update_Position()
    {
        GameObject.transform.position = new Vector3(
            Hex.GameObject.transform.position.x + X_OFFSET,
            Hex.GameObject.transform.position.y + Y_OFFSET,
            Hex.GameObject.transform.position.z
        );
        GameObject.transform.parent = Hex.GameObject.transform.transform;
    }
}
