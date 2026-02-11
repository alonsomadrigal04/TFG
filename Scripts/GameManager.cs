using Godot;
using System;

public partial class GameManager : Node
{
    [Export] NodePath dialogManagerPath;
    DialogManager dialogManager;

    public override void _Ready()
    {
        CharacterDatabase.LoadFromJson("Data\\Characters.json");
        BackgroundDataBase.Load();
        ToolKit.InitializePositions();

        dialogManager = GetNode<DialogManager>(dialogManagerPath);

        dialogManager.StartDialogScene("test2");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept") && !ActionBus.IsBusy)
        {
            dialogManager.OnNextRequested();
        }
    }
}
