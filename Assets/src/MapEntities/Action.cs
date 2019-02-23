public class Action {
    public delegate bool Can_Be_Activated_Delegate(WorldMapEntity entity);
    public delegate void Activate_Delegate(WorldMapEntity entity);

    public string Name { get; private set; }
    public string Texture { get; private set; }
    public string Tooltip { get; private set; }
    public SpriteManager.SpriteType Texture_Type { get; private set; }
    public Can_Be_Activated_Delegate Can_Be_Activated { get; private set; }
    public Activate_Delegate Activate { get; private set; }

    public Action(string name, string texture, string tooltip, SpriteManager.SpriteType texture_type, Can_Be_Activated_Delegate can_be_activated, Activate_Delegate activate)
    {
        Name = name;
        Texture = texture;
        Tooltip = tooltip;
        Texture_Type = texture_type;
        Can_Be_Activated = can_be_activated;
        Activate = activate;
    }
}
