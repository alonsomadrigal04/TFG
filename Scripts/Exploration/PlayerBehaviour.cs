using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class PlayerBehaviour : CharacterBody3D
{
    [ExportGroup("MOVEMENT SETTINGS")]
    private float speed = 5f;
    [Export] public float Speed 
    { 
        get { return speed; }
        set
        {
            speed = value;
            animatedSprite3D.SpeedScale = value / 2;
        } 
    }
    [Export] public float Aceleration { get; set; } = 50f;
    [Export] public AnimatedSprite3D animatedSprite3D;

    [ExportGroup("SFX")]
    [Export] public InteractFlavourAnimation exclamationSprite;
    [Export] public DustParticles dustParticles;
    [Export] MeshInstance3D shadowProxy;
    Direction lastDirection = Direction.Front;
    public bool IsBlocked
    {
        get => GameManager.Instance.IsDialogueActive;
        set => GameManager.Instance.IsDialogueActive = value;
    }
    readonly List<IInteractable> interactablesInRange = [];

    public override void _Ready()
    {
        animatedSprite3D.SpeedScale = speed;
        shadowProxy.Show();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsBlocked)
        {
            ManageMovement(delta);
            ManageInteractions();
        }
    }
    private void ManageMovement(double delta)
    {
        Velocity += GetGravity() * (float)delta;

        var direction = Vector3.Zero;
        var newDirection = lastDirection;

        if (Input.IsActionPressed("moveRight"))
        {
            animatedSprite3D.Animation = "Right";
            newDirection = Direction.Right;
            direction.X += 1;
        }
        if (Input.IsActionPressed("moveLeft"))
        {
            animatedSprite3D.Animation = "Left";
            newDirection = Direction.Left;
            direction.X -= 1;
        }
        if (Input.IsActionPressed("moveFar"))
        {
            newDirection = Direction.Back;
            direction.Z -= 1;
        }
        if (Input.IsActionPressed("moveNear"))
        {
            animatedSprite3D.Animation = "Front";
            newDirection = Direction.Front;
            direction.Z += 1;
        }

        if (direction != Vector3.Zero) 
        {
            direction = direction.Normalized();
            dustParticles.Emitting = true;
        }

        if(direction == Vector3.Zero)
        {
            animatedSprite3D.Animation = "Idle";
            dustParticles.Emitting = false;
        }

        if (newDirection != lastDirection)
        {
            lastDirection = newDirection;
            dustParticles.ChangeDirection(newDirection);
        } 
            

        Velocity = new Vector3(direction.X * Speed, Velocity.Y, direction.Z * Speed);
        MoveAndSlide();
    }

    void ManageInteractions()
    {
        if (Input.IsActionJustPressed("interact") && interactablesInRange.Count > 0)
        {
            GD.Print("Interact");
            IInteractable element = interactablesInRange.OrderByDescending(i => i.priority).FirstOrDefault();
            element?.Interact();
        }
        if (Input.IsActionJustPressed("sprint"))
        {
            Speed = 7;
        }
        if (Input.IsActionJustReleased("sprint"))
        {
            Speed = 5;
        }

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
        if(interactablesInRange.Count <= 0)
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
    Back
}