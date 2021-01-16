using System;
using System.Collections.Generic;
using UnityEngine;

public class Army : WorldMapEntity {
    private static readonly float MANPOWER_REGEN_CITY = 0.15f;
    private static readonly float MANPOWER_REGEN_OWN_LAND = 0.1f;
    private static readonly float MANPOWER_REGEN_NEUTRAL_LAND = 0.025f;
    private static readonly float MANPOWER_REGEN_ENEMY_LAND = 0.0f;

    private static readonly List<string> CAMP_ANIMATION = new List<string>() { "camp_1", "camp_2", "camp_3" };
    private static readonly float CAMP_ANIMATION_FPS = 5.0f;
    
    //Ideas
    //Army specializations:
    //Cooldown for changing
    //Examples:
    //Recon squad
    //-limited size
    //+los
    //Militia taskforce
    public int Max_Size { get; private set; }
    public List<Unit> Units { get; private set; }
    public float Raiding_Income { get; private set; }
    public List<string> Camp_Animation { get; private set; }
    public float Camp_Animation_FPS { get; private set; }

    private bool text_initialized;
    private GameObject TextMesh_game_object;
    private TextMesh TextMesh { get { return TextMesh_game_object.GetComponentInChildren<TextMesh>(); } }


    public Army(WorldMapHex hex, Army prototype, Player owner, Unit first_unit) : base(hex, prototype, owner, false)
    {
        text_initialized = false;
        Max_Size = prototype.Max_Size;
        Camp_Animation = prototype.Camp_Animation != null ? Helper.Copy_List(prototype.Camp_Animation) : null;
        Camp_Animation_FPS = prototype.Camp_Animation_FPS;
        if(first_unit != null) {
            first_unit.Army = this;
            Units = new List<Unit>() { first_unit };
        } else {
            Units = new List<Unit>();
        }
        Current_Movement = Max_Movement;

        TextMesh_game_object = new GameObject("Army text " + Id);
        TextMesh_game_object.transform.SetParent(GameObject.transform);
        TextMesh_game_object.transform.position = new Vector3(
            GameObject.transform.position.x + 0.0f,
            GameObject.transform.position.y + 0.6f,
            GameObject.transform.position.z
        );
        TextMesh_game_object.AddComponent<TextMesh>();
        TextMesh.fontSize = 0;
        TextMesh.characterSize = 0.175f;
        TextMesh.anchor = TextAnchor.MiddleCenter;
        TextMesh.alignment = TextAlignment.Center;
        text_initialized = true;
        Update_Text();
        
        Actions.Add(new Action("Transfer Units", "transfer", "Transfers selected units to adjancent hex, mergin them to army " + Environment.NewLine +
            "if it has one or creating a new army if hex is empty", SpriteManager.SpriteType.UI,
            delegate (WorldMapEntity entity) {
                return BottomGUIManager.Instance.Selected_Units.Count != 0;
            },
            delegate (WorldMapEntity entity) {
                MessageManager.Instance.Show_Message("Select adjancent hex to transfer units");
                MouseManager.Instance.Set_Select_Hex_Mode(true, delegate(Hex selected_hex_p) {
                    if(!(selected_hex_p is WorldMapHex)) {
                        CustomLogger.Instance.Error("Selected hex is not WorldMapHex");
                        return;
                    }
                    WorldMapHex selected_hex = selected_hex_p as WorldMapHex;
                    //Check for adjancency
                    if (!selected_hex.Is_Adjancent_To(Hex)) {
                        return;
                    }
                    //Check for invalid entities blocking hex
                    if(selected_hex.Entity != null && (!selected_hex.Entity.Is_Owned_By(Owner) || selected_hex.Entity is Worker)) {
                        MessageManager.Instance.Show_Message("Select hex contains an entity that is not a your army");
                        return;
                    }
                    //Check if hex is passable
                    if (!selected_hex.Passable_For(this)) {
                        MessageManager.Instance.Show_Message("Select hex is impassable");
                        return;
                    }
                    //Check if hex has army with space for selected units
                    if(selected_hex.Entity != null && (selected_hex.Entity as Army).Max_Size < (selected_hex.Entity as Army).Units.Count + BottomGUIManager.Instance.Selected_Units.Count) {
                        MessageManager.Instance.Show_Message("Select hex contains army which only has space for " +
                            ((selected_hex.Entity as Army).Max_Size - (selected_hex.Entity as Army).Units.Count) + " unit" +
                            Helper.Plural(((selected_hex.Entity as Army).Max_Size - (selected_hex.Entity as Army).Units.Count)));
                        return;
                    }
                    //Check that all selected units have mp
                    foreach(Unit u in BottomGUIManager.Instance.Selected_Units) {
                        if(u.Current_Campaing_Map_Movement <= 0.0f) {
                            MessageManager.Instance.Show_Message("Selected unit lacks movement points");
                            return;
                        }
                    }
                    if (selected_hex.Entity == null) {
                        selected_hex.Entity = new Army(selected_hex, Owner.Faction.Army_Prototype, Owner, null);
                    }
                    foreach (Unit u in BottomGUIManager.Instance.Selected_Units) {
                        u.Current_Campaing_Map_Movement -= selected_hex.Movement_Cost;
                        u.Army = selected_hex.Entity as Army;
                    }
                    List<Unit> transfered_units = new List<Unit>();
                    for (int i = 0; i < Units.Count; i++) {
                        if (BottomGUIManager.Instance.Selected_Units.Contains(Units[i])) {
                            transfered_units.Add(Units[i]);
                        }
                    }
                    foreach (Unit u in transfered_units) {
                        (selected_hex.Entity as Army).Units.Add(u);
                        Units.Remove(u);
                    }
                    (selected_hex.Entity as Army).Update_Text();
                    if (Units.Count == 0) {
                        Delete();
                    } else {
                        Update_Text();
                        BottomGUIManager.Instance.Update_Current_Entity();
                    }
                });
            })
        );

        Actions.Add(new Action("Disband Units", "disband", "Disbands selected units", SpriteManager.SpriteType.UI,
            delegate (WorldMapEntity entity) {
                return BottomGUIManager.Instance.Selected_Units.Count != 0;
            },
            delegate (WorldMapEntity entity) {
                List<int> disbanded_unit_indices = new List<int>();
                for(int i = 0; i < Units.Count; i++) {
                    if (BottomGUIManager.Instance.Selected_Units.Contains(Units[i])) {
                        disbanded_unit_indices.Add(i);
                    }
                }
                foreach(int i in disbanded_unit_indices) {
                    Units.RemoveAt(i);
                }
                if (Units.Count == 0) {
                    Delete();
                } else {
                    Update_Text();
                }
            })
        );
    }

