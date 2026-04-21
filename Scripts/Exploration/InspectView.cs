using Godot;
using System;
using Utility;

public partial class InspectView : Control
{
    [Export] Control DebugScreen;

    [ExportGroup("VIEWPORTS")]
    [Export] SubViewportContainer subViewportContainer;
    [Export] Camera3D viewportCamera;
    [Export] SubViewport subViewport;
    [Export] CanvasLayer canvasLayer;

    [ExportGroup("OBJECT")]
    [Export] ColorRect backgroundFilter;
    [Export] Node3D objectContainer;

    [ExportGroup("INSPECT VIEW FIELDS")]
    [Export] Button takeButton;
    [Export] Button leaveButton;
    [Export] Label comentaryLabel;
    [Export] Sprite2D thinkingSilhouette;

    [ExportGroup("ANIMATION SETTINGS")]
    [Export] float maxBlur = 1f;
    [Export] float maxDarkness = 0.5f;
    [Export] float transitionDuration = 2f;

    [Export] float idealSize = 4f;
    [Export] float apparitionDuration = 1f;

    [Export] float zoomFov = 40f;
    [Export] float normalFov = 75f;
    [Export] float zoomSpeed = 5f;

    [Export] Vector2 rotationSpeed = new(0.5f, 0.5f);
    [Export] float damping = 6f;

    [ExportGroup("UI IDLE ANIMATION")]
    [Export] PackedScene pickParticles;
    [Export] float textRevealDuration = 1.5f;

    [Export] AnimationPlayer animationPlayer;
    [Export] AnimationPlayer hoverAnimationPlayer;
    [Export] AudioStreamPlayer disappearSound;

    public ObjectBehaviour ObjectInspected { get; private set; }

    Node3D currentInstance;
    MeshInstance3D currentMesh;

    float interactionRange = 3f;
    bool blockInput = true;
    bool dragging = false;
    bool isZoomed = false;
    public bool EnableSelect = true;

    Vector2 lastMousePos;
    Vector2 rotationVelocity = Vector2.Zero;

    Tween textTween;

    public override void _Ready()
    {
        if (!ValidateReferences())
            return;

        InitializeLayout();
        ChangeVisibility(false);
        DebugScreen.Hide();
        Hide();

        GameStateManager.Instance.RegisterInspectView(this);

        leaveButton.Pressed += HandleLeaveAction;
        takeButton.Pressed += HandleTakeAction;

        leaveButton.MouseEntered += () => hoverAnimationPlayer.Play("LeaveHover");
        takeButton.MouseEntered += () => hoverAnimationPlayer.Play("TakeHover");
        leaveButton.MouseExited += () => hoverAnimationPlayer.Play("LeaveUnHover");
        takeButton.MouseExited += () => hoverAnimationPlayer.Play("TakeUnHover");

        animationPlayer.AnimationFinished += HandleAnimationEnded;
    }

    bool ValidateReferences()
    {
        if (backgroundFilter == null ||
            subViewportContainer == null ||
            subViewport == null ||
            objectContainer == null ||
            viewportCamera == null ||
            canvasLayer == null ||
            takeButton == null ||
            leaveButton == null)
        {
            GD.PrintErr("[InspectView] Missing exported references.");
            return false;
        }

        if (backgroundFilter.Material is not ShaderMaterial)
        {
            GD.PrintErr("[InspectView] Background filter requires ShaderMaterial.");
            return false;
        }

        return true;
    }

    void InitializeLayout()
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;

        backgroundFilter.Size = screenSize;

        var shader = (ShaderMaterial)backgroundFilter.Material;
        shader.SetShaderParameter("lod", 0.0f);
        shader.SetShaderParameter("darkness", 0.0f);

        subViewportContainer.Size = screenSize;
        subViewport.Size = (Vector2I)screenSize;

