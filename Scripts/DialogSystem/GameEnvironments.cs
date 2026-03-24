using Godot;
using System;
[GlobalClass]
public partial class GameEnvironments : Node
{
    
    [Export] PackedScene gameplay;
    [Export] PackedScene mainMenu;
    [Export] PackedScene campfireScene;
    [Export] PackedScene onlyTextBox;
    [Export] PackedScene chapterEnv;
    [Export] PackedScene test;



    //[Export] PackedScene splashScreen;

    public static PackedScene Gameplay => instance.gameplay;
    public static PackedScene MainMenu => instance.mainMenu;
    public static PackedScene CampfireScene => instance.campfireScene;
    public static PackedScene OnlyTextBox => instance.onlyTextBox;
    public static PackedScene ChapterEnv => instance.chapterEnv;
    public static PackedScene Test => instance.test;

    //public static PackedScene SplashScreen => instance.splashScreen;

    static GameEnvironments instance;
    static PackedScene mainGameplayScene;

    public override void _EnterTree() => instance ??= this;

    public override void _ExitTree()
    {
        if (instance == this) instance = null;
    }

    public override void _Ready()
    {
        mainGameplayScene = gameplay;
    }
    

    // Debugging only methods
    public static void SetGameplayScene(PackedScene newGameplay) => instance.gameplay = newGameplay;
    public static void ResetGameplayScene() => SetGameplayScene(mainGameplayScene);

    static PackedScene GetEnvironment(ExploreZone zone)
    {
        return zone switch
        {
            ExploreZone.Campfire => CampfireScene,
            ExploreZone.Test => Test,
            ExploreZone.Other => null,
            _ => null
        };
    }
    public static PackedScene ParseEnvironment(string raw)
    {
        if (!Enum.TryParse<ExploreZone>(raw, true, out var zone))
        {
            GD.PrintErr($"[Game Environment] {raw} is not a validExploreZone Name");
        }

        return GetEnvironment(zone);
    }
}

public enum ExploreZone
{
    Campfire,
    Test,
    Other
}
