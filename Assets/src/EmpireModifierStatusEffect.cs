public class EmpireModifierStatusEffect : IStatusEffect
{
    public string Name { get; private set; }
    public int Current_Duration { get; set; }
    public int Duration { get; private set; }
    public EmpireModifiers Modifiers { get; private set; }

    public EmpireModifierStatusEffect(string name, int duration, EmpireModifiers modifiers)
    {
        Name = name;
        Duration = duration;
        Current_Duration = duration;
        Modifiers = modifiers;
    }
}