    public Army(string name, string texture, int max_size) : base(name, -1, Map.MovementType.Land, -1, texture)
    {
        Max_Size = max_size;
        Camp_Animation = null;
        Camp_Animation_FPS = -1.0f;
    }

    public Army(string name, string texture, int max_size, List<string> camp_animation, float camp_animation_fps) : base(name, -1, Map.MovementType.Land, -1, texture)
    {
        Max_Size = max_size;
        Camp_Animation = camp_animation;
        Camp_Animation_FPS = camp_animation_fps;
    }

    public override float Max_Movement
    {
        get {
            float min_movement = -1.0f;
            if(Units == null) {
                return 0.0f;
            }
            foreach(Unit unit in Units) {
                if(min_movement == -1.0f || min_movement > unit.Max_Campaing_Map_Movement) {
                    min_movement = unit.Max_Campaing_Map_Movement;
                }
            }
            return min_movement;
        }
        protected set {
            return;
        }
    }

    public override float Current_Movement
    {
        get {
            float min_movement = -1.0f;
            if (Units == null) {
                return 0.0f;
            }
            foreach (Unit unit in Units) {
                if (min_movement == -1.0f || min_movement > unit.Current_Campaing_Map_Movement) {
                    min_movement = unit.Current_Campaing_Map_Movement;
                }
            }
            return min_movement;
        }

        protected set {
            if(Units == null) {
                return;
            }
            foreach (Unit unit in Units) {
                if (unit.Current_Campaing_Map_Movement < value) {
                    unit.Current_Campaing_Map_Movement = value;
                }
            }
        }
    }

