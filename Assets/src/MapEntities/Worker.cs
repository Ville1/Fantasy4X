using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Worker : WorldMapEntity, Trainable
{
    private static readonly string IMPROVEMENT_READY_SOUND_EFFECT = "building_ready_sfx";

    public List<Improvement> Buildable_Improvements { get; private set; }

    public int Cost { get; private set; }
    public int Production_Required { get; private set; }
    public Improvement Improvement_Under_Construction { get; private set; }
    public float Improvement_Progress { get; private set; }
    public List<string> Working_Animation { get; private set; }
    public float Working_Animation_FPS { get; private set; }
    public Technology Technology_Required { get; private set; }
    public float Work_Speed { get; private set; }
    public bool Requires_Coast { get { return Movement_Type == Map.MovementType.Water; } }
    public float Mineral_Reroll_Chance { get; private set; }

    public Worker(WorldMapHex hex, Worker prototype, Player owner) : base(hex, prototype, owner, true)
    {
        Buildable_Improvements = new List<Improvement>();
        foreach(Improvement improvement in prototype.Buildable_Improvements) {
            Buildable_Improvements.Add(improvement);
        }
        Work_Speed = prototype.Work_Speed;
        Cost = prototype.Cost;
        Upkeep = prototype.Upkeep;
        Production_Required = prototype.Production_Required;
        Technology_Required = prototype.Technology_Required;
        Working_Animation = Helper.Copy_List(prototype.Working_Animation);
        Working_Animation_FPS = prototype.Working_Animation_FPS;
        Mineral_Reroll_Chance = prototype.Mineral_Reroll_Chance;
        Update_Actions_List();
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="max_movement"></param>
    /// <param name="los"></param>
    /// <param name="texture"></param>
    public Worker(string name, float max_movement, Map.MovementType movement, int los, string texture, List<string> working_animation, float working_animation_fps,
        List<Improvement> buildable_improvements, float work_speed, int cost, int production_required, float upkeep, Technology technology_required, float reroll_chance) :
            base(name, max_movement, movement, los, texture)
    {
        Buildable_Improvements = buildable_improvements;
        Work_Speed = work_speed;
        Cost = cost;
        Upkeep = upkeep;
        Production_Required = production_required;
        Working_Animation = working_animation;
        Working_Animation_FPS = working_animation_fps;
        Technology_Required = technology_required;
        Mineral_Reroll_Chance = reroll_chance;
    }

    public void Start_Construction(Improvement improvement)
    {
        if (!improvement.Can_Be_Build_On.Contains(Hex.Terrain)) {
            return;
        }
        Improvement_Progress = 0.0f;
        Improvement_Under_Construction = improvement;
        Current_Movement = 0.0f;
        Start_Animation(Working_Animation, Working_Animation_FPS);
    }

    public override void End_Turn()
    {
        base.End_Turn();
        Update_Actions_List();
        if(Improvement_Under_Construction != null) {
            City city = null;
            foreach(City c in Hex.In_Work_Range_Of) {
                if (c.Is_Owned_By(Owner)) {
                    city = c;
                    break;
                }
            }
            Improvement_Progress += Work_Speed * Current_Improvement_Build_Speed_Multiplier;
            if(Improvement_Progress >= Improvement_Under_Construction.Build_Time) {
                Owner.Queue_Notification(new Notification("Improvement completed: " + Improvement_Under_Construction.Name, Improvement_Under_Construction.Texture, SpriteManager.SpriteType.Improvement, IMPROVEMENT_READY_SOUND_EFFECT, delegate() {
                    BottomGUIManager.Instance.Current_Entity = this;
                    CameraManager.Instance.Set_Camera_Location(Hex);
                }));
                if(Hex.Improvement != null) {
                    Hex.Improvement.Delete();
                }
                Hex.Improvement = new Improvement(Hex, Improvement_Under_Construction);
                if(Hex.Improvement.Extracts_Minerals && Hex.Can_Spawn_Minerals && !Hex.Is_Prospected_By(Owner)) {
                    Hex.Prospect(Owner);
                    if (Hex.Mineral == null && Mineral_Reroll_Chance > 0.0f && RNG.Instance.Next(0, 100) <= Mathf.RoundToInt(100.0f * Mineral_Reroll_Chance)) {
                        CustomLogger.Instance.Debug(string.Format("Worker #{0} rerolled mineral!", Id));
                        if (RNG.Instance.Next(100) <= Mathf.RoundToInt(World.Instance.Map.Mineral_Spawn_Rate * 100.0f)) {
                            Hex.Spawn_Mineral();
                            CustomLogger.Instance.Debug(string.Format("{0} spawned!", Hex.Mineral.Name));
                        } else {
                            CustomLogger.Instance.Debug("Nothing spawned");
                        }
                    }
                    if (Hex.Mineral != null) {
                        Owner.Queue_Notification(new Notification("Mineral found: " + Hex.Mineral.Name, "prospect", SpriteManager.SpriteType.UI, null, delegate () {
                            BottomGUIManager.Instance.Current_Entity = this;
                            CameraManager.Instance.Set_Camera_Location(Hex);
                        }));
                    }
                }
                Stop_Building();
            }
        }
    }

    /// <summary>
    /// Try moving to a new hex
    /// </summary>
    /// <param name="new_hex"></param>
    /// <param name="ignore_movement_restrictions"></param>
    /// <returns></returns>
    public override bool Move(WorldMapHex new_hex, bool ignore_movement_restrictions = false, bool update_los = true, WorldMapHex jump_over_hex = null)
    {
        if(Improvement_Under_Construction != null) {
            Stop_Building();
        }
        return base.Move(new_hex, ignore_movement_restrictions, update_los, jump_over_hex);
    }

    public void Stop_Building()
    {
        Improvement_Progress = 0;
        Improvement_Under_Construction = null;
        Stop_Animation();
    }

    public void Update_Actions_List()
    {
        Actions.Clear();
        foreach (Improvement improvement in Buildable_Improvements) {
            if(improvement.Technology_Required != null && !Owner.Researched_Technologies.Any(x => improvement.Technology_Required.Name == x.Name)) {
                continue;
            }
            Actions.Add(new Action(string.Format("Build {0}", improvement.Name), improvement.Texture, improvement.Get_Tooltip(this), SpriteManager.SpriteType.Improvement,
                delegate (WorldMapEntity entity) {
                    Improvement i = improvement;
                    return i.Can_Be_Build_On.Contains(entity.Hex.Terrain) && (!i.Requires_Nearby_City || entity.Hex.In_Work_Range_Of.Count != 0);
                },
                delegate (WorldMapEntity entity) {
                    Improvement i = improvement;
                    (entity as Worker).Start_Construction(i);
                })
            );
        }
    }

    /// <summary>
    /// Checks hex and technology requirements
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public List<Improvement> Get_Buildable_Improvements(WorldMapHex hex)
    {
        List<Improvement> list = new List<Improvement>();
        foreach (Improvement improvement in Buildable_Improvements) {
            if ((improvement.Technology_Required != null && !Owner.Researched_Technologies.Any(x => improvement.Technology_Required.Name == x.Name)) ||
                !improvement.Can_Be_Build_On.Contains(hex.Terrain) || (improvement.Requires_Nearby_City && hex.In_Work_Range_Of.Count == 0)) {
                continue;
            }
            list.Add(improvement);
        }
        return list;
    }

    /// <summary>
    /// Checks hex and technology requirements
    /// </summary>
    public List<Improvement> Currently_Buildable_Improvements
    {
        get {
            return Get_Buildable_Improvements(Hex);
        }
    }

    public int Turns_Left
    {
        get {
            if(Improvement_Under_Construction == null) {
                return -1;
            }
            return Mathf.CeilToInt((Improvement_Under_Construction.Build_Time - Improvement_Progress) / (Work_Speed * Current_Improvement_Build_Speed_Multiplier));
        }
    }

    public int Construction_Time_Estimate(Improvement improvement)
    {
        return Mathf.CeilToInt(improvement.Build_Time / (Work_Speed * Current_Improvement_Build_Speed_Multiplier));
    }

    private float Current_Improvement_Build_Speed_Multiplier
    {
        get {
            City city = null;
            foreach (City c in Hex.In_Work_Range_Of) {
                if (c.Is_Owned_By(Owner)) {
                    city = c;
                    break;
                }
            }
            if(city != null) {
                return 1.0f + city.Total_Improvement_Construction_Speed_Bonus;
            }
            return 1.0f + Owner.EmpireModifiers.Improvement_Constuction_Speed_Bonus;
        }
    }

    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder();
            tooltip.Append(Name);
            tooltip.Append(Environment.NewLine).Append("Upkeep: ").Append(Math.Round(Upkeep, 2).ToString("0.00"));
            tooltip.Append(Environment.NewLine).Append("Work Speed: ").Append(Math.Round(Work_Speed, 1).ToString("0.0"));
            if(Mineral_Reroll_Chance != 0.0f) {
                tooltip.Append(Environment.NewLine).Append("Mineral Reroll Chance: ").Append(Helper.Float_To_String(Mineral_Reroll_Chance * 100.0f, 0)).Append("%");
            }
            tooltip.Append(Environment.NewLine).Append("Movement: ").Append(Mathf.RoundToInt(Max_Movement));
            if(Movement_Type == Map.MovementType.Water) {
                tooltip.Append(Environment.NewLine).Append("Water Unit");
            } else if (Movement_Type == Map.MovementType.Amphibious) {
                tooltip.Append(Environment.NewLine).Append("Amphibious Unit").Append(Mathf.RoundToInt(Max_Movement));
            }
            tooltip.Append(Environment.NewLine).Append("Improvements:");
            foreach(Improvement improvement in Buildable_Improvements) {
                tooltip.Append(Environment.NewLine).Append("- ").Append(improvement.Name);
            }
            return tooltip.ToString();
        }
    }

    public WorkerSaveData Save_Data
    {
        get {
            WorkerSaveData data = new WorkerSaveData();
            data.Name = Name;
            data.Hex_X = Hex.Coordinates.X;
            data.Hex_Y = Hex.Coordinates.Y;
            data.Movement = Current_Movement;
            data.Improvement_Progress = Improvement_Progress;
            data.Improvement_Under_Construction = Improvement_Under_Construction == null ? string.Empty : Improvement_Under_Construction.Name;
            data.Path = Stored_Path != null ? Stored_Path.Select(x => new CoordinateSaveData() { X = x.Coordinates.X, Y = x.Coordinates.Y }).ToList() : null;
            data.Sleep = Sleep;
            return data;
        }
    }

    public void Load(WorkerSaveData data)
    {
        Current_Movement = data.Movement;
        Improvement_Progress = data.Improvement_Progress;
        Improvement_Under_Construction = string.IsNullOrEmpty(data.Improvement_Under_Construction) ? null : Buildable_Improvements.First(x => x.Name == data.Improvement_Under_Construction);
        Stored_Path = data.Path == null || data.Path.Count == 0 ? null : data.Path.Select(x => World.Instance.Map.Get_Hex_At(x.X, x.Y)).ToList();
        Sleep = data.Sleep;
        if (Improvement_Under_Construction != null) {
            Start_Animation(Working_Animation, Working_Animation_FPS);
        }
    }
}
