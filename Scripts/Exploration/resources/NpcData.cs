using Godot;

[GlobalClass]
public partial class NpcData : Resource
{
    [Export] public string Name { get; set; }
    [Export] public Texture2D WordPortrait { get; set; }
    [Export] public string InitDialog { get; set; }
    [Export] public int MaxBanters { get; set; }
    bool hasTalkedOnce = false;
    int BanterIndex = 0;

    public string GetDialog()
    {
        if (!hasTalkedOnce)
        {
            hasTalkedOnce = true;
            return InitDialog;
        }
        if(BanterIndex + 1 > MaxBanters)
            return $"{Name}Banter{MaxBanters}";
        return $"{Name}Banter{BanterIndex++}";
    }
    
}