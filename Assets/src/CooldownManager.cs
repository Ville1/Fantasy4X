using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CooldownManager<T> : IEnumerable<T> where T : ICooldown {
    private List<CoodownData<T>> list;

    public CooldownManager()
    {
        list = new List<CoodownData<T>>();
    }

    public void End_Turn()
    {
        List<CoodownData<T>> off_cooldown = new List<CoodownData<T>>();
        foreach(CoodownData<T> data in list) {
            data.Current_Cooldown--;
            if(data.Current_Cooldown <= 0) {
                off_cooldown.Add(data);
            }
        }
        foreach(CoodownData<T> data in off_cooldown) {
            list.Remove(data);
        }
    }

    public int Get_Cooldown(T obj)
    {
        CoodownData<T> data = list.FirstOrDefault(x => x.Object.Id == obj.Id);
        return data == null ? 0 : data.Current_Cooldown;
    }

    public void Set_Cooldown(T obj, int cooldown = -1)
    {
        if(obj.Cooldown == 0 && cooldown == -1) {
            return;
        }
        CoodownData<T> existing_data = list.FirstOrDefault(x => x.Object.Id == obj.Id);
        if(existing_data == null) {
            list.Add(new CoodownData<T>(obj, cooldown == -1 ? obj.Cooldown : cooldown));
        } else {
            existing_data.Current_Cooldown = cooldown == -1 ? obj.Cooldown : cooldown;
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        return list.Select(x => x.Object).ToList().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.Select(x => x.Object).ToList().GetEnumerator();
    }

    private class CoodownData<Q> where Q : ICooldown
    {
        public CoodownData(Q obj, int current_cooldown)
        {
            Object = obj;
            Current_Cooldown = current_cooldown;
        }

        public Q Object { get; set; }
        public int Current_Cooldown { get; set; }
    }
}
