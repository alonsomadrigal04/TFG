using Godot;
using System;

public partial class CameraBehaviour : Camera3D
{
	[Export] Node3D target;
	Vector3 offset = new(0f, 3.5f, 7.2f);
	float followSpeed = 5f;
	
	public override void _Ready()
	{
		if (target == null) 
			GD.PushError("[CAMERABEHAVIOUR] TARGET NOT SET");
		else
			GlobalPosition = target.GlobalPosition + offset;
	}

	public override void _Process(double delta)
	{
		if (target == null)
			GD.PushError("[CAMERABEHAVIOUR] TARGET NOT SET");

		Vector3 desiredPosition = target.GlobalPosition + offset;

		GlobalPosition = GlobalPosition.Lerp(
			desiredPosition,
			followSpeed * (float)delta
		);
	}
}