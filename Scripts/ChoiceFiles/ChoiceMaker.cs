using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;

public partial class ChoiceMaker : Node
{
    [Export] MultiaudioPlayerModule multiaudioPlayerModule;
    public event Action<string> ChoiceSelected;

    [Export] AudioStream impact;

    [ExportGroup("Options")]
    [Export] AnimationPlayer animationPlayer;
    [Export] HBoxContainer optionsContainer;
    [Export] ShaderMaterial optionShader;
    [Export] int separation;

    [ExportGroup("Animated Objects")]
    [Export] ColorRect shaderBack;
    [Export] ColorRect grayBack;

    string[] nextOptions;
    List<Button> currentButtons = [];
    Tween buttonExitTween;


    public void ShowChoices(DialogLine line)
    {
        multiaudioPlayerModule.PlaySound(impact);

        SetUpOptions(line);
    }

    async void SetUpOptions(DialogLine line)
    {
        SetActiveShaders();
        animationPlayer.Play("IntroQuestion");
        await ToSignal(GetTree().CreateTimer(0.8f), "timeout");
        string[] texts = line.Text.Split('|', StringSplitOptions.TrimEntries);
        for(int i = 0; i < texts.Length; i++)
        {
            CreateOptionButton(texts[i], texts.Length, i);
        }

        nextOptions = line.Next?.Split('|', StringSplitOptions.TrimEntries) ?? [];

    }

    void SetActiveShaders()
    {
        shaderBack.Visible = true;
        grayBack.Visible = true;
    }

    void CreateOptionButton(string optionText, int totalOptions, int i)
    {
        float buttonWidth = CalculateButtonWidth(totalOptions);
        float buttonHeight = optionsContainer.Size.Y;
        optionsContainer.AddThemeConstantOverride("separation", separation);

        var wrapper = CreateWrapper(buttonWidth, buttonHeight);
        var button = CreateButton(optionText, buttonWidth, buttonHeight, i);
        var overlay = CreateShaderOverlay();

        currentButtons.Add(button);

        button.SelectedSignal += ProcessSelection;

        button.AddChild(overlay);
        wrapper.AddChild(button);
        
        optionsContainer.AddChild(wrapper);

        SetButtonTextSize();

        AnimateButtonEntry(button, wrapper.GetIndex());
    }

    private void SetButtonTextSize()
    {
        int count = currentButtons.Count;
        if (count == 0)
            return;

        float baseSize = 48f;
        float scale = 1f / Mathf.Sqrt(count);
        float finalSize = Mathf.Round(baseSize * scale);

        foreach (var c in currentButtons)
        {
            c.SetFontSize((int)finalSize);
        }
    }


    void ProcessSelection(int uid)
    {
        AnimateOutChoice();
        string nextUid = nextOptions[uid];
        ChoiceSelected?.Invoke(nextUid);

    }

    void AnimateOutChoice()
    {
        animationPlayer.Play("OutQuestion");

        for(int i = 0; i < currentButtons.Count; i++)
        {
            AnimateExitButtons(currentButtons[i], i);
        }

    }


    float CalculateButtonWidth(int totalOptions)
    {
        const float spacing = 40f;
        float totalSpacing = spacing * (totalOptions - 1);
        float availableWidth = optionsContainer.Size.X - totalSpacing;
        return availableWidth / totalOptions;
    }

    static Control CreateWrapper(float width, float height)
    {
        return new Control
        {
            CustomMinimumSize = new Vector2(width, height)
        };
    }

    static ChoiceButton CreateButton(string text, float width, float height, int uid)
    {
        // TODO: all this parametres should be in the default constructor of this class
        var button = new ChoiceButton
        {
            Text = text,
            CustomMinimumSize = new Vector2(width, height),
            PivotOffset = new Vector2(width / 2, height / 2),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter | Control.SizeFlags.Expand,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            Uid = uid
        };
        return button;
    }

    ColorRect CreateShaderOverlay()
    {
        var overlay = new ColorRect
        {
            AnchorLeft = 0,
            AnchorTop = 0,
            AnchorRight = 1,
            AnchorBottom = 1,

            OffsetLeft = 0,
            OffsetTop = 0,
            OffsetRight = 0,
            OffsetBottom = 0,

            Material = optionShader,
            MouseFilter = Control.MouseFilterEnum.Pass
        };

        return overlay;
    }

    void AnimateExitButtons(Button button, int index)
    {
        GD.Print(index);

        buttonExitTween = CreateTween()
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.In);
        float delay = 0.1f * index;
        buttonExitTween.TweenProperty(button, "position", new Vector2(button.Position.X, 1000), 0.5f).SetDelay(delay);
        buttonExitTween.Parallel().TweenProperty(button, "rotation_degrees", -10f, 0.5f).SetDelay(delay);

        if (index == currentButtons.Count - 1)
        {
            buttonExitTween.TweenCallback(Callable.From(ClearButtons));
        }

    }

    void ClearButtons()
    {
        foreach (var b in currentButtons)
            b.GetParent().QueueFree();

        currentButtons.Clear();
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


}
