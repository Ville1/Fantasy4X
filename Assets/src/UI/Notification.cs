public class Notification {
    private static int current_id = 0;

    public int Id { get; private set; }
    public string Name { get; set; }
    public string Texture { get; set; }
    public string Sound_Effect { get; set; }
    public SpriteManager.SpriteType Sprite_Type { get; set; }
    public NotificationManager.Notification_On_Click On_Click { get; set; }

    public Notification(string name, string texture, SpriteManager.SpriteType sprite_type, string sound_effect, NotificationManager.Notification_On_Click on_click)
    {
        Id = current_id;
        current_id++;
        Name = name;
        Texture = texture;
        Sprite_Type = sprite_type;
        Sound_Effect = sound_effect;
        On_Click = on_click;
    }
}
