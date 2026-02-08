using Godot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Utility;

public static class GodotExtensions
{
    ///<summary>Defers the execution of the specified action until the current frame has finished processing.</summary>
    ///<param name="action">The <see cref="Action"/> to be invoked later using Godot's deferred call system.</param>
    public static void CallDeferred(Action action) => Callable.From(action).CallDeferred();

    /// <summary>
    /// Adds a new parent node above the specified <paramref name="root"/> node.
    /// Performs the reparenting deferred to avoid scene tree modification errors.
    /// </summary>
    /// <param name="root">The node that will receive the new parent.</param>
    /// <param name="newParent">The node to insert as the new parent above <paramref name="root"/>.</param>
    /// <param name="inheritTransform">
    /// If true, the transform of <paramref name="root"/> is transferred to <paramref name="newParent"/> 
    /// and <paramref name="root"/> transform is reset.
    /// </param>
    public static void AddParent(this Node root, Node newParent, bool inheritTransform = true)
    {
        Node oldParent = root.GetParent();

        if (oldParent == null)
        {
            GD.PushError("Can't AddParent to a node without previous parent.");
            return;
        }

        if (inheritTransform)
        {
            if (root is Node2D root2D && newParent is Node2D newParent2D)
            {
                newParent2D.Transform = root2D.Transform;
                root2D.Transform = Transform2D.Identity;
            }
            else if (root is Node3D root3D && newParent is Node3D newParent3D)
            {
                newParent3D.Transform = root3D.Transform;
                root3D.Transform = Transform3D.Identity;
            }
        }

        CallDeferred(() => oldParent.AddChild(newParent));
        CallDeferred(() => root.Reparent(newParent, false));
    }

