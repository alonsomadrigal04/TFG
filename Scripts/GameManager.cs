using Godot;
using System;

public partial class GameManager : Node
{
    [Export] NodePath dialogManagerPath;
    DialogManager dialogManager;

    public override void _Ready()
    {
        CharacterDatabase.LoadFromJson("Data\\Characters.json");
        ToolKit.InitializePositions();

        dialogManager = GetNode<DialogManager>(dialogManagerPath);

        dialogManager.StartDialogScene("test2");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            dialogManager.OnNextRequested();
        }
    }
}
