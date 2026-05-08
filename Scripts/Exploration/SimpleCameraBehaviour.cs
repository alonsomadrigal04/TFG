using Godot;

public partial class SimpleCameraBehaviour : Camera3D
{
    [Export] Node3D target;

    [Export] Vector3 offset = new(0, 5, 8);

    [Export] float followSpeed = 5f;

    public override void _Ready()
    {
        if (target == null)
        {
            GD.PushError("[CAMERA] Target not assigned.");
            return;
        }

        GlobalPosition = target.GlobalPosition + offset;
        LookAt(target.GlobalPosition);
    }

    public override void _Process(double delta)
    {
        if (target == null)
            return;

        float dt = (float)delta;

        Vector3 desiredPosition = target.GlobalPosition + offset;

        GlobalPosition = GlobalPosition.Lerp(
            desiredPosition,
            followSpeed * dt
        );

        LookAt(target.GlobalPosition);
    }
}