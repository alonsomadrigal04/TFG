using System;
using Godot;
using Godot.Collections;
using Utility;

public partial class UiStage : Node
{
    public static UiStage Instance { get; private set; }
    [Export] Control textBox;
    [Export] DialogManager dialogManager;
    [Export] Label rememberText;
    [Export] Sprite2D rememberImage;
    [Export] Control itemNotification;
    [Export] Label itemNotificationLabel;
    [Export] Label dramaticText;
    [Export] ItemInteractionMenu inventory;
    Vector2 itemNotificationOriginalPosition;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        GD.Print(inventory);
        Instance = this;

        itemNotificationOriginalPosition = itemNotification.Position;
        itemNotification.Modulate = itemNotification.Modulate with {A = 0};
        itemNotification.Hide();
        dramaticText.Hide();

    }

    public void HideTextBox()
    {
        textBox.Hide();
    }

    public void HideInventory()
    {
        inventory.Hide();
    }
    public void ShowTextBox()
    {
        textBox.Show();
    }

    public bool IsTextBoxHide() =>  !textBox.Visible;

    public void AnimateShowTextBox()
    {
        ActionBus.ActionStarted();
        
        Tween tween = CreateTween();
        textBox.Show();
        Vector2 originalPosition = textBox.Position;
        textBox.Position = new Vector2(originalPosition.X, originalPosition.Y + 50);

        textBox.Modulate = textBox.Modulate with { A = 0 };
        tween.SetTrans(Tween.TransitionType.Quint)   
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(textBox, "modulate:a", 1f, 0.5f);
        tween.SetParallel().TweenProperty(textBox, "position:y", originalPosition.Y, 0.5f);

        tween.Finished += ActionBus.ActionFinished;
    }

    public void AnimateDramaticText(string title)
    {
        ActionBus.ActionStarted();
        dramaticText.Text = char.ToUpper(title[0]) + title[1..];
        dramaticText.Show();
        AudioManager.Instance.Chorus1.Play();
        Vector2 originalScale = dramaticText.Scale;

        dramaticText.Modulate = dramaticText.Modulate with {A = 0};
        

        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        tween.SetParallel();
        tween.TweenDelegate<Vector2>(
            v => dramaticText.Scale = v,
            new Vector2(1,1),
            new Vector2(1,1) * 0.5f,
            2f
        );

        tween.TweenDelegate(
            v => dramaticText.Modulate = new Color(1,1,1, v),
            0,
            1,
            1f
        );

        tween.SetParallel(false);
                tween.TweenDelegate(
            v => dramaticText.Modulate = new Color(1,1,1, v),
            1,
            0,
            1f
        );

        tween.Finished += () =>{
            ActionBus.ActionFinished();
            dramaticText.Scale = originalScale;
        };
    }

    public void AnimateRemember(string name)
    {
        rememberText.Text = $"{name} recordará esto.";
        rememberText.Modulate = rememberText.Modulate with { A = 0 };
        rememberText.Show();
        rememberImage.Modulate = rememberText.Modulate with { A = 0 };
        rememberImage.Show();
        Vector2 originalPosition = rememberText.Position;
        rememberText.Position = new(originalPosition.X, -rememberText.Size.Y -originalPosition.Y);

        Vector2 originalPositionImage = rememberImage.Position;
        rememberImage.Position = new(originalPositionImage.X, -rememberImage.Texture.GetSize().Y -originalPositionImage.Y);

        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Quint)   
            .SetEase(Tween.EaseType.Out)
            .SetParallel();
        
        AudioManager.Instance.Remember.Play();

        tween.TweenProperty(rememberText, "position", originalPosition, 1f);
        tween.TweenProperty(rememberText, "modulate:a", 1f, 1f);

        tween.TweenProperty(rememberText, "modulate:a", 0f, 1f).SetDelay(5f);

        Tween tweenImage = CreateTween();
        tweenImage.SetTrans(Tween.TransitionType.Quint)   
            .SetEase(Tween.EaseType.Out)
            .SetParallel();
        tweenImage.TweenProperty(rememberImage, "position", originalPositionImage, 1f);
        tweenImage.TweenProperty(rememberImage, "modulate:a", 1f, 1f);

        tweenImage.TweenProperty(rememberImage, "modulate:a", 0f, 1f).SetDelay(5f);

        tween.Finished += () =>
        {
            rememberImage.Hide();
            rememberText.Hide();
        };


    }

    public void AnimateHideTextBox()
    {
        ActionBus.ActionStarted();
        
        Tween tween = CreateTween();
        Vector2 originalPosition = textBox.Position;
        tween.SetTrans(Tween.TransitionType.Quint)   
            .SetEase(Tween.EaseType.In);
        tween.TweenProperty(textBox, "modulate:a", 0f, 0.5f);
        tween.SetParallel().TweenProperty(textBox, "position:y", originalPosition.Y + 50, 0.5f);


        tween.Finished += () => { 
            ActionBus.ActionFinished();
            textBox.Hide();
            textBox.Position = originalPosition;};
    }

    public async void AddItemAnimation(ObjectData objectData)
    {
        itemNotification.Show();
        itemNotificationLabel.Text = $"Has obtenido {objectData.Name}";
        AudioManager.Instance.NewItem.Play();

        Vector2 originalPosition = itemNotificationOriginalPosition;
        Vector2 hiddenPosition = originalPosition + new Vector2(0f, -300f);

        itemNotification.Position = hiddenPosition;
        itemNotification.Modulate = Colors.White with { A = 0f };

        Tween tweenIn = CreateTween();
        tweenIn.SetParallel(true);

        tweenIn.TweenProperty(itemNotification, "position", originalPosition, 0.32f)
            .SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);

        tweenIn.TweenProperty(itemNotification, "modulate", Colors.White, 0.16f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

        await ToSignal(tweenIn, Tween.SignalName.Finished);

        await ToSignal(GetTree().CreateTimer(3.0f), SceneTreeTimer.SignalName.Timeout);

        Tween tweenOut = CreateTween();
        tweenOut.SetParallel(true);

        tweenOut.TweenProperty(itemNotification, "position", hiddenPosition, 0.28f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);

        tweenOut.TweenProperty(itemNotification, "modulate", Colors.White with { A = 0f }, 0.24f)
            .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);
    }

    public void CleanEffects()
    {
        ActionBus.RunAfterActions(() => 
            AnimateHideTextBox()
        );
        UncoverTextBox();
        ChangeBoxStyle(TextboxTypes.Normal);
        MoveUI(ScreenPosition.Down);
    }

    public void ChangeBoxStyle(TextboxTypes style)
    {
        dialogManager.textTyper.ChangeTextBox(style);
    }

    public void UncoverTextBox()
    {
        textBox.Modulate = textBox.Modulate with { A = 1f };
    }

    internal void PlaySound(CommandToken commandToken)
    {
        if(SoundsDataBase.LoadedSounds.TryGetValue(commandToken.Arguments[0], out AudioStream sound))
        {
           AudioManager.Instance.PlaySfx(sound);
        }
        else
        {
            GD.Print(commandToken.Arguments[0] + " sound was not found");
        }
    }

    internal void MoveUI(ScreenPosition position)
    {
        dialogManager.SetTextBoxPosition(position);
    }

    public void PlayMusic(CommandToken commandToken)
    {
        if(SoundsDataBase.LoadedSounds.TryGetValue(commandToken.Arguments[0], out AudioStream sound))
        {
            AudioManager.Instance.PlayMusic(sound);
        }
        else
        {
            GD.Print("Something goes wrong");
        }
    }

    internal void StopMusic()
    {
        AudioManager.Instance.StopMusic();
    }

    internal void StopSFX()
    {
        AudioManager.Instance.FadeOutAllSfx();
    }
}