    public override int LoS
    {
        get {
            int max_los = -1;
            if(Units == null) {
                return 1;
            }
            foreach (Unit unit in Units) {
                if (max_los == -1 || max_los > unit.LoS) {
                    max_los = unit.LoS;
                }
            }
            return max_los;
        }
        protected set {
            return;
        }
    }


    public bool Has_Space
    {
        get {
            if(Units == null) {
                return false;
            }
            return Units.Count < Max_Size;
        }
    }

    public bool Add_Unit(Unit unit)
    {
        if (!Has_Space) {
            return false;
        }
        unit.Army = this;
        Units.Add(unit);
        Update_Text();
        return true;
    }

    public override float Upkeep
    {
        get {
            float upkeep = 0.0f;
            foreach(Unit unit in Units) {
                upkeep += unit.Upkeep;
            }
            if(Hex.City != null) {
                upkeep *= (1.0f - Hex.City.Garrison_Upkeep_Reduction);
            }
            return upkeep;
        }
        protected set {
            return;
        }
    }

    public override float Mana_Upkeep
    {
        get {
            float upkeep = 0.0f;
            foreach (Unit unit in Units) {
                upkeep += unit.Mana_Upkeep;
            }
            return upkeep;
        }
        protected set {
            return;
        }
    }

    public bool Units_Have_Movement_Left
    {
        get {
            foreach(Unit u in Units) {
                if(u.Current_Campaing_Map_Movement <= 0.0f) {
                    return false;
                }
            }
            return true;
        }
    }

    public override bool Move(WorldMapHex new_hex, bool ignore_movement_restrictions = false, bool update_los = true)
    {
        if(new_hex.Is_Adjancent_To(Hex) && Units_Have_Movement_Left && new_hex.Entity != null && new_hex.Entity is Army && !new_hex.Entity.Is_Owned_By(Owner)) {
            Attack(new_hex.Entity as Army);
            return true;
        }
        WorldMapHex old_hex = Hex;
        bool success = base.Move(new_hex, ignore_movement_restrictions, update_los);
        if(success)  {
            if (!ignore_movement_restrictions) {
                foreach (Unit unit in Units) {
                    unit.Current_Campaing_Map_Movement -= Hex.Movement_Cost;
                }
            }
            if(Hex.Civilian != null && !Hex.Civilian.Is_Owned_By(Owner)) {
                Hex.Civilian.Delete();
            }
            if(Hex.City != null && !Hex.City.Is_Owned_By(Owner)) {
                Hex.City.Conquer(Owner);
            }
            if(old_hex.Village != null) {
                old_hex.Village.Update_Owner();
            }
            if (Hex.Village != null && !Hex.Village.Is_Owned_By(Owner)) {
                Hex.Village.Update_Owner();
            }
            if(Hex.Owner != null && (Blocks_Hex_Working || Has_Raider)) {
                foreach (City city in Hex.In_Work_Range_Of) {
                    if(city.Owner.Is_Neutral || city.Is_Owned_By(Owner)) {
                        continue;
                    }
                    if (Blocks_Hex_Working && city.Unassing_Pop(Hex)) {
                        city.Auto_Apply_Unemployed_Pops();
                        city.Owner.Queue_Notification(new Notification("Worked hex blocked", Texture, SpriteManager.SpriteType.Unit, null, delegate() {
                            CameraManager.Instance.Set_Camera_Location(Hex);
                        }));
                    } else if (Has_Raider) {
                        city.Owner.Queue_Notification(new Notification("Raiders", Texture, SpriteManager.SpriteType.Unit, null, delegate () {
                            CameraManager.Instance.Set_Camera_Location(Hex);
                        }));
                        city.Yields_Changed();
                    }
                }
            }
        }
        return success;
    }

