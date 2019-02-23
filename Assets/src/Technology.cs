﻿using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Technology {
    public string Name { get; private set; }
    public int Research_Required { get; private set; }
    public float Research_Acquired { get; set; }
    public Player Owner { get; set; }
    public List<AI.Tag> Tags { get; private set; }
    public EmpireModifiers EmpireModifiers { get; set; }

    /// <summary>
    /// Key = tech tree position: 0 = top most, 1 = second from top ..., 7+ special link rules
    /// </summary>
    public Dictionary<int, Technology> Leads_To { get; set; }
    public List<Technology> Prequisites { get; set; }

    public Technology(string name, int research_required, List<AI.Tag> tags)
    {
        Name = name;
        Research_Required = research_required;
        Research_Acquired = 0.0f;
        Leads_To = new Dictionary<int, Technology>();
        Prequisites = new List<Technology>();
        Tags = tags;
        EmpireModifiers = new EmpireModifiers();
    }

    /// <summary>
    /// Creates imperfect clone of parameter technology (does not clone links)
    /// </summary>
    /// <param name="technology"></param>
    public Technology(Technology technology, Player owner)
    {
        Name = technology.Name;
        Research_Required = technology.Research_Required;
        Research_Acquired = 0;
        Owner = owner;
        Tags = new List<AI.Tag>();
        foreach(AI.Tag tag in technology.Tags) {
            Tags.Add(tag);
        }
        EmpireModifiers = technology.EmpireModifiers;
        Leads_To = new Dictionary<int, Technology>();
        foreach(KeyValuePair<int, Technology> pair in technology.Leads_To) {
            Leads_To.Add(pair.Key, pair.Value);
        }
        Prequisites = new List<Technology>();
        foreach(Technology t in technology.Prequisites) {
            Prequisites.Add(t);
        }
    }

    public int Turns_Left_Estimate
    {
        get {
            return Mathf.CeilToInt((Research_Required - Research_Acquired) / (Owner.Total_Science * 1.0f));
        }
    }

    public void Link(Technology tech, int tree_position = -1)
    {
        if(tree_position == -1) {
            tree_position = 7;
            while (Leads_To.ContainsKey(tree_position)) {
                tree_position++;
            }
        }
        if (!Leads_To.ContainsKey(tree_position)) {
            Leads_To.Add(tree_position, tech);
        }
        if (!tech.Prequisites.Contains(this)) {
            tech.Prequisites.Add(this);
        }
    }

    public bool Is_Researched
    {
        get {
            return Research_Acquired >= Research_Required;
        }
    }

    public bool Can_Be_Researched
    {
        get {
            if (Is_Researched) {
                return false;
            }
            foreach(Technology t in Prequisites) {
                if (!t.Is_Researched) {
                    return false;
                }
            }
            return true;
        }
    }

    public override string ToString()
    {
        return Name;
    }

    public List<string> Get_Unlocks()
    {
        List<string> unlocks = new List<string>();
        foreach(Building building in Owner.Faction.Buildings) {
            if(building.Technology_Required != null && building.Technology_Required.Name == Name) {
                unlocks.Add(building.Name);
            }
        }
        foreach (Improvement improvement in Owner.Faction.Improvements) {
            if (improvement.Technology_Required != null && improvement.Technology_Required.Name == Name) {
                unlocks.Add(improvement.Name);
            }
        }
        foreach (Trainable trainable in Owner.Faction.Units) {
            if (trainable.Technology_Required != null && trainable.Technology_Required.Name == Name) {
                unlocks.Add(trainable.Name);
            }
        }
        return unlocks;
    }

    public List<Technology> All_Techs_This_Leads_To
    {
        get {
            List<Technology> all = new List<Technology>();
            foreach(KeyValuePair<int, Technology> pair in Leads_To) {
                Find_Techs_Recursive(pair.Value, all);
            }
            return all;
        }
    }

    private void Find_Techs_Recursive(Technology technology, List<Technology> all)
    {
        if (!all.Contains(technology)) {
            all.Add(technology);
        }
        foreach (KeyValuePair<int, Technology> pair in technology.Leads_To) {
            Find_Techs_Recursive(pair.Value, all);
        }
    }

    public Technology Clone(Player owner)
    {
        Dictionary<string, Technology> cloned_technologies = new Dictionary<string, Technology>();
        Technology clone = new Technology(this, owner);
        cloned_technologies.Add(Name, clone);

        foreach(KeyValuePair<int, Technology> pair in clone.Leads_To) {
            Clone_Recursive(pair.Value, owner, cloned_technologies);
        }

        Dictionary<string, Dictionary<int, Technology>> leads_to_help = new Dictionary<string, Dictionary<int, Technology>>();
        Dictionary<string, List<Technology>> prerequisites_help = new Dictionary<string, List<Technology>>();

        foreach(KeyValuePair<string, Technology> cloned_technology in cloned_technologies) {
            leads_to_help.Add(cloned_technology.Key, new Dictionary<int, Technology>());
            foreach(KeyValuePair<int, Technology> pair in cloned_technology.Value.Leads_To) {
                leads_to_help[cloned_technology.Key].Add(pair.Key, cloned_technologies[pair.Value.Name]);
            }
            List<Technology> cloned_previous_technologies = new List<Technology>();
            foreach(Technology technology in cloned_technology.Value.Prequisites) {
                cloned_previous_technologies.Add(cloned_technologies[technology.Name]);
            }
            prerequisites_help.Add(cloned_technology.Key, cloned_previous_technologies);
        }

        foreach(KeyValuePair<string, Dictionary<int, Technology>> pair in leads_to_help) {
            cloned_technologies[pair.Key].Leads_To = pair.Value;
        }
        foreach(KeyValuePair<string, List<Technology>> pair in prerequisites_help) {
            cloned_technologies[pair.Key].Prequisites = pair.Value;
        }

        return clone;
    }

    private void Clone_Recursive(Technology technology, Player owner, Dictionary<string, Technology> cloned_technologies)
    {
        if (!cloned_technologies.ContainsKey(technology.Name)) {
            cloned_technologies.Add(technology.Name, new Technology(technology, owner));
        }
        foreach (KeyValuePair<int, Technology> pair in technology.Leads_To) {
            Clone_Recursive(pair.Value, owner, cloned_technologies);
        }
    }

    public string Tooltip
    {
        get {
            StringBuilder tooltip = new StringBuilder();
            tooltip.Append(Math.Round(Research_Acquired, 1)).Append(" / ").Append(Research_Required).Append(" (").Append(Turns_Left_Estimate).Append(" turn").
                Append(Helper.Plural(Turns_Left_Estimate)).Append(")");
            if (!EmpireModifiers.Empty) {
                tooltip.Append(Environment.NewLine).Append(EmpireModifiers.Tooltip);
            }
            List<string> unlocks = Get_Unlocks();
            if(unlocks.Count != 0) {
                tooltip.Append(Environment.NewLine).Append("Unlocks: ").Append(string.Join(", ", Get_Unlocks().ToArray()));
            }
            return tooltip.ToString();
        }
    }
}
