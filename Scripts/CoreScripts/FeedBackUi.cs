using System;
using Godot;
using Godot.Collections;
using Utility;

public partial class FeedBackUi : Control
{
    [Export] HBoxContainer toastContainer;
    [Export] Label feedbackToastLeft;
    [Export] Label feedbackToastRight;
    [Export] TextureRect inputIcon;
    [Export] float offSetAnimation = 40;



    [Export] Dictionary<Key, Texture2D> keyActionsIcon;
    Dictionary<string, string> keysActionName = new()
    {
        { "interact", "ACTION_INTERACT" }
    };

    public override void _Ready()
    {
        //TODO erasae this
        TranslationServer.SetLocale("es");
        toastContainer.Modulate = toastContainer.Modulate with {A = 0};
        toastContainer.Hide();
    }
    
    void SetInteractionPrompt(string action, UiPosition uiPosition = UiPosition.Down)
    {

        if (!InputMap.HasAction(action))
        {
            GD.PrintErr($"[FeedbackUi] {action} does not exist in InputMap");
            return;
        }

        if(!keysActionName.TryGetValue(action, out string keyAction))
        {
            GD.PrintErr($"[FeedbackUi] {action} has no entry in LocalizationTable");
            return;
        }

        Key? key = GetActionKey(action);
        if (!keyActionsIcon.TryGetValue(key.Value, out Texture2D icon))
        {
            GD.PrintErr($"[FeedbackUi] {key.Value} does not have a sprite in dictionary");
            return;
        }
        toastContainer.Show();
        feedbackToastLeft.Text = $"{Tr("TEXT_PRESS").ToUpper()}";

        inputIcon.Texture = icon;

        feedbackToastRight.Text = $"{Tr("TEXT_FOR").ToUpper()} {Tr(keyAction).ToUpper()}";

        SetTextPosition(uiPosition);
        StartFadeAnimation(uiPosition);
    }

    void StartFadeAnimation(UiPosition uiPosition)
    {
        float screenMargin = 20f;

        Vector2 finalPosition = toastContainer.Position;

        finalPosition.Y += uiPosition == UiPosition.Down
            ? -screenMargin
            : screenMargin;

        Vector2 finalOffset = new( 0,
        (uiPosition == UiPosition.Down) ? -offSetAnimation : offSetAnimation);

        toastContainer.Position += finalOffset;

        Tween tween = CreateTween();

        tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Quint);

        tween.SetParallel(true);

        tween.TweenDelegate<Vector2>(
            v => toastContainer.Position = v,
            toastContainer.Position,
            finalPosition,
            0.5f
        );

        tween.TweenDelegate<float>(
            v => toastContainer.Modulate = new Color(1,1,1,v),
            toastContainer.Modulate.A,
            1,
            0.5f
        );

    }


    public static string GetActionDisplayName(string actionName)
    {
        var events = InputMap.ActionGetEvents(actionName);

        foreach (var ev in events)
        {
            if (ev is InputEventKey keyEvent)
                return OS.GetKeycodeString(keyEvent.PhysicalKeycode);
        }

        return "?";
    }

    public static Key? GetActionKey(string action)
    {
        var events = InputMap.ActionGetEvents(action);

        foreach (var ev in events)
        {
            if (ev is InputEventKey keyEvent)
                return keyEvent.PhysicalKeycode;
        }

        return null;
    }

    void SetTextPosition(UiPosition uiPosition)
    {
        LayoutPreset layoutPreset = (uiPosition == UiPosition.Down) ? LayoutPreset.CenterBottom : LayoutPreset.CenterTop;
        toastContainer.SetAnchorsAndOffsetsPreset(layoutPreset);
    }

    public override void _Input(InputEvent input)
    {
        if (input.IsActionPressed("testAction"))
        {
            SetInteractionPrompt("interact", UiPosition.Down);
        }
    }
}

public enum UiPosition
{
    Top,
    Down
}
