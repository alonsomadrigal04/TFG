using Godot;
using System;
using Utility;

public partial class ItemInteractionMenu : Control
{
    [ExportGroup("Spinner Config")]
    [Export] Sprite2D Spinner { get; set; }
    [Export] Sprite2D[] Icons { get; set; }
    Vector2 ShowPosition { get; set; }
    [Export] Vector2 HiddenPosition { get; set; } = new Vector2(-200, -200);

    [ExportGroup("UI Elements")]
    [Export] VBoxContainer VBoxContainer { get; set; }
    [Export] Label ItemName { get; set; }
    [Export] AudioManager sounds;
    [Export] Sprite2D decorationCopperFrame;
    [Export] int rotationCopperSpeed = 3;
    [Export] Sprite2D ButtonSpinnerDisplay;

    int currentIndex = 0;
    Tween rotationTween;
    Tween appearanceTween;
    bool isMenuOpen = false;
    
    readonly float stepAngle = Mathf.DegToRad(120f);
    int activeIconInternalIndex = 0;

    public override void _Ready()
    {
        ShowPosition = Spinner.Position;
        Spinner.Position = HiddenPosition;
        VBoxContainer.Modulate = new Color(1, 1, 1, 0);
        VBoxContainer.Scale = Vector2.Zero;
        VBoxContainer.Visible = false;

        if (GetInventorySize() > 0)
        {
            InitialSetup();
        }
    }

    public override void _Process(double delta)
    {
        KeepIconsUpright();
        AnimateFrame(delta);
    }

    void AnimateFrame(double delta)
    {
        decorationCopperFrame.Rotation += Mathf.DegToRad(rotationCopperSpeed) * (float)delta;
    }


    public override void _Input(InputEvent @event)
    {
        if (GetInventorySize() <= 0) return;

        if (isMenuOpen)
        {
            if (@event.IsActionPressed("ScrollDown"))
                TryRotate(1);
            else if (@event.IsActionPressed("ScrollUp"))
                TryRotate(-1);
        }

        if (@event is InputEventMouseButton mouse && mouse.Pressed)
        {
            HandleClick(mouse.GlobalPosition);
        }
    }

    void HandleClick(Vector2 globalPos)
    {
        Rect2 spinnerRect = new(ButtonSpinnerDisplay.GlobalPosition - (ButtonSpinnerDisplay.GetRect().Size / 2), ButtonSpinnerDisplay.GetRect().Size);

        if (spinnerRect.HasPoint(globalPos))
        {
            if (!isMenuOpen) ToggleMenu(true);
        }
        else
        {
            if (isMenuOpen) ToggleMenu(false);
        }
    }

    void ToggleMenu(bool show)
    {
        isMenuOpen = show;
        appearanceTween?.Kill();
        appearanceTween = CreateTween().SetParallel(true);
        appearanceTween.SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

        float duration = 0.4f;

        if (show)
        {
            sounds.ClockDisplay.Play();
            VBoxContainer.Visible = true;
            appearanceTween.TweenProperty(Spinner, "position", ShowPosition, duration);
            
            appearanceTween.TweenDelegate<float>(v => VBoxContainer.Modulate = new Color(1, 1, 1, v), 0f, 1f, duration);
            appearanceTween.TweenProperty(VBoxContainer, "scale", Vector2.One, duration);
        }
        else
        {
            sounds.ClockHide.Play();
            appearanceTween.TweenProperty(Spinner, "position", HiddenPosition, duration);
            
            appearanceTween.TweenDelegate<float>(v => VBoxContainer.Modulate = new Color(1, 1, 1, v), 1f, 0f, duration);
            appearanceTween.TweenProperty(VBoxContainer, "scale", Vector2.Zero, duration);
            
            appearanceTween.Chain().TweenCallback(Callable.From(() => VBoxContainer.Visible = false));
        }
    }

    void TryRotate(int direction)
    {
        if (rotationTween != null && rotationTween.IsRunning()) return;
        RotateWheel(direction);
    }

    void RotateWheel(int direction)
    {
        int size = GetInventorySize();
        currentIndex = WrapIndex(currentIndex + direction, size);
        activeIconInternalIndex = Mathf.PosMod(activeIconInternalIndex + direction, 3);
        AnimateRotation(-stepAngle * direction);
    }

    void AnimateRotation(float angleDelta)
    {
        rotationTween?.Kill();
        sounds.Spining.Play();

        float targetRotation = Spinner.Rotation + angleDelta;

        rotationTween = CreateTween();
        rotationTween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        rotationTween.TweenProperty(Spinner, "rotation", targetRotation, 0.25f);
        
        rotationTween.Finished += () => {
            RefreshAllIcons();
            UpdateUI();
        };
    }

    int GetInventorySize() => ObjectDataBase.GetInventoryLength();

    void InitialSetup()
    {
        currentIndex = 0;
        activeIconInternalIndex = 0;
        RefreshAllIcons();
        UpdateUI();
    }

    void RefreshAllIcons()
    {
        int size = GetInventorySize();
        if (size == 0) return;
        SetIconData(activeIconInternalIndex, currentIndex);
        SetIconData((activeIconInternalIndex + 1) % 3, currentIndex + 1);
        SetIconData(Mathf.PosMod(activeIconInternalIndex - 1, 3), currentIndex - 1);
    }

    void SetIconData(int iconArrayIndex, int inventoryIndex)
    {
        int size = GetInventorySize();
        int actualInvIdx = WrapIndex(inventoryIndex, size);
        var data = ObjectDataBase.PlayerInventory[actualInvIdx];
        Icons[iconArrayIndex].Texture = data?.Icon;
        Icons[iconArrayIndex].Visible = data != null;
    }

    void UpdateUI() => ItemName.Text = ObjectDataBase.PlayerInventory[currentIndex]?.Name ?? "";

    static int WrapIndex(int index, int size) => size == 0 ? 0 : (index % size + size) % size;

    void KeepIconsUpright()
    {
        float inv = -Spinner.Rotation;
        foreach (var icon in Icons) icon.Rotation = inv;
    }

}