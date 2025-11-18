using Game.Common.Modules;
using Godot;
using System;
using System.Linq;

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

    private string[] nextOptions;
    private Button[] currentButtons;


    public void ShowChoices(DialogLine line)
    {
        //multiaudioPlayerModule.PlaySound(impact);

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
        var overlay = CreateShaderOverlay(buttonWidth, buttonHeight);

        _ = currentButtons.Append(button);

        button.SelectedSignal += ProcessSelection;

        button.AddChild(overlay);
        wrapper.AddChild(button);
        optionsContainer.AddChild(wrapper);

        AnimateButtonEntry(button, wrapper.GetIndex());
    }

    private void ProcessSelection(int uid)
    {
        //TODO: activate HideAnimation
        AnimateButtonExit();
        string nextUid = nextOptions[uid];
        ChoiceSelected?.Invoke(nextUid);
    }

    private void AnimateButtonExit()
    {
        animationPlayer.Play("OutQuestion");

        foreach(var c in  )
        {
            //TODO
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


}
