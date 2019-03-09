using System.Collections.Generic;
using UnityEngine;

public class WorldMapEntity : Ownable {
    private static int current_id = 0;

    public string Name { get; protected set; }
    public string Texture { get; private set; }
    public WorldMapHex Hex { get; protected set; }
    public virtual float Current_Movement { get; protected set; }
    public virtual float Max_Movement { get; protected set; }
    public int Id { get; private set; }
    public virtual int LoS { get; protected set; }
    public GameObject GameObject { get; private set; }
    public SpriteRenderer SpriteRenderer { get { return GameObject.GetComponent<SpriteRenderer>(); } }
    public List<WorldMapHex> Last_Hexes_In_Los { get; private set; }
    public List<Action> Actions { get; private set; }
    public List<Sprite> Current_Animation { get; private set; }
    public int Animation_Index { get; private set; }
    public float Animation_FPS { get; private set; }
    public virtual float Upkeep { get; protected set; }
    public virtual float Mana_Upkeep { get; protected set; }
    public bool Is_Civilian { get; private set; }
    public List<WorldMapHex> Stored_Path { get; set; }
    public int Stored_Path_Index { get; set; }
    public Flag Flag { get; private set; }
    public Map.MovementType Movement_Type { get; private set; }

    private float animation_frame_time_left;
    protected bool wait_turn;
    private bool sleep;

