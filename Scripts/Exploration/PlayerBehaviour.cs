using Godot;
using System;
using System.Collections.Generic;
using System.Linq;


public partial class PlayerBehaviour : CharacterBody3D
{
    [ExportGroup("MOVEMENT SETTINGS")]
    [Export] public float Speed { get; set; } = 5f;
    [Export] public float Aceleration { get; set; } = 50f;

    [ExportGroup("SFX")]
    [Export] public InteractFlavourAnimation exclamationSprite;
    Direction lastDirection = Direction.front;
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
            newDirection = Direction.right;
            direction.X += 1;
        }
        if (Input.IsActionPressed("moveLeft"))
        {
            newDirection = Direction.left;
            direction.X -= 1;
        }
        if (Input.IsActionPressed("moveFar"))
        {
            newDirection = Direction.back;
            direction.Z -= 1;
        }
        if (Input.IsActionPressed("moveNear"))
        {
            newDirection = Direction.front;
            direction.Z += 1;
        }

        if (direction != Vector3.Zero) 
        {
            direction = direction.Normalized();
        }

        if (newDirection != lastDirection) lastDirection = newDirection;

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

enum Direction
{
    right,
    left,
    front,
    back
}