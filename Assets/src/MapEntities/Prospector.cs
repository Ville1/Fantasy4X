﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Prospector : WorldMapEntity, Trainable
{
    public int Cost { get; private set; }
    public int Production_Required { get; private set; }
    public int Prospect_Progress { get; private set; }
    public int Prospect_Turns { get; private set; }
    public List<string> Working_Animation { get; private set; }
    public float Working_Animation_FPS { get; private set; }
    public Technology Technology_Required { get; private set; }
    private bool prospecting;

    public Prospector(WorldMapHex hex, Prospector prototype, Player owner) : base(hex, prototype, owner, true)
    {
        Cost = prototype.Cost;
        Upkeep = prototype.Upkeep;
        Production_Required = prototype.Production_Required;
        Technology_Required = prototype.Technology_Required;
        Working_Animation = Helper.Copy_List(prototype.Working_Animation);
        Working_Animation_FPS = prototype.Working_Animation_FPS;
        Prospect_Progress = 0;
        Prospect_Turns = prototype.Prospect_Turns;
        prospecting = false;

        Update_Actions_List();
    }

    /// <summary>
    /// Prototype constructor
    /// </summary>
    /// <param name="name"></param>
    /// <param name="max_movement"></param>
    /// <param name="los"></param>
    /// <param name="texture"></param>
    public Prospector(string name, float max_movement, int los, string texture, List<string> working_animation, float working_animation_fps,
        int cost, int production_required, float upkeep, Technology technology_required, int prospect_turns) :
            base(name, max_movement, los, texture)
    {
        Cost = cost;
        Upkeep = upkeep;
        Production_Required = production_required;
        Working_Animation = working_animation;
        Working_Animation_FPS = working_animation_fps;
        Technology_Required = technology_required;
        Prospect_Turns = prospect_turns;
    }

    public override void End_Turn()
    {
        base.End_Turn();
        if (Prospecting) {
            Prospect_Progress++;
            if (Prospect_Progress >= Prospect_Turns) {
                Hex.Prospect(Owner);
                Prospecting = false;
                if(Hex.Mineral != null) {
                    Owner.Queue_Notification(new Notification("Mineral found: " + Hex.Mineral.Name, "prospect", SpriteManager.SpriteType.UI, null, delegate() {
                        BottomGUIManager.Instance.Current_Entity = this;
                        CameraManager.Instance.Set_Camera_Location(Hex);
                    }));
                }
            }
        }
    }

    /// <summary>
    /// Try moving to a new hex
    /// </summary>
    /// <param name="new_hex"></param>
    /// <param name="ignore_movement_restrictions"></param>
    /// <returns></returns>
    public override bool Move(WorldMapHex new_hex, bool ignore_movement_restrictions = false, bool update_los = true)
    {
        Prospecting = false;
        bool success = base.Move(new_hex, ignore_movement_restrictions, update_los);
        if (success && !Main.Instance.Other_Players_Turn) {
            World.Instance.Map.Map_Mode = World.Instance.Map.Map_Mode;
        }
        return success;
    }
    
    public bool Prospecting
    {
        get {
            return prospecting;
        }
        set {
            if(prospecting == value) {
                return;
            }
            prospecting = value;
            if (prospecting) {
                Prospect_Progress = 0;
                Current_Movement = 0.0f;
                Start_Animation(Working_Animation, Working_Animation_FPS);
            } else {
                Stop_Animation();
            }
        }
    }
    
    public void Update_Actions_List()
    {
        Actions.Clear();
        Actions.Add(new Action("Prospect", "prospect", null, SpriteManager.SpriteType.UI,
            delegate (WorldMapEntity entity) {
                return entity.Hex.Can_Spawn_Minerals;
            },
            delegate (WorldMapEntity entity) {
                (entity as Prospector).Prospecting = true;
            })
        );
    }

    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder();
            tooltip.Append(Name);
            tooltip.Append(Environment.NewLine).Append("Upkeep: ").Append(Math.Round(Upkeep, 2).ToString("0.00"));
            tooltip.Append(Environment.NewLine).Append("Turns to Prospect: ").Append(Prospect_Turns);
            tooltip.Append(Environment.NewLine).Append("Movement: ").Append(Mathf.RoundToInt(Max_Movement));
            return tooltip.ToString();
        }
    }
}