using Game.Common.Modules;
using Godot;
using System;

public partial class ChoiceMaker : Node
{
    [Export] MultiaudioPlayerModule multiaudioPlayerModule;
    [Export] AudioStream impact;

    [ExportGroup("Options")]
    [Export] AnimationPlayer animationPlayer;
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

    async void SetUpOptions(DialogLine line)
    {
        SetActiveShaders();
        animationPlayer.Play("IntroQuestion");
        await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
        string[] texts = line.Text.Split('|', StringSplitOptions.TrimEntries);
        foreach (var c in texts)
        {
            CreateOptionButton(c, texts.Length);
        }

        nextOptions = line.Next?.Split('|', StringSplitOptions.TrimEntries) ?? [];

    }

    void SetActiveShaders()
    {
        shaderBack.Visible = true;
        grayBack.Visible = true;
    }

    void CreateOptionButton(string optionText, int totalOptions)
    {
        float buttonWidth = CalculateButtonWidth(totalOptions);
        float buttonHeight = optionsContainer.Size.Y;
        optionsContainer.AddThemeConstantOverride("separation", 40);

        var wrapper = CreateWrapper(buttonWidth, buttonHeight);
        var button = CreateButton(optionText, buttonWidth, buttonHeight);
        var overlay = CreateShaderOverlay(buttonWidth, buttonHeight);

        button.AddChild(overlay);
        wrapper.AddChild(button);
        optionsContainer.AddChild(wrapper);

        AnimateButtonEntry(button, wrapper.GetIndex());
    }

    float CalculateButtonWidth(int totalOptions)
    {
        const float spacing = 40f;
        float totalSpacing = spacing * (totalOptions - 1);
        float availableWidth = optionsContainer.Size.X - totalSpacing;
        return availableWidth / totalOptions;
    }

    Control CreateWrapper(float width, float height)
    {
        return new Control
        {
            CustomMinimumSize = new Vector2(width, height)
        };
    }

    Button CreateButton(string text, float width, float height)
    {
        var button = new Button
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, height),
            PivotOffset = new Vector2(width / 2, height / 2)
        };
        button.SetFontSize(46);
        return button;
    }

    ColorRect CreateShaderOverlay(float width, float height)
    {
        return new ColorRect
        {
            CustomMinimumSize = new Vector2(width, height),
            Material = optionShader,
            MouseFilter = Control.MouseFilterEnum.Pass
        };
    }

    void AnimateButtonEntry(Button button, int index)
    {
        Tween tween = CreateTween()
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.Out);

        button.Position = new Vector2(0, -1000);
        button.RotationDegrees = -10f;

        float delay = 0.1f * index;
        tween.TweenProperty(button, "position", Vector2.Zero, 0.5f).SetDelay(delay);
        tween.Parallel().TweenProperty(button, "rotation_degrees", 0f, 0.5f).SetDelay(delay);
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