    public override void End_Turn()
    {
        base.End_Turn();
        float manpower_regen = MANPOWER_REGEN_NEUTRAL_LAND;
        if(Hex.City != null) {
            manpower_regen = MANPOWER_REGEN_CITY;
        } else if (Hex.Owner != null && Hex.Is_Owned_By(Owner)) {
            manpower_regen = MANPOWER_REGEN_OWN_LAND;
        } else if(Hex.Owner != null && !Hex.Is_Owned_By(Owner)) {
            manpower_regen = MANPOWER_REGEN_ENEMY_LAND;
        }
        Yields raided_yields = new Yields();
        bool raid = Hex.Owner != null && !Hex.Owner.Is_Neutral && !Hex.Is_Owned_By(Owner) && Hex.In_Work_Range_Of.Count != 0;
        foreach (Unit unit in Units) {
            unit.Current_Campaing_Map_Movement = unit.Max_Campaing_Map_Movement;
            unit.Regen_Manpower(manpower_regen);
            foreach(Ability ability in unit.Abilities) {
                if(raid && ability.On_Worked_Hex_Raid != null) {
                    raided_yields.Add(ability.On_Worked_Hex_Raid(ability, unit, Hex));
                }
                if(ability.On_Turn_End != null) {
                    ability.On_Turn_End(ability, unit);
                }
            }
        }
        Raiding_Income = raided_yields.Cash;
        Owner.Cash += Raiding_Income;
        if(Raiding_Income > 0.0f) {
            Hex.Owner.Queue_Notification(new Notification("Raiders", Texture, SpriteManager.SpriteType.Unit, null, delegate () {
                CameraManager.Instance.Set_Camera_Location(Hex);
            }));
        }
        Update_Text();
    }

    public void Attack(Army army)
    {
        foreach (Unit u in Units) {
            u.Current_Campaing_Map_Movement = 0.0f;
        }
        CombatManager.Instance.Start_Combat(this, army, army.Hex);
    }

    public void Start_Combat()
    {
        foreach(Unit u in Units) {
            u.Start_Combat();
        }
    }

    public void End_Combat()
    {
        List<Unit> defeated_units = new List<Unit>();
        foreach (Unit unit in Units) {
            unit.End_Combat();
            if(unit.Manpower <= 0.0f) {
                defeated_units.Add(unit);
            }
        }
        foreach (Unit unit in defeated_units) {
            Units.Remove(unit);
        }
        if(Units.Count == 0) {
            Delete();
        } else {
            Update_Text();
        }
    }

    public void End_Combat_Turn()
    {
        foreach (Unit u in Units) {
            u.End_Combat_Turn();
        }
    }

    public float Relative_Strenght
    {
        get {
            float relative_strenght = 0.0f;
            foreach(Unit u in Units) {
                relative_strenght += u.Relative_Strenght;
            }
            return relative_strenght;
        }
    }

    public float Current_Relative_Strenght
    {
        get {
            float relative_strenght = 0.0f;
            foreach (Unit u in Units) {
                relative_strenght += u.Current_Relative_Strenght;
            }
            return relative_strenght;
        }
    }

    public float Get_Relative_Strenght_When_On_Hex(WorldMapHex hex, bool current_str, bool attacker)
    {
        float relative_strenght = 0.0f;
        foreach (Unit u in Units) {
            relative_strenght += u.Get_Relative_Strenght_When_On_Hex(hex, current_str, attacker);
        }
        return relative_strenght;
    }

    public List<Army> Adjancent_Enemies
    {
        get {
            List<Army> enemies = new List<Army>();
            foreach(WorldMapHex hex in Hex.Get_Adjancent_Hexes()) {
                if(hex.Entity != null && hex.Entity is Army && !hex.Entity.Is_Owned_By(Owner)) {
                    enemies.Add(hex.Entity as Army);
                }
            }
            return enemies;
        }
    }

