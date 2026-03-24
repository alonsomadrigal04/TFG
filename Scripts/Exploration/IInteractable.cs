using Godot;
using System;

public interface IInteractable
{
	void Interact();
	int priority { get; set; }
}
