using Game;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Utility;

public partial class InventoryBehaviour : Control
{
    [Export] AnimationPlayer animationPlayer;
    public static InventoryBehaviour Instance { get; private set; }
    public override void _EnterTree() => Instance = this;
    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    [ExportGroup("Inventory Panels")]
    [Export] ColorRect BgInventory;
    [Export] ColorRect LeftPanel;
    [Export] ColorRect rightPanel;
    [Export] AudioStreamPlayer flipPageSounds;
    [Export] Label itemDescription;
    List<ItemSlotBehaviour> itemsSlots = [];

    int selectedIndex = 0;

    [Export] int visibleSlots = 4;
    [Export] Control objectContainer;
    [Export] ItemSlotBehaviour sampleItem;

    [Export] PackedScene itemSlotScene;

    Vector2 mainObjectPosition;
    Vector2 stepOffset;

    public bool inventoryOpened = false;
    bool animationsFinished = true;

    public override void _Ready()
    {
        Hide();
        itemDescription.Text = "";
        SetInitialPositions();
        SetUpObjects();
        sampleItem.QueueFree();
        animationPlayer.AnimationFinished += HandleAnimationFinished;
    }

    void SetUpObjects()
    {
        stepOffset = new Vector2(sampleItem.Size.X / 15f, sampleItem.Size.Y + sampleItem.Size.Y / 5f);
        mainObjectPosition = sampleItem.Position;

        // AddItem("sample");
        // AddItem("cafetera");


        RefreshInventoryUI();
    }

    void RefreshInventoryUI()
    {
        foreach (var slot in itemsSlots)
            slot.QueueFree();

        itemsSlots.Clear();

        foreach (ObjectData item in ObjectDataBase.PlayerInventory)
        {
            var slot = itemSlotScene.Instantiate<ItemSlotBehaviour>();
            slot.SetItem(item);
            objectContainer.AddChild(slot);
            itemsSlots.Add(slot);
        }

        UpdateSlots(true);
    }

    void SetInitialPositions()
    {
        BgInventory.Hide();

        DebugService.Register("Item Slot selected:",() => selectedIndex.ToString());

        Vector2 screenSize = ToolKit.GetScreenSize();

        LeftPanel.Position = -LeftPanel.Size;
        rightPanel.Position = screenSize + new Vector2(200, 0);
    }

    void HandleAnimationFinished(StringName animName)
    {
        if (animName == "OpenInventory")
        {
            animationPlayer.Play("IdleInventory");
            animationsFinished = true;
            BgInventory.Show();
        }

        if (animName == "CloseInventory")
        {
            animationsFinished = true;
            Hide();
            BgInventory.Hide();
        }
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("openInventory") && !inventoryOpened && animationsFinished && GameManager.GlobalInputsEnabled && /*GameManager.GameStarted && */!PauseManager.GamePaused)
        {
            Show();
            RefreshInventoryUI();
            UpdateSlots();
            animationPlayer.Play("OpenInventory");
            inventoryOpened = true;
            animationsFinished = false;
        }

        if (e.IsActionPressed("closeInventory") && inventoryOpened && animationsFinished)
        {
            animationPlayer.Play("CloseInventory");
            inventoryOpened = false;
            animationsFinished = false;
        }

        if (!inventoryOpened) return;

        if (e.IsActionPressed("moveDownInventory"))
        {
            MoveDown();
        }

        if (e.IsActionPressed("moveUpInventory"))
        {
            MoveUp();
        }
    }

    void MoveDown()
    {
        if (selectedIndex >= itemsSlots.Count - 1)
            return;
        flipPageSounds.Play();
        selectedIndex++;
        UpdateSlots();
    }

    void MoveUp()
    {
        if (selectedIndex <= 0)
            return;
        flipPageSounds.Play();
        selectedIndex--;
        UpdateSlots();
    }

    void UpdateSlots(bool instant = false)
    {
        if (itemsSlots.Count == 0) return;

        Tween tween = CreateTween();
        tween.SetParallel(true);

        for (int i = 0; i < itemsSlots.Count; i++)
        {
            int relativeIndex = i - selectedIndex;

            Vector2 targetPosition = mainObjectPosition + stepOffset * relativeIndex;

            ItemSlotBehaviour slot = itemsSlots[i];

            if (instant)
            {
                slot.Position = targetPosition;
                continue;
            }
            float scale = (i == selectedIndex) ? 1.2f : 0.9f;
            if(i == selectedIndex)
                AnimateDescription(slot);
            tween.SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);

            tween.TweenDelegate<Vector2>(
                value => slot.Scale = value,
                slot.Scale,
                Vector2.One * scale,
                0.4f
            );
            float alpha = Mathf.Clamp(1.0f - Mathf.Abs(relativeIndex) * 0.3f, 0.2f, 1f);
            tween.SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);
            tween.TweenDelegate<float>(
                value => slot.Modulate = new Color(1,1,1,value),
                slot.Modulate.A,
                alpha,
                0.2f
            );

            tween.TweenDelegate<Vector2>(
                value => slot.Position = value,
                slot.Position,
                targetPosition,
                0.2f
            );
        }
    }

    void AnimateDescription(ItemSlotBehaviour slot)
    {
        itemDescription.Text = slot.ObjectData.Description;

        Vector2 basePosition = itemDescription.Position;
        Color originalColor = itemDescription.Modulate;

        Tween tween = CreateTween();

        tween.SetParallel(true);

        tween.TweenDelegate<Vector2>(
            v => itemDescription.Scale = v,
            Vector2.One * 0.9f,
            Vector2.One * 1.08f,
            0.08f
        ).SetTrans(Tween.TransitionType.Back)
        .SetEase(Tween.EaseType.Out);

        tween.TweenDelegate<Color>(
            v => itemDescription.Modulate = v,
            originalColor,
            new Color(1f, 0.8f, 0.8f),
            0.08f
        );

        tween.SetParallel(false);

        for (int i = 0; i < 6; i++)
        {
            Vector2 offset = new(
                (float)GD.RandRange(-4f, 4f),
                (float)GD.RandRange(-4f, 4f)
            );

            tween.TweenDelegate<Vector2>(
                v => itemDescription.Position = v,
                basePosition,
                basePosition + offset,
                0.02f
            );
        }

        tween.TweenDelegate<Vector2>(
            v => itemDescription.Position = v,
            itemDescription.Position,
            basePosition,
            0.05f
        );

        tween.SetParallel(true);

        tween.TweenDelegate<Vector2>(
            v => itemDescription.Scale = v,
            itemDescription.Scale,
            Vector2.One,
            0.12f
        ).SetTrans(Tween.TransitionType.Sine)
        .SetEase(Tween.EaseType.Out);

        tween.TweenDelegate<Color>(
            v => itemDescription.Modulate = v,
            itemDescription.Modulate,
            Colors.White,
            0.12f
        );
    }


    public void AddItem(ObjectData item)
    {
        ObjectDataBase.AddObjectToInventory(item);
        UiStage.Instance.AddItemAnimation(item);
        UpdateSlots();
        GD.Print(ObjectDataBase.PlayerInventory);
    }
    public void AddItem(string item)
    {
        ObjectDataBase.AddObjectByName(item);
        ObjectData objectData = ObjectDataBase.GetObject(item);
        UiStage.Instance.AddItemAnimation(objectData);
    }
}