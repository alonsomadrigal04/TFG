using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class PlayerBehaviour : CharacterBody3D
{
    [ExportGroup("MOVEMENT SETTINGS")]
    [Export] public float Speed { get; set; } = 5f;
    [Export] public float Aceleration { get; set; } = 50f;
    [Export] public AnimatedSprite3D animatedSprite3D;

    [ExportGroup("SFX")]
    [Export] public InteractFlavourAnimation exclamationSprite;
    [Export] public DustParticles dustParticles;
    Direction lastDirection = Direction.Front;
    bool isBlocked = false;
    readonly List<IInteractable> interactablesInRange = [];

    public override void _PhysicsProcess(double delta)
    {
        if (!isBlocked)
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
            newDirection = Direction.Right;
            direction.X += 1;
        }
        if (Input.IsActionPressed("moveLeft"))
        {
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
            animatedSprite3D.Animation = "Walk";
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
            IInteractable element = interactablesInRange.OrderByDescending(i => i.priority).FirstOrDefault();
            element?.Interact();
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
        isBlocked = newBlockState;
    }
}

public enum Direction
{
    Right,
    Left,
    Front,
    Back
}