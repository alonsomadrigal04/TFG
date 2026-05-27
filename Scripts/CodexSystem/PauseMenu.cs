using Godot;
using System;
using System.Linq.Expressions;

public partial class PauseMenu : Control
{
    bool GamePaused = false;
    [ExportGroup("BUTTONS")]
    [Export] Button bContinue;
    [Export] Button bCodex;
    [Export] Button bSound;
    [Export] Button bInventory;
    [Export] Button bExit;
    [Export] Button bReturn;


    [ExportGroup("MENU DISPLAYERS")]
    MenuDisplayed actualMenu = MenuDisplayed.None;

    [Export] Control soundMenu;
    [Export] Control inventoryMenu;
    [Export] Control codexMenu;

    [ExportGroup("Animators")]
    [Export] AnimationPlayer pauseAnimator;
    [Export] AnimationPlayer soundAnimator;
    [Export] AnimationPlayer codexAnimator;
    [Export] AnimationPlayer inventoryAnimator;

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
        bReturn.Pressed += ReturnAction;

        bReturn.Hide();
        soundMenu.Hide();
        codexMenu.Hide();
        inventoryMenu.Hide();

        bCodex.Pressed += () => SetMenuType(MenuDisplayed.Codex);
        bSound.Pressed += () => SetMenuType(MenuDisplayed.Sound);
        bInventory.Pressed += () => SetMenuType(MenuDisplayed.Inventory);

    }

    AnimationPlayer GetAnimator(MenuDisplayed menu) => menu switch
    {
        MenuDisplayed.Codex => codexAnimator,
        MenuDisplayed.Inventory => inventoryAnimator,
        MenuDisplayed.Sound => soundAnimator,
        MenuDisplayed.None => null,
        _ => null
    };

    void ReturnAction()
    {
        pauseAnimator.Play("ReturnOut");

        AnimationPlayer anim = GetAnimator(actualMenu);

        anim.Play("DisplayOut");

        actualMenu = MenuDisplayed.None;       
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
        pauseAnimator.Play("ReturnIn");
        bReturn.Show();
        AnimationPlayer anim = GetAnimator(type);
        anim.Play("DisplayIn");

        switch (type)
        {
            case MenuDisplayed.Codex:
            inventoryMenu.Hide();
            codexMenu.Show();
            soundMenu.Hide();
            break;
            
            case MenuDisplayed.Inventory:
            inventoryMenu.Show();
            codexMenu.Hide();
            soundMenu.Hide();
            break;

            case MenuDisplayed.Sound:
            soundMenu.Show();
            codexMenu.Hide();
            inventoryMenu.Hide();
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
