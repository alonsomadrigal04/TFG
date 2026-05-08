using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerBehaviour : CharacterBody3D
{
    [Export] public bool isDebugging = false;

    [ExportGroup("MOVEMENT SETTINGS")]
    [Export] private float speed = 5f;
    [Export] public float Speed
    {
        get => speed;
        set
        {
            speed = value;
            if (animatedSprite3D != null)
                animatedSprite3D.SpeedScale = value / 2f;
        }
    }

    [Export] public float Aceleration { get; set; } = 50f;

    [Export] public AnimatedSprite3D animatedSprite3D;

    [ExportGroup("SFX")]
    [Export] public InteractFlavourAnimation exclamationSprite;
    [Export] public DustParticles dustParticles;
    [Export] MeshInstance3D shadowProxy;
    [Export] CollisionShape3D bodyCollider;

    readonly List<IInteractable> interactablesInRange = [];

    readonly Dictionary<Direction, string> walkAnimationList = new()
    {
        { Direction.Back,  "Front" },
        { Direction.Front, "Front" },
        { Direction.Left,  "Left"  },
        { Direction.Right, "Right" }
    };

    readonly Dictionary<Direction, string> idleAnimationList = new()
    {
        { Direction.Back,  "IdleFront" },
        { Direction.Front, "IdleFront" },
        { Direction.Left,  "IdleLeft" },
        { Direction.Right, "IdleRight" }
    };

    Direction currentInputDirection = Direction.None;
    Direction currentMovementDirection = Direction.None;
    Direction idleFacingDirection = Direction.Front;
    Direction facingDirection = Direction.Front;
    bool isTurning = false;

    int lastFrame = -1;
    readonly Dictionary<Direction, int[]> stepFrames = new()
    {
        { Direction.Front, new[] { 0, 23, 48, 70 } },
        { Direction.Left, new[] { 0, 32 } },
        { Direction.Right, new[] { 0, 32 } },
        { Direction.Back, new[] { 0, 23, 48, 70 } },
    };


    Tween rotationTween;
    int transitionToken = 0;

    public bool IsBlocked
    {
        get
        {
            if (!isDebugging)
                return GameManager.Instance.IsDialogueActive;

            return false;
        }

        set
        {
            if(!isDebugging)
                GameManager.Instance.IsDialogueActive = value;
        }

    }

    public override void _Ready()
    {
        if (animatedSprite3D != null)
            animatedSprite3D.SpeedScale = speed / 2f;
        shadowProxy?.Show();
        SetIdleAnimation(idleFacingDirection);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isDebugging || !IsBlocked)
        {
            ManageMovement(delta);
            HandleFootsteps();
            ManageInteractions();
        }
    }

    void HandleFootsteps()
    {
        if (currentInputDirection == Direction.None)
            return;

        int frame = animatedSprite3D.Frame;

        if (frame == lastFrame)
            return;

        lastFrame = frame;

        if (IsStepFrame(frame))
        {
            PlayFootstep();
        }
    }

    void PlayFootstep()
    {
        if(isDebugging) return;
        AudioManager.Instance.WalkingSounds.Play();
    }

    bool IsStepFrame(int frame)
    {
        if (stepFrames.TryGetValue(currentInputDirection, out int[] frameList))
        {
            return frameList.Contains(frame);
        }

        return frame == 2 || frame == 6;
    }

    void ManageMovement(double delta)
    {
        Vector3 move = Vector3.Zero;
        Direction newInputDirection = ReadInputDirection(ref move);

        if (move != Vector3.Zero)
        {
            move = move.Normalized();
            dustParticles.Emitting = true;
        }
        else
        {
            dustParticles.Emitting = false;
        }

        currentInputDirection = newInputDirection;
        dustParticles.ChangeDirection(newInputDirection);
        UpdateVisualState(newInputDirection);

        Vector3 desiredVelocity = move * Speed;

        Velocity = new Vector3(
            Mathf.MoveToward(Velocity.X, desiredVelocity.X, Aceleration * (float)delta),
            Velocity.Y,
            Mathf.MoveToward(Velocity.Z, desiredVelocity.Z, Aceleration * (float)delta)
        );

        MoveAndSlide();
    }

    Direction ReadInputDirection(ref Vector3 move)
    {
        if (Input.IsActionPressed("moveRight"))
        {
            move.X = 1;
            return Direction.Right;
        }
        if (Input.IsActionPressed("moveLeft"))
        {
            move.X = -1;
            return Direction.Left;
        }
        if (Input.IsActionPressed("moveFar"))
        {
            move.Z = -1;
            return Direction.Back;
        }
        if (Input.IsActionPressed("moveNear"))
        {
            move.Z = 1;
            return Direction.Front;
        }

        return Direction.None;
    }

    void UpdateVisualState(Direction newDirection)
    {
        
        if (isTurning)
            return;

        if (newDirection == Direction.None)
        {
            SetIdleAnimation(facingDirection);
            return;
        }

        if (newDirection == facingDirection)
        {
            SetWalkAnimation(facingDirection);
            return;
        }
        RotateTo(newDirection);
    }

    void RotateTo(Direction newDirection)
    {
        isTurning = true;

        KillRotationTween();

        float turnAmount = Mathf.DegToRad(90f);

        float directionSign = GetTurnSign(facingDirection, newDirection);

        float targetYaw = directionSign * turnAmount;


        rotationTween = CreateTween();
        rotationTween.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);

        rotationTween.TweenProperty(this, "rotation:y", targetYaw, 0.12f);

        rotationTween.Finished += () =>
        {
            facingDirection = newDirection;

            SetWalkAnimation(facingDirection);

            Tween back = CreateTween();
            rotationTween = back;
            back.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
            back.TweenProperty(this, "rotation:y", 0f, 0.12f);

            back.Finished += () =>
            {
                isTurning = false;
            };
        };
    }

    float GetTurnSign(Direction from, Direction to)
    {

        if (to == Direction.Right)
            return 1f;

        if (to == Direction.Left)
            return -1f;

        if (from == Direction.Left)
            return 1f;

        if (from == Direction.Right)
            return -1f;

        return 1f;
    }


    void KillRotationTween()
    {
        if (rotationTween != null && IsInstanceValid(rotationTween))
        {
            rotationTween.Kill();
            rotationTween = null;
        }
    }

    void SetWalkAnimation(Direction destiny)
    {
        ApplyAnimation(destiny, walkAnimationList, flipRight: destiny == Direction.Right);
    }

    void SetIdleAnimation(Direction destiny)
    {
        ApplyAnimation(destiny, idleAnimationList, flipRight: destiny == Direction.Right);
    }

    void ApplyAnimation(Direction destiny, Dictionary<Direction, string> animationMap, bool flipRight)
    {
        lastFrame = -1;
        if (animatedSprite3D == null)
            return;

        if (!animationMap.TryGetValue(destiny, out string newAnimation))
            newAnimation = "IdleFront";

        if (animatedSprite3D.Animation != newAnimation)
            animatedSprite3D.Animation = newAnimation;

        animatedSprite3D.FlipH = flipRight;
    }

    static float DirectionToYaw(Direction dir)
    {
        return dir switch
        {
            Direction.Front => 0f,
            Direction.Right => Mathf.DegToRad(90f),
            Direction.Back => Mathf.DegToRad(180f),
            Direction.Left => Mathf.DegToRad(-90f),
            _ => 0f
        };
    }

    void ManageInteractions()
    {
        if (Input.IsActionJustPressed("interact") && interactablesInRange.Count > 0)
        {
            IInteractable element = interactablesInRange.OrderByDescending(i => i.priority).FirstOrDefault();
            element?.Interact();
            SetIdleAnimation(currentInputDirection);
        }

        if (Input.IsActionJustPressed("sprint"))
            Speed = 7f;

        if (Input.IsActionJustReleased("sprint"))
            Speed = 5f;
    }

    public void RegisterInteractable(IInteractable newElement)
    {
        if (!interactablesInRange.Contains(newElement))
        {
            interactablesInRange.Add(newElement);
            exclamationSprite.ShowInteraction();
        }
    }

    public void UnregisterInteractable(IInteractable element)
    {
        interactablesInRange.Remove(element);

        if (interactablesInRange.Count <= 0)
            exclamationSprite.HideInteraction();
    }

    public void SetInputBlocked(bool newBlockState)
    {
        IsBlocked = newBlockState;
    }
}

public enum Direction
{
    Right,
    Left,
    Front,
    Back,
    None
}