    ///<summary>Returns all child nodes of a specific type from the given node.</summary>
    ///<typeparam name="T">The type of nodes to search for.</typeparam>
    ///<param name="node">The parent node whose children will be searched.</param>
    ///<param name="recursive">
    /// If true, the search includes all descendants recursively; 
    /// if false, only direct children are checked.
    ///</param>
    ///<returns>
    /// An enumerable collection of nodes of type <typeparamref name="T"/> found among the children (and optionally descendants) of the given node.
    ///</returns>
    public static IEnumerable<T> GetChildrenOfType<T>(this Node node, bool recursive = false)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T match) yield return match;
            if (!recursive) continue;

            foreach (T descendant in child.GetChildrenOfType<T>(true))
                yield return descendant;
        }
    }

    /// <summary>Retrieves all nodes of the specified type <typeparamref name="T"/> within the current <see cref="SceneTree"/>.</summary>
    /// <typeparam name="T">The type of nodes to search for.</typeparam>
    /// <param name="tree">The <see cref="SceneTree"/> from which to retrieve nodes.</param>
    /// <returns>An enumerable collection containing all nodes of type <typeparamref name="T"/> found in the tree.</returns>
    /// <remarks>This method performs a recursive search starting from the tree's root node.  
    /// It is functionally equivalent to calling <see cref="GetChildrenOfType{T}(Node, bool)"/> on <c>GetTree().Root</c> with the <c>recursive</c> parameter set to <c>true</c>.
    /// </remarks>
    public static IEnumerable<T> GetAllNodesOfType<T>(this SceneTree tree) => tree.Root.GetChildrenOfType<T>(true);

    /// <summary>Attempts to retrieve the active SceneTree instance from the engine.</summary>
    /// <returns>The current <see cref="SceneTree"/> if the engine's main loop has been initialized; otherwise, returns <c>null</c>.</returns>
    /// <remarks>This method is useful in static contexts or early initialization phases, where the SceneTree might not yet exist.</remarks>
    public static SceneTree TryGetTree()
    {
        if (Engine.GetMainLoop() is not SceneTree tree)
        {
            GD.PushError("Tree has not been initialised yet.");
            return null;
        }
        return tree;
    }

    /// <summary>Creates a method tween that calls the given setter delegate over time, interpolating between two Variant values.</summary>
    /// <typeparam name="T">The type of the value being interpolated.</typeparam>
    /// <param name="tween">The Tween instance this extension operates on.</param>
    /// <param name="setter">The delegate method to be called with the interpolated value each frame.</param>
    /// <param name="from">The initial value of the interpolation.</param>
    /// <param name="to">The final value of the interpolation.</param>
    /// <param name="duration">The total duration of the tween in seconds.</param>
    /// <returns>The created <see cref="MethodTweener"/> used to further configure the tween.</returns>
    public static MethodTweener TweenDelegate<T>(this Tween tween, Action<T> setter, Variant from, Variant to, float duration)
        => tween.TweenMethod(Callable.From(setter), from, to, duration);

    /// <summary>Creates a method tween that calls the given setter delegate over time, interpolating between two float values.</summary>
    /// <param name="tween">The Tween instance this extension operates on.</param>
    /// <param name="setter">The delegate method to be called with the interpolated value each frame.</param>
    /// <param name="from">The initial value of the interpolation.</param>
    /// <param name="to">The final value of the interpolation.</param>
    /// <param name="duration">The total duration of the tween in seconds.</param>
    /// <returns>The created <see cref="MethodTweener"/> used to further configure the tween.</returns>
    public static MethodTweener TweenDelegate(this Tween tween, Action<float> setter, float from, float to, float duration)
        => tween.TweenMethod(Callable.From(setter), from, to, duration);

    public static async Task AwaitFinished(this Tween tween, CancellationToken token, bool killOnCancel = true)
    {
        if (tween == null || !GodotObject.IsInstanceValid(tween)) return;

        var tcs = new TaskCompletionSource();

        void OnFinished() { if (!tcs.Task.IsCompleted) tcs.TrySetResult(); }

        tween.Finished += OnFinished;

        using (token.Register(() =>
        {
            if (killOnCancel && GodotObject.IsInstanceValid(tween)) tween.Kill();
            if (!tcs.Task.IsCompleted) tcs.TrySetResult();
        }))
        {
            await tcs.Task;
        }

        tween.Finished -= OnFinished;
    }

    public static void SetAnchorOffsetToZero(this Control node)
    {
        node.OffsetBottom = 0;
        node.OffsetLeft = 0;
        node.OffsetTop = 0;
        node.OffsetRight = 0;
    }

    /// <summary>Creates and emits a temporary burst of GPU particles by duplicating the source emitter.
    /// The spawned particle system will automatically free itself after its lifetime expires.</summary>
    /// <param name="particles">The source <see cref="GpuParticles2D"/> to duplicate.</param>
    /// <param name="makeUniqueMaterial">If true, duplicates the ProcessMaterial to avoid shared references between instances.</param>
    /// <returns>The spawned <see cref="GpuParticles2D"/> instance that performs the burst.</returns>
    public static GpuParticles2D SpawnBurst(this GpuParticles2D particles, bool makeUniqueMaterial = true)
    {
        GpuParticles2D spawn = particles.Duplicate() as GpuParticles2D;

        if (makeUniqueMaterial && spawn.ProcessMaterial != null) // Make process material unique
            spawn.ProcessMaterial = spawn.ProcessMaterial.Duplicate() as Material;

        particles.GetParent().AddChild(spawn, false, Node.InternalMode.Front);
        spawn.GlobalPosition = particles.GlobalPosition;
        spawn.Emitting = spawn.OneShot = true;
        spawn.Name = particles.Name + "Burst";
        double lifetime = particles.GetTotalLifetime();
        spawn.GetTree().CreateTimer(lifetime, false).Timeout += spawn.QueueFree;
        
        return spawn;
    }

    /// <summary> Calculates the total lifetime of a particle system, including all chained sub-emitters recursively.</summary>
    /// <param name="particles">The root particle system to evaluate.</param>
    /// <returns>The combined lifetime of this emitter and all its sub-emitters.</returns>
    public static double GetTotalLifetime(this GpuParticles2D particles)
    {
        GpuParticles2D subemitter = particles.GetNodeOrNull<GpuParticles2D>(particles.SubEmitter);

        if (subemitter == null) return particles.Lifetime;

        return particles.Lifetime + subemitter.GetTotalLifetime();
    }

    /// <summary>Sets the particle emission direction based on a given angle in radians,
    /// without modifying the emitter's own rotation.</summary>
    /// <param name="particles">The GPUParticles2D node to modify.</param>
    /// <param name="angle">The emission angle in radians.</param>
    /// <param name="makeUniqueMaterial">If true, duplicates the ProcessMaterial to avoid shared references.</param>
    public static void SetEmissionDirection(this GpuParticles2D particles, float angle, bool makeUniqueMaterial = true)
    {
        if (particles.ProcessMaterial is not ParticleProcessMaterial mat)
            return;

        if (makeUniqueMaterial)
            mat = (ParticleProcessMaterial)mat.Duplicate();

        Vector3 direction = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);

        mat.Direction = direction.Normalized();
        particles.ProcessMaterial = mat;
    }

    ///<summary>Asynchronously waits for a specified amount of in-game seconds, using the calling node's process context.</summary>
    /// <remarks>This wait is affected by <see cref="Engine.TimeScale"/> and <see cref="Node.ProcessMode"/>.
    /// It yields on either <see cref="SceneTree.SignalName.ProcessFrame"/> or <see cref="SceneTree.SignalName.PhysicsFrame"/>,  
    /// depending on the <paramref name="usePhysicsDelta"/> flag. </remarks>
    /// <param name="context">The node that calls this function. Automatically provided when used as an extension.</param>
    /// <param name="seconds">The number of seconds to wait before continuing execution.</param>
    /// <param name="usePhysicsDelta">If true, waits on physics frames using <see cref="Node._PhysicsProcess(double)"/>; otherwise uses normal frames.</param>
    public static async Task WaitGameSeconds(this Node context, float seconds, bool usePhysicsDelta = false, CancellationToken token = default)
    {
        if (context.GetTree() == null) return;

        string frameEndSignal = usePhysicsDelta ? SceneTree.SignalName.PhysicsFrame : SceneTree.SignalName.ProcessFrame;
        double elapsed = 0f;

        while (elapsed < seconds)
        {
            await context.ToSignal(context.GetTree(), frameEndSignal);
            if (token.IsCancellationRequested || !GodotObject.IsInstanceValid(context)) return;
            elapsed += context.GetEffectiveDeltaTime(usePhysicsDelta);
        }
    }

    public static async Task WaitGameSecondsUnscaled(this Node context, float seconds, bool usePhysicsDelta = false, CancellationToken token = default)
    {
        if (context.GetTree() == null) return;

        string frameEndSignal = usePhysicsDelta ? SceneTree.SignalName.PhysicsFrame : SceneTree.SignalName.ProcessFrame;
        double elapsed = 0;
        ulong lastTicks = Time.GetTicksUsec();

        while (elapsed < seconds)
        {
            await context.ToSignal(context.GetTree(), frameEndSignal);
            if (token.IsCancellationRequested || !GodotObject.IsInstanceValid(context)) return;

            ulong now = Time.GetTicksUsec();
            if (context.IsEffectivelyProcessing()) elapsed += (now - lastTicks) / 1_000_000.0;

            lastTicks = now;
        }
    }


    ///<summary>Asynchronously waits until a given condition becomes true, using the calling node's process context.</summary>
    /// <remarks>This wait is affected by <see cref="Engine.TimeScale"/> and <see cref="Node.ProcessMode"/>.  
    /// It yields on either <see cref="SceneTree.SignalName.ProcessFrame"/> or <see cref="SceneTree.SignalName.PhysicsFrame"/>,  
    /// depending on the <paramref name="usePhysicsDelta"/> flag.</remarks>
    /// <param name="context">The node that calls this function. Automatically provided when used as an extension.</param>
    /// <param name="condition">A function that returns true when the wait should end.</param>
    /// <param name="usePhysicsDelta">If true, waits on physics frames using <see cref="Node._PhysicsProcess(double)"/>; otherwise uses normal frames.</param>
    public static async Task WaitUntil(this Node context, Func<bool> condition, bool usePhysicsDelta = false, CancellationToken token = default)
    {
        if (context.GetTree() == null) return;

        SceneTree tree = context.GetTree();
        string signal = usePhysicsDelta
            ? SceneTree.SignalName.PhysicsFrame
            : SceneTree.SignalName.ProcessFrame;

        while (!condition())
        {
            await tree.ToSignal(tree, signal);
            if (token.IsCancellationRequested || !GodotObject.IsInstanceValid(context)) return;
        }
    }

    /// <summary> Gets the effective <see cref="Node.ProcessModeEnum"/> applied to this node,
    /// resolving inherited values by walking up the parent hierarchy. </summary>
    /// <param name="node">The node whose real process mode will be resolved.</param>
    /// <returns>The first non-<see cref="Node.ProcessModeEnum.Inherit"/> mode found, or <see cref="Node.ProcessModeEnum.Always"/> if none is defined.</returns>
    public static Node.ProcessModeEnum GetEffectiveProcessMode(this Node node)
    {
        Node current = node;

        while (current != null)
        {
            if (current.ProcessMode != Node.ProcessModeEnum.Inherit)
                return current.ProcessMode;

            current = current.GetParent();
        }
        return Node.ProcessModeEnum.Always;
    }

    public static bool IsEffectivelyProcessing(this Node node)
    {
        Node.ProcessModeEnum mode = node.GetEffectiveProcessMode();
        SceneTree tree = node.GetTree();

        return
            mode == Node.ProcessModeEnum.Always ||
            (mode == Node.ProcessModeEnum.Pausable && !tree.Paused) ||
            (mode == Node.ProcessModeEnum.WhenPaused && tree.Paused);
    }
    
    /// <summary>Returns the real delta time for this node, taking into account its effective process mode
    /// and the current paused state of the <see cref="SceneTree"/>.</summary>
    /// <param name="node">The node whose effective delta time will be evaluated.</param>
    /// <param name="usePhysicsDelta">If true, uses physics delta time; otherwise uses process delta time.</param>
    /// <returns>The effective delta time, or 0 if the node should not process under current conditions.</returns>
    public static double GetEffectiveDeltaTime(this Node node, bool usePhysicsDelta = false)
    {
        SceneTree tree = node.GetTree();
        if (tree == null) return 0d;

        return !node.IsEffectivelyProcessing() ? 0d : usePhysicsDelta ? tree.Root.GetPhysicsProcessDeltaTime() : tree.Root.GetProcessDeltaTime();
    }

    /// <summary>Creates and assigns a new <see cref="ShaderMaterial"/> with the provided shader code to the given <see cref="CanvasItem"/>.</summary>
    /// <param name="target">The <see cref="CanvasItem"/> that will receive the new shader material.</param>
    /// <param name="shaderCode">The shader code to compile and apply to the material.</param>
    /// <returns>The newly created <see cref="ShaderMaterial"/> instance assigned to the target.</returns>
    /// <remarks>This method instantiates a new <see cref="Shader"/> using the specified code,  
    /// wraps it in a <see cref="ShaderMaterial"/>, and assigns it to the <see cref="CanvasItem.Material"/> property.  
    /// Useful for applying custom shaders dynamically at runtime.</remarks>
    public static ShaderMaterial SetShaderMaterial(this CanvasItem target, string shaderCode)
    {
        Shader shader = new() { Code = shaderCode };
        ShaderMaterial shaderMat = new() { Shader = shader };
        target.Material = shaderMat;
        return shaderMat;
    }
}
