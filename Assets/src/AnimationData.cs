using System.Collections.Generic;

public class AnimationData {
    public string Effect_Name { get; private set; }
    public List<string> Sprites { get; private set; }
    public float FPS { get; private set; }
    public bool Repeat { get; private set; }
    public bool Is_Effect { get { return !string.IsNullOrEmpty(Effect_Name); } }

    public AnimationData(string effect_name)
    {
        Effect_Name = effect_name;
        Sprites = null;
        FPS = -1.0f;
        Repeat = false;
    }

    public AnimationData(List<string> sprites, float fps, bool repeat)
    {
        Effect_Name = null;
        Sprites = Helper.Copy_List(sprites);
        FPS = fps;
        Repeat = repeat;
    }
}
