using Godot;
using System;
using System.ComponentModel;
using System.Security.AccessControl;
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
    [Export] MeshInstance3D objectMesh;

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
    [Export] float sliceDuration = 1.5f;

    [Export] Vector2 rotationSpeed = new(0.5f, 0.5f);
    [Export] float damping = 6f;
    [ExportGroup("UI IDLE ANIMATION")]
    [Export] PackedScene pickParticles;
    [Export] float uiFloatAmplitude = 5f;
    [Export] float uiFloatSpeed = 2f;
    [Export] float uiScalePop = 1.1f;
    [Export] float textRevealDuration = 1.5f;
    Tween textTween;

    float uiIdleTime = 0f;
    bool uiIdleActive = false;

    Vector2 thinkingInitialPos;
    Vector2 labelInitialPos;

    public ObjectBehaviour ObjectInspected { get; private set; }
    Vector2 screenSize;
    float safeX = 200f;
    float yOffset = 100f;
    Vector2 originalTakePosition;
    Vector2 originalLeavePosition;

    bool blockInput = true;
    bool dragging = false;
    bool isZoomed = false;
    public bool EnableSelect = true;
    Vector2 lastMousePos;
    Vector2 rotationVelocity = Vector2.Zero;
    Tween uiIdleTween;
    [Export] AnimationPlayer animationPlayer;
    [Export] AnimationPlayer hoverAnimationPlayer;


    [Export] AudioStreamPlayer disappearSound;

    public override void _Ready()
    {
        if (!ValidateReferences())
            return;

        InitializeLayout();
        ChangeVisibility(false);
        DebugScreen.Hide();

        GameStateManager.Instance.RegisterInspectView(this);

        leaveButton.Pressed += HandleLeaveAction;
        leaveButton.MouseEntered += () => hoverAnimationPlayer.Play("LeaveHover");
        takeButton.MouseEntered += () => hoverAnimationPlayer.Play("TakeHover");

        leaveButton.MouseExited += () => hoverAnimationPlayer.Play("LeaveUnHover");
        takeButton.MouseExited += () => hoverAnimationPlayer.Play("TakeUnHover");



        takeButton.Pressed += HandleTakeAction;
        animationPlayer.AnimationFinished += HandleAnimationEnded;
        //takeButton.Pressed += HandleTakeAction();

    }


    private void HandleAnimationEnded(StringName animName)
    {
        switch (animName)
        {
            case "OpenInspectView":
                StartThinkingIdle();
                break;
            case "CloseInspectView":
                ClosingInspectView();
                break;
            default:
                break;
        }
    }

    void ClosingInspectView()
    {
        ObjectInspected = null;
        ChangeVisibility(false);
    }


    void StartThinkingIdle()
    {
        animationPlayer.Play("IdleSiluette");
        blockInput = false;
    }

    void HandleLeaveAction() => HideObject();


    void HandleTakeAction()
    {
        EnableSelect = false;
        if(ObjectInspected == null) return;
        InventoryBehaviour.Instance.AddItem(ObjectInspected.ObjectInfo);
        HideObject();
        DisapearAnimation();
    }

    void DisapearAnimation()
    {
        if (ObjectInspected == null || ObjectInspected.visualMesh == null)
            return;
        var objectRef = ObjectInspected;

        var mesh = ObjectInspected.visualMesh;

        Tween tween = CreateTween();

        tween.SetParallel().SetTrans(Tween.TransitionType.Expo).SetEase(Tween.EaseType.Out);

        tween.TweenProperty( mesh, "position:y", mesh.Position.Y + 2.5f, 1f );
        tween.TweenProperty( mesh, "rotation_degrees:y", mesh.RotationDegrees.Y + 360f, 1f);
        tween.TweenProperty( mesh, "rotation_degrees:x", mesh.RotationDegrees.X + 45f, 1f);

        tween.Chain().SetParallel().SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);

        tween.TweenProperty( mesh, "scale", new Vector3(0.1f, 0.1f, 0.1f), 1.0f);

        tween.Chain().SetParallel().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);

        tween.TweenProperty(mesh,"scale",Vector3.Zero,0.2f);
        tween.Finished += () =>
        {
            disappearSound.Play();
            if (pickParticles.Instantiate() is GpuParticles3D particlesInstance)
            {
                particlesInstance.Position = objectRef.Position + new Vector3(0, 3f, 0);
                particlesInstance.Emitting = true;
                AddChild(particlesInstance);
            }
            objectRef.QueueFree();
            if (ObjectInspected == objectRef)
                ObjectInspected = null;

            EnableSelect = true;
        };
    }

    bool ValidateReferences()
    {
        if (backgroundFilter == null ||
            subViewportContainer == null ||
            subViewport == null ||
            objectContainer == null ||
            objectMesh == null ||
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
        screenSize = GetViewport().GetVisibleRect().Size;
        thinkingInitialPos = thinkingSilhouette.Position;
        labelInitialPos = comentaryLabel.Position;
        backgroundFilter.Size = screenSize;

        var shader = (ShaderMaterial)backgroundFilter.Material;
        shader.SetShaderParameter("lod", 0.0f);
        shader.SetShaderParameter("darkness", 0.0f);

        subViewportContainer.Size = screenSize;
        subViewport.Size = (Vector2I)screenSize;

        objectContainer.Scale = Vector3.Zero;
        originalTakePosition = takeButton.Position;
        originalLeavePosition = leaveButton.Position;
        takeButton.Position = new Vector2(-safeX, screenSize.Y - yOffset);
        leaveButton.Position = new Vector2(screenSize.X + safeX, screenSize.Y - yOffset);
    }

    public void DisplayObject(ObjectBehaviour obj)
    {
        if(!EnableSelect) return;
        if (obj == null || obj.ObjectInfo == null)
        {
            GD.PrintErr("[InspectView] Invalid object passed to Show.");
            return;
        }

        ObjectInspected = obj;

        SetupObject();
        ResetState();
        ChangeVisibility(true);
        ObjectIntroAnimation();
    }

    void HideObject()
    {
        ObjectLeftAnimation();
        GameStateManager.Instance.ChangeState(State.Explore);
    }

    void SetupObject()
    {
        objectMesh.Mesh = ObjectInspected.ObjectInfo.Model;
        objectMesh.MaterialOverride = ObjectInspected.ObjectInfo.Material;

        if (objectMesh.Mesh == null)
        {
            GD.PrintErr("[InspectView] Object has no mesh.");
            return;
        }

        Vector3 initialSize = objectMesh.GetAabb().Size;

        float maxDimension = Mathf.Max(initialSize.X,
                             Mathf.Max(initialSize.Y, initialSize.Z));

        if (maxDimension <= 0.0001f)
        {
            GD.PrintErr("[InspectView] Invalid mesh dimensions.");
            return;
        }

        float scale = idealSize / maxDimension;
        objectMesh.Scale = new Vector3(scale, scale, scale);
        objectMesh.Position = new Vector3(0f, -(initialSize.Y * scale) / 2f, 0f);
    }

    void ResetState()
    {
        comentaryLabel.VisibleRatio = 0;
        blockInput = true;
        dragging = false;
        rotationVelocity = Vector2.Zero;
        objectContainer.Rotation = Vector3.Zero;
        objectContainer.Scale = Vector3.Zero;
        viewportCamera.Fov = normalFov;
        thinkingSilhouette.Scale = Vector2.Zero;
        comentaryLabel.Scale = Vector2.Zero;
        thinkingSilhouette.Position = thinkingInitialPos;
        comentaryLabel.Position = labelInitialPos;

        uiIdleActive = false;
        uiIdleTime = 0f;
    }

    void ObjectIntroAnimation()
    {
        animationPlayer.Play("OpenInspectView");
        StartShowingText();
    }

    void StartShowingText()
    {
        if (comentaryLabel is not Label richText)
        {
            GD.PrintErr("ComentaryLabel must be a RichTextLabel to use visible_ratio.");
            return;
        }

        textTween?.Kill();

        richText.VisibleRatio = 0f;
        richText.Text = ObjectInspected.ObjectInfo.Comentary;

        textTween = CreateTween();
        textTween.TweenProperty(
            richText,
            "visible_ratio",
            1f,
            textRevealDuration
        ).SetTrans(Tween.TransitionType.Sine)
        .SetEase(Tween.EaseType.InOut);
    }

    void ObjectLeftAnimation()
    {
        animationPlayer.Play("CloseInspectView");
    }

    public override void _Process(double delta)
    {
        if (blockInput)
            return;

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

            Vector3 euler = objectContainer.RotationDegrees;
            euler.X = Mathf.Clamp(euler.X, -90f, 90f);
            objectContainer.RotationDegrees = euler;
        }
        else
        {
            dragging = false;

            if (rotationVelocity.Length() > 0.0001f)
            {
                objectContainer.RotateY(rotationVelocity.X);
                objectContainer.RotateX(rotationVelocity.Y);

                rotationVelocity =
                    rotationVelocity.Lerp(Vector2.Zero, damping * dt);
            }
        }
    }

    void HandleZoom(float dt)
    {
        if(isZoomed && Input.IsActionJustReleased("unZoom"))
            isZoomed = false;
        if(!isZoomed && Input.IsActionJustReleased("zoom"))
            isZoomed = true;
        float targetFov  = isZoomed ? zoomFov : normalFov;

        viewportCamera.Fov =
            Mathf.Lerp(viewportCamera.Fov, targetFov, zoomSpeed * dt);
    }

    void ChangeVisibility(bool value)
    {
        Visible = value;
        canvasLayer.Visible = value;
    }
}