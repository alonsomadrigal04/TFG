using System;
using Godot;

public partial class LorePoint : Node3D, IInteractable
{
    [Export] Texture2D iteractIcon;
    [Export] Area3D interactionArea;
    [Export] AudioStream idleSound;
    [Export] AudioStream idleInteraction;
    [Export] string dialogTag;
    [Export] InteractIconAnimation interactIconAnimation;
    [Export] float YOffset = 200f;
    public int priority { get; set ; } = 3;

    public override void _Ready()
    {
        interactionArea.BodyEntered += OnBodyEntered;
        interactionArea.BodyExited += OnBodyExit;

        interactIconAnimation.InitializeValues(YOffset, true, iteractIcon);
    }

    void OnBodyEntered(Node3D body)
    {
        if(body is PlayerBehaviour player)
        {
            player.RegisterInteractable(this);
        }
    }

    void OnBodyExit(Node3D body)
    {
        if(body is PlayerBehaviour player)
        {
            player.UnregisterInteractable(this);
        }
    }

    public void Interact()
    {
        AudioManager.Instance.DialogInteract.Play();
		CameraBehaviour.Instance.Shake();
        GameStateManager.Instance.ChangeState(State.Dialog, dialogTag);
    }

    public void SetInteractionArea()
    {
        
    }
}