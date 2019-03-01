using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class AI : IConfigListener
{
    //TODO: Knows if scouted city changes owners

    private static readonly float enemy_strenght_memory_decay = 0.01f;
    private static readonly float desired_army_strenght = 1.25f;
    private static readonly float desired_min_main_army_strenght = 1.0f;
    private static readonly float desired_defence_army_strenght = 0.25f;
    private static readonly float scout_attack_undefended_city_range_multiplier = 2.0f;
    private static readonly float scout_attack_undefended_civilian_range_multiplier = 2.0f;
    private static readonly float army_attack_carefulness = 0.01f;
    private static readonly float city_attack_carefulness = 0.05f;//Should be > army_attack_carefulness
    private static readonly float aggressiveness = 1.0f;
    private static readonly int careful_after_turns = 10;
    private static readonly float carefulness_strenght_multiplier = 0.5f;

    public static bool Default_Show_Moves = false;

    public enum Level { Inactive, Easy, Medium, Hard }
    public enum Tag { Food, Production, Cash, Science, Culture, Mana, Faith, Happiness, Health, Order, Military }
    public enum LogType { General, Economy, Military, Diagnostic }

    public bool Log_Actions { get; set; }
    public List<LogType> Logged_Action_Types { get; set; }
    public bool Show_Moves { get; set; }
    public bool Follow_Moves { get; set; }
    public float Time_Between_Actions { get; set; }
    public Player Player { get; private set; }
    public Level AI_Level { get; private set; }

    private float act_cooldown;
    private Dictionary<Tag, float> priorities;
    private Dictionary<City, Dictionary<Tag, float>> city_specific_priorities;
    private float desired_production;
    private float total_production;
    private int desired_workers;
    private int worker_count;
    private int desired_prospectors;
    private int prospector_count;
    private int desired_scouting_army_count;
    private int desired_main_army_count;
    private Worker worker_prototype;
    private Prospector prospector_prototype;
    private Dictionary<Army, WorldMapHex> scouting_armies;
    private Dictionary<City, Army> defence_armies;
    private Dictionary<Army, ArmyOrder> main_armies;
    private Dictionary<City, Trainable> cities_training_scout_armies;
    private Dictionary<City, object[]> cities_training_defence_armies;
    private Dictionary<City, Trainable> cities_training_main_armies;
    private Dictionary<Player, float> observed_max_enemy_army_strenght;
    private Dictionary<Player, float> observed_enemy_army_strenght_on_this_turn;
    private List<Army> armies_seen_this_turn;
    private float own_main_army_strenght;
    private float own_defence_army_strenght;
    private List<object[]> spectator_city_action_list;
    private List<City> scouted_enemy_cities;
    private List<City> undefended_enemy_cities;
    private List<WorldMapEntity> undefended_enemy_civilians;
    private Dictionary<Player, int> turns_since_army_was_scouted;
    private bool last_action_was_visible;
    private List<WorldMapHex> hexes_needing_prospecting;

    public AI(Level level, Player player)
    {
        Log_Actions = false;
        Logged_Action_Types = new List<LogType>() { LogType.General, LogType.Economy, LogType.Diagnostic };
        Show_Moves = Default_Show_Moves;
        ConfigManager.Instance.Register_Listener(this);
        
        AI_Level = level;
        Player = player;
        priorities = new Dictionary<Tag, float>();
        city_specific_priorities = new Dictionary<City, Dictionary<Tag, float>>();
        scouting_armies = new Dictionary<Army, WorldMapHex>();
        defence_armies = new Dictionary<City, Army>();
        main_armies = new Dictionary<Army, ArmyOrder>();
        cities_training_scout_armies = new Dictionary<City, Trainable>();
        cities_training_defence_armies = new Dictionary<City, object[]>();
        cities_training_main_armies = new Dictionary<City, Trainable>();
        observed_max_enemy_army_strenght = new Dictionary<Player, float>();
        observed_enemy_army_strenght_on_this_turn = new Dictionary<Player, float>();
        armies_seen_this_turn = new List<Army>();
        spectator_city_action_list = new List<object[]>();
        scouted_enemy_cities = new List<City>();
        undefended_enemy_cities = new List<City>();
        undefended_enemy_civilians = new List<WorldMapEntity>();
        turns_since_army_was_scouted = new Dictionary<Player, int>();
        last_action_was_visible = false;
        hexes_needing_prospecting = new List<WorldMapHex>();
    }

    public void On_Delete()
    {
        ConfigManager.Instance.Unregister_Listener(this);
    }

    public void Update_Settings()
    {
        Time_Between_Actions = ConfigManager.Instance.Current_Config.AI_Action_Delay;
        Follow_Moves = ConfigManager.Instance.Current_Config.AI_Follow_Moves;
    }

    public void Start_Turn()
    {
        //TODO: This call should not be needed
        Main.Instance.Update_Flags();

        if(AI_Level == Level.Inactive || Player.Is_Neutral) {
            return;
        }

        Log(string.Format("---- {0}: starting turn ----", Player.Name), LogType.General);
        Stopwatch stopwatch = Stopwatch.StartNew();
        act_cooldown = 0.0f;
        observed_enemy_army_strenght_on_this_turn.Clear();
        armies_seen_this_turn.Clear();
        spectator_city_action_list.Clear();

        //Check if armies have been deleted between turns
        //Scouting armies
        List<Army> deleted_armies = new List<Army>(); 
        foreach(KeyValuePair<Army, WorldMapHex> pair in scouting_armies) {
            if (!Player.WorldMapEntitys.Contains(pair.Key)) {
                deleted_armies.Add(pair.Key);
            }
        }
        foreach(Army a in deleted_armies) {
            scouting_armies.Remove(a);
        }
        //Defence armies
        deleted_armies.Clear();
        foreach (KeyValuePair<City, Army> pair in defence_armies) {
            if (!Player.WorldMapEntitys.Contains(pair.Value)) {
                deleted_armies.Add(pair.Value);
            }
        }
        foreach (Army a in deleted_armies) {
            defence_armies.Remove(defence_armies.First(x => x.Value == a).Key);
        }
        //Main armies
        deleted_armies.Clear();
        foreach (KeyValuePair<Army, ArmyOrder> pair in main_armies) {
            if (!Player.WorldMapEntitys.Contains(pair.Key)) {
                deleted_armies.Add(pair.Key);
            }
        }
        foreach (Army a in deleted_armies) {
            main_armies.Remove(a);
        }

        Manage_Empire();

        foreach(Player player in Main.Instance.Players) {
            if (!observed_max_enemy_army_strenght.ContainsKey(player)) {
                continue;
            }
            observed_max_enemy_army_strenght[player] = observed_max_enemy_army_strenght[player] * (1.0f - enemy_strenght_memory_decay);
        }

        undefended_enemy_cities.Clear();
        undefended_enemy_civilians.Clear();

        Update_Scouting();

        own_main_army_strenght = 0.0f;
        own_defence_army_strenght = 0.0f;
        foreach (WorldMapEntity entity in Player.WorldMapEntitys) {
            if (entity is Army) {
                if (main_armies.ContainsKey(entity as Army)) {
                    own_main_army_strenght += (entity as Army).Relative_Strenght;
                } else if (defence_armies.ContainsValue(entity as Army)) {
                    own_defence_army_strenght += (entity as Army).Relative_Strenght;
                }
            }
        }

        desired_main_army_count = (int)Mathf.Clamp((World.Instance.Map.Width * World.Instance.Map.Height) / 800, 1.0f, 3.0f);
        Log(string.Format("Start turn: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);
    }

    private void Update_Scouting()
    {
        foreach (KeyValuePair<WorldMapHex, WorldMapEntity> los_and_source in Player.LoS) {
            if (los_and_source.Key.Entity != null && los_and_source.Key.Entity is Army && !los_and_source.Key.Entity.Is_Owned_By(Player) &&
                    !armies_seen_this_turn.Contains(los_and_source.Key.Entity as Army)) {
                if (!observed_enemy_army_strenght_on_this_turn.ContainsKey(los_and_source.Key.Entity.Owner)) {
                    observed_enemy_army_strenght_on_this_turn.Add(los_and_source.Key.Entity.Owner, (los_and_source.Key.Entity as Army).Relative_Strenght);
                } else {
                    observed_enemy_army_strenght_on_this_turn[los_and_source.Key.Entity.Owner] += (los_and_source.Key.Entity as Army).Relative_Strenght;
                }
                armies_seen_this_turn.Add(los_and_source.Key.Entity as Army);
                if (!turns_since_army_was_scouted.ContainsKey(los_and_source.Key.Entity.Owner)) {
                    turns_since_army_was_scouted.Add(los_and_source.Key.Entity.Owner, 0);
                } else {
                    turns_since_army_was_scouted[los_and_source.Key.Entity.Owner] = 0;
                }
            }
            if (los_and_source.Key.City != null && !los_and_source.Key.City.Is_Owned_By(Player)) {
                if (!scouted_enemy_cities.Contains(los_and_source.Key.City)) {
                    scouted_enemy_cities.Add(los_and_source.Key.City);
                }
                if (los_and_source.Key.Entity == null && !undefended_enemy_cities.Contains(los_and_source.Key.City)) {
                    undefended_enemy_cities.Add(los_and_source.Key.City);
                }
            }
            if (los_and_source.Key.Civilian != null && !los_and_source.Key.Civilian.Is_Owned_By(Player) && los_and_source.Key.Entity == null
                    && !undefended_enemy_civilians.Contains(los_and_source.Key.Civilian)) {
                undefended_enemy_civilians.Add(los_and_source.Key.Civilian);
            }
        }
    }

    private float Largest_Observed_Max_Enemy_Army_Strenght
    {
        get {
            float largest = 0.0f;
            foreach(KeyValuePair<Player, float> pair in observed_max_enemy_army_strenght) {
                if(pair.Value > largest) {
                    largest = pair.Value;
                }
            }
            return largest;
        }
    }

    public void Act(float delta_s)
    {
        if(AI_Level == Level.Inactive || Player.Is_Neutral) {
            Main.Instance.Next_Turn();
            return;
        }
        act_cooldown -= delta_s;
        if (act_cooldown > 0.0f) {
            return;
        }
        act_cooldown += last_action_was_visible || Show_Moves ? Time_Between_Actions : 0.0f;

        Stopwatch stopwatch = Stopwatch.StartNew();

        //Show city actions
        if (Show_Moves && Follow_Moves && spectator_city_action_list.Count != 0) {
            City city = spectator_city_action_list[0][0] as City;
            string action = spectator_city_action_list[0][1] as string;
            spectator_city_action_list.RemoveAt(0);
            CameraManager.Instance.Set_Camera_Location(city.Hex);
            MessageManager.Instance.Show_Message(action);
            return;
        }

        //Move units
        foreach (WorldMapEntity entity in Player.WorldMapEntitys) {
            if (entity.Wait_Turn) {
                continue;
            }
            if (entity is Worker && (entity as Worker).Improvement_Under_Construction == null) {
                Manage_Worker(entity as Worker);
                return;
            } else if (entity is Prospector && !(entity as Prospector).Prospecting) {
                Manage_Prospector(entity as Prospector);
                return;
            } else if (entity is Army) {
                Army army = entity as Army;
                if (scouting_armies.ContainsKey(army)) {
                    Manage_Scout_Army(army);
                    return;
                }
                if (defence_armies.ContainsValue(army)) {
                    Manage_Defence_Army(army, defence_armies.First(x => x.Value == army).Key);
                    return;
                }
                if (main_armies.ContainsKey(army)) {
                    Manage_Main_Army(army);
                    return;
                }
            }
            //TODO: add way less act cooldown if in fog of war (maybe dont "return;"?)
        }
        
        Log(string.Format("Act: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);

        Log(string.Format("---- {0}: ending turn ----", Player.Name), LogType.General);
        foreach(KeyValuePair<Player, float> pair in observed_enemy_army_strenght_on_this_turn) {
            if (!observed_max_enemy_army_strenght.ContainsKey(pair.Key)) {
                observed_max_enemy_army_strenght.Add(pair.Key, pair.Value);
            } else if(observed_max_enemy_army_strenght[pair.Key] < pair.Value) {
                observed_max_enemy_army_strenght[pair.Key] = pair.Value;
            }
        }
        foreach(Player player in Main.Instance.Players) {
            if (turns_since_army_was_scouted.ContainsKey(player)) {
                turns_since_army_was_scouted[player]++;
            }
        }
        Main.Instance.Next_Turn();
    }

    private void Manage_Empire()
    {
        Log("--- Empire management ---", LogType.Economy);

        Stopwatch stopwatch = Stopwatch.StartNew();
        priorities.Clear();
        foreach (Tag tag in Enum.GetValues(typeof(Tag))) {
            priorities.Add(tag, 0.0f);
        }
        city_specific_priorities.Clear();

        //Production stuff
        total_production = 0.0f;
        foreach (City city in Player.Cities) {
            total_production += city.Get_Base_Yields(false).Production;
        }
        desired_production = 5.0f;
        if (Main.Instance.Round > 5) {
            if (Main.Instance.Round <= 20) {
                desired_production = Main.Instance.Round;
            } else {
                desired_production = 20.0f + ((Main.Instance.Round - 20) * 0.1f);
            }
        }

        //Workers
        desired_workers = Player.Cities.Count;
        worker_count = 0;
        foreach (WorldMapEntity entity in Player.WorldMapEntitys) {
            if (entity is Worker) {
                worker_count++;
            }
        }
        int workers_in_training = 0;
        foreach (City city in Player.Cities) {
            if (city.Unit_Under_Production is Worker) {
                workers_in_training++;
            }
        }
        desired_workers -= workers_in_training;
        worker_prototype = Player.Faction.Units.First(x => x is Worker && !x.Requires_Coast) as Worker;

        //Prospectors
        hexes_needing_prospecting.Clear();
        bool has_tiles_needing_prospecting = false;
        int prospectors_in_training = 0;
        foreach (City city in Player.Cities) {
            foreach (WorldMapHex hex in city.Hexes_In_Work_Range) {
                if (hex.Can_Spawn_Minerals && !hex.Is_Prospected_By(Player) && hex.Passable) {
                    has_tiles_needing_prospecting = true;
                    if((hex.Entity == null || hex.Entity.Is_Owned_By(Player)) && hex.Civilian == null) {
                        hexes_needing_prospecting.Add(hex);
                    }
                }
            }
            if (city.Unit_Under_Production is Prospector) {
                prospectors_in_training++;
            }
        }
        prospector_prototype = Player.Faction.Units.First(x => x is Prospector) as Prospector;
        desired_prospectors = has_tiles_needing_prospecting ? 1 - prospectors_in_training : 0;

        //Delete excess prospectors
        List<Prospector> delete_these = new List<Prospector>();
        while (prospector_count - prospectors_in_training > desired_prospectors) {
            foreach(WorldMapEntity entity in Player.WorldMapEntitys) {
                if(entity is Prospector) {
                    delete_these.Add(entity as Prospector);
                    prospector_count--;
                }
            }
        }
        if(delete_these.Count != 0) {
            Log(string.Format("Too many prospectors, count: {0} desired: {1}, removing excessive prospectors",
                prospector_count - prospectors_in_training, desired_prospectors), LogType.Economy);
            foreach(WorldMapEntity entity in delete_these) {
                entity.Delete();
            }
        }

        foreach (City city in Player.Cities) {
            Manage_City(city);
        }
        
        //Calculate average priorities
        foreach (Tag tag in Enum.GetValues(typeof(Tag))) {
            priorities[tag] /= Player.Cities.Count;
        }

        Log("-- Overall priorities --", LogType.Economy);
        foreach (KeyValuePair<Tag, float> pair in priorities) {
            Log(string.Format("{0}: {1}", pair.Key.ToString(), Math.Round(pair.Value, 1)), LogType.Economy);
        }

        //Set research
        float base_preference = 10.0f;
        if (Player.Current_Technology == null) {
            Log("-- Evaluating technology options --", LogType.Economy);
            Dictionary<Technology, float> technology_options = new Dictionary<Technology, float>();
            foreach (Technology technology in Player.Root_Technology.All_Techs_This_Leads_To) {
                if (technology.Can_Be_Researched) {
                    float preference = base_preference;

                    foreach (Tag tag in Enum.GetValues(typeof(Tag))) {
                        if (technology.Tags.Contains(tag)) {
                            preference += priorities[tag];
                        }
                    }

                    float research_impact = (technology.Research_Required + 100.0f) / 200.0f;
                    preference /= research_impact;

                    Log(string.Format("{0} -> preference: {1}", technology.Name, Math.Round(preference, 1)), LogType.Economy);
                    technology_options.Add(technology, preference);
                }
            }

            if (technology_options.Count == 0) {
                Log("No options", LogType.Economy);
            } else {
                List<Technology> technologies_by_preference = technology_options.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                Player.Current_Technology = technologies_by_preference[0];
                Log("Researching: " + technologies_by_preference[0].Name, LogType.Economy);
            }
        }

        //Assing army roles
        List<WorldMapEntity> help = new List<WorldMapEntity>();
        foreach (WorldMapEntity e in Player.WorldMapEntitys) {
            help.Add(e);
        }

        foreach (WorldMapEntity entity in help) {
            if (entity.Is_Civilian) {
                continue;
            }
            Army army = entity as Army;
            //Assing roles for new armies
            //TODO: army.Units.Count == 1 is propably not needed
            if (!main_armies.ContainsKey(army) && !scouting_armies.ContainsKey(army) && !defence_armies.ContainsValue(army)) {
                Log("Assigning role for army #" + army.Id, LogType.Military);
                if (army.Units.Count == 1 && army.Hex.City != null && cities_training_scout_armies.ContainsKey(army.Hex.City)) {
                    //New scout army
                    Log("Role assigned: scout", LogType.Military);
                    scouting_armies.Add(army, null);
                    cities_training_scout_armies.Remove(army.Hex.City);
                } else if (army.Units.Count == 1 && army.Hex.City != null && cities_training_defence_armies.ContainsKey(army.Hex.City)) {
                    //New defence army
                    Log("Role assigned: defence(city#" + (cities_training_defence_armies[army.Hex.City][1] as City).Id + ")", LogType.Military);
                    defence_armies.Add(cities_training_defence_armies[army.Hex.City][1] as City, army);
                    cities_training_defence_armies.Remove(army.Hex.City);
                } else if (army.Units.Count == 1 && army.Hex.City != null && cities_training_main_armies.ContainsKey(army.Hex.City)) {
                    //New main army
                    Log("Role assigned: main", LogType.Military);
                    main_armies.Add(army, null);
                    cities_training_main_armies.Remove(army.Hex.City);
                } else {
                    CustomLogger.Instance.Warning("Could not find role for army #" + army.Id);
                }
            }
            //Split new armies from existing ones
            if (army.Hex.City != null && army.Units.Count > 1) {
                if (cities_training_scout_armies.ContainsKey(army.Hex.City) && army.Units.Any(x => x.Name == cities_training_scout_armies[army.Hex.City].Name)) {
                    Log("Splitting a new scout army from army #" + army.Id, LogType.Military);
                    WorldMapHex hex_for_army = null;
                    foreach (WorldMapHex adjancent_hex in army.Hex.Get_Adjancent_Hexes()) {
                        if (adjancent_hex.Entity == null && adjancent_hex.Passable) {
                            hex_for_army = adjancent_hex;
                            break;
                        }
                    }
                    if (hex_for_army == null) {
                        Log("None of adjancent hexes are free", LogType.Military);
                        continue;
                    }
                    //TODO: Duplicated splitting code
                    Unit unit = army.Units.First(x => x.Name == cities_training_scout_armies[army.Hex.City].Name);
                    unit.Current_Campaing_Map_Movement -= hex_for_army.Movement_Cost;
                    hex_for_army.Entity = new Army(hex_for_army, Player.Faction.Army_Prototype, Player, unit);
                    cities_training_scout_armies.Remove(army.Hex.City);
                    scouting_armies.Add(hex_for_army.Entity as Army, null);
                } else if (cities_training_defence_armies.ContainsKey(army.Hex.City) && army.Units.Any(x => x.Name == (cities_training_defence_armies[army.Hex.City][0] as Trainable).Name)) {
                    Log("Splitting a new defence army from army #" + army.Id, LogType.Military);
                    WorldMapHex hex_for_army = null;
                    foreach (WorldMapHex adjancent_hex in army.Hex.Get_Adjancent_Hexes()) {
                        if (adjancent_hex.Entity == null && adjancent_hex.Passable) {
                            hex_for_army = adjancent_hex;
                            break;
                        }
                    }
                    if (hex_for_army == null) {
                        Log("None of adjancent hexes are free", LogType.Military);
                        continue;
                    }
                    //TODO: Duplicated splitting code
                    Unit unit = army.Units.First(x => x.Name == (cities_training_defence_armies[army.Hex.City][0] as Trainable).Name);
                    unit.Current_Campaing_Map_Movement -= hex_for_army.Movement_Cost;
                    hex_for_army.Entity = new Army(hex_for_army, Player.Faction.Army_Prototype, Player, unit);
                    cities_training_defence_armies.Remove(army.Hex.City);
                    defence_armies.Add(cities_training_defence_armies[army.Hex.City][1] as City, hex_for_army.Entity as Army);
                } else if (cities_training_main_armies.ContainsKey(army.Hex.City) && army.Units.Any(x => x.Name == cities_training_main_armies[army.Hex.City].Name)) {
                    Log("Splitting a new main army from army #" + army.Id, LogType.Military);
                    WorldMapHex hex_for_army = null;
                    foreach (WorldMapHex adjancent_hex in army.Hex.Get_Adjancent_Hexes()) {
                        if (adjancent_hex.Entity == null && adjancent_hex.Passable) {
                            hex_for_army = adjancent_hex;
                            break;
                        }
                    }
                    if (hex_for_army == null) {
                        Log("None of adjancent hexes are free", LogType.Military);
                        continue;
                    }
                    //TODO: Duplicated splitting code
                    Unit unit = army.Units.First(x => x.Name == cities_training_main_armies[army.Hex.City].Name);
                    unit.Current_Campaing_Map_Movement -= hex_for_army.Movement_Cost;
                    hex_for_army.Entity = new Army(hex_for_army, Player.Faction.Army_Prototype, Player, unit);
                    cities_training_main_armies.Remove(army.Hex.City);
                    main_armies.Add(hex_for_army.Entity as Army, null);
                }
            }
        }

        //Train armies
        //TODO: Finances

        //Defence armies
        foreach (City city in Player.Cities) {
            bool train_more = false;
            if(!cities_training_defence_armies.Any(x => (x.Value[1] as City) == city)) {
                if (!defence_armies.ContainsKey(city)) {
                    Log("Missing defence army for city #" + city.Id, LogType.Military);
                    train_more = true;
                } else if (Largest_Observed_Max_Enemy_Army_Strenght * desired_defence_army_strenght > defence_armies[city].Relative_Strenght) {
                    Log(string.Format("Training more defenders for city #{0} ({1} current str / {2} desired str)", city.Id, defence_armies[city].Relative_Strenght,
                        Largest_Observed_Max_Enemy_Army_Strenght * desired_defence_army_strenght), LogType.Military);
                    train_more = true;
                }
            }

            if (train_more) {
                foreach (City training_city in Player.Cities) {
                    if (training_city.Unit_Under_Production != null) {
                        continue;
                    }
                    Log("-- Evaluating defence unit options from city #" + training_city.Id + " --", LogType.Military);
                    Dictionary<Trainable, float> defence_options = new Dictionary<Trainable, float>();

                    foreach (Trainable unit in training_city.Get_Unit_Options(false)) {
                        if (unit is Worker) {
                            continue;
                        }
                        float preference = base_preference;

                        preference += (unit as Unit).Get_Relative_Strenght_When_On_Hex(training_city.Hex, false, false);
                        preference /= (((unit as Unit).Production_Required + (unit as Unit).Cost + 100.0f) / 200.0f);

                        Log(string.Format("{0} -> preference: {1}", unit.Name, Math.Round(preference, 1)), LogType.Military);
                        defence_options.Add(unit, preference);
                    }

                    if (defence_options.Count == 0) {
                        Log("No options", LogType.Military);
                    } else {
                        List<Trainable> units_by_preference = defence_options.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                        training_city.Change_Production(units_by_preference[0]);
                        cities_training_defence_armies.Add(training_city, new object[2] { units_by_preference[0], city });
                        Log("Training defensive unit: " + units_by_preference[0].Name, LogType.Military);
                        Update_Spectator_View_On_City_Action(training_city, "Training defensive unit: " + units_by_preference[0].Name);
                        break;
                    }
                }
            }
        }

        //Scouting armies
        desired_scouting_army_count = (World.Instance.Map.Width * World.Instance.Map.Height) / 400;
        base_preference = 10.0f;
        if (scouting_armies.Count + cities_training_scout_armies.Count < desired_scouting_army_count) {
            Log(string.Format("Missing scout armies {0} + {1} / {2}", scouting_armies.Count, cities_training_scout_armies.Count, desired_scouting_army_count), LogType.Military);
            foreach (City city in Player.Cities) {
                if (city.Unit_Under_Production != null) {
                    continue;
                }
                Log("-- Evaluating scout options from city #" + city.Id + " --", LogType.Military);
                Dictionary<Trainable, float> scout_options = new Dictionary<Trainable, float>();

                foreach (Trainable unit in city.Get_Unit_Options(false)) {
                    if (unit is Worker || unit is Prospector) {
                        continue;
                    }
                    float preference = base_preference;
                    preference += (unit as Unit).Max_Campaing_Map_Movement;
                    preference += (unit as Unit).LoS;
                    preference /= (unit as Unit).Upkeep;
                    preference /= (((unit as Unit).Production_Required + (unit as Unit).Cost + 100.0f) / 200.0f);
                    Log(string.Format("{0} -> preference: {1}", unit.Name, Math.Round(preference, 1)), LogType.Military);
                    scout_options.Add(unit, preference);
                }

                if (scout_options.Count == 0) {
                    Log("No options", LogType.Military);
                } else {
                    List<Trainable> units_by_preference = scout_options.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                    city.Change_Production(units_by_preference[0]);
                    cities_training_scout_armies.Add(city, units_by_preference[0]);
                    Log("Training scout: " + units_by_preference[0].Name, LogType.Military);
                    Update_Spectator_View_On_City_Action(city, "Training scout: " + units_by_preference[0].Name);
                    break;//TODO: train multiple at once?
                }
            }
        }

        //Main armies
        float desired_strenght = desired_min_main_army_strenght;
        if (Largest_Observed_Max_Enemy_Army_Strenght * desired_army_strenght > desired_strenght) {
            desired_strenght = Largest_Observed_Max_Enemy_Army_Strenght * desired_army_strenght;
        }
        if (own_main_army_strenght < desired_strenght) {
            Log(string.Format("Main army strengh bellow desired strenght: {0} < {1}", Math.Round(own_main_army_strenght, 1), Math.Round(desired_strenght, 1)), LogType.Military);
            foreach (City training_city in Player.Cities) {
                if (training_city.Unit_Under_Production != null) {
                    continue;
                }
                Log("-- Evaluating main unit options from city #" + training_city.Id + " --", LogType.Military);
                Dictionary<Trainable, float> unit_options = new Dictionary<Trainable, float>();

                foreach (Trainable unit in training_city.Get_Unit_Options(false)) {
                    if (unit is Worker) {
                        continue;
                    }
                    float preference = base_preference;

                    preference += (unit as Unit).Relative_Strenght;
                    preference /= (((unit as Unit).Production_Required + (unit as Unit).Cost + 100.0f) / 200.0f);

                    Log(string.Format("{0} -> preference: {1}", unit.Name, Math.Round(preference, 1)), LogType.Military);
                    unit_options.Add(unit, preference);
                }

                if (unit_options.Count == 0) {
                    Log("No options", LogType.Military);
                } else {
                    List<Trainable> units_by_preference = unit_options.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                    training_city.Change_Production(units_by_preference[0]);
                    cities_training_main_armies.Add(training_city, units_by_preference[0]);
                    Log("Training main unit: " + units_by_preference[0].Name, LogType.Military);
                    Update_Spectator_View_On_City_Action(training_city, "Training main unit: " + units_by_preference[0].Name);
                    break;//TODO: Train multiple at once?
                }
            }
        }

        Log(string.Format("Manage empire: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);
    }

    private void Manage_City(City city)
    {
        Log("-- Managing city #" + city.Id + " --", LogType.Economy);

        Stopwatch stopwatch = Stopwatch.StartNew();

        Yields base_yields = new Yields(city.Get_Base_Yields(false));
        Yields total_yields = new Yields(city.Yields);

        //DETERMINE PRIORITIES

        Dictionary<Tag, float> city_priorities = new Dictionary<Tag, float>();
        foreach (Tag tag in Enum.GetValues(typeof(Tag))) {
            city_priorities.Add(tag, 0.0f);
        }

        //Cash
        if (Player.Income <= 0.0f) {
            city_priorities[Tag.Cash] += (10.0f + ((Player.Income * -200.0f) / Player.Cash));
        } else if (Player.Cash < 500.0f) {
            city_priorities[Tag.Cash] += (500.0f - Player.Cash) / 50.0f;
        }

        //Food
        if (total_yields.Food < 0.0f) {
            city_priorities[Tag.Food] = (-2.0f * total_yields.Food) + 10.0f;
        } else if (total_yields.Food < 5.0f) {
            city_priorities[Tag.Food] = (5.0f - total_yields.Food) * 2.0f;
        }

        //Production
        if (total_production < desired_production) {
            city_priorities[Tag.Production] += (desired_production - total_production) / 2.0f;
        }
        float city_prod_priority = (desired_production / Player.Cities.Count) * 0.5f;
        if (base_yields.Production < city_prod_priority && city_prod_priority > city_priorities[Tag.Production]) {
            city_priorities[Tag.Production] = city_prod_priority;
        }

        //----------------------------------------
        //-- TODO: use breakpoints from City.cs --
        //----------------------------------------

        //Happiness
        if (city.Happiness <= -25.0f) {
            city_priorities[Tag.Happiness] = 100.0f;
        } else if (city.Happiness < -10.0f) {
            city_priorities[Tag.Happiness] = -4.0f * city.Happiness;
        } else if (city.Happiness < 0.0f) {
            city_priorities[Tag.Happiness] = (-2.0f * city.Happiness) + 5.0f;
        } else if (city.Happiness < 10.0f) {
            city_priorities[Tag.Happiness] = (10.0f - city.Happiness) + 5.0f;
        }

        //Health
        if (city.Health <= -30.0f) {
            city_priorities[Tag.Health] = 50.0f;
        } else if (city.Health < 0.0f) {
            city_priorities[Tag.Health] = (-1.5f * city.Health) + 5.0f;
        } else if (city.Health < 10.0f) {
            city_priorities[Tag.Health] = (10.0f - city.Health) / 2.0f;
        }

        //Order
        if (city.Order <= -10.0f) {
            city_priorities[Tag.Order] = 75.0f;
        } else if (city.Order < 0.0f) {
            city_priorities[Tag.Order] = -7.5f * city.Order;
        }

        //Military
        float largest_army = Largest_Observed_Max_Enemy_Army_Strenght;
        if (own_main_army_strenght < largest_army * desired_army_strenght) {
            city_priorities[Tag.Military] = (largest_army * desired_army_strenght) - own_main_army_strenght;
        }

        Log("- City priorities -", LogType.Economy);
        foreach (KeyValuePair<Tag, float> pair in city_priorities) {
            priorities[pair.Key] += pair.Value;
            Log(string.Format("{0}: {1}", pair.Key.ToString(), Math.Round(pair.Value, 1)), LogType.Economy);
        }
        city_specific_priorities.Add(city, city_priorities);


        //Train worker
        if (worker_count < desired_workers && city.Unit_Under_Production == null && city.Can_Train(worker_prototype)) {
            city.Change_Production(worker_prototype);
            Log(string.Format("Worker count < desired workers ({0} < {1}) => Training a new worker", worker_count, desired_workers), LogType.Economy);
            Update_Spectator_View_On_City_Action(city, "Training a new worker");
            worker_count++;
        }

        //Train prospector
        if (prospector_count < desired_prospectors && city.Unit_Under_Production == null && city.Can_Train(prospector_prototype)) {
            city.Change_Production(prospector_prototype);
            Log(string.Format("Prospector count < desired prospectors ({0} < {1}) => Training a new prospector", prospector_count, desired_prospectors), LogType.Economy);
            Update_Spectator_View_On_City_Action(city, "Training a new prospector");
            prospector_count++;
        }

        //Select building
        float base_preference = 10.0f;
        if (city.Building_Under_Production == null) {
            Log("- Evaluating building options -", LogType.Economy);
            //TODO: saving money for desired buildings
            Dictionary<Building, float> building_options = new Dictionary<Building, float>();
            foreach (Building building in city.Get_Building_Options(false)) {
                float preference = base_preference;

                //Base yields
                preference += (building.Yields.Food + (building.Yields.Food * city_priorities[Tag.Food]));
                preference += (building.Yields.Production + (building.Yields.Production * city_priorities[Tag.Production]));
                preference += (building.Yields.Cash + (building.Yields.Cash * city_priorities[Tag.Cash]));
                preference += (building.Yields.Science + (building.Yields.Science * city_priorities[Tag.Science]));
                preference += (building.Yields.Culture + (building.Yields.Culture * city_priorities[Tag.Culture]));
                preference += (building.Yields.Mana + (building.Yields.Mana * city_priorities[Tag.Mana]));
                preference += (building.Yields.Faith + (building.Yields.Faith * city_priorities[Tag.Faith]));

                //Percentage yields
                preference += (building.Percentage_Yield_Bonuses.Food + (((building.Percentage_Yield_Bonuses.Food / 100.0f) * base_yields.Food) * city_priorities[Tag.Food]));
                preference += (building.Percentage_Yield_Bonuses.Production + (((building.Percentage_Yield_Bonuses.Production / 100.0f) * base_yields.Production) * city_priorities[Tag.Production]));
                preference += (building.Percentage_Yield_Bonuses.Cash + (((building.Percentage_Yield_Bonuses.Cash / 100.0f) * base_yields.Cash) * city_priorities[Tag.Cash]));
                preference += (building.Percentage_Yield_Bonuses.Science + (((building.Percentage_Yield_Bonuses.Science / 100.0f) * base_yields.Science) * city_priorities[Tag.Science]));
                preference += (building.Percentage_Yield_Bonuses.Culture + (((building.Percentage_Yield_Bonuses.Culture / 100.0f) * base_yields.Culture) * city_priorities[Tag.Culture]));
                preference += (building.Percentage_Yield_Bonuses.Mana + (((building.Percentage_Yield_Bonuses.Mana / 100.0f) * base_yields.Mana) * city_priorities[Tag.Mana]));
                preference += (building.Percentage_Yield_Bonuses.Faith + (((building.Percentage_Yield_Bonuses.Faith / 100.0f) * base_yields.Faith) * city_priorities[Tag.Faith]));

                //Happiness, health & order
                preference += (building.Happiness + (building.Happiness * city_priorities[Tag.Happiness]));
                preference += (building.Base_Happiness_From_Pops_Delta + (building.Base_Happiness_From_Pops_Delta * city.Population * city_priorities[Tag.Happiness]));
                preference += (building.Health + (building.Health * city_priorities[Tag.Health]));
                preference += (building.Base_Health_From_Pops_Delta + (building.Base_Health_From_Pops_Delta * city.Population * city_priorities[Tag.Health]));
                preference += (building.Order + (building.Order * city_priorities[Tag.Order]));
                preference += (building.Base_Order_From_Pops_Delta + (building.Base_Order_From_Pops_Delta * city.Population * city_priorities[Tag.Order]));

                //Misc
                preference += ((building.Food_Storage / 10) + ((building.Food_Storage / 10) * city_priorities[Tag.Food]));
                preference += ((building.Unit_Training_Speed_Bonus * 10.0f) + ((building.Unit_Training_Speed_Bonus * base_yields.Production) * city_priorities[Tag.Production] * city_priorities[Tag.Military]));
                preference += ((building.Building_Constuction_Speed_Bonus * 10.0f) + ((building.Building_Constuction_Speed_Bonus * base_yields.Production) * city_priorities[Tag.Production]));
                preference += 2.0f * (building.Pop_Growth_Additive_Bonus + (building.Pop_Growth_Multiplier_Bonus * city.Pop_Growth));
                preference += ((building.Building_Upkeep_Reduction * 10.0f) + ((building.Building_Upkeep_Reduction * city.Building_Upkeep) * city_priorities[Tag.Cash]));
                preference += ((building.Garrison_Upkeep_Reduction * 10.0f) + ((building.Garrison_Upkeep_Reduction * (defence_armies.ContainsKey(city) ? defence_armies[city].Upkeep : 0.0f)) * city_priorities[Tag.Cash]));

                if (building.Tags != null) {
                    foreach(KeyValuePair<Tag, float> pair in building.Tags) {
                        preference += (pair.Value + (pair.Value * city_priorities[pair.Key]));
                    }
                }

                float production_impact = ((building.Production_Required + 100.0f) / 200.0f);
                float cost_impact = ((building.Cost + 100.0f) / 200.0f);
                preference /= ((production_impact + cost_impact) / 2.0f);

                if (building.Upkeep > Player.Income) {
                    preference *= (float)Math.Pow(0.90f, (building.Upkeep - Player.Income));
                }

                Log(string.Format("{0} -> preference: {1}", building.Name, Math.Round(preference, 1)), LogType.Economy);
                building_options.Add(building, preference);
            }

            if (building_options.Count == 0) {
                Log("No options", LogType.Economy);
            } else {
                List<Building> buildings_by_preference = building_options.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                city.Change_Production(buildings_by_preference[0]);
                Log("Building: " + buildings_by_preference[0].Name, LogType.Economy);
                Update_Spectator_View_On_City_Action(city, "Building: " + buildings_by_preference[0].Name);
            }
        }

        //Reapply worked hexes
        List<WorldMapHex> help_list = new List<WorldMapHex>();
        foreach (WorldMapHex hex in city.Worked_Hexes) {
            help_list.Add(hex);
        }

        foreach (WorldMapHex hex in help_list) {
            city.Unassing_Pop(hex);
        }

        List<WorldMapHex> hexes_that_can_be_worked = city.Hexes_That_Can_Be_Worked;
        Log("- Evaluating hex options -", LogType.Economy);
        base_preference = 10.0f;
        Dictionary<WorldMapHex, float> hex_options = new Dictionary<WorldMapHex, float>();
        foreach (WorldMapHex hex in hexes_that_can_be_worked) {
            float preference = base_preference;

            preference += (hex.Yields.Food + (hex.Yields.Food * city_priorities[Tag.Food]));
            preference += (hex.Yields.Production + (hex.Yields.Production * city_priorities[Tag.Production]));
            preference += (hex.Yields.Cash + (hex.Yields.Cash * city_priorities[Tag.Cash]));
            preference += (hex.Yields.Science + (hex.Yields.Science * city_priorities[Tag.Science]));
            preference += (hex.Yields.Culture + (hex.Yields.Culture * city_priorities[Tag.Culture]));
            preference += (hex.Yields.Mana + (hex.Yields.Mana * city_priorities[Tag.Mana]));
            preference += (hex.Yields.Faith + (hex.Yields.Faith * city_priorities[Tag.Faith]));

            preference += (hex.Happiness + (hex.Happiness * city_priorities[Tag.Happiness]));
            preference += (hex.Health + (hex.Health * city_priorities[Tag.Health]));
            preference += (hex.Order + (hex.Order * city_priorities[Tag.Order]));

            //Log(string.Format("{0} -> preference: {1}", hex.ToString(), Math.Round(preference, 1)));
            hex_options.Add(hex, preference);
        }
        List<WorldMapHex> hexes_by_preference = hex_options.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
        int i = 0;
        Log("- Assigning pops -", LogType.Economy);
        while (city.Unemployed_Pops > 0 && hexes_that_can_be_worked.Count > 0) {
            city.Assing_Pop(hexes_by_preference[i]);
            hexes_that_can_be_worked.Remove(hexes_by_preference[i]);
            Log(string.Format("Assigning pop to {0}", hexes_by_preference[i].ToString()), LogType.Economy);
            i++;
        }
        if (i == 0) {
            Log("Could not assign any pops", LogType.Economy);
        }

        Log(string.Format("Manage city: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);
    }

    private void Log(string message, LogType type)
    {
        if (!Log_Actions || !Logged_Action_Types.Contains(type)) {
            return;
        }
        CustomLogger.Instance.Debug(message);
    }

    /// <summary>
    /// TODO: replace improvements
    /// </summary>
    /// <param name="worker"></param>
    private void Manage_Worker(Worker worker)
    {
        Log("--- Managing worker #" + worker.Id + " ---", LogType.Economy);

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (worker.Stored_Path != null) {
            WorldMapHex last_hex = worker.Stored_Path_Target;
            if (last_hex.Entity != null && !last_hex.Entity.Is_Owned_By(Player)) {
                //TODO: In general be more careful with workers
                Log("Enemy on target hex, canceling move to " + last_hex.ToString(), LogType.Economy);
                worker.Clear_Stored_Path();
                return;
            }
            Log("Following path to " + last_hex, LogType.Economy);
            if (!worker.Follow_Stored_Path()) {
                Log("Path blocked", LogType.Economy);
                worker.Clear_Stored_Path();
                return;
            } else {
                Update_Spectator_View_On_Move(worker);
                last_action_was_visible = worker.Hex.Visible_To_Viewing_Player;
                if(worker.Current_Movement <= 0.0f) {
                    worker.Wait_Turn = true;
                }
            }
            if(worker.Stored_Path == null) {
                Log("Target reached", LogType.Economy);

                City city = null;
                foreach (City c in Player.Cities) {
                    if (c.Hexes_In_Work_Range.Contains(worker.Hex)) {
                        city = c;
                        break;
                    }
                }
                if (city == null) {
                    CustomLogger.Instance.Error("City to which worker's hex belongs to was not found");
                    worker.Wait_Turn = true;
                    return;
                }

                Log("- Evaluating improvement options -", LogType.Economy);
                
                Dictionary<Improvement, float> improvement_options = new Dictionary<Improvement, float>();
                foreach (Improvement improvement in worker.Currently_Buildable_Improvements) {
                    float preference = Calculate_Improvement_Preference(improvement, worker.Hex, city);
                    Log(string.Format("{0} -> preference: {1}", improvement.Name, Math.Round(preference, 1)), LogType.Economy);
                    improvement_options.Add(improvement, preference);
                }

                if (improvement_options.Count == 0) {
                    Log("No options", LogType.Economy);
                    worker.Wait_Turn = true;
                } else {
                    List<Improvement> improvements_by_preference = improvement_options.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
                    if(worker.Hex.Improvement != null && worker.Hex.Improvement.Name == improvements_by_preference[0].Name) {
                        Log("Hex already has prefered improvement: " + improvements_by_preference[0].Name, LogType.Economy);
                        worker.Wait_Turn = true;
                    } else {
                        worker.Start_Construction(improvements_by_preference[0]);
                        Log("Building improvement: " + improvements_by_preference[0].Name, LogType.Economy);
                    }
                }
            }
            return;
        }

        Log("- Evaluating hex options -", LogType.Economy);
        List<WorldMapHex> hexes_to_be_evaluated = new List<WorldMapHex>();
        foreach (City city in Player.Cities) {
            foreach (WorldMapHex hex in city.Hexes_In_Work_Range) {
                if ((hex.Entity == null || hex.Entity.Is_Owned_By(Player) && hex.Civilian == null) && hex.Passable && worker.Get_Buildable_Improvements(hex).Count != 0
                    && !Player.WorldMapEntitys.Any(x => x.Is_Civilian && x.Stored_Path_Target == hex)) {
                    hexes_to_be_evaluated.Add(hex);
                }
            }
        }

        Dictionary<WorldMapHex, float> hexes_and_preferences = new Dictionary<WorldMapHex, float>();
        Dictionary<WorldMapHex, List<PathfindingNode>> hexes_and_paths = new Dictionary<WorldMapHex, List<PathfindingNode>>();
        foreach (WorldMapHex hex in hexes_to_be_evaluated) {
            List<PathfindingNode> path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(worker), worker.Hex.Get_Specific_PathfindingNode(worker),
                hex.Get_Specific_PathfindingNode(worker));
            if(path.Count == 0) {
                continue;
            }

            City city = null;
            foreach (City c in Player.Cities) {
                if (c.Hexes_In_Work_Range.Contains(hex)) {
                    city = c;
                    break;
                }
            }
            if (city == null) {
                CustomLogger.Instance.Error("City to which hex that needs evaluating belongs to was not found");
                continue;
            }

            float largest_preference = 0;
            foreach (Improvement improvement in worker.Get_Buildable_Improvements(hex)) {
                float preference = Calculate_Improvement_Preference(improvement, hex, city);
                if(hex.Improvement != null) {
                    preference -= Calculate_Improvement_Preference(hex.Improvement, hex, city);
                }
                if(preference > largest_preference) {
                    largest_preference = preference;
                }
            }
            if(largest_preference > 0.0f) {
                Log(string.Format("{0} -> preference: {1}", hex.ToString(), Math.Round(largest_preference, 1)), LogType.Economy);
                if (hexes_and_preferences.ContainsKey(hex)) {
                    //TODO: Cities share workable tiles?
                    hexes_and_preferences[hex] = largest_preference;
                } else {
                    hexes_and_preferences.Add(hex, largest_preference);
                }
                hexes_and_paths.Add(hex, path);
            }
        }

        if(hexes_and_preferences.Count == 0) {
            Log("No hexes needing improvements", LogType.Economy);
            worker.Wait_Turn = true;
            return;
        }
        
        List<WorldMapHex> hexes_by_preference = hexes_and_preferences.OrderByDescending(x => x.Value).Select(x => x.Key).ToList();
        Log("Sending worker to hex: " + hexes_by_preference[0].ToString(), LogType.Economy);
        worker.Create_Stored_Path(hexes_and_paths[hexes_by_preference[0]]);
        Log(string.Format("Manage worker: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);
    }

    private float Calculate_Improvement_Preference(Improvement improvement, WorldMapHex hex, City city)
    {
        float base_preference = 10.0f;
        float preference = base_preference;

        if (!city_specific_priorities.ContainsKey(city)) {
            //TODO: City was captured?
            return 0.0f;
        }

        Improvement preview = improvement.Preview(hex, Player);

        preference += (preview.Yields.Food + (preview.Yields.Food * city_specific_priorities[city][Tag.Food]));
        preference += (preview.Yields.Production + (preview.Yields.Production * city_specific_priorities[city][Tag.Production]));
        preference += (preview.Yields.Cash + (preview.Yields.Cash * city_specific_priorities[city][Tag.Cash]));
        preference += (preview.Yields.Science + (preview.Yields.Science * city_specific_priorities[city][Tag.Science]));
        preference += (preview.Yields.Culture + (preview.Yields.Culture * city_specific_priorities[city][Tag.Culture]));
        preference += (preview.Yields.Mana + (preview.Yields.Mana * city_specific_priorities[city][Tag.Mana]));
        preference += (preview.Yields.Faith + (preview.Yields.Faith * city_specific_priorities[city][Tag.Faith]));

        preference += (preview.Happiness + (preview.Happiness * city_specific_priorities[city][Tag.Happiness]));
        preference += (preview.Health + (preview.Health * city_specific_priorities[city][Tag.Health]));
        preference += (preview.Order + (preview.Order * city_specific_priorities[city][Tag.Order]));

        return preference;
    }

    private void Manage_Prospector(Prospector prospector)
    {
        Log("--- Managing prospector #" + prospector.Id + " ---", LogType.Economy);

        Stopwatch stopwatch = Stopwatch.StartNew();

        if (hexes_needing_prospecting.Count == 0) {
            //Since we are deleting prospectors, this should not happen
            Log("No hexes needind prospecting", LogType.Economy);
            prospector.Wait_Turn = true;
            return;
        }

        if (hexes_needing_prospecting.Contains(prospector.Hex)) {
            Log("Prospecting hex " + prospector.Hex.ToString(), LogType.Economy);
            prospector.Prospecting = true;
            last_action_was_visible = true;
            return;
        }

        Dictionary<WorldMapHex, int> hexes_and_distances = new Dictionary<WorldMapHex, int>();
        Dictionary<WorldMapHex, List<PathfindingNode>> hexes_and_paths = new Dictionary<WorldMapHex, List<PathfindingNode>>();

        foreach (WorldMapHex hex in hexes_needing_prospecting) {
            List<PathfindingNode> path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(prospector), prospector.Hex.Get_Specific_PathfindingNode(prospector),
                hex.Get_Specific_PathfindingNode(prospector));
            if (path.Count != 0) {
                hexes_and_distances.Add(hex, path.Count);
                hexes_and_paths.Add(hex, path);
            }
        }

        if (hexes_and_distances.Count == 0) {
            Log("Could not find path to any hex that needs prospecting", LogType.Economy);
            prospector.Wait_Turn = true;
            last_action_was_visible = false;
            return;
        }

        List<WorldMapHex> hexes_sorted_by_distance = hexes_and_distances.OrderBy(x => x.Value).Select(x => x.Key).ToList();

        Log("Moving prospector towards hex " + hexes_sorted_by_distance[0].ToString(), LogType.Economy);
        if (!prospector.Move(World.Instance.Map.Get_Hex_At(hexes_and_paths[hexes_sorted_by_distance[0]][1].Coordinates))) {
            prospector.Wait_Turn = true;
        }
        last_action_was_visible = prospector.Hex.Visible_To_Viewing_Player;
        Update_Spectator_View_On_Move(prospector);
        Log(string.Format("Manage prospector: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);
    }

    private void Manage_Scout_Army(Army army)
    {
        Log("--- Managing scout army #" + army.Id + " ---", LogType.Military);

        //TODO: if turns_since_army_was_scouted > careful turns -> rescout

        Stopwatch stopwatch = Stopwatch.StartNew();
        List<PathfindingNode> path = null;

        foreach (City city in undefended_enemy_cities) {
            List<PathfindingNode> path_to_city = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army),
                city.Hex.Get_Specific_PathfindingNode(army));
            if (path_to_city.Count != 0 && path_to_city.Count <= army.Max_Movement * scout_attack_undefended_city_range_multiplier) {
                Log("Attacking undefended city #" + city.Id, LogType.Military);
                path = path_to_city;
                scouting_armies[army] = city.Hex;
                break;
            }
        }

        if (path == null) {
            //TODO: re-evaluate if target moved
            foreach (WorldMapEntity civilian in undefended_enemy_civilians) {
                List<PathfindingNode> path_to_civilian = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army),
                    civilian.Hex.Get_Specific_PathfindingNode(army));
                if (path_to_civilian.Count != 0 && path_to_civilian.Count <= army.Max_Movement * scout_attack_undefended_civilian_range_multiplier) {
                    Log("Attacking undefended civilian #" + civilian.Id, LogType.Military);
                    path = path_to_civilian;
                    scouting_armies[army] = civilian.Hex;
                    break;
                }
            }
        }

        if (scouting_armies[army] == null) {
            WorldMapHex target_hex = World.Instance.Map.Random_Hex;
            path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army), target_hex.Get_Specific_PathfindingNode(army));
            int max_iterations = 1000;
            int iteration = 0;
            while (target_hex.Is_Explored_By(Player) && path.Count != 0) {
                target_hex = World.Instance.Map.Random_Hex;
                path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army), target_hex.Get_Specific_PathfindingNode(army));
                iteration++;
                if (iteration > max_iterations) {
                    Log("Could not find unexplored hex", LogType.Military);
                    break;
                }
            }
            while (path.Count == 0) {
                target_hex = World.Instance.Map.Random_Hex;
                path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army), target_hex.Get_Specific_PathfindingNode(army));
            }

            Log("New target assigned: " + target_hex.ToString(), LogType.Military);
            scouting_armies[army] = target_hex;
        }

        Log("Moving scout army towards hex " + scouting_armies[army].ToString(), LogType.Military);
        if (path == null) {
            path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army), scouting_armies[army].Get_Specific_PathfindingNode(army));
            if (path.Count == 0) {
                Log("Path blocked", LogType.Military);
                scouting_armies[army] = null;
                last_action_was_visible = false;
                return;
            }
        }
        if (!army.Move(World.Instance.Map.Get_Hex_At(path[1].Coordinates))) {
            army.Wait_Turn = true;
        } else if (army.Hex == scouting_armies[army]) {
            Log("Destination reached: " + army.Hex.ToString(), LogType.Military);
            scouting_armies[army] = null;
        }
        Update_Scouting();
        Update_Spectator_View_On_Move(army);
        last_action_was_visible = army.Hex.Visible_To_Viewing_Player;
        Log(string.Format("Manage scout army: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);
    }

    private void Manage_Defence_Army(Army army, City city)
    {
        Log("--- Managing defence army #" + army.Id + " ---", LogType.Military);
        if (army.Hex == city.Hex) {
            army.Wait_Turn = true;
            last_action_was_visible = false;
            return;
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        Log("Moving defence army towards it's city #" + city.Id, LogType.Military);
        List<PathfindingNode> path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army), city.Hex.Get_Specific_PathfindingNode(army));
        if (path.Count == 0) {
            Log("Path blocked", LogType.Military);
            army.Wait_Turn = true;
            last_action_was_visible = false;
            return;
        }
        if (!army.Move(World.Instance.Map.Get_Hex_At(path[1].Coordinates))) {
            army.Wait_Turn = true;
        }
        Update_Scouting();
        Update_Spectator_View_On_Move(army);
        last_action_was_visible = army.Hex.Visible_To_Viewing_Player;
        Log(string.Format("Manage defence army: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);
    }

    private void Manage_Main_Army(Army army)
    {
        Log("--- Managing main army #" + army.Id + " ---", LogType.Military);

        if(army.Current_Movement <= 0.0f) {
            Log("No movement left", LogType.Military);
            army.Wait_Turn = true;
            //No return?
        }

        Stopwatch stopwatch = Stopwatch.StartNew();

        //Attack adjancent enemies
        if (main_armies[army] == null || main_armies[army].Type != ArmyOrder.OrderType.Defend_City) {
            foreach(Army enemy in army.Adjancent_Enemies) {
                if(enemy.Get_Relative_Strenght_When_On_Hex(enemy.Hex, true, false) * (1.0f + army_attack_carefulness) <
                        army.Get_Relative_Strenght_When_On_Hex(enemy.Hex, true, true)) {
                    Log("Attacking enemy army #" + enemy.Id, LogType.Military);
                    army.Attack(enemy);
                    //TODO: delete?
                    //TODO: reduce observed str after battle
                    last_action_was_visible = false;
                    return;
                }
            }
        }


        //Assingning order: priority = most important first

        //Merging
        if(main_armies[army] == null && main_armies.Count > desired_main_army_count) {
            Army weakest_main_army = null;
            for(int i = 0; i < main_armies.Count; i++) {
                if(main_armies.ElementAt(i).Key == army) {
                    continue;
                }
                if(weakest_main_army == null || (main_armies.ElementAt(i).Key.Relative_Strenght < weakest_main_army.Relative_Strenght &&
                        main_armies.ElementAt(i).Key.Max_Size >= (main_armies.ElementAt(i).Key.Units.Count + army.Units.Count))) {
                    weakest_main_army = main_armies.ElementAt(i).Key;
                }
            }
            if(weakest_main_army != null) {
                Log("New order: merge with army #" + weakest_main_army.Id, LogType.Military);
                main_armies[army] = new ArmyOrder(ArmyOrder.OrderType.Merge, weakest_main_army);
            } else {
                Log("Could not find army to merge to", LogType.Military);
            }
        }

        //Attacking enemy cities
        if (main_armies[army] == null) {
            foreach(City city in scouted_enemy_cities) {
                if (!observed_max_enemy_army_strenght.ContainsKey(city.Owner) || !turns_since_army_was_scouted.ContainsKey(city.Owner)) {
                    //TODO: Scouting might not be updated yet at this point?
                    continue;
                }
                int enemy_city_count = 0;
                foreach(City c in scouted_enemy_cities) {
                    if (c.Is_Owned_By(city.Owner)) {
                        enemy_city_count++;
                    }
                }
                float own_strenght = army.Get_Relative_Strenght_When_On_Hex(city.Hex, true, true) * aggressiveness;
                float enemy_strenght = observed_max_enemy_army_strenght[city.Owner] / enemy_city_count;
                if(turns_since_army_was_scouted[city.Owner] > careful_after_turns) {
                    own_strenght *= carefulness_strenght_multiplier;
                }

                if (own_strenght > enemy_strenght) {
                    Log(string.Format("New order: attack enemy city #{0} ({1} vs {2} str)", city.Id, Math.Round(own_strenght, 1), Math.Round(enemy_strenght, 1)), LogType.Military);
                    main_armies[army] = new ArmyOrder(ArmyOrder.OrderType.Attack_City, city.Hex);
                    break;
                }
            }
        }

        //Defending civilians
        if (main_armies[army] == null && army.Hex.Civilian == null) {
            foreach(WorldMapEntity entity in Player.WorldMapEntitys) {
                if (entity.Is_Civilian && entity.Hex.Entity == null && !main_armies.Any(x => x.Value != null && x.Value.Hex_Target == entity.Hex)) {
                    Log("New order: defending civilian", LogType.Military);
                    main_armies[army] = new ArmyOrder(ArmyOrder.OrderType.Defend_Civilian, entity.Hex);
                    break;
                }
            }
        }
        
        //Moving out of cities
        if (main_armies[army] == null && army.Hex.City != null) {
            Log("New order: Moving out of city", LogType.Military);
            WorldMapHex closest_empty_hex = army.Hex.Find_Closest_Hex(delegate(WorldMapHex hex) { return hex.Entity == null && hex.Passable; });
            if(closest_empty_hex == null) {
                CustomLogger.Instance.Warning("Could not find an empty hex to move to");
                army.Wait_Turn = true;
                last_action_was_visible = false;
                return;
            }
            main_armies[army] = new ArmyOrder(ArmyOrder.OrderType.Move_To, closest_empty_hex);
        }
        
        //Carry out order
        if(main_armies[army] == null) {
            Log("No orders", LogType.Military);
            army.Wait_Turn = true;
            last_action_was_visible = false;
            return;
        }

        List<PathfindingNode> path = null;

        if (main_armies[army].Type == ArmyOrder.OrderType.Move_To) {
            Log("Following orders to move to " + main_armies[army].Hex_Target.ToString(), LogType.Military);
            path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army), main_armies[army].Hex_Target.Get_Specific_PathfindingNode(army));
        } else if(main_armies[army].Type == ArmyOrder.OrderType.Defend_Civilian) {
            Log("Following orders to defend civilian at " + main_armies[army].Hex_Target.ToString(), LogType.Military);
            if (main_armies[army].Hex_Target.Civilian == null) {
                Log("Civilian moved, canceling order", LogType.Military);
                main_armies[army] = null;
                last_action_was_visible = false;
                return;
            }
            if(army.Hex == main_armies[army].Hex_Target) {
                Log("Target reached", LogType.Military);
                main_armies[army] = null;
                army.Wait_Turn = true;
                last_action_was_visible = false;
                return;
            }
            path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army), army.Hex.Get_Specific_PathfindingNode(army), main_armies[army].Hex_Target.Get_Specific_PathfindingNode(army));
        } else if(main_armies[army].Type == ArmyOrder.OrderType.Merge) {
            Log("Following orders to merge with army #" + main_armies[army].Army_Target.Id, LogType.Military);
            if (main_armies[army].Army_Target.Max_Size < main_armies[army].Army_Target.Units.Count + army.Units.Count) {
                Log("Canceling order, target army does not have enough space", LogType.Military);
                main_armies[army] = null;
                last_action_was_visible = false;
                return;
            }
            if (army.Hex.Is_Adjancent_To(main_armies[army].Army_Target.Hex)) {
                foreach(Unit u in army.Units) {
                    u.Current_Campaing_Map_Movement -= main_armies[army].Army_Target.Hex.Movement_Cost;
                    main_armies[army].Army_Target.Units.Add(u);
                }
                WorldMapHex help_hex = main_armies[army].Army_Target.Hex;
                army.Delete();
                main_armies.Remove(army);
                Log("Merge done", LogType.Military);
                last_action_was_visible = help_hex.Visible_To_Viewing_Player;
                return;
            }
            path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army, true, main_armies[army].Army_Target.Hex),
                army.Hex.Get_Specific_PathfindingNode(army, main_armies[army].Army_Target.Hex), main_armies[army].Army_Target.Hex.Get_Specific_PathfindingNode(army, main_armies[army].Army_Target.Hex));
        } else if(main_armies[army].Type == ArmyOrder.OrderType.Attack_City) {
            Log("Following orders to attack city #" + main_armies[army].Hex_Target.City.Id, LogType.Military);
            if(main_armies[army].Hex_Target.Current_LoS == WorldMapHex.LoS_Status.Visible && main_armies[army].Hex_Target.Entity != null &&
                    main_armies[army].Hex_Target.Entity is Army && ((main_armies[army].Hex_Target.Entity as Army).Current_Relative_Strenght * (1.0f + city_attack_carefulness)) > army.Current_Relative_Strenght) {
                Log("City is too well defended, aborting attack order", LogType.Military);
                main_armies[army] = null;
                last_action_was_visible = false;
                return;
            }
            path = Pathfinding.Path(World.Instance.Map.Get_Specific_PathfindingNodes(army, true, main_armies[army].Hex_Target),
                army.Hex.Get_Specific_PathfindingNode(army, main_armies[army].Hex_Target), main_armies[army].Hex_Target.Get_Specific_PathfindingNode(army, main_armies[army].Hex_Target));
        }

        if(path == null) {
            CustomLogger.Instance.Warning("No implementation for: " + main_armies[army].Type.ToString());
            army.Wait_Turn = true;
            last_action_was_visible = false;
            return;
        }
        
        if (path.Count == 0) {
            Log("Path blocked", LogType.Military);
            army.Wait_Turn = true;
            last_action_was_visible = false;
            return;
        }
        if (!army.Move(World.Instance.Map.Get_Hex_At(path[1].Coordinates))) {
            army.Wait_Turn = true;
        }
        Update_Scouting();
        Update_Spectator_View_On_Move(army);
        last_action_was_visible = army.Hex.Visible_To_Viewing_Player;
        Log(string.Format("Manage main army: {0} ms", stopwatch.ElapsedMilliseconds), LogType.Diagnostic);
    }

    /// <summary>
    /// TODO: rename?
    /// </summary>
    /// <param name="entity"></param>
    private void Update_Spectator_View_On_Move(WorldMapEntity entity)
    {
        if ((Show_Moves || entity.Hex.Visible_To_Viewing_Player) && Follow_Moves) {
            BottomGUIManager.Instance.Current_Entity = entity;
            CameraManager.Instance.Set_Camera_Location(entity.Hex);
        }
    }

    private void Update_Spectator_View_On_City_Action(City city, string action)
    {
        if (Show_Moves && Follow_Moves) {
            spectator_city_action_list.Add(new object[2] { city, action });
        }
    }

    private class ArmyOrder
    {
        public enum OrderType { Move_To, Merge, Defend_Civilian, Defend_City, Attack_City, Attack_Army, Attack_Civilian }

        public OrderType Type { get; set; }
        public WorldMapHex Hex_Target { get; set; }
        public Army Army_Target { get; set; }

        public ArmyOrder(OrderType type, WorldMapHex hex_target)
        {
            Type = type;
            Hex_Target = hex_target;
            Army_Target = null;
        }

        public ArmyOrder(OrderType type, Army army_target)
        {
            Type = type;
            Hex_Target = null;
            Army_Target = army_target;
        }
    }

    public void Combat_Act(float delta_s)
    {
        if (!CombatManager.Instance.Active_Combat) {
            CustomLogger.Instance.Error("There is no combat going on");
            return;
        }
        act_cooldown -= delta_s;
        if (act_cooldown > 0.0f) {
            return;
        }
        act_cooldown += Time_Between_Actions;

        Unit unit = null;
        Army army = CombatManager.Instance.Army_1.Is_Owned_By(Player) ? CombatManager.Instance.Army_1 : CombatManager.Instance.Army_2;
        Army enemy_army = CombatManager.Instance.Army_1.Is_Owned_By(Player) ? CombatManager.Instance.Army_2 : CombatManager.Instance.Army_1;

        if (CombatManager.Instance.Deployment_Mode) {
            foreach (Unit u in army.Units) {
                CombatMapHex hex = CombatManager.Instance.Map.Random_Hex;
                while (!u.Deploy(hex)) {
                    hex = CombatManager.Instance.Map.Random_Hex;
                }
            }
            CombatManager.Instance.Next_Turn();
            return;
        }

        foreach (Unit u in army.Units) {
            if (u.Controllable && !u.Wait_Turn) {
                unit = u;
                break;
            }
        }

        if (unit == null) {
            CombatManager.Instance.Next_Turn();
            return;
        }

        if (CombatUIManager.Instance.Active) {
            CombatUIManager.Instance.Current_Unit = unit;
        }

        //Zombie AI
        //Attacking
        List<CombatMapHex> hexes_in_range = unit.Hex.Get_Adjancent_Hexes();
        if(unit.Can_Ranged_Attack) {
            hexes_in_range = unit.Get_Hexes_In_Attack_Range();
        }
        foreach(CombatMapHex hex in hexes_in_range) {
            if(hex.Unit != null && !hex.Unit.Army.Is_Owned_By(Player)) {
                if(unit.Attack(hex.Unit, false) != null) {
                    if (CombatUIManager.Instance.Active) {
                        CombatUIManager.Instance.Update_Current_Unit();
                    }
                    break;
                }
            }
        }
        if(!unit.Can_Attack && unit.Current_Movement <= 0.0f) {
            unit.Wait_Turn = true;
            return;
        }

        //Moving
        Unit closest_enemy = null;
        int closest_distance = -1;
        foreach(Unit enemy in enemy_army.Units) {
            if(closest_distance == -1 || (enemy.Hex.Distance(unit.Hex) < closest_distance)) {
                closest_enemy = enemy;
                closest_distance = enemy.Hex.Distance(unit.Hex);
            }
        }
        if (closest_enemy.Hex.Is_Adjancent_To(unit.Hex)) {
            unit.Wait_Turn = true;
            return;
        }
        if(!unit.Pathfind(closest_enemy.Hex, unit.Relative_Stamina >= 0.75f)) {
            //Path blocked
            unit.Wait_Turn = true;
        }
        if (CombatUIManager.Instance.Active) {
            CombatUIManager.Instance.Update_Current_Unit();
        }
    }
}
