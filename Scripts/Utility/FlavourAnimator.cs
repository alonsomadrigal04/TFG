using Components;
using Godot;
using Godot.Collections;
using System;
using System.Runtime.InteropServices;


[GlobalClass]
public partial class FlavourAnimator : Node
{
    [Export] AudioManager sounds;
    [Export] Dictionary<FlavourType, GpuParticles2D> MiniAnimations = [];
    public static FlavourAnimator Instance {get; private set;}

    public override void _Ready()
    {
        if(Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Plays an small animation with or without sound 
    /// </summary>
    /// <param name="type">Type of the Flavour Animation in string</param>
    public void PlayFlavour(string type, Character speaker)
    {
        switch (ToolKit.ParseEnum<FlavourType>(type))
        {
            case FlavourType.question:
                sounds.question.Play();
                SpawnParticles(MiniAnimations[FlavourType.question], speaker);
            break;
            case FlavourType.exclamation:
                sounds.exclamation.Play();
                SpawnParticles(MiniAnimations[FlavourType.exclamation], speaker);
            break;
            default:
                GD.PrintErr($"Flavour Type: {type} Not recognised");
            break;
        }
    }

    static void SpawnParticles(GpuParticles2D gpuParticles2D, Character speaker)
    {
        TextureRect portrait = CharacterStage.Instance.GetCharacterPortrait(speaker);
        if (portrait == null)
            return;
        
        gpuParticles2D.Show();

        Vector2 portraitCenter = portrait.Position + portrait.Size * 0.5f;

        Vector2 screenCenter = portrait.GetViewportRect().Size * 0.5f;

        Vector2 directionToCenter = (screenCenter - portraitCenter).Normalized();

        if (gpuParticles2D.ProcessMaterial is ParticleProcessMaterial material)
        {
            float force = 50f;
            material.Gravity = new Vector3(directionToCenter.X * force, directionToCenter.Y * force -50, 0);
        }

        Vector2 horizontalOffset = directionToCenter * 40f;
        Vector2 verticalOffset = new(0, -portrait.Size.Y * 0.4f);

        gpuParticles2D.Position = portraitCenter + horizontalOffset + verticalOffset;

        gpuParticles2D.Restart();
        gpuParticles2D.Emitting = true;
    }

}



public enum FlavourType
{
    question,
    exclamation
}