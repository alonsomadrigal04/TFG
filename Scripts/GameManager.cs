using Godot;
using System;

public partial class GameManager : Node
{
    [Export] private NodePath dialogManagerPath;
    private DialogManager dialogManager;

    public override void _Ready()
    {
        CharacterDatabase.LoadFromJson("Data\\Characters.json");

        dialogManager = GetNode<DialogManager>(dialogManagerPath);

        dialogManager.StartDialogScene("test1");
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("ui_accept"))
        {
            dialogManager.OnNextRequested();
        }
    }
}