        objectContainer.Scale = Vector3.Zero;
    }

    public void DisplayObject(ObjectBehaviour obj)
    {
        if (!EnableSelect) return;

        if (obj == null || obj.ObjectInfo == null)
        {
            GD.PrintErr("[InspectView] Invalid object.");
            return;
        }

        ObjectInspected = obj;

        SetupObject();
        ResetState();
        ChangeVisibility(true);
        animationPlayer.Play("OpenInspectView");
        StartShowingText();
    }

    void SetupObject()
    {
        if (currentInstance != null)
        {
            currentInstance.QueueFree();
            currentInstance = null;
            currentMesh = null;
        }

        if (ObjectInspected.ObjectInfo.Scene == null)
        {
            GD.PrintErr("[InspectView] Object has no scene.");
            return;
        }

        currentInstance = ObjectInspected.ObjectInfo.Scene.Instantiate<Node3D>();
        objectContainer.AddChild(currentInstance);

        currentMesh = currentInstance.FindChild("*", true, false) as MeshInstance3D;

        if (currentMesh == null)
        {
            GD.PrintErr("[InspectView] No MeshInstance3D found.");
            return;
        }

        if (ObjectInspected.ObjectInfo.Material != null)
            currentMesh.MaterialOverride = ObjectInspected.ObjectInfo.Material;

        Vector3 size = currentMesh.GetAabb().Size;

        float maxDim = Mathf.Max(size.X, Mathf.Max(size.Y, size.Z));
        float scale = idealSize / maxDim;

        currentInstance.Scale = new Vector3(scale, scale, scale);
        currentInstance.Position = new Vector3(0f, -(size.Y * scale) / 2f, 0f);
    }

    void ResetState()
    {
        blockInput = true;
        dragging = false;
        rotationVelocity = Vector2.Zero;
        objectContainer.Rotation = Vector3.Zero;
        objectContainer.Scale = Vector3.Zero;
        viewportCamera.Fov = normalFov;
    }

    void StartShowingText()
    {
        comentaryLabel.VisibleRatio = 0f;
        comentaryLabel.Text = ObjectInspected.ObjectInfo.Comentary;

        textTween?.Kill();

        textTween = CreateTween();
        textTween.TweenProperty(
            comentaryLabel,
            "visible_ratio",
            1f,
            textRevealDuration
        );
    }

    void HandleLeaveAction()
    {
        HideObject();
    }

    void HandleTakeAction()
    {
        EnableSelect = false;

        if (ObjectInspected == null) return;

        InventoryBehaviour.Instance.AddItem(ObjectInspected.ObjectInfo);
        HideObject();
        DisappearAnimation();
    }

    void DisappearAnimation()
    {
        if (currentInstance == null)
            return;

        var objRef = ObjectInspected;
        var mesh = currentInstance;

        Tween tween = CreateTween();

        tween.SetParallel();
        tween.TweenProperty(mesh, "position:y", mesh.Position.Y + 2.5f, 1f);
        tween.TweenProperty(mesh, "rotation_degrees:y", mesh.RotationDegrees.Y + 360f, 1f);

        tween.Chain();
        tween.TweenProperty(mesh, "scale", Vector3.Zero, 0.5f);

        tween.Finished += () =>
        {
            disappearSound.Play();

            if (pickParticles.Instantiate() is GpuParticles3D particles)
            {
                particles.Position = objRef.Position + new Vector3(0, 3f, 0);
                particles.Emitting = true;
                AddChild(particles);
            }

            objRef.QueueFree();
            ObjectInspected = null;
            EnableSelect = true;
        };
    }

    void HandleAnimationEnded(StringName anim)
    {
        if (anim == "OpenInspectView")
            blockInput = false;

        if (anim == "CloseInspectView")
        {
            ObjectInspected = null;
            ChangeVisibility(false);
        }
    }

    void HideObject()
    {
        animationPlayer.Play("CloseInspectView");
        GameStateManager.Instance.ChangeState(State.Explore);
    }

    public override void _Process(double delta)
    {
        if (blockInput) return;

        float dt = (float)delta;
        HandleRotation(dt);
        HandleZoom(dt);
    }

    void HandleRotation(float dt)
    {
        Vector2 mousePos = GetViewport().GetMousePosition();

        if (Input.IsActionPressed("singleClick"))
        {
            if (!dragging)
            {
                dragging = true;
                lastMousePos = mousePos;
                return;
            }

            Vector2 deltaMouse = mousePos - lastMousePos;
            lastMousePos = mousePos;

            rotationVelocity = deltaMouse * rotationSpeed * dt;

            objectContainer.RotateY(deltaMouse.X * rotationSpeed.Y * dt);
            objectContainer.RotateX(deltaMouse.Y * rotationSpeed.X * dt);
        }
        else
        {
            dragging = false;

            if (rotationVelocity.Length() > 0.001f)
            {
                objectContainer.RotateY(rotationVelocity.X);
                objectContainer.RotateX(rotationVelocity.Y);
                rotationVelocity = rotationVelocity.Lerp(Vector2.Zero, damping * dt);
            }
        }
    }

    void HandleZoom(float dt)
    {
        if (isZoomed && Input.IsActionJustReleased("unZoom"))
            isZoomed = false;

        if (!isZoomed && Input.IsActionJustReleased("zoom"))
            isZoomed = true;

        float target = isZoomed ? zoomFov : normalFov;
        viewportCamera.Fov = Mathf.Lerp(viewportCamera.Fov, target, zoomSpeed * dt);
    }

    void ChangeVisibility(bool value)
    {
        Visible = value;
        canvasLayer.Visible = value;
    }
}