    public bool Push_Into(WorldMapHex hex)
    {
        if(hex.Entity == null) {
            return Move(hex);
        }
        if (!Push_Move_Target(hex.Entity)) {
            return false;
        }
        return Move(hex, true);
    }

    private bool Push_Move_Target(WorldMapEntity entity)
    {
        if (!Hex.Is_Adjancent_To(entity.Hex)) {
            return false;
        }
        Coordinates coordinates = new Coordinates(entity.Hex.Coordinates);
        Map.Direction direction = Hex.Coordinates.Direction(coordinates).Value;
        coordinates.Shift(direction);
        WorldMapHex opposite = World.Instance.Map.Get_Hex_At(coordinates);
        if (opposite != null && opposite.Entity == null && opposite.Passable_For(entity)) {
            return entity.Move(opposite, true, false);
        }
        for (int i = 1; i < 3; i++) {
            Map.Direction rotated_direction = Helper.Rotate(direction, i);
            Coordinates c = new Coordinates(entity.Hex.Coordinates);
            c.Shift(rotated_direction);
            WorldMapHex hex = World.Instance.Map.Get_Hex_At(c);
            if (hex != null && hex.Entity == null && hex.Passable_For(entity)) {
                return entity.Move(hex, true, false);
            }
            rotated_direction = Helper.Rotate(direction, -i);
            c = new Coordinates(entity.Hex.Coordinates);
            c.Shift(rotated_direction);
            hex = World.Instance.Map.Get_Hex_At(c);
            if (hex != null && hex.Entity == null && hex.Passable_For(entity)) {
                return entity.Move(hex, true, false);
            }
        }
        return false;
    }

    public void Update_Text()
    {
        if (!text_initialized || Was_Deleted) {
            return;
        }
        if(Relative_Strenght < 1000.0f) {
            TextMesh.text = Mathf.RoundToInt(Relative_Strenght).ToString();
        } else {
            TextMesh.text = string.Format("{0}k", Math.Round(Relative_Strenght / 1000.0f, 1));
        }
    }

    public Ability.CityEffects Get_City_Effects(City city)
    {
        Ability.CityEffects effects = new Ability.CityEffects();
        foreach(Unit unit in Units) {
            foreach(Ability ability in unit.Abilities) {
                if(ability.Get_City_Effects != null) {
                    effects.Add(ability.Get_City_Effects(ability, city));
                }
            }
        }
        return effects;
    }

    public float Order
    {
        get {
            float order = 1.0f;
            float str = Current_Relative_Strenght;
            //TODO: use readonly variables
            if(str > 100.0f && str <= 500.0f) {
                order += (str - 100.0f) / 200.0f;
            } else if(str > 500.0f && str <= 1000.0f) {
                order += 2.0f + ((str - 500.0f) / 400.0f);
            } else if(str > 1000.0f) {
                order += 3.25f;
            }
            return order;
        }
    }

    public bool Blocks_Hex_Working
    {
        get {
            foreach(Unit unit in Units) {
                if (unit.Tags.Contains(Unit.Tag.Blocks_Hex_Working)) {
                    return true;
                }
            }
            return false;
        }
    }

    public bool Has_Raider
    {
        get {
            foreach (Unit unit in Units) {
                foreach(Ability ability in unit.Abilities) {
                    if(ability.On_Worked_Hex_Raid != null) {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public override bool Wait_Turn
    {
        get {
            return base.Wait_Turn;
        }

        set {
            wait_turn = value;
            if (Wait_Turn) {
                Sleep = false;
            }
        }
    }

    public override bool Sleep
    {
        get {
            return base.Sleep;
        }

        set {
            bool change = base.Sleep != value;
            base.Sleep = value;
            if (change) {
                if (!Sleep) {
                    Stop_Animation();
                } else {
                    if(Camp_Animation == null) {
                        Start_Animation(CAMP_ANIMATION, CAMP_ANIMATION_FPS);
                    } else {
                        Start_Animation(Camp_Animation, Camp_Animation_FPS);
                    }
                }
            }
        }
    }
}
