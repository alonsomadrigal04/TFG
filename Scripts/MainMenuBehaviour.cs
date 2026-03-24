using Godot;
using System;

public partial class MainMenuBehaviour : Node
{
    [Export] Button startButton;

    public override void _EnterTree()
    {
        startButton.Pressed += StartGame;
    }

    void StartGame()
    {
        GameManager.GameStarted = true;
        GameManager.Instance.StartNewChapter();
    }
}
