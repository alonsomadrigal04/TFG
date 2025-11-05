using Game.Common.Modules;
using Godot;
using System;

public partial class ChoiceMaker : Node
{
    [Export] MultiaudioPlayerModule multiaudioPlayerModule;
    [Export] AudioStream impact;

    [ExportGroup("Options")]
    [Export] HBoxContainer optionsContainer;
    [Export] ShaderMaterial optionShader;
    [Export] int separation;

    [ExportGroup("Animated Objects")]
    [Export] ColorRect shaderBack;
    [Export] ColorRect grayBack;

    private string[] nextOptions;


    public void ShowChoices(DialogLine line)
    {
        //multiaudioPlayerModule.PlaySound(impact);

        SetUpOptions(line);

        //animationPlayer.Play("IntroQuestion");
    }

    void SetUpOptions(DialogLine line)
    {
        string[] texts = line.Text.Split('|', StringSplitOptions.TrimEntries);
        foreach (var c in texts)
        {
            CreateOptionButton(c, texts.Length);
        }

        nextOptions = line.Next?.Split('|', StringSplitOptions.TrimEntries) ?? [];

    }

    void CreateOptionButton(string optionText, int optionsQuantity)
    {
        float spacing = 40f;
        float totalSpacing = spacing * (optionsQuantity - 1);
        float availableWidth = optionsContainer.Size.X - totalSpacing;
        float buttonWidth = availableWidth / optionsQuantity;
        float buttonHeight = optionsContainer.Size.Y;

        Button button = new()
        {
            Text = optionText,
            CustomMinimumSize = new Vector2(buttonWidth, buttonHeight),
        };

        ColorRect colorRect = new()
        {
            CustomMinimumSize = new Vector2(buttonWidth, buttonHeight),
            Material = optionShader,
            MouseFilter = Control.MouseFilterEnum.Pass
        };

        button.AddThemeFontSizeOverride("font_size", 46);
        button.CustomMinimumSize = new Vector2(buttonWidth, buttonHeight);
        optionsContainer.AddThemeConstantOverride("separation", (int)spacing);
        button.AddChild(colorRect);
        optionsContainer.AddChild(button);
    }




    void OnOption1Selected(InputEvent @event)
    {
        if (!IsClick(@event)) return;
        ProcessSelection(0);
    }

    void OnOption2Selected(InputEvent @event)
    {
        if (!IsClick(@event)) return;
        ProcessSelection(1);
    }

    bool IsClick(InputEvent @event)
    {
        return @event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left;
    }

    void ProcessSelection(int optionIndex)
    {
        //animationPlayer.Play("OutQuestion");

        string nextUid = nextOptions.Length > optionIndex ? nextOptions[optionIndex] : null;


        // if (!string.IsNullOrEmpty(nextUid))
        //     OnOptionSelected?.Invoke(nextUid);
    }

}
