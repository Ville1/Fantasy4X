using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CityGUIManager : MonoBehaviour {
    private bool SHOW_HIDDEN_TRADE_ROUTES = true;//As question marks

    public static CityGUIManager Instance;

    private City current_city;

    public GameObject Name_Panel;
    public Text Name_Text;
    public Button Previous_Button;
    public Button Next_Button;

    public GameObject Building_Production_Panel;
    public Image Building_Under_Production_Image;
    public Text Building_Under_Production_Name_Text;
    public Text Building_Under_Production_Progress_Text;
    public Text Building_Under_Production_Turns_Text;
    public Image Building_Under_Production_Production_Image;

    public GameObject Unit_Production_Panel;
    public Image Unit_Under_Production_Image;
    public Text Unit_Under_Production_Name_Text;
    public Text Unit_Under_Production_Progress_Text;
    public Text Unit_Under_Production_Turns_Text;
    public Image Unit_Under_Production_Production_Image;

    public GameObject Select_Unit_Panel;
    public GameObject Select_Unit_Content;
    public GameObject Select_Unit_Row_Prototype;

    public GameObject Select_Building_Panel;
    public Button Select_Building_Panel_Up_Button;
    public Button Select_Building_Panel_Down_Button;

    public GameObject Info_Panel;

    public GameObject Culture_Panel;
    public Text Culture_Own_Text;
    public Text Culture_Range_Text;
    public Text Culture_Out_Text;
    public Text Culture_In_Text;

    public Text City_Size_Text;
    public Text City_Size_Progress_Text;

    public Text Food_Text;
    public Text Production_Text;
    public Text Cash_Text;
    public Text Science_Text;
    public Text Culture_Text;
    public Text Mana_Text;
    public Text Faith_Text;
    public Text Growth_Text;

    public Text Happiness_Text;
    public Image Happiness_Image;
    public Text Health_Text;
    public Image Health_Image;
    public Text Order_Text;
    public Image Order_Image;

    public Text Food_Storage_Text;

    public GameObject Unemployed_Panel;
    public Text Unemployed_Text;

    public GameObject Buildings_Panel;
    public Button Buildings_Panel_Up_Button;
    public Button Buildings_Panel_Down_Button;

    public GameObject Trade_Panel;

    public Color Cant_Be_Worked_Highlight;
    public Color Worked_Hex_Highlight;
    
    private List<GameObject> building_game_objects;
    private List<GameObject> building_list_game_objects;
    private int building_list_offset;
    private int select_building_list_offset;
    private List<WorldMapHex> highlighted_hexes;
    private List<GameObject> trade_route_gos;
    private RowScrollView<Trainable> unit_selection_options;

    /// <summary>
    /// Initializiation
    /// </summary>
    private void Start()
    {
        if (Instance != null) {
            CustomLogger.Instance.Error(LogMessages.MULTIPLE_INSTANCES);
            return;
        }
        Instance = this;

        Name_Panel.SetActive(false);
        Building_Production_Panel.SetActive(false);
        Unit_Production_Panel.SetActive(false);
        Info_Panel.SetActive(false);
        Select_Unit_Panel.SetActive(false);
        Select_Building_Panel.SetActive(false);
        Unemployed_Panel.SetActive(false);
        Buildings_Panel.SetActive(false);
        Culture_Panel.SetActive(false);
        Trade_Panel.SetActive(false);

        highlighted_hexes = new List<WorldMapHex>();
        unit_selection_options = new RowScrollView<Trainable>("unit_selection_options", Select_Unit_Content, Select_Unit_Row_Prototype, 20.0f);
    }

    /// <summary>
    /// Per frame update
    /// </summary>
    private void Update()
    { }

    public bool Active
    {
        get {
            return Building_Production_Panel.activeSelf;
        }
        set {
            if((!value && !Active) || Main.Instance.Other_Players_Turn) {
                return;
            }
            Name_Panel.SetActive(value);
            Building_Production_Panel.SetActive(value);
            Unit_Production_Panel.SetActive(value);
            Info_Panel.SetActive(value);
            Buildings_Panel.SetActive(value);
            Select_Unit_Panel.SetActive(false);
            Select_Building_Panel.SetActive(false);
            Unemployed_Panel.SetActive(false);
            Culture_Panel.SetActive(value);
            Trade_Panel.SetActive(value);
            ScoresGUIManager.Instance.Active = !Active;
            //MenuManager.Instance.Active = !Active;
            if (Main.Instance.Game_Is_Running) {
                BottomGUIManager.Instance.Active = !Active;
            }
            if (Active) {
                HexPanelManager.Instance.Active = false;
                SelectTechnologyPanelManager.Instance.Active = false;
                CameraManager.Instance.Set_Camera_Location(Current_City.Hex);
                CameraManager.Instance.Set_City_Zoom(true);
                
                building_list_offset = 0;
                select_building_list_offset = 0;
                if (building_list_game_objects == null) {
                    building_list_game_objects = new List<GameObject>();
                    int i = 1;
                    while (true) {
                        GameObject go = GameObject.Find(Buildings_Panel.name + "/Building" + i);
                        if(go == null) {
                            break;
                        }
                        building_list_game_objects.Add(go);
                        i++;
                    }
                }
                Update_Hex_Highlights();
            } else {
                TooltipManager.Instance.Unregister_Tooltips_By_Owner(gameObject);
                foreach(WorldMapHex hex in highlighted_hexes) {
                    hex.Clear_Highlight();
                }
                highlighted_hexes.Clear();
                UnitInfoGUIManager.Instance.Active = false;
            }
            if(World.Instance.Map != null) {
                World.Instance.Map.Map_Mode = Active ? WorldMapHex.InfoText.Yields :
                    (BottomGUIManager.Instance.Current_Entity is Prospector && !Main.Instance.Other_Players_Turn ? WorldMapHex.InfoText.Minerals : WorldMapHex.InfoText.None);
            }
            CameraManager.Instance.Lock_Camera = Active;
            CameraManager.Instance.Set_City_Zoom(false);
        }
    }

    public City Current_City
    {
        get {
            return current_city;
        }
        set {
            current_city = value;
            if(current_city == null) {
                Active = false;
                return;
            }
            Active = true;
            Update_GUI();
        }
    }

    private void Update_GUI()
    {
        //Bottom GUI Manager stuff
        if (BottomGUIManager.Instance.Current_Entity != null && BottomGUIManager.Instance.Current_Entity is Worker) {
            (BottomGUIManager.Instance.Current_Entity as Worker).Update_Actions_List();
            BottomGUIManager.Instance.Update_Entity_Info();
        }

        World.Instance.Map.Map_Mode = World.Instance.Map.Map_Mode;
        Name_Text.text = current_city.Name;
        Previous_Button.interactable = current_city.Owner.Cities.Count != 1;
        Next_Button.interactable = current_city.Owner.Cities.Count != 1;

        //Unit training
        if (current_city.Unit_Under_Production != null) {
            Unit_Under_Production_Image.gameObject.SetActive(true);
            Unit_Under_Production_Image.overrideSprite = SpriteManager.Instance.Get(current_city.Unit_Under_Production.Texture, SpriteManager.SpriteType.Unit);
            Unit_Under_Production_Name_Text.text = current_city.Unit_Under_Production.Name;
            Unit_Under_Production_Progress_Text.text = string.Format("{0} / {1}", Math.Round(current_city.Unit_Production_Acquired, 1), current_city.Unit_Under_Production.Production_Required);
            Unit_Under_Production_Turns_Text.text = string.Format("{0} turns", current_city.Unit_Under_Production_Turns_Left > 0 ? current_city.Unit_Under_Production_Turns_Left.ToString() : "N/A");
            Unit_Under_Production_Production_Image.gameObject.SetActive(true);
            TooltipManager.Instance.Register_Tooltip(Unit_Under_Production_Image.gameObject, current_city.Unit_Under_Production.Tooltip, gameObject);
            TooltipManager.Instance.Register_Tooltip(Unit_Under_Production_Name_Text.gameObject, current_city.Unit_Under_Production.Tooltip, gameObject);
        } else {
            Unit_Under_Production_Image.gameObject.SetActive(false);
            Unit_Under_Production_Name_Text.text = "Nothing";
            Unit_Under_Production_Progress_Text.text = "";
            Unit_Under_Production_Turns_Text.text = "";
            Unit_Under_Production_Production_Image.gameObject.SetActive(false);
            TooltipManager.Instance.Unregister_Tooltip(Unit_Under_Production_Image.gameObject);
            TooltipManager.Instance.Unregister_Tooltip(Unit_Under_Production_Name_Text.gameObject);
        }
        //Building construction
        if (current_city.Building_Under_Production != null) {
            Building_Under_Production_Image.gameObject.SetActive(true);
            Building_Under_Production_Image.overrideSprite = SpriteManager.Instance.Get(current_city.Building_Under_Production.Texture, SpriteManager.SpriteType.Building);
            Building_Under_Production_Name_Text.text = current_city.Building_Under_Production.Name;
            Building_Under_Production_Progress_Text.text = string.Format("{0} / {1}", Math.Round(current_city.Building_Production_Acquired, 1), current_city.Building_Under_Production.Production_Required);
            Building_Under_Production_Turns_Text.text = string.Format("{0} turns", current_city.Building_Under_Production_Turns_Left > 0 ? current_city.Building_Under_Production_Turns_Left.ToString() : "N/A");
            Building_Under_Production_Production_Image.gameObject.SetActive(true);
            TooltipManager.Instance.Register_Tooltip(Building_Under_Production_Image.gameObject, current_city.Building_Under_Production.Tooltip, gameObject);
            TooltipManager.Instance.Register_Tooltip(Building_Under_Production_Name_Text.gameObject, current_city.Building_Under_Production.Tooltip, gameObject);
        } else {
            Building_Under_Production_Image.gameObject.SetActive(false);
            Building_Under_Production_Name_Text.text = "Nothing";
            Building_Under_Production_Progress_Text.text = "";
            Building_Under_Production_Turns_Text.text = "";
            Building_Under_Production_Production_Image.gameObject.SetActive(false);
            TooltipManager.Instance.Unregister_Tooltip(Building_Under_Production_Image.gameObject);
            TooltipManager.Instance.Unregister_Tooltip(Building_Under_Production_Name_Text.gameObject);
        }

        //Info
        //City Size
        City_Size_Text.text = Current_City.Size.ToString();
        TooltipManager.Instance.Register_Tooltip(City_Size_Text.gameObject, Player.Faction.City_Yields[Current_City.Size].ToString(), gameObject);
        KeyValuePair<City.CitySize, int>? next_size = Current_City.Population_Required_For_Next_City_Size();
        City_Size_Progress_Text.text = next_size != null ? string.Format("{0} / {1}", Current_City.Population, next_size.Value.Value) : Current_City.Population.ToString();
        if(next_size != null) {
            TooltipManager.Instance.Register_Tooltip(City_Size_Progress_Text.gameObject, string.Format("Current population: {0}{1}Population required for {2}: {3}", Current_City.Population, Environment.NewLine,
                next_size.Value.Key.ToString(), next_size.Value.Value.ToString()), gameObject);
        }

        //Yields
        Food_Text.text = Helper.Float_To_String(Current_City.Yields.Food, 1, true);
        TooltipManager.Instance.Register_Tooltip(Food_Text.gameObject, Current_City.Statistics.Food.Tooltip + Environment.NewLine + Current_City.Statistics.Food_Percent.Tooltip, gameObject);
        Production_Text.text = Helper.Float_To_String(Current_City.Yields.Production, 1, true);
        TooltipManager.Instance.Register_Tooltip(Production_Text.gameObject, Current_City.Statistics.Production.Tooltip + Environment.NewLine + Current_City.Statistics.Production_Percent.Tooltip, gameObject);
        Cash_Text.text = Helper.Float_To_String(Current_City.Yields.Cash, 1, true);
        TooltipManager.Instance.Register_Tooltip(Cash_Text.gameObject, Current_City.Statistics.Cash.Tooltip + Environment.NewLine + Current_City.Statistics.Cash_Percent.Tooltip, gameObject);
        Science_Text.text = Helper.Float_To_String(Current_City.Yields.Science, 1, true);
        TooltipManager.Instance.Register_Tooltip(Science_Text.gameObject, Current_City.Statistics.Science.Tooltip + Environment.NewLine + Current_City.Statistics.Science_Percent.Tooltip, gameObject);
        Culture_Text.text = Helper.Float_To_String(Current_City.Yields.Culture, 1, true);
        TooltipManager.Instance.Register_Tooltip(Culture_Text.gameObject, Current_City.Statistics.Culture.Tooltip + Environment.NewLine + Current_City.Statistics.Culture_Percent.Tooltip, gameObject);
        Mana_Text.text = Helper.Float_To_String(Current_City.Yields.Mana, 1, true);
        TooltipManager.Instance.Register_Tooltip(Mana_Text.gameObject, Current_City.Statistics.Mana.Tooltip + Environment.NewLine + Current_City.Statistics.Mana_Percent.Tooltip, gameObject);
        Faith_Text.text = Helper.Float_To_String(Current_City.Yields.Faith, 1, true);
        TooltipManager.Instance.Register_Tooltip(Faith_Text.gameObject, Current_City.Statistics.Faith.Tooltip + Environment.NewLine + Current_City.Statistics.Faith_Percent.Tooltip, gameObject);
        //Growth
        Growth_Text.text = string.Format("{0} / {1} +{2} per turn{3}{4} turns", Math.Round(Current_City.Pop_Growth_Acquired, 2), Current_City.Pop_Growth_Required,
            Math.Round(Current_City.Pop_Growth, 2), Environment.NewLine,
            Current_City.Pop_Growth_Estimated_Turns != -1 ? Current_City.Pop_Growth_Estimated_Turns.ToString() : "N/A");
        TooltipManager.Instance.Register_Tooltip(Growth_Text.gameObject, Current_City.Statistics.Growth.Tooltip + Environment.NewLine + Current_City.Statistics.Growth_Percent.Tooltip, gameObject);
        //Happiness, health, crime
        Happiness_Text.text = Math.Round(Current_City.Happiness, 2).ToString();
        TooltipManager.Instance.Register_Tooltip(Happiness_Text.gameObject, Current_City.Statistics.Happiness.Tooltip, gameObject);
        Update_HHO_Status_Image(Current_City.Happiness, City.LOW_HAPPINESS_WARNING_THRESHOLD, City.VERY_LOW_HAPPINESS_WARNING_THRESHOLD,
            City.LOW_HAPPINESS_TEXTURE, City.VERY_LOW_HAPPINESS_TEXTURE, Happiness_Image);
        Health_Text.text = Math.Round(Current_City.Health, 2).ToString();
        TooltipManager.Instance.Register_Tooltip(Health_Text.gameObject, Current_City.Statistics.Health.Tooltip, gameObject);
        Update_HHO_Status_Image(Current_City.Health, City.LOW_HEALTH_WARNING_THRESHOLD, City.VERY_LOW_HEALTH_WARNING_THRESHOLD,
            City.LOW_HEALTH_TEXTURE, City.VERY_LOW_HEALTH_TEXTURE, Health_Image);
        Order_Text.text = Math.Round(Current_City.Order, 2).ToString();
        TooltipManager.Instance.Register_Tooltip(Order_Text.gameObject, Current_City.Statistics.Order.Tooltip, gameObject);
        Update_HHO_Status_Image(Current_City.Order, City.LOW_ORDER_WARNING_THRESHOLD, City.VERY_LOW_ORDER_WARNING_THRESHOLD,
            City.LOW_ORDER_TEXTURE, City.VERY_LOW_ORDER_TEXTURE, Order_Image);
        //Food storage
        Food_Storage_Text.text = string.Format("{0} / {1}", Helper.Float_To_String(Current_City.Food_Stored, 1), Current_City.Max_Food_Stored);

        //Unemployed pops
        if (Current_City.Unemployed_Pops != 0) {
            Unemployed_Panel.SetActive(true);
            Unemployed_Text.text = string.Format("{0} Unemployed pop{1}!", Current_City.Unemployed_Pops, Current_City.Unemployed_Pops != 1 ? "s" : "");
        } else {
            Unemployed_Panel.SetActive(false);
        }

        //Building list
        for(int i = 0; i < building_list_game_objects.Count; i++) {
            building_list_game_objects[i].SetActive(false);
        }
        for(int i = building_list_offset; i < Current_City.Buildings.Count; i++) {
            if(i - building_list_offset >= building_list_game_objects.Count) {
                break;
            }
            GameObject go = building_list_game_objects[i - building_list_offset];
            Building building = Current_City.Buildings[i];
            go.SetActive(true);
            go.GetComponentInChildren<Image>().overrideSprite = SpriteManager.Instance.Get(building.Texture, SpriteManager.SpriteType.Building);
            go.GetComponentInChildren<Text>().text = building.Name;
            foreach(Button button in go.GetComponentsInChildren<Button>()) {
                if (button.name == "PauseButton") {
                    button.GetComponentInChildren<Image>().overrideSprite = SpriteManager.Instance.Get(building.Paused ? "resume" : "pause", SpriteManager.SpriteType.UI);
                    button.interactable = building.Can_Be_Paused;
                    Button.ButtonClickedEvent button_event = new Button.ButtonClickedEvent();
                    button_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                        Building b = building;
                        Pause_Building(b);
                    }));
                    button.onClick = button_event;
                } else {
                    Button.ButtonClickedEvent button_event = new Button.ButtonClickedEvent();
                    button_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                        Building b = building;
                        Delete_Building(b);
                    }));
                    button.onClick = button_event;
                }
            }
            
            TooltipManager.Instance.Register_Tooltip(go.GetComponentInChildren<Image>().gameObject, building.Tooltip, gameObject);
            TooltipManager.Instance.Register_Tooltip(go.GetComponentInChildren<Text>().gameObject, building.Tooltip, gameObject);
        }
        if(Current_City.Buildings.Count <= building_list_game_objects.Count) {
            Buildings_Panel_Up_Button.interactable = false;
            Buildings_Panel_Down_Button.interactable = false;
        } else {
            Buildings_Panel_Up_Button.interactable = building_list_offset != 0;
            Buildings_Panel_Down_Button.interactable = true;
        }

        //Culture
        float[] culture_data = Current_City.Owned_Culture;
        Culture_Own_Text.text = string.Format("{0}% {1}{2}%/t",
            Mathf.RoundToInt(culture_data[0] * 100.0f),
            (Current_City.Estimated_Owned_Culture_Change >= 0.0f ? "+" : string.Empty),
            Math.Round(Current_City.Estimated_Owned_Culture_Change * 100.0f, 1)
        );
        Dictionary<Influencable, float> influenced_cities_and_villages = Current_City.Influenced_Cities_And_Villages;
        Dictionary<City, float> cities_influenced_by = Current_City.Cities_Influenced_By;
        Culture_Out_Text.text = influenced_cities_and_villages.Count.ToString();
        Culture_Range_Text.text = Mathf.RoundToInt(Current_City.Cultural_Influence_Range).ToString();
        Culture_In_Text.text = cities_influenced_by.Count.ToString();
        //Tooltip owned
        TooltipManager.Instance.Register_Tooltip(Culture_Own_Text.gameObject, string.Format("Own: {0}{1}Enemy:{2}{3}Resistance:{4}%",
            Mathf.RoundToInt(culture_data[1]), Environment.NewLine,
            Mathf.RoundToInt(culture_data[2]), Environment.NewLine,
            Mathf.RoundToInt(Current_City.Cultural_Influence_Resistance * 100.0f)
            ), gameObject);
        //Tooltip out
        StringBuilder tooltip_builder = new StringBuilder();
        foreach (KeyValuePair<Influencable, float> influence_data in influenced_cities_and_villages) {
            tooltip_builder.Append(influence_data.Key.Name);
            if (influence_data.Key.Owner.Is_Neutral) {
                tooltip_builder.Append(" (N)");
            } else if (!influence_data.Key.Is_Owned_By_Current_Player) {
                tooltip_builder.Append(" (E)");
            }
            tooltip_builder.Append(" +").Append(Helper.Float_To_String(influence_data.Value, 1)).Append(Environment.NewLine);
        }
        if(tooltip_builder.Length != 0) {
            tooltip_builder.Remove(tooltip_builder.Length - 2, 1);
        }
        TooltipManager.Instance.Register_Tooltip(Culture_Out_Text.gameObject, tooltip_builder.ToString(), gameObject);
        //Tooltip in
        tooltip_builder = new StringBuilder();
        foreach (KeyValuePair<City, float> influence_data in cities_influenced_by) {
            tooltip_builder.Append(influence_data.Key.Name);
            if (influence_data.Key.Owner.Is_Neutral) {
                tooltip_builder.Append(" (N)");
            } else if (!influence_data.Key.Is_Owned_By_Current_Player) {
                tooltip_builder.Append(" (E)");
            }
            tooltip_builder.Append(" +").Append(Helper.Float_To_String(influence_data.Value, 1)).Append(Environment.NewLine);
        }
        if (tooltip_builder.Length != 0) {
            tooltip_builder.Remove(tooltip_builder.Length - 2, 1);
        }
        TooltipManager.Instance.Register_Tooltip(Culture_In_Text.gameObject, tooltip_builder.ToString(), gameObject);

        //Trade routes
        int index = 1;
        if (trade_route_gos == null) {
            trade_route_gos = new List<GameObject>();
            while (true) {
                GameObject gameObject = GameObject.Find(string.Format("{0}/Route{1}", Trade_Panel.name, index));
                if(gameObject == null) {
                    break;
                }
                trade_route_gos.Add(gameObject);
                index++;
            }
        }
        foreach(GameObject route_go in trade_route_gos) {
            route_go.SetActive(false);
        }
        index = 0;
        foreach(TradeRoute route in Current_City.Trade_Routes.Where(x => SHOW_HIDDEN_TRADE_ROUTES || !x.Hidden).OrderByDescending(x => x.Yields.Total).ToList()) {
            if(index >= trade_route_gos.Count) {
                CustomLogger.Instance.Warning("Not enough UI space for trade routes");
                break;
            }
            GameObject route_go = trade_route_gos[index];
            route_go.SetActive(true);
            GameObject.Find(string.Format("{0}/{1}/PartnerText", Trade_Panel.name, route_go.name)).GetComponentInChildren<Text>().text = route.Hidden ? "???" : route.Target.Name;
            GameObject.Find(string.Format("{0}/{1}/TotalYieldsText", Trade_Panel.name, route_go.name)).GetComponentInChildren<Text>().text =
                route.Hidden ? "?" : (route.Active ? Math.Round(route.Yields.Total, 1).ToString("0.0") : "N/A");
            TooltipManager.Instance.Register_Tooltip(GameObject.Find(string.Format("{0}/{1}/PartnerText", Trade_Panel.name, route_go.name)), route.Tooltip, gameObject);
            index++;
        }

        TopGUIManager.Instance.Update_GUI();
    }

    public void Pause_Building(Building building)
    {
        if (!building.Can_Be_Paused) {
            return;
        }
        building.Paused = !building.Paused;
        Update_GUI();
    }

    public void Delete_Building(Building building)
    {
        if (Current_City.Delete_Building(building)) {
            Update_GUI();
        }
    }

    public void Buildings_List_Up_On_Click()
    {
        if(building_list_offset == 0) {
            return;
        }
        building_list_offset--;
        Update_GUI();
    }

    public void Buildings_List_Down_On_Click()
    {
        building_list_offset++;
        Update_GUI();
    }

    private Player Player
    {
        get {
            return Main.Instance.Current_Player;
        }
    }

    /// <summary>
    /// TODO: List scrolling
    /// Select_Unit_On_Click <-> Select_Building_On_Click = duplicate code
    /// </summary>
    public void Select_Building_On_Click()
    {
        if (Select_Building_Panel.activeSelf) {
            Select_Building_Panel.SetActive(false);
            return;
        }
        Select_Building_Panel.SetActive(true);
        Select_Unit_Panel.SetActive(false);
        Update_Select_Build_List();
    }

    private void Update_Select_Build_List()
    {
        if (building_game_objects == null) {
            building_game_objects = new List<GameObject>();
            int index = 1;
            while (true) {
                GameObject obj = GameObject.Find(Select_Building_Panel.gameObject.name + "/Building" + index);
                if (obj == null) {
                    break;
                }
                obj.SetActive(false);
                building_game_objects.Add(obj);
                index++;
            }
        }

        foreach (GameObject go in building_game_objects) {
            go.SetActive(false);
        }

        List<Building> available_buildings = Current_City.Get_Building_Options(true);
        for (int i = select_building_list_offset; i < available_buildings.Count; i++) {
            if (i - select_building_list_offset >= building_game_objects.Count) {
                break;
            }
            Building building = available_buildings[i];
            building_game_objects[i - select_building_list_offset].SetActive(true);
            string path = Select_Building_Panel.gameObject.name + "/" + building_game_objects[i - select_building_list_offset].name + "/";
            GameObject.Find(path + "Image").GetComponent<Image>().overrideSprite = SpriteManager.Instance.Get(building.Texture, SpriteManager.SpriteType.Building);
            GameObject.Find(path + "NameText").GetComponent<Text>().text = building.Name;
            GameObject.Find(path + "CashCostText").GetComponent<Text>().text = string.Format("{0}({1})", building.Cost.ToString(), Math.Round(building.Upkeep, 2).ToString("0.00"));
            GameObject.Find(path + "ProductionCostText").GetComponent<Text>().text = building.Production_Required.ToString();
            GameObject.Find(path + "TurnsText").GetComponent<Text>().text = string.Format("{0} turns", Current_City.Production_Time_Estimate(building) != -1 ?
                Current_City.Production_Time_Estimate(building).ToString() : "N/A");

            Button.ButtonClickedEvent button_event = new Button.ButtonClickedEvent();
            button_event.AddListener(new UnityEngine.Events.UnityAction(delegate () {
                Building b = building;
                Select_Building(b);
                Select_Building_Panel.SetActive(false);
            }));
            GameObject.Find(path + "SelectButton").GetComponent<Button>().onClick = button_event;
            GameObject.Find(path + "SelectButton").GetComponent<Button>().interactable = Current_City.Can_Build(building);

            TooltipManager.Instance.Register_Tooltip(GameObject.Find(path + "Image").GetComponent<Image>().gameObject, building.Tooltip, gameObject);
            TooltipManager.Instance.Register_Tooltip(GameObject.Find(path + "NameText").GetComponent<Text>().gameObject, building.Tooltip, gameObject);
        }

        if (available_buildings.Count <= building_game_objects.Count) {
            Select_Building_Panel_Up_Button.interactable = false;
            Select_Building_Panel_Down_Button.interactable = false;
        } else {
            Select_Building_Panel_Up_Button.interactable = select_building_list_offset != 0;
            Select_Building_Panel_Down_Button.interactable = true;
        }
    }

    public void Select_Building_Up_On_Click()
    {
        if (select_building_list_offset == 0) {
            return;
        }
        select_building_list_offset--;
        Update_Select_Build_List();
    }

    public void Select_Building_Down_On_Click()
    {
        select_building_list_offset++;
        Update_Select_Build_List();
    }
    
    public void Select_Unit_On_Click()
    {
        if (Select_Unit_Panel.activeSelf) {
            Select_Unit_Panel.SetActive(false);
            return;
        }
        Select_Unit_Panel.SetActive(true);
        Select_Building_Panel.SetActive(false);
        Update_Select_Unit_List();
    }

    private void Update_Select_Unit_List()
    {
        List<Trainable> available_units = Current_City.Get_Unit_Options(true);
        unit_selection_options.Clear();

        foreach(Trainable available_unit in available_units) {
            int turns = Current_City.Production_Time_Estimate(available_unit);
            unit_selection_options.Add(available_unit, new List<UIElementData>() {
                new UIElementData("Image", available_unit.Texture, SpriteManager.SpriteType.Unit),
                new UIElementData("NameText", available_unit.Name, Current_City.Can_Train(available_unit) ? (Color?)null : Color.red),
                new UIElementData("CashCostText", string.Format("{0}({1})", available_unit.Cost, Helper.Float_To_String(available_unit.Upkeep, 2))),
                new UIElementData("ProductionCostText", available_unit.Production_Required.ToString()),
                new UIElementData("TurnsText", string.Format("{0} turn{1}", turns, Helper.Plural(turns))),
                new UIElementData("SelectButton", null, delegate() {
                    if(available_unit is Unit) {
                        UnitInfoGUIManager.Instance.Open(available_unit as Unit, true);
                    } else {
                        Select_Unit(available_unit);
                    }
                })
            });
        }
    }
    
    public void Clear_Unit_Selection()
    {
        Select_Unit(null);
    }

    public void Select_Unit(Trainable unit)
    {
        if (unit == null || Current_City.Can_Train(unit)) {
            Current_City.Change_Production(unit);
            Update_GUI();
            TopGUIManager.Instance.Update_GUI();
        }
        Select_Unit_Panel.SetActive(false);
    }

    public void Select_Building(Building building)
    {
        Current_City.Change_Production(building);
        Update_GUI();
        TopGUIManager.Instance.Update_GUI();
    }

    public void Hex_On_Click(WorldMapHex hex)
    {
        if(hex.Current_LoS == WorldMapHex.LoS_Status.Unexplored || hex.Distance(Current_City.Hex) > Current_City.Work_Range) {
            Active = false;
            return;
        }
        Select_Unit_Panel.SetActive(false);
        Select_Building_Panel.SetActive(false);
        if (hex.Owner == null && Current_City.Unemployed_Pops > 0) {
            if (Current_City.Assing_Pop(hex)) {
                Update_GUI();
                Update_Hex_Highlights();
            }
        } else if (hex.Is_Owned_By(Current_City.Owner)) {
            if (Current_City.Unassing_Pop(hex)) {
                Update_GUI();
                Update_Hex_Highlights();
            }
        }
    }

    private int City_Index
    {
        get {
            int current_index = 0;
            for (int i = 0; i < current_city.Owner.Cities.Count; i++) {
                if (current_city.Owner.Cities[i].Id == current_city.Id) {
                    current_index = i;
                    break;
                }
            }
            return current_index;
        }
    }

    public void Previous_City()
    {
        if(current_city.Owner.Cities.Count == 1) {
            return;
        }
        int current_index = City_Index;
        current_index--;
        if(current_index < 0) {
            current_index = current_city.Owner.Cities.Count - 1;
        }
        Current_City = current_city.Owner.Cities[current_index];
        CameraManager.Instance.Lock_Camera = false;
        CameraManager.Instance.Set_Camera_Location(Current_City.Hex);
        CameraManager.Instance.Lock_Camera = true;
    }

    public void Next_City()
    {
        if (current_city.Owner.Cities.Count == 1) {
            return;
        }
        int current_index = City_Index;
        current_index++;
        if (current_index >= current_city.Owner.Cities.Count) {
            current_index = 0;
        }
        Current_City = current_city.Owner.Cities[current_index];
        CameraManager.Instance.Lock_Camera = false;
        CameraManager.Instance.Set_Camera_Location(Current_City.Hex);
        CameraManager.Instance.Lock_Camera = true;
    }

    private void Update_Hex_Highlights()
    {
        foreach(WorldMapHex hex in highlighted_hexes) {
            hex.Clear_Highlight();
        }
        highlighted_hexes.Clear();

        List<WorldMapHex> hexes_that_cant_be_worked = Current_City.Hex.Get_Hexes_Around(6);

        foreach (WorldMapHex hex_that_can_be_worked in Current_City.Hexes_That_Can_Be_Worked) {
            if (hexes_that_cant_be_worked.Contains(hex_that_can_be_worked)) {
                hexes_that_cant_be_worked.Remove(hex_that_can_be_worked);
            }
        }

        foreach (WorldMapHex hex_that_cant_be_worked in hexes_that_cant_be_worked) {
            if(hex_that_cant_be_worked.Current_LoS != WorldMapHex.LoS_Status.Visible) {
                continue;
            }
            hex_that_cant_be_worked.Highlight = Cant_Be_Worked_Highlight;
            highlighted_hexes.Add(hex_that_cant_be_worked);
        }

        foreach (WorldMapHex worked_hex in Current_City.Worked_Hexes) {
            worked_hex.Highlight = Worked_Hex_Highlight;
            highlighted_hexes.Add(worked_hex);
        }
    }

    private void Update_HHO_Status_Image(float value, float low_threshold, float very_low_threshold, string low_texture, string very_low_texture,
        Image image)
    {
        if(value > low_threshold) {
            image.gameObject.SetActive(false);
            return;
        }
        image.gameObject.SetActive(true);
        image.sprite = SpriteManager.Instance.Get(value <= very_low_threshold ? very_low_texture : low_texture, SpriteManager.SpriteType.UI);
    }
}
