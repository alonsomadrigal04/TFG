using System.Collections.Generic;
using Godot;

namespace Game.Common.Modules;

//Todo: Joan pls this would be so much better as a singleton (~_~) - Karl
//Todo: Would be cool for this to use pooling - Karl
[GlobalClass]
public partial class MultiaudioPlayerModule : Node
{
    readonly List<AudioStreamPlayer> activePlayers = new();

    /// <summary> Plays a sound dynamically with custom volume and pitch and returns the AudioStreamPlayer instance. </summary>
    public AudioStreamPlayer PlaySound(AudioStream sound, float volume = 1f, float pitch = 1f)
    {
        if (sound == null) return null;

        AudioStreamPlayer player = new()
        {
            Stream = sound,
            VolumeDb = LinearToDb(volume),
            PitchScale = pitch
        };

        player.Finished += () =>
        {
            if (activePlayers.Contains(player))
            {
                activePlayers.Remove(player);
                RemoveChild(player);
                player.QueueFree();
            }
        };

        AddChild(player);
        activePlayers.Add(player);
        player.Play();
        return player;
    }

    /// <summary> Plays a random sound chosen from the given list, with the specified volume and pitch and returns the AudioStreamPlayer instance. </summary>
    public AudioStreamPlayer PlaySound(List<AudioStream> sounds, float volume = 1f, float pitch = 1f)
    {
        if (sounds == null || sounds.Count == 0) return null;

        int index = (int)(GD.Randi() % sounds.Count);
        AudioStream chosen = sounds[index];

        return PlaySound(chosen, volume, pitch);
    }

    /// <summary> Stops all currently playing sounds. </summary>
    public void StopAll()
    {
        foreach (var player in activePlayers)
            player.Stop();

        activePlayers.Clear();
    }

    /// <summary> Converts linear volume [0,1] to decibels. </summary>
    static float LinearToDb(float linear)
    {
        return linear <= 0 ? -80f : 20f * Mathf.Log(linear);
    }
}