using Godot;
using System;

public partial class ObjectBehaviour : StaticBody3D, IInteractable
{
    [Export] public ObjectData ObjectInfo { get; private set; }
    [Export] Texture2D inspectImg;
    [Export] public MeshInstance3D visualMesh;
    [Export] CollisionShape3D colliderBody;
    [Export] Area3D interactionArea;
    [Export] CollisionShape3D interactionRangeShape;
    [Export] InteractIconAnimation interactIcon;
    [Export] float interactionRange = 3f;
    

    public int priority { get; set; } = 10;

    Vector3 size;

    public override void _Ready()
    {
        if (!ValidateReferences())
            return;

        if (!ValidateObjectInfo())
            return;

        ConfigureVisualMesh();
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
        if (visualMesh == null || colliderBody == null || interactionArea == null || interactionRangeShape == null || interactIcon == null)
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

        if (ObjectInfo.Model == null)
        {
            GD.PrintErr("[ObjectBehaviour] Object has no model assigned.");
            size = Vector3.Zero;
            return false;
        }

        return true;
    }

    private void ConfigureVisualMesh()
    {
        if (ObjectInfo.Material == null)
            GD.PrintErr("[ObjectBehaviour] Object has no material assigned.");

        visualMesh.Mesh = ObjectInfo.Model;
        visualMesh.Scale = new Vector3(0.3f, 0.3f, 0.3f);
        visualMesh.MaterialOverride = ObjectInfo.Material;

        size = visualMesh.GetAabb().Size * visualMesh.Scale;
    }

    private void ConfigureColliderBody()
    {
        colliderBody.Position = new Vector3(0f, size.Y / 2f, 0f);

        if (colliderBody.Shape is BoxShape3D box)
            box.Size = size;
    }

	/// <summary>
	/// Configures the interaction area by setting its position and shape based on the object's size and the specified interaction range.
	/// </summary>
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