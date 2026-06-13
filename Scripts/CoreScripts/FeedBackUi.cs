using System;
using Godot;
using Godot.Collections;
using Utility;

public partial class FeedBackUi : Control
{

    public static FeedBackUi Instance { get => instance; set => instance = value; }
    static FeedBackUi instance;
    public override void _EnterTree() => Instance = this;
    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    [Export] HBoxContainer toastContainer;
    [Export] Label feedbackToastLeft;
    [Export] Label feedbackToastRight;
    [Export] TextureRect inputIcon;
    [Export] float offSetAnimation = 40;

    bool isToastEnabled = false;

    UiPosition actualPosition;

    Vector2 baseePosition;
    Tween currentTween;

    string pendingAction;
    UiPosition pendingPosition;
    bool hasPending;

    [Export] Dictionary<string, Texture2D> inputIcons;
    Dictionary<string, string> keysActionName = new()
    {
        { "interact", "ACTION_INTERACT" },
        { "drag", "ACTION_DRAG"},
        { "zoom", "ACTION_ZOOM"},
        { "unZoom", "ACTION_UNZOOM"},

    };

    public override void _Ready()
    {
        TranslationServer.SetLocale("es");

        toastContainer.Hide();
        toastContainer.Modulate = toastContainer.Modulate with {A = 0};

        CallDeferred(nameof(InitBasePosition));
    }

    void InitBasePosition()
    {
        baseePosition = toastContainer.Position;
    }

    public static string GetInputIconId(string action)
    {
        var events = InputMap.ActionGetEvents(action);

        foreach (var ev in events)
        {
            switch (ev)
            {
                case InputEventKey keyEvent:
                    return $"KEY_{keyEvent.PhysicalKeycode}".ToUpper();

                case InputEventMouseButton mouseEvent:
                    return mouseEvent.ButtonIndex switch
                    {
                        MouseButton.Left => "MOUSE_LEFT",
                        MouseButton.Right => "MOUSE_RIGHT",
                        MouseButton.Middle => "MOUSE_MIDDLE",
                        MouseButton.WheelUp => "MOUSE_WHEEL_UP",
                        MouseButton.WheelDown => "MOUSE_WHEEL_DOWN",
                        _ => $"MOUSE_{mouseEvent.ButtonIndex}"
                    };
            }
        }

        return null;
    }
    
    public void SetInteractionPrompt(string action, UiPosition uiPosition = UiPosition.Down)
    {
        if (isToastEnabled)
        {
            pendingAction = action;
            pendingPosition = uiPosition;
            hasPending = true;

            StartFadeOutAnimation(OnFadeOutFinished);
            return;
        }

        ShowToast(action, uiPosition);
    }

    void OnFadeOutFinished()
    {
        isToastEnabled = false;

        toastContainer.Hide();

        if (hasPending)
        {
            hasPending = false;
            ShowToast(pendingAction, pendingPosition);
        }
    }

    public static InputEvent GetPrimaryInputEvent(string action)
    {
        var events = InputMap.ActionGetEvents(action);

        return events.Count > 0
            ? events[0]
            : null;
    }

    void StartFadeOnAnimation(UiPosition uiPosition = UiPosition.Down)
    {
        currentTween?.Kill();

        float screenMargin = 20f;

        Vector2 finalPosition = toastContainer.Position;

        finalPosition.Y += uiPosition == UiPosition.Down
            ? -screenMargin
            : screenMargin;

        Vector2 startPosition = finalPosition + new Vector2(
            0,
            uiPosition == UiPosition.Down
                ? -offSetAnimation
                : offSetAnimation
        );

        toastContainer.Position = startPosition;

        Color color = toastContainer.Modulate;
        color.A = 0;
        toastContainer.Modulate = color;

        currentTween = CreateTween();
        currentTween.SetEase(Tween.EaseType.Out);
        currentTween.SetTrans(Tween.TransitionType.Quint);
        currentTween.SetParallel();

        currentTween.TweenProperty(toastContainer, "position", finalPosition, 0.5f);
        currentTween.TweenProperty(toastContainer, "modulate:a", 1.0f, 0.5f);
    }

    public void StartFadeOutAnimation(Action callback = null)
    {
        if(!isToastEnabled) return;

        currentTween?.Kill();

        Vector2 finalPosition = toastContainer.Position + new Vector2(
            0,
            actualPosition == UiPosition.Down
                ? -offSetAnimation
                : offSetAnimation
        );

        currentTween = CreateTween();

        currentTween.SetEase(Tween.EaseType.Out);
        currentTween.SetTrans(Tween.TransitionType.Quint);
        currentTween.SetParallel();

        currentTween.TweenProperty(toastContainer, "position", finalPosition, 0.5f);
        currentTween.TweenProperty(toastContainer, "modulate:a", 0.0f, 0.5f);

        currentTween.Finished += () =>
        {
            callback?.Invoke();
        };
    }

    void ShowToast(string action, UiPosition uiPosition)
    {
        if (!InputMap.HasAction(action))
            return;

        if (!keysActionName.TryGetValue(action, out string keyAction))
            return;

        string iconId = GetInputIconId(action);

        if (!inputIcons.TryGetValue(iconId, out Texture2D icon))
            return;

        toastContainer.Show();

        feedbackToastLeft.Text = $"{Tr("TEXT_PRESS").ToUpper()}";
        inputIcon.Texture = icon;
        feedbackToastRight.Text = $"{Tr("TEXT_FOR").ToUpper()} {Tr(keyAction).ToUpper()}";


        SetTextPosition(uiPosition);
        StartFadeOnAnimation(uiPosition);
        
        actualPosition = uiPosition;
        isToastEnabled = true;
    }

    void SetTextPosition(UiPosition uiPosition)
    {
        LayoutPreset layoutPreset = (uiPosition == UiPosition.Down) ? LayoutPreset.CenterBottom : LayoutPreset.CenterTop;
        toastContainer.SetAnchorsAndOffsetsPreset(layoutPreset);
        toastContainer.QueueSort();
    }

    public override void _Input(InputEvent input)
    {
        if (input.IsActionPressed("testAction"))
        {
            SetInteractionPrompt("interact", UiPosition.Down);
        }

        if (input.IsActionPressed("testDesaction"))
        {
            StartFadeOutAnimation();
        }
    }
}

public enum UiPosition
{
    Top,
    Down
}
