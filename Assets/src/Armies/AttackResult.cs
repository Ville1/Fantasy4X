public class AttackResult {
    public Unit Target { get; set; }
    public float Manpower_Delta { get; set; }
    public float Damage_Effectiveness { get; set; }
    public float Morale_Delta { get; set; }
    public float Stamina_Delta { get; set; }
    public bool? Can_Attack { get; set; }
    public float? Movement { get; set; }
    public float Final_Attack { get; set; }
    public float Base_Attack { get; set; }
    public float Final_Defence { get; set; }
    public float Base_Defence { get; set; }

    public AttackResult()
    {

    }

    public bool Empty
    {
        get {
            return Manpower_Delta == 0.0f && Damage_Effectiveness == 0.0f && Morale_Delta == 0.0f && Stamina_Delta == 0.0f && Can_Attack == null && Movement == null &&
                Final_Attack == 0.0f && Base_Attack == 0.0f && Final_Defence == 0.0f && Base_Defence == 0.0f;
        }
    }

    public float Attack_Effectiveness
    {
        get {
            return Final_Attack / Base_Attack;
        }
    }

    public float Defence_Effectiveness
    {
        get {
            return Final_Defence / Base_Defence;
        }
    }
}
