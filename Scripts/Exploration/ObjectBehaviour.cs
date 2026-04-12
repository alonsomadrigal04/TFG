using Godot;
using System;

public partial class ObjectBehaviour : StaticBody3D, IInteractable
{
    [Export] public ObjectData ObjectInfo { get; private set; }
    [Export] Texture2D inspectImg;
    [Export] CollisionShape3D colliderBody;
    [Export] Area3D interactionArea;
    [Export] CollisionShape3D interactionRangeShape;
    [Export] InteractIconAnimation interactIcon;
    [Export] float interactionRange = 3f;

    public int priority { get; set; } = 10;

    private Vector3 size = Vector3.One;
    private Node3D instanceRoot;
    private MeshInstance3D meshInstance;

    public override void _Ready()
    {
        if (!ValidateReferences())
            return;

        if (!ValidateObjectInfo())
            return;

        InstantiateVisual();
        ConfigureColliderBody();
        ConfigureInteractionArea();
        ConfigureInteractIcon();
    }

    public override void _ExitTree()
    {
        if (interactionArea != null)
        {
            interactionArea.BodyEntered -= OnBodyEntered;
            interactionArea.BodyExited -= OnBodyExited;
        }
    }

    private bool ValidateReferences()
    {
        if (colliderBody == null || interactionArea == null || interactionRangeShape == null || interactIcon == null)
        {
            GD.PrintErr("[ObjectBehaviour] Missing exported references.");
            return false;
        }

        return true;
    }

    private bool ValidateObjectInfo()
    {
        if (ObjectInfo == null)
        {
            GD.PrintErr("[ObjectBehaviour] ObjectInfo is null.");
            return false;
        }

        if (ObjectInfo.Scene == null)
        {
            GD.PrintErr("[ObjectBehaviour] Object has no scene assigned.");
            return false;
        }

        return true;
    }

    private void InstantiateVisual()
    {
        instanceRoot = ObjectInfo.Scene.Instantiate<Node3D>();
        AddChild(instanceRoot);

        meshInstance = instanceRoot.FindChild("*", true, false) as MeshInstance3D;

        if (meshInstance == null)
        {
            GD.PrintErr("[ObjectBehaviour] No MeshInstance3D found in scene.");
            size = Vector3.One;
            return;
        }

        if (ObjectInfo.Material != null)
            meshInstance.MaterialOverride = ObjectInfo.Material;

        // instanceRoot.Scale = new Vector3(1f, 0.3f, 0.3f);

        size = meshInstance.GetAabb().Size * instanceRoot.Scale;
    }

    private void ConfigureColliderBody()
    {
        colliderBody.Position = new Vector3(0f, size.Y / 2f, 0f);

        if (colliderBody.Shape is BoxShape3D box)
            box.Size = size;
    }

    private void ConfigureInteractionArea()
    {
        interactionArea.Position = new Vector3(0f, size.Y / 2f, 0f);

        if (interactionRangeShape.Shape is SphereShape3D sphere)
            sphere.Radius = interactionRange;

        interactionArea.BodyEntered += OnBodyEntered;
        interactionArea.BodyExited += OnBodyExited;
    }

    private void ConfigureInteractIcon()
    {
        interactIcon.InitializeValues(size.Y, false, inspectImg);
    }

    public void Interact()
    {
        GameStateManager.Instance.ChangeState(State.Inspect, this);
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body is PlayerBehaviour player)
        {
            interactIcon.Activate();
            player.RegisterInteractable(this);
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (body is PlayerBehaviour player)
        {
            interactIcon.Desactivate();
            player.UnregisterInteractable(this);
        }
    }
}