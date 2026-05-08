using Godot;
using System;
using Utility;

public partial class InspectView : Control
{
    [Export] bool isDebugMode = false;
    [Export] Control debugScreen;

    [ExportGroup("Viewport")]
    [Export] SubViewportContainer viewportContainer;
    [Export] SubViewport viewport;
    [Export] Camera3D viewportCamera;
    [Export] CanvasLayer canvasLayer;

    [ExportGroup("Object Display")]
    [Export] ColorRect backgroundFilter;
    [Export] Node3D objectRoot;

    [ExportGroup("UI")]
    [Export] Button takeButton;
    [Export] Button leaveButton;
    [Export] Label commentaryLabel;
    [Export] Sprite2D thinkingSilhouette;

    [ExportGroup("Transitions")]
    [Export] float maxBlur = 1f;
    [Export] float maxDarkness = 0.5f;
    [Export] float transitionDuration = 2f;

    [Export] float targetObjectSize = 4f;
    [Export] float objectAppearDuration = 1f;

    [ExportGroup("Zoom")]
    [Export] float zoomFov = 40f;
    [Export] float defaultFov = 75f;
    [Export] float zoomLerpSpeed = 5f;

    [ExportGroup("Rotation")]
    [Export] Vector2 rotationSensitivity = new(0.5f, 0.5f);
    [Export] float rotationDamping = 6f;

    [ExportGroup("Effects")]
    [Export] PackedScene pickParticlesScene;
    [Export] float textRevealDuration = 1.5f;
    [Export] AudioStreamPlayer disappearSound;

    [ExportGroup("Animation Players")]
    [Export] AnimationPlayer transitionAnimationPlayer;
    [Export] AnimationPlayer hoverAnimationPlayer;

    public ObjectBehaviour CurrentObject { get; private set; }
    public bool CanInspect { get; private set; } = true;

    Node3D displayedObject;
    MeshInstance3D displayedMesh;

    bool isInputBlocked = true;
    bool isDragging;
    bool isZoomEnabled;

    Vector2 previousMousePosition;
    Vector2 rotationVelocity = Vector2.Zero;

    Tween commentaryTween;

    public override void _Ready()
    {
        if (!HasValidReferences())
            return;

        ConfigureViewport();
        ConfigureInitialState();
        ConnectSignals();

        GameStateManager.Instance.RegisterInspectView(this);
    }

    /// <summary>
    /// Displays an inspectable object inside the viewport.
    /// </summary>
    public void DisplayObject(ObjectBehaviour targetObject)
    {
        if (!CanInspect)
            return;

        if (!IsValidInspectable(targetObject))
            return;

        CurrentObject = targetObject;

        CreateDisplayInstance();
        ResetInspectionState();
        ShowInspectView();
        PlayOpenAnimation();
        AnimateCommentaryText();
    }

    public override void _Process(double delta)
    {
        if (isInputBlocked)
            return;

        float deltaTime = (float)delta;

        UpdateObjectRotation(deltaTime);
        UpdateZoom(deltaTime);
    }

