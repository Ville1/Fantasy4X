using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

public class CityStatistics {

    public Statistic Food { get; private set; }
    public Statistic Food_Percent { get; private set; }
    public Statistic Production { get; private set; }
    public Statistic Production_Percent { get; private set; }
    public Statistic Cash { get; private set; }
    public Statistic Cash_Percent { get; private set; }
    public Statistic Science { get; private set; }
    public Statistic Science_Percent { get; private set; }
    public Statistic Culture { get; private set; }
    public Statistic Culture_Percent { get; private set; }
    public Statistic Mana { get; private set; }
    public Statistic Mana_Percent { get; private set; }
    public Statistic Faith { get; private set; }
    public Statistic Faith_Percent { get; private set; }

    public Statistic Happiness { get; private set; }
    public Statistic Health { get; private set; }
    public Statistic Order { get; private set; }

    public Statistic Growth { get; private set; }
    public Statistic Growth_Percent { get; private set; }

    public CityStatistics()
    {
        Food = new Statistic();
        Food_Percent = new Statistic(true);
        Production = new Statistic();
        Production_Percent = new Statistic(true);
        Cash = new Statistic();
        Cash_Percent = new Statistic(true);
        Science = new Statistic();
        Science_Percent = new Statistic(true);
        Culture = new Statistic();
        Culture_Percent = new Statistic(true);
        Mana = new Statistic();
        Mana_Percent = new Statistic(true);
        Faith = new Statistic();
        Faith_Percent = new Statistic(true);
        Happiness = new Statistic();
        Health = new Statistic();
        Order = new Statistic();
        Growth = new Statistic();
        Growth_Percent = new Statistic(true);
    }

    public void Add(string source, Yields yields)
    {
        Food.Add(source, yields.Food);
        Production.Add(source, yields.Production);
        Cash.Add(source, yields.Cash);
        Science.Add(source, yields.Science);
        Culture.Add(source, yields.Culture);
        Mana.Add(source, yields.Mana);
        Faith.Add(source, yields.Faith);
    }

    public void Add_Precentage(string source, Yields yields)
    {
        Food_Percent.Add(source, yields.Food);
        Production_Percent.Add(source, yields.Production);
        Cash_Percent.Add(source, yields.Cash);
        Science_Percent.Add(source, yields.Science);
        Culture_Percent.Add(source, yields.Culture);
        Mana_Percent.Add(source, yields.Mana);
        Faith_Percent.Add(source, yields.Faith);
    }

    public void Clear_Yields()
    {
        Food.Clear();
        Food_Percent.Clear();
        Production.Clear();
        Production_Percent.Clear();
        Cash.Clear();
        Cash_Percent.Clear();
        Science.Clear();
        Science_Percent.Clear();
        Culture.Clear();
        Culture_Percent.Clear();
        Mana.Clear();
        Mana_Percent.Clear();
        Faith.Clear();
        Faith_Percent.Clear();
    }
}

public class Statistic
{
    public Dictionary<string, float> Data { get; private set; }
    bool is_percent;

    public Statistic(bool is_percent = false)
    {
        Data = new Dictionary<string, float>();
        this.is_percent = is_percent;
    }

    public void Add(string source, float delta)
    {
        if(delta == 0.0f) {
            return;
        }
        if (Data.ContainsKey(source)) {
            Data[source] += delta;
        } else {
            Data.Add(source, delta);
        }
    }

    public void Clear()
    {
        Data.Clear();
    }

    public string Tooltip
    {
        get {
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < Data.Count; i++) {
                KeyValuePair<string, float> pair = Data.ElementAt(i);
                builder.Append(pair.Key).Append(": ");
                if(pair.Value >= 0.0f) {
                    builder.Append("+");
                }
                builder.Append(Math.Round(pair.Value, 2));
                if (is_percent) {
                    builder.Append("%");
                }
                if (i != Data.Count - 1) {
                    builder.Append(Environment.NewLine);
                }
            }
            return builder.ToString();
        }
    }
}
