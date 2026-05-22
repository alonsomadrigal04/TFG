using Godot;
using System;

public partial class PauseMenu : Control
{
    bool GamePaused = false;
    [ExportGroup("BUTTONS")]
    [Export] Button bContinue;
    [Export] Button bCodex;
    [Export] Button bSound;
    [Export] Button bInventory;
    [Export] Button bExit;

    [ExportGroup("MENU DISPLAYERS")]
    MenuDisplayed actualMenu = MenuDisplayed.None;

    [Export] Control soundMenu;
    [Export] Control inventoryMenu;
    [Export] Control codexMenu;

    [ExportGroup("Animators")]
    [Export] AnimationPlayer pauseAnimator;
    [Export] AnimationPlayer soundAnimator;
    [Export] AnimationPlayer codexAnimator;
    [Export] AnimationPlayer inventotyAnimator;

    public void SetPause(bool gamePaused)
    {
        if(gamePaused)
            pauseAnimator.Play("PauseOn");
        else
            pauseAnimator.Play("PauseOff");
    }

    public override void _Ready()
    {
        bContinue.Pressed += ResumeGame;
    }

    void ResumeGame()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
            SetPause(false);
            actualMenu = MenuDisplayed.None;
        }
    }

    void SetMenuType(MenuDisplayed type)
    {
        actualMenu = type;
        switch (type)
        {
            case MenuDisplayed.Codex:
            inventoryMenu.Hide();
            codexMenu.Show();
            soundMenu.Hide();
            codexAnimator.Play("DisplayIn");
            break;
            
            case MenuDisplayed.Inventory:
            inventoryMenu.Show();
            codexMenu.Hide();
            soundMenu.Hide();
            inventotyAnimator.Play("DisplayIn");
            break;

            case MenuDisplayed.Sound:
            soundMenu.Show();
            codexMenu.Hide();
            inventoryMenu.Hide();
            soundAnimator.Play("DisplayIn");
            break;

        }
    }

}

enum MenuDisplayed{

    Codex,
    Inventory,
    Sound,
    None
}
