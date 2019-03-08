using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StatusEffectList<T> : IEnumerable<T> where T : IStatusEffect {
    private List<T> list;
    
    public StatusEffectList()
    {
        list = new List<T>();
    }

    public void End_Turn()
    {
        List<T> expiring_status_effects = new List<T>();
        foreach (T status_effect in list) {
            status_effect.Current_Duration--;
            if (status_effect.Current_Duration == 0) {
                expiring_status_effects.Add(status_effect);
            }
        }
        foreach (T expiring_status_effect in expiring_status_effects) {
            list.Remove(expiring_status_effect);
        }
    }

    public void Apply_Status_Effect(T status_effect, bool stacks)
    {
        while (list.Any(x => x.Name == status_effect.Name) && !stacks) {
            T old_effect = list.First(x => x.Name == status_effect.Name);
            list.Remove(old_effect);
        }
        list.Add(status_effect);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return list.GetEnumerator();
    }

    public int Count
    {
        get {
            return list.Count;
        }
    }
}
