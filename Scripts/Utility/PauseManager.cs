using Game.Utility;
using Godot;
using System.Collections.Generic;
using Utility;

namespace Game;

public static class PauseManager
{
    public static bool GamePaused
    {
        get => gamePaused;
        set
        {
            if (gamePaused == value) return;
            gamePaused = value;
            if (gamePaused) PauseGame();
            else ResumeGame();
        }
    }

    static bool gamePaused = false;
    static readonly List<(GpuParticles3D, double, Node.ProcessModeEnum)> pausedParticles = [];

    static void PauseGame()
    {
        GodotExtensions.TryGetTree().Paused = true;
        PauseParticles();
    }

    static void ResumeGame()
    {
        GodotExtensions.TryGetTree().Paused = false;
        ResumeParticles();
    }

    static void PauseParticles()
    {
        foreach (GpuParticles3D particles in GodotExtensions.TryGetTree().GetAllNodesOfType<GpuParticles3D>())
        {
            pausedParticles.Add((particles, particles.SpeedScale, particles.ProcessMode));
            particles.ProcessMode = Node.ProcessModeEnum.Disabled;
            particles.SpeedScale = 0f;
        }
    }

    static void ResumeParticles()
    {
        foreach (var tuple in pausedParticles)
        {
            GpuParticles3D particles = tuple.Item1;
            double oldSpeedScale = tuple.Item2;
            Node.ProcessModeEnum processMode = tuple.Item3;

            if (GodotObject.IsInstanceValid(particles))
            {
                particles.ProcessMode = processMode;
                particles.SpeedScale = oldSpeedScale;
            }
        }

        pausedParticles.Clear();
    }
}
