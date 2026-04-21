using Godot;
using System;
using System.Collections.Generic;

public partial class CameraBehaviour : Camera3D
{
    [Export] Node3D target;
    public bool IsActive = false;

    Vector3 offset = new(0f, 3.5f, 7.2f);
    float followSpeed = 5f;

    readonly Dictionary<MeshInstance3D, float> fade = [];

    public override void _Ready()
    {
        if (target == null)
            GD.PushError("[CAMERA] TARGET NOT SET");
        else
            GlobalPosition = target.GlobalPosition + offset;
    }

    public override void _Process(double delta)
    {
        if (target == null || !IsActive)
            return;

        float d = (float)delta;

        Vector3 desired = target.GlobalPosition + offset;
        GlobalPosition = GlobalPosition.Lerp(desired, followSpeed * d);

        //HandleOcclusion(d);
    }

    void HandleOcclusion(float delta)
    {
        var space = GetWorld3D().DirectSpaceState;

        Vector3 from = GlobalPosition;
        Vector3 to = target.GlobalPosition;

        var query = PhysicsRayQueryParameters3D.Create(from, to);
        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var hit = space.IntersectRay(query);

        HashSet<MeshInstance3D> currentlyBlocked = [];

        if (hit.Count > 0)
        {
            Node collider = hit["collider"].As<Node>();
			GD.Print(collider.Name);

            MeshInstance3D mesh = FindMeshUpwards(collider);
			GD.Print(mesh);


            if (mesh != null && mesh.IsInGroup("occludable"))
                currentlyBlocked.Add(mesh);
        }

        var keys = new List<MeshInstance3D>(fade.Keys);

        foreach (var m in keys)
        {
            if (!currentlyBlocked.Contains(m))
            {
                fade[m] -= delta * 2.5f;

                if (fade[m] <= 0f)
                {
                    fade.Remove(m);
                    Restore(m);
                }
                else
                {
                    ApplyFade(m, fade[m]);
                }
            }
        }

        foreach (var m in currentlyBlocked)
        {
            if (!fade.ContainsKey(m))
                fade[m] = 0f;

            fade[m] = Mathf.Clamp(fade[m] + delta * 5f, 0f, 1f);

            ApplyFade(m, fade[m]);
        }
    }

    MeshInstance3D FindMeshUpwards(Node node)
    {
        while (node != null)
        {
            if (node is MeshInstance3D mesh)
                return mesh;

            node = node.GetParent();
        }
        return null;
    }

    void ApplyFade(MeshInstance3D mesh, float t)
    {
        if (mesh == null)
            return;


        if (mesh.GetActiveMaterial(0) is not StandardMaterial3D mat)
            return;

        if (!mat.ResourceLocalToScene)
        {
            mat = (StandardMaterial3D)mat.Duplicate();
            mesh.SetSurfaceOverrideMaterial(0, mat);
        }

        mat.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;

        Color c = mat.AlbedoColor;
        c.A = Mathf.Lerp(1f, 0.15f, t);
        mat.AlbedoColor = c;
    }

    void Restore(MeshInstance3D mesh)
    {
        if (mesh == null)
            return;

        if (mesh.GetActiveMaterial(0) is not StandardMaterial3D mat)
            return;

        Color c = mat.AlbedoColor;
        c.A = 1f;
        mat.AlbedoColor = c;
    }
}