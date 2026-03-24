using Godot;
using System;

public partial class ItemInteractionMenu : Node
{
    [ExportGroup("Spinner Images")]
    [Export] Sprite2D Spinner { get; set; }
    [Export] Sprite2D IconItem1 { get; set; }
    [Export] Sprite2D IconItem2 { get; set; }
    [Export] Sprite2D IconItem3 { get; set; }
    Sprite2D CurrentVisibleIcon;
    [ExportGroup("IInteractiveScreen")]
    [Export] VBoxContainer VBoxContainer { get; set; }
    [Export] Button UseItem { get; set; }
    [Export] Label ItemName { get; set; }

    int CurrentIndex { get; set; }
    Tween RotationTween { get; set; }

    static float StepAngle => Mathf.DegToRad(120f);

    public override void _Ready()
    {
        HideMenu();
        UpdateVisibility();
        UpdateVisuals();
        CurrentVisibleIcon = IconItem1;
    }

    public override void _Process(double delta)
    {
        KeepIconsUpright();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ScrollDown"))
            TryRotate(1);

        if (@event.IsActionPressed("ScrollUp"))
            TryRotate(-1);

        if (@event is InputEventMouseButton mouse && mouse.Pressed)
            HandleClick(mouse.Position);
    }

    int GetInventorySize() => ObjectDataBase.GetInventoryLength();

    bool CanRotate()
    {
        return true;
    }

    void TryRotate(int direction)
    {
        if (!CanRotate())
            return;

        HideMenu();

        if(RotationTween != null && RotationTween.IsRunning())
            return;

        Rotate(direction);
    }

    void Rotate(int direction)
    {
        int size = GetInventorySize();

        if (size == 2)
        {
            CurrentIndex = CurrentIndex == 0 ? 1 : 0;
        }
        else
        {
            CurrentIndex = WrapIndex(CurrentIndex + direction);
        }

        UpdateCurrentVisibleIcon(direction);

        AnimateRotation(-StepAngle * direction);
    }

    void UpdateCurrentVisibleIcon(int direction)
    {
        if (direction > 0) // ScrollDown
        {
            // Icon1 -> Icon3 -> Icon2 -> Icon1
            if (CurrentVisibleIcon == IconItem1)
                CurrentVisibleIcon = IconItem3;
            else if (CurrentVisibleIcon == IconItem3)
                CurrentVisibleIcon = IconItem2;
            else
                CurrentVisibleIcon = IconItem1;
        }
        else // ScrollUp
        {
            // inverso
            if (CurrentVisibleIcon == IconItem1)
                CurrentVisibleIcon = IconItem2;
            else if (CurrentVisibleIcon == IconItem2)
                CurrentVisibleIcon = IconItem3;
            else
                CurrentVisibleIcon = IconItem1;
        }
    }


    void AnimateRotation(float delta)
    {
        RotationTween?.Kill();

        float targetRotation = Spinner.Rotation + delta;

        RotationTween = CreateTween();
        RotationTween.TweenProperty(Spinner, "rotation", targetRotation, 0.2f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        RotationTween.Finished += UpdateVisuals;
    }

    int WrapIndex(int index)
    {
        int size = GetInventorySize();
        if (size == 0) return 0;
        return (index % size + size) % size;
    }

    ObjectData GetItem(int index)
    {
        if (GetInventorySize() == 0) return null;
        return ObjectDataBase.PlayerInventory[WrapIndex(index)];
    }

    void UpdateVisuals()
    {
        int size = GetInventorySize();

        if (size == 0)
            return;

        var current = GetItem(CurrentIndex);

        ObjectData previous;
        ObjectData next;

        if (size == 1)
        {
            previous = current;
            next = current;
        }
        else if (size == 2)
        {
            previous = GetItem(CurrentIndex + 1);
            next = previous;
        }
        else
        {
            previous = GetItem(CurrentIndex - 1);
            next = GetItem(CurrentIndex + 1);
        }

        if (CurrentVisibleIcon == IconItem1)
        {
            SetIcon(IconItem1, current);
            SetIcon(IconItem2, next);
            SetIcon(IconItem3, previous);
        }
        else if (CurrentVisibleIcon == IconItem2)
        {
            SetIcon(IconItem2, current);
            SetIcon(IconItem3, next);
            SetIcon(IconItem1, previous);
        }
        else
        {
            SetIcon(IconItem3, current);
            SetIcon(IconItem1, next);
            SetIcon(IconItem2, previous);
        }

        ItemName.Text = current.Name;
    }

    void SetIcon(Sprite2D sprite, ObjectData data)
    {
        sprite.Visible = data != null;
        if (data != null)
            sprite.Texture = data.Icon;
    }

    void KeepIconsUpright()
    {
        float inverse = -Spinner.Rotation;

        IconItem1.Rotation = inverse;
        IconItem2.Rotation = inverse;
        IconItem3.Rotation = inverse;
    }

    void UpdateVisibility()
    {
        Spinner.Visible = GetInventorySize() > 0;
    }

    void HandleClick(Vector2 position)
    {
        if (IsClickInsideSpinner(position))
        {
            ShowMenu(position);
            return;
        }

        if (!IsClickInsideMenu(position))
            HideMenu();
    }

    bool IsClickInsideSpinner(Vector2 position)
    {
        return Spinner.GetRect().HasPoint(Spinner.ToLocal(position));
    }

    bool IsClickInsideMenu(Vector2 position)
    {
        return VBoxContainer.GetGlobalRect().HasPoint(position);
    }

    void ShowMenu(Vector2 position)
    {
        VBoxContainer.Visible = true;
        VBoxContainer.GlobalPosition = position;
    }

    void HideMenu()
    {
        VBoxContainer.Visible = false;
    }
}