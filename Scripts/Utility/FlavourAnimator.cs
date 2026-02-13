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
                sounds.question.Play();
                SpawnParticles(MiniAnimations[FlavourType.exclamation], speaker);
            break;
            default:
                GD.PrintErr($"Flavour Type: {type} Not recognised");
            break;
        }
    }

    void SpawnParticles(GpuParticles2D gpuParticles2D, Character speaker)
    {
        if(speaker != null)
        {
            TextureRect portrait = CharacterStage.Instance.GetCharacterPortrait(speaker);
            if(portrait != null)
            {
                gpuParticles2D.Show();
                gpuParticles2D.Position = portrait.Position + portrait.Size * 0.25f;
                gpuParticles2D.Emitting = true;
            }
        }
    }
}



public enum FlavourType
{
    question,
    exclamation
}