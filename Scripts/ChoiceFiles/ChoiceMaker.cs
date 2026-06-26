using Components;
using Game.Common.Modules;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

public partial class ChoiceMaker : Node
{
    public event Action<string> ChoiceSelected;

    [ExportGroup("Options")]
    [Export] CanvasLayer choiceLayoutLayer;
    [Export] CanvasLayer choiceShaderLayer;

    [Export] AnimationPlayer animationPlayer;
    [Export] HBoxContainer optionsContainer;
    [Export] ShaderMaterial optionShader;
    [Export] int separation;
    const float MaxSizeButton = 400f;

    [ExportGroup("Animated Objects")]
    [Export] ColorRect shaderBack;
    [Export] ColorRect grayBack;
    [ExportGroup("Button Settings")]
    [Export] PackedScene choiceButton;


    string[] nextOptions;
    List<Button> currentButtons = [];
    Tween buttonExitTween;

    public override void _Ready()
    {
        optionsContainer.Hide();
        choiceShaderLayer.Hide();
    }
    public void ShowChoices(DialogLine line)
    {
        choiceLayoutLayer.Show();
        optionsContainer.Show();
        choiceShaderLayer.Show();
        string[] optionType = line.Type.Split('/', StringSplitOptions.TrimEntries);
        if(optionType.Length == 1)
            GD.PrintErr("Choice subType not declarated: try choice/Impact, choice/Soft");
        SetUpOptions(line, optionType[1]);
    }

    async void SetUpOptions(DialogLine line, string optionType)
    {
        switch (optionType)
        {
            case "important":
                SetActiveShaders();
                animationPlayer.Play("ImportantIN");
                break;
            case "normal":
                SetActiveShaders();
                animationPlayer.Play("NormalIN");
                break;
            case "soft":
                SetActiveShaders();
                animationPlayer.Play("SoftIN");
                break;
            default:
                break;
        }
        await ToSignal(GetTree().CreateTimer(1.2f), "timeout");
        string[] texts = line.Text.Split('|', StringSplitOptions.TrimEntries);
        for(int i = 0; i < texts.Length; i++)
        {
            CreateOptionButton(texts[i], texts.Length, i);
        }

        nextOptions = line.Next?.Split('|', StringSplitOptions.TrimEntries) ?? [];

    }

    void SetActiveShaders()
    {
        shaderBack.Show();
        grayBack.Show();
    }

    void CreateOptionButton(string optionText, int totalOptions, int i)
    {
        float buttonWidth = CalculateButtonWidth(totalOptions);
        float buttonHeight = optionsContainer.Size.Y;
        optionsContainer.AddThemeConstantOverride("separation", separation);

        var wrapper = CreateWrapper(buttonWidth, buttonHeight);
        var button = CreateButton(optionText, buttonWidth, i);

        currentButtons.Add(button);

        button.SelectedSignal += ProcessSelection;
        button.MouseEntered += HoverSounds;
        
        wrapper.AddChild(button);
        
        optionsContainer.AddChild(wrapper);

        SetButtonTextSize();

        AnimateButtonEntry(button, wrapper.GetIndex());
    }

    void HoverSounds() => AudioManager.Instance.Hover.Play();

    void SetButtonTextSize()
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


    async void ProcessSelection(int uid)
    {
        AudioManager.Instance.outQuestion.Play();
        await AnimateOutChoice();
        string nextUid = nextOptions[uid];
        grayBack.Hide();
        shaderBack.Hide();

        choiceLayoutLayer.Hide();
        choiceShaderLayer.Hide();
        optionsContainer.Hide();

        ChoiceSelected?.Invoke(nextUid);
    }

    async Task AnimateOutChoice()
    {
        animationPlayer.Play("OutQuestion");
        //sounds.outQuestion.Play();

        for(int i = 0; i < currentButtons.Count; i++)
        {
            AnimateExitButtons(currentButtons[i], i);
        }

        await ToSignal(GetTree().CreateTimer(0.6f), "timeout");
    }


    float CalculateButtonWidth(int totalOptions)
    {
        const float spacing = 40f;
        float totalSpacing = spacing * (totalOptions - 1);
        float availableWidth = optionsContainer.Size.X - totalSpacing;
        if(availableWidth > MaxSizeButton)
            return MaxSizeButton;
        return availableWidth / totalOptions;
    }

    static Control CreateWrapper(float width, float height)
    {
        return new Control
        {
            CustomMinimumSize = new Vector2(width, width),
            VisibilityLayer = 9
        };
    }

    ChoiceButton CreateButton(string text, float width, int uid)
    {
        // TODO: all this parametres should be in the default constructor of this class
        ChoiceButton newButton = choiceButton.Instantiate<ChoiceButton>();

        newButton.SetProperties(width, uid);
        newButton.choiceLabel.Text = text;
        return newButton;
    }

    void AnimateExitButtons(Button button, int index)
    {
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
        {
            b.MouseEntered -= HoverSounds;
            b.GetParent().QueueFree();
        }

        currentButtons.Clear();
    }


    void AnimateButtonEntry(Button button, int index)
    {
        AudioStreamPlayer player = AudioManager.sfxPool.GetReleased();
        player.Stream = AudioManager.Instance.flipCard.Stream;
        player.Play();

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
