using Godot;
using System;
using System.Collections.Generic;

public partial class CameraBehaviour : Camera3D
{

    public static CameraBehaviour Instance { get => instance; set => instance = value; }

    static CameraBehaviour instance;
    public override void _EnterTree() => Instance = this;

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }
    [Export] Node3D target;
    public bool IsActive = false;

    Vector3 offset = new(0f, 3.5f, 7.2f);
    float followSpeed = 5f;

    readonly Dictionary<MeshInstance3D, float> fade = [];

    float shakeTimeLeft = 0f;
    float shakeDuration = 0f;
    float shakeIntensity = 0f;
    Vector3 shakeOffset = Vector3.Zero;
    RandomNumberGenerator rng = new();

    public override void _Ready()
    {
        rng.Randomize();
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

        if (shakeTimeLeft > 0f)
        {
            shakeTimeLeft -= d;

            float t = shakeTimeLeft / shakeDuration;
            float currentIntensity = shakeIntensity * t;

            shakeOffset = new Vector3(
                rng.RandfRange(-1f, 1f),
                rng.RandfRange(-1f, 1f),
                rng.RandfRange(-1f, 1f)
            ) * currentIntensity;
        }
        else
        {
            shakeOffset = Vector3.Zero;
        }

        desired += shakeOffset;

        GlobalPosition = GlobalPosition.Lerp(desired, followSpeed * d);

        HandleOcclusion(d);
    }
    public void Shake(float duration = 0.4f, float intensity = 1f)
    {
        shakeDuration = duration;
        shakeTimeLeft = duration;
        shakeIntensity = intensity;
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

            MeshInstance3D mesh = FindMeshUpwards(collider);


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
        mat.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor;
        c.A = 1f;
        mat.AlbedoColor = c;
    }
}