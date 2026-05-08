using Godot;
using System.Collections.Generic;

public partial class CameraBehaviour : Camera3D
{
    public static CameraBehaviour Instance { get; private set; }

    public override void _EnterTree() => Instance = this;

    public override void _ExitTree()
    {
        if (Instance == this)
            Instance = null;
    }

    [Export] Node3D target;

    [Export] float followSpeed = 5f;
    [Export] float offsetSmooth = 4f;
    [Export] float lookSmooth = 8f;

    public bool IsActive = true;

    int viewMode = 0;

    readonly List<Vector3> offsetList =
    [
        new Vector3(0f, 5f, 9f),
        new Vector3(0f, 10f, 5f)
    ];

    Vector3 currentOffset;
    Vector3 targetOffset;

    readonly Dictionary<MeshInstance3D, float> fade = [];

    float shakeTimeLeft = 0f;
    float shakeDuration = 0f;
    float shakeIntensity = 0f;

    Vector3 shakeOffset = Vector3.Zero;

    readonly RandomNumberGenerator rng = new();

    public override void _Ready()
    {
        rng.Randomize();

        if (target == null)
        {
            GD.PushError("[CAMERA] TARGET NOT SET");
            return;
        }

        currentOffset = offsetList[viewMode];
        targetOffset = currentOffset;

        GlobalPosition = target.GlobalPosition + currentOffset;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (target == null || !IsActive)
            return;

        float dt = (float)delta;

        HandleShake(dt);

        currentOffset = currentOffset.Lerp(
            targetOffset,
            1f - Mathf.Exp(-offsetSmooth * dt)
        );

        Vector3 desiredPosition =
            target.GlobalPosition +
            currentOffset +
            shakeOffset;

        GlobalPosition = GlobalPosition.Lerp(
            desiredPosition,
            1f - Mathf.Exp(-followSpeed * dt)
        );

        SmoothLookAt(target.GlobalPosition, dt);

        HandleOcclusion(dt);
    }

    void SmoothLookAt(Vector3 targetPosition, float dt)
    {
        Basis targetBasis = Transform.LookingAt(
            targetPosition + new Vector3(0,3,0),
            Vector3.Up
        ).Basis;

        Transform3D t = Transform;

        t.Basis = t.Basis.Slerp(
            targetBasis,
            1f - Mathf.Exp(-lookSmooth * dt)
        );

        Transform = t;
    }

    void HandleShake(float dt)
    {
        if (shakeTimeLeft > 0f)
        {
            shakeTimeLeft -= dt;

            float t = shakeTimeLeft / shakeDuration;

            float intensity = shakeIntensity * t;

            shakeOffset = new Vector3(
                rng.RandfRange(-1f, 1f),
                rng.RandfRange(-1f, 1f),
                rng.RandfRange(-1f, 1f)
            ) * intensity;
        }
        else
        {
            shakeOffset = Vector3.Zero;
        }
    }

    public void Shake(float duration = 0.4f, float intensity = 1f)
    {
        shakeDuration = duration;
        shakeTimeLeft = duration;
        shakeIntensity = intensity;
    }

    public void SetViewMode(CameraViewMode mode)
    {
        switch (mode)
        {
            case CameraViewMode.Up:
                viewMode = 1;
                targetOffset = offsetList[1];
                break;

            case CameraViewMode.Normal:
                viewMode = 0;
                targetOffset = offsetList[0];
                break;
        }
    }

    void HandleOcclusion(float delta)
    {
        var space = GetWorld3D().DirectSpaceState;

        var query = PhysicsRayQueryParameters3D.Create(
            GlobalPosition,
            target.GlobalPosition
        );

        query.CollideWithAreas = false;
        query.CollideWithBodies = true;

        var hit = space.IntersectRay(query);

        HashSet<MeshInstance3D> blocked = [];

        if (hit.Count > 0)
        {
            Node collider = hit["collider"].As<Node>();

            MeshInstance3D mesh = FindMeshUpwards(collider);

            if (mesh != null && mesh.IsInGroup("occludable"))
                blocked.Add(mesh);
        }

        foreach (var mesh in new List<MeshInstance3D>(fade.Keys))
        {
            if (!blocked.Contains(mesh))
            {
                fade[mesh] -= delta * 2.5f;

                if (fade[mesh] <= 0f)
                {
                    fade.Remove(mesh);
                    Restore(mesh);
                }
                else
                {
                    ApplyFade(mesh, fade[mesh]);
                }
            }
        }

        foreach (var mesh in blocked)
        {
            if (!fade.ContainsKey(mesh))
                fade[mesh] = 0f;

            fade[mesh] = Mathf.Clamp(
                fade[mesh] + delta * 5f,
                0f,
                1f
            );

            ApplyFade(mesh, fade[mesh]);
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
        if (mesh?.GetActiveMaterial(0) is not StandardMaterial3D mat)
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
        if (mesh?.GetActiveMaterial(0) is not StandardMaterial3D mat)
            return;

        Color c = mat.AlbedoColor;

        mat.Transparency =
            BaseMaterial3D.TransparencyEnum.AlphaScissor;

        c.A = 1f;

        mat.AlbedoColor = c;
    }
}

public enum CameraViewMode
{
    Up,
    Normal
}