    public WorldMapEntity(WorldMapHex hex, WorldMapEntity prototype, Player owner, bool civilian)
    {
        if(hex.Entity != null && !civilian) {
            CustomLogger.Instance.Error("Creating an instance of WorldMapEntity on a hex which already has one");
        } else if(hex.Civilian != null && civilian) {
            CustomLogger.Instance.Error("Creating an instance of WorldMapEntity on a hex which already has one");
        }
        Owner = owner;
        if (Owner.WorldMapEntitys.Contains(this)) {
            CustomLogger.Instance.Warning("This entity has already been added to owner's entity list");
        } else {
            Owner.WorldMapEntitys.Add(this);
        }
        Hex = hex;
        if (civilian) {
            Hex.Civilian = this;
        } else {
            Hex.Entity = this;
        }
        Name = prototype.Name;
        Max_Movement = prototype.Max_Movement;
        Current_Movement = Max_Movement;
        Movement_Type = prototype.Movement_Type;
        Texture = prototype.Texture;
        LoS = prototype.LoS;
        Is_Civilian = civilian;
        Id = current_id;
        current_id++;

        Wait_Turn = false;

        Last_Hexes_In_Los = new List<WorldMapHex>();
        Actions = new List<Action>();

        Flag = new Flag(Hex);

        GameObject = new GameObject();
        GameObject.name = ToString();
        GameObject.transform.position = Hex.GameObject.transform.position;
        GameObject.transform.parent = Hex.GameObject.transform.transform;
        GameObject.AddComponent<SpriteRenderer>();
        SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(Texture, SpriteManager.SpriteType.Unit);
        GameObject.SetActive(Hex.Visible_To_Viewing_Player);
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="max_movement"></param>
    /// <param name="movement_type"></param>
    /// <param name="los"></param>
    /// <param name="texture"></param>
    public WorldMapEntity(string name, float max_movement, Map.MovementType movement_type, int los, string texture)
    {
        Name = name;
        Max_Movement = max_movement;
        Movement_Type = movement_type;
        LoS = los;
        Texture = texture;
    }


    public virtual bool Wait_Turn
    {
        get {
            return wait_turn;
        }
        set {
            wait_turn = value;
            if (wait_turn) {
                sleep = false;
            }
        }
    }

    public virtual bool Sleep
    {
        get {
            return sleep;
        }
        set {
            sleep = value;
            if (sleep) {
                wait_turn = false;
            }
        }
    }

    public bool Is_Idle
    {
        get {
            return !Wait_Turn && !Sleep && Current_Movement > 0.0f && Stored_Path == null && (!(this is Worker) || (this as Worker).Improvement_Under_Construction == null)
                && (!(this is Prospector) || !(this as Prospector).Prospecting);
        }
    }

    public void Start_Turn()
    {
        Last_Hexes_In_Los.Clear();
    }

    public virtual void End_Turn()
    {
        Current_Movement = Max_Movement;
        Owner.Cash -= Upkeep;
        Owner.Mana -= Mana_Upkeep;
        Wait_Turn = false;
    }

    /// <summary>
    /// Try moving to a new hex
    /// TODO: update_los does nothing?
    /// </summary>
    /// <param name="new_hex"></param>
    /// <param name="ignore_movement_restrictions"></param>
    /// <returns></returns>
    public virtual bool Move(WorldMapHex new_hex, bool ignore_movement_restrictions = false, bool update_los = true)
    {
        if(!new_hex.Passable_For(this) || (!Is_Civilian && new_hex.Entity != null) || (Is_Civilian && new_hex.Civilian != null)) {
            return false;
        }
        if(!ignore_movement_restrictions && (!Hex.Is_Adjancent_To(new_hex) || Current_Movement <= 0.0f)) {
            return false;
        }

        if (Is_Civilian) {
            Hex.Civilian = null;
            new_hex.Civilian = this;
        } else {
            Hex.Entity = null;
            new_hex.Entity = this;
        }

        Hex = new_hex;
        GameObject.transform.position = Hex.GameObject.transform.position;
        GameObject.transform.parent = Hex.GameObject.transform.transform;
        Wait_Turn = false;
        Sleep = false;
        if (!ignore_movement_restrictions) {
            Current_Movement -= new_hex.Get_Movement_Cost(Movement_Type);
        }
        GameObject.SetActive(Hex.Visible_To_Viewing_Player);
        Flag.Move(Hex);
        World.Instance.Map.Update_LoS(this);
        return true;
    }

    public override string ToString()
    {
        return string.Format("WorldMapEntity({0}): {1}", Id, Name);
    }

    public List<WorldMapHex> Get_Hexes_In_LoS()
    {
        return Hex.Get_Hexes_In_LoS(LoS);
    }

    public void Start_Animation(List<string> textures, float animation_fps)
    {
        Current_Animation = new List<Sprite>();
        Animation_Index = 0;
        Animation_FPS = animation_fps;
        foreach (string t in textures) {
            Current_Animation.Add(SpriteManager.Instance.Get_Sprite(t, SpriteManager.SpriteType.Unit_Animation));
        }
        animation_frame_time_left = 1.0f / Animation_FPS;
    }

    public void Stop_Animation()
    {
        Current_Animation = null;
        Animation_Index = 0;
        Animation_FPS = 0.0f;
        SpriteRenderer.sprite = SpriteManager.Instance.Get_Sprite(Texture, SpriteManager.SpriteType.Unit);
    }

    public void Update(float delta_s)
    {
        if(Current_Animation == null) {
            return;
        }
        animation_frame_time_left -= delta_s;
        if(animation_frame_time_left <= 0.0f) {
            animation_frame_time_left += (1.0f / Animation_FPS);
            Animation_Index++;
            if(Animation_Index >= Current_Animation.Count) {
                Animation_Index = 0;
            }
            SpriteRenderer.sprite = Current_Animation[Animation_Index];
        }
    }
    
    public void Delete()
    {
        Owner.WorldMapEntitys.Remove(this);
        if (Is_Civilian) {
            Hex.Civilian = null;
        } else {
            Hex.Entity = null;
        }
        Flag.Delete();
        GameObject.Destroy(GameObject);
        if(BottomGUIManager.Instance.Current_Entity == this) {
            BottomGUIManager.Instance.Current_Entity = null;
        }
    }

    public WorldMapHex Stored_Path_Target
    {
        get {
            if(Stored_Path == null) {
                return null;
            }
            return Stored_Path[Stored_Path.Count - 1];
        }
    }

    public void Clear_Stored_Path()
    {
        Stored_Path = null;
        Stored_Path_Index = 0;
    }

    public bool Follow_Stored_Path()
    {
        if(Stored_Path == null || Stored_Path_Index >= Stored_Path.Count) {
            return false;
        }
        bool success = Move(Stored_Path[Stored_Path_Index]);
        if (success) {
            if(Hex == Stored_Path_Target) {
                Clear_Stored_Path();
                return true;
            }
            Stored_Path_Index++;
        }
        return success;
    }

    public void Create_Stored_Path(List<PathfindingNode> nodes)
    {
        Stored_Path = new List<WorldMapHex>();
        Stored_Path_Index = 0;
        for (int i = 0; i < nodes.Count; i++) {
            WorldMapHex hex = World.Instance.Map.Get_Hex_At(nodes[i].Coordinates);
            if(hex == Hex && i == 0) {
                continue;
            }
            Stored_Path.Add(hex);
        }
    }
}
