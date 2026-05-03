using System;
using Godot;

public partial class CameraZone : Area3D
{

    public override void _Ready()
    {
        BodyEntered += SetCameraUp;
        BodyExited += SetCameraNormal;
        
    }

    void SetCameraNormal(Node3D body)
    {
        if(body is PlayerBehaviour){
            CameraBehaviour.Instance.SetViewMode(CameraViewMode.Normal);
        }
    }

    void SetCameraUp(Node3D body)
    {
        GD.Print("Switching camera mode...");
        if(body is PlayerBehaviour){
            CameraBehaviour.Instance.SetViewMode(CameraViewMode.Up);
        }
    }
}