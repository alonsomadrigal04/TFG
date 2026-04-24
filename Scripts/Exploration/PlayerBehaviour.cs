using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerBehaviour : CharacterBody3D
{
    [Export] private bool isDebugging = false;

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
    [Export] private MeshInstance3D shadowProxy;

    private readonly List<IInteractable> interactablesInRange = [];

    private readonly Dictionary<Direction, string> animationList = new()
    {
        { Direction.Back,  "Front" },
        { Direction.Front, "Front" },
        { Direction.Left,  "Left"  },
        { Direction.Right, "Right" },
        { Direction.None,  "Idle"  }
    };

    private Direction currentInputDirection = Direction.None;
    private Direction currentMovementDirection = Direction.None;
    private Tween rotationTween;
    private int transitionToken = 0;

    public bool IsBlocked
    {
        get => GameManager.Instance.IsDialogueActive;
        set => GameManager.Instance.IsDialogueActive = value;
    }

    public override void _Ready()
    {
        if (animatedSprite3D != null)
            animatedSprite3D.SpeedScale = speed / 2f;

        shadowProxy?.Show();
        SetAnimation(Direction.None);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isDebugging || !IsBlocked)
        {
            ManageMovement(delta);
            ManageInteractions();
        }
        if (IsBlocked)
        {
            SetAnimation(Direction.None);
            dustParticles.Emitting = false;
            
        }
    }

    private void ManageMovement(double delta)
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

        if (newInputDirection != currentInputDirection)
        {
            currentInputDirection = newInputDirection;
            dustParticles.ChangeDirection(newInputDirection);
            UpdateVisualState(newInputDirection);
        }

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
        Direction result = Direction.None;

        if (Input.IsActionPressed("moveRight"))
        {
            result = Direction.Right;
            move.X += 1;
        }
        if (Input.IsActionPressed("moveLeft"))
        {
            result = Direction.Left;
            move.X -= 1;
        }
        if (Input.IsActionPressed("moveFar"))
        {
            result = Direction.Back;
            move.Z -= 1;
        }
        if (Input.IsActionPressed("moveNear"))
        {
            result = Direction.Front;
            move.Z += 1;
        }

        return result;
    }

    private void UpdateVisualState(Direction newDirection)
    {
        if (newDirection == currentMovementDirection)
            return;

        int myToken = ++transitionToken;
        KillRotationTween();

        if (newDirection == Direction.None)
        {
            if (currentMovementDirection == Direction.None)
            {
                SetAnimation(Direction.None);
                return;
            }

            PlayStopSequence(currentMovementDirection, myToken);
            currentMovementDirection = Direction.None;
            return;
        }

        PlayMoveSequence(currentMovementDirection, newDirection, myToken);
        currentMovementDirection = newDirection;
    }

    private void PlayMoveSequence(Direction from, Direction to, int token)
    {
        float targetYaw = DirectionToYaw(to);
        float frontYaw = 0f;

        if (to == Direction.Front)
        {
            SetAnimation(Direction.Front);
            Rotation = new Vector3(Rotation.X, frontYaw, Rotation.Z);
            return;
        }

        Tween firstTurn = CreateTween();
        rotationTween = firstTurn;
        firstTurn.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
        firstTurn.TweenProperty(this, "rotation:y", targetYaw, 0.14f);

        firstTurn.Finished += () =>
        {
            if (token != transitionToken)
                return;

            SetAnimation(to);

            Tween secondTurn = CreateTween();
            rotationTween = secondTurn;
            secondTurn.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
            secondTurn.TweenProperty(this, "rotation:y", frontYaw, 0.14f);
        };
    }

    private void PlayStopSequence(Direction from, int token)
    {
        float targetYaw = DirectionToYaw(from);
        float frontYaw = 0f;

        if (from == Direction.Front)
        {
            SetAnimation(Direction.None);
            Rotation = new Vector3(Rotation.X, frontYaw, Rotation.Z);
            return;
        }

        Tween firstTurn = CreateTween();
        rotationTween = firstTurn;
        firstTurn.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
        firstTurn.TweenProperty(this, "rotation:y", targetYaw, 0.14f);

        firstTurn.Finished += () =>
        {
            if (token != transitionToken)
                return;

            SetAnimation(Direction.None);

            Tween secondTurn = CreateTween();
            rotationTween = secondTurn;
            secondTurn.SetTrans(Tween.TransitionType.Quart).SetEase(Tween.EaseType.Out);
            secondTurn.TweenProperty(this, "rotation:y", frontYaw, 0.14f);
        };
    }

    private void KillRotationTween()
    {
        if (rotationTween != null && IsInstanceValid(rotationTween))
        {
            rotationTween.Kill();
            rotationTween = null;
        }
    }

    private void SetAnimation(Direction destiny)
    {
        if (animatedSprite3D == null)
            return;

        if (!animationList.TryGetValue(destiny, out string newAnimation))
            newAnimation = "Idle";

        if (animatedSprite3D.Animation != newAnimation)
            animatedSprite3D.Animation = newAnimation;

        bool flip = ShouldFlip(destiny);

        animatedSprite3D.FlipH = flip;
    }

    private static bool ShouldFlip(Direction dir) => dir == Direction.Right;

    private static float DirectionToYaw(Direction dir)
    {
        return dir switch
        {
            Direction.Front => 0f,
            Direction.Right => Mathf.DegToRad(90f),
            Direction.Back  => Mathf.DegToRad(180f),
            Direction.Left  => Mathf.DegToRad(-90f),
            _ => 0f
        };
    }

    private void ManageInteractions()
    {
        if (Input.IsActionJustPressed("interact") && interactablesInRange.Count > 0)
        {
            IInteractable element = interactablesInRange.OrderByDescending(i => i.priority).FirstOrDefault();
            element?.Interact();
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