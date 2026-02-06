using Godot;
using System;

public partial class BackgroundStage : Node
{
    [Export] TextureRect imageFrame;
    public static BackgroundStage Instance {get; private set;}

    public override void _Ready()
    {
        if(Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public void SetBackground(string bgname)
    {
        BackgroundDataBase.LoadedBackgrounds.TryGetValue(bgname, out Texture2D newBg);
        imageFrame.Texture = newBg;
        //TODO: make cool animation transition
    }

    
}