using Godot;
using System;

public partial class PauseMenu : Control
{
    [Export] Button continueButton;
    [Export] Button exitButton;

    public override void _Ready()
    {
        continueButton.Pressed += ResumeGame;
        exitButton.Pressed += ExitGame;

    }

    void ResumeGame() => GameManager.Instance.PauseGame();


    void ExitGame() => GameManager.Instance.ExitGame();
}