    /// <summary>
    /// Validates required exported references.
    /// </summary>
    bool HasValidReferences()
    {
        bool missingReferences =
            backgroundFilter == null ||
            viewportContainer == null ||
            viewport == null ||
            viewportCamera == null ||
            objectRoot == null ||
            canvasLayer == null ||
            takeButton == null ||
            leaveButton == null;

        if (missingReferences)
        {
            GD.PrintErr("[InspectView] Missing exported references.");
            return false;
        }

        if (backgroundFilter.Material is not ShaderMaterial)
        {
            GD.PrintErr("[InspectView] BackgroundFilter requires ShaderMaterial.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Initializes viewport and shader configuration.
    /// </summary>
    void ConfigureViewport()
    {
        Vector2 screenSize = GetViewport().GetVisibleRect().Size;

        backgroundFilter.Size = screenSize;
        viewportContainer.Size = screenSize;
        viewport.Size = (Vector2I)screenSize;

        ShaderMaterial shader = (ShaderMaterial)backgroundFilter.Material;

        shader.SetShaderParameter("lod", 0f);
        shader.SetShaderParameter("darkness", 0f);

        objectRoot.Scale = Vector3.Zero;
    }

    /// <summary>
    /// Configures initial visibility and state.
    /// </summary>
    void ConfigureInitialState()
    {
        SetInspectViewVisible(false);

        debugScreen.Hide();
        Hide();
    }

    /// <summary>
    /// Connects UI and animation signals.
    /// </summary>
    void ConnectSignals()
    {
        leaveButton.Pressed += HandleLeavePressed;
        takeButton.Pressed += HandleTakePressed;

        leaveButton.MouseEntered += () => hoverAnimationPlayer.Play("LeaveHover");
        leaveButton.MouseExited += () => hoverAnimationPlayer.Play("LeaveUnHover");

        takeButton.MouseEntered += () => hoverAnimationPlayer.Play("TakeHover");
        takeButton.MouseExited += () => hoverAnimationPlayer.Play("TakeUnHover");

        transitionAnimationPlayer.AnimationFinished += OnAnimationComplete;
    }

    /// <summary>
    /// Checks if the object can be inspected.
    /// </summary>
    bool IsValidInspectable(ObjectBehaviour targetObject)
    {
        if (targetObject == null || targetObject.ObjectInfo == null)
        {
            GD.PrintErr("[InspectView] Invalid inspectable object.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates the viewport instance for the inspected object.
    /// </summary>
    void CreateDisplayInstance()
    {
        ClearCurrentDisplay();

        if (CurrentObject.ObjectInfo.Scene == null)
        {
            GD.PrintErr("[InspectView] Object scene is null.");
            return;
        }

        displayedObject = CurrentObject.ObjectInfo.Scene.Instantiate<Node3D>();
        objectRoot.AddChild(displayedObject);

        displayedMesh = displayedObject.FindChild("*", true, false) as MeshInstance3D;

        if (displayedMesh == null)
        {
            GD.PrintErr("[InspectView] No MeshInstance3D found.");
            return;
        }

        ApplyDisplayMaterial();
        ScaleAndPositionObject();
    }

    /// <summary>
    /// Removes the currently displayed object instance.
    /// </summary>
    void ClearCurrentDisplay()
    {
        if (displayedObject == null)
            return;

        displayedObject.QueueFree();

        displayedObject = null;
        displayedMesh = null;
    }

    /// <summary>
    /// Applies override material to the displayed mesh.
    /// </summary>
    void ApplyDisplayMaterial()
    {
        if (CurrentObject.ObjectInfo.Material == null)
            return;

        displayedMesh.MaterialOverride = CurrentObject.ObjectInfo.Material;
    }

    /// <summary>
    /// Scales and centers the displayed object.
    /// </summary>
    void ScaleAndPositionObject()
    {
        Vector3 meshSize = displayedMesh.GetAabb().Size;

        float largestAxis = Mathf.Max(meshSize.X, Mathf.Max(meshSize.Y, meshSize.Z));
        float scaleFactor = targetObjectSize / largestAxis;

        displayedObject.Scale = Vector3.One * scaleFactor;

        displayedObject.Position = new Vector3(
            0f,
            -(meshSize.Y * scaleFactor) / 2f,
            0f
        );
    }

    /// <summary>
    /// Resets interaction and camera state.
    /// </summary>
    void ResetInspectionState()
    {
        isInputBlocked = true;
        isDragging = false;
        isZoomEnabled = false;

        rotationVelocity = Vector2.Zero;

        objectRoot.Rotation = Vector3.Zero;
        objectRoot.Scale = Vector3.Zero;

        viewportCamera.Fov = defaultFov;
    }

    /// <summary>
    /// Starts commentary reveal animation.
    /// </summary>
    void AnimateCommentaryText()
    {
        commentaryLabel.VisibleRatio = 0f;
        commentaryLabel.Text = CurrentObject.ObjectInfo.Comentary;

        commentaryTween?.Kill();

        commentaryTween = CreateTween();

        commentaryTween.TweenProperty(
            commentaryLabel,
            "visible_ratio",
            1f,
            textRevealDuration
        );
    }

    /// <summary>
    /// Plays inspect opening transition.
    /// </summary>
    void PlayOpenAnimation()
    {
        transitionAnimationPlayer.Play("OpenInspectView");
    }

    /// <summary>
    /// Handles leave button interaction.
    /// </summary>
    void HandleLeavePressed()
    {
        CloseInspectView();
    }

    /// <summary>
    /// Handles take button interaction.
    /// </summary>
    void HandleTakePressed()
    {
        if (CurrentObject == null)
            return;

        CanInspect = false;

        transitionAnimationPlayer.Play("CloseInspectView");
        AnimateObjectPickup();

        GameStateManager.Instance.ChangeState(State.Explore);
    }


    /// <summary>
    /// Spawns pickup particle effect.
    /// </summary>
    void SpawnParticles(Vector3 position)
    {
        if (pickParticlesScene.Instantiate() is not GpuParticles3D particles)
            return;

        particles.GlobalPosition = position;

        AddChild(particles);

        particles.Emitting = true;
    }

    /// <summary>
    /// Handles inspect transition animation completion.
    /// </summary>
    void OnAnimationComplete(StringName animationName)
    {
        if (animationName == "OpenInspectView")
        {
            isInputBlocked = false;
            return;
        }

        if (animationName == "CloseInspectView")
        {
            SetInspectViewVisible(false);
        }
    }

    void AnimateObjectPickup()
    {
        if (CurrentObject == null)
            return;

        Node3D worldObject = CurrentObject;

        Tween tween = CreateTween();

        tween.SetParallel();
        tween.SetTrans(Tween.TransitionType.Quart);
        tween.SetEase(Tween.EaseType.Out);

        tween.TweenProperty(
            worldObject,
            "position:y",
            worldObject.Position.Y + 2.5f,
            0.8f
        );

        tween.TweenProperty(
            worldObject,
            "rotation_degrees:y",
            worldObject.RotationDegrees.Y + 360f,
            0.8f
        );

        tween.Chain();

        tween.TweenProperty(
            worldObject,
            "scale",
            new Vector3(0.01f, 0.01f, 0.01f),
            0.25f
        );

        tween.Finished += () =>
        {
            if (!isDebugMode)
                InventoryBehaviour.Instance.AddItem(CurrentObject.ObjectInfo);

            disappearSound.Play();

            SpawnParticles(worldObject.GlobalPosition);

            CurrentObject.QueueFree();
            CurrentObject = null;

            CanInspect = true;
        };
    }


    /// <summary>
    /// Closes the inspect interface.
    /// </summary>
    void CloseInspectView()
    {
        transitionAnimationPlayer.Play("CloseInspectView");

        GameStateManager.Instance.ChangeState(State.Explore);
    }

    /// <summary>
    /// Updates object rotation using mouse drag input.
    /// </summary>
    void UpdateObjectRotation(float deltaTime)
    {
        Vector2 currentMousePosition = GetViewport().GetMousePosition();

        if (Input.IsActionPressed("singleClick"))
        {
            HandleDraggingRotation(currentMousePosition, deltaTime);
            return;
        }

        ApplyRotationInertia(deltaTime);
    }

    /// <summary>
    /// Handles direct drag rotation.
    /// </summary>
    void HandleDraggingRotation(Vector2 currentMousePosition, float deltaTime)
    {
        if (!isDragging)
        {
            isDragging = true;
            previousMousePosition = currentMousePosition;
            return;
        }

        Vector2 mouseDelta = currentMousePosition - previousMousePosition;

        previousMousePosition = currentMousePosition;

        rotationVelocity = mouseDelta * rotationSensitivity * deltaTime;

        objectRoot.RotateY(mouseDelta.X * rotationSensitivity.Y * deltaTime);
        objectRoot.RotateX(mouseDelta.Y * rotationSensitivity.X * deltaTime);
    }

    /// <summary>
    /// Applies smooth rotational slowdown.
    /// </summary>
    void ApplyRotationInertia(float deltaTime)
    {
        isDragging = false;

        if (rotationVelocity.Length() <= 0.001f)
            return;

        objectRoot.RotateY(rotationVelocity.X);
        objectRoot.RotateX(rotationVelocity.Y);

        rotationVelocity = rotationVelocity.Lerp(
            Vector2.Zero,
            rotationDamping * deltaTime
        );
    }

    /// <summary>
    /// Updates camera zoom interpolation.
    /// </summary>
    void UpdateZoom(float deltaTime)
    {
        if (isZoomEnabled && Input.IsActionJustReleased("unZoom"))
            isZoomEnabled = false;

        if (!isZoomEnabled && Input.IsActionJustReleased("zoom"))
            isZoomEnabled = true;

        float targetFov = isZoomEnabled
            ? zoomFov
            : defaultFov;

        viewportCamera.Fov = Mathf.Lerp(
            viewportCamera.Fov,
            targetFov,
            zoomLerpSpeed * deltaTime
        );
    }

    /// <summary>
    /// Sets inspect view visibility.
    /// </summary>
    void SetInspectViewVisible(bool isVisible)
    {
        Visible = isVisible;
        canvasLayer.Visible = isVisible;
    }

    /// <summary>
    /// Shows inspect interface.
    /// </summary>
    void ShowInspectView()
    {
        SetInspectViewVisible(true);
    }
}