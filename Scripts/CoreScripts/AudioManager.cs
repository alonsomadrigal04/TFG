using Game.Utility;
using Godot;
using System;
[GlobalClass]
public partial class AudioManager : Node
{

    public static AudioManager Instance { get => instance; set => instance = value; }

    static AudioManager instance;
    public override void _EnterTree() => Instance = this;

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }
    
    static public Pool<AudioStreamPlayer> sfxPool;

    static AudioStreamPlayer musicPlayer;
    Tween musicTween;

    const string BUS_SFX = "Sfx";
    const string BUS_MUSIC = "Fademusic";

    public override void _Ready()
    {
        sfxPool = new Pool<AudioStreamPlayer>(
            create: () =>
            {
                AudioStreamPlayer p = new()
                {
                    Bus = BUS_SFX
                };
                AddChild(p);
                return p;
            },
            isReleased: (p) => !p.Playing
        );

        musicPlayer = new AudioStreamPlayer
        {
            Bus = BUS_MUSIC
        };

        AddChild(musicPlayer);
    }

    /// <summary>
    /// Plays an SFX using the pool.
    /// </summary>
    public void PlaySfx(AudioStream stream, float volumeDb = 0f)
    {
        var p = sfxPool.GetReleased();
        p.Stream = stream;
        p.VolumeDb = volumeDb;
        p.Play();
    }

    /// <summary>
    /// Stops all active SFX immediately.
    /// </summary>
    public void StopAllSfx()
    {
        foreach (var child in GetChildren())
        {
            if (child is AudioStreamPlayer p && p.Bus == BUS_SFX)
                p.Stop();
        }
    }

    /// <summary>
    /// Plays music with optional fade transition.
    /// </summary>
    public async void PlayMusic(AudioStream stream, float fadeTime = 0.5f)
    {
        if (musicPlayer.Stream == stream && musicPlayer.Playing)
            return;

        musicTween?.Kill();

        if (musicPlayer.Playing)
        {
            await FadeOutMusic(fadeTime);
        }

        musicPlayer.Stream = stream;
        musicPlayer.VolumeDb = -80f;
        musicPlayer.Play();

        await FadeInMusic(fadeTime);
    }

    /// <summary>
    /// Stops music with optional fade.
    /// </summary>
    public async void StopMusic(float fadeTime = 0.5f)
    {
        if (!musicPlayer.Playing)
            return;

        musicTween?.Kill();

        await FadeOutMusic(fadeTime);
        musicPlayer.Stop();
    }

    /// <summary>
    /// Fades in the current music.
    /// </summary>
    async System.Threading.Tasks.Task FadeInMusic(float time)
    {
        musicTween = CreateTween();
        musicTween.TweenProperty(musicPlayer, "volume_db", 0f, time);
        await ToSignal(musicTween, Tween.SignalName.Finished);
    }

    /// <summary>
    /// Fades out the current music.
    /// </summary>
    async System.Threading.Tasks.Task FadeOutMusic(float time)
    {
        musicTween = CreateTween();
        musicTween.TweenProperty(musicPlayer, "volume_db", -80f, time);
        await ToSignal(musicTween, Tween.SignalName.Finished);
    }

    public static bool IsMusicPlaying()
    {
        return musicPlayer != null && musicPlayer.Playing;
    }


    /// <summary>
    /// Mutes or unmutes the music bus instantly.
    /// </summary>
    public void SetMusicMute(bool mute)
    {
        int idx = AudioServer.GetBusIndex(BUS_MUSIC);
        AudioServer.SetBusMute(idx, mute);
    }

    /// <summary>
    /// Mutes or unmutes the SFX bus instantly.
    /// </summary>
    public void SetSfxMute(bool mute)
    {
        int idx = AudioServer.GetBusIndex(BUS_SFX);
        AudioServer.SetBusMute(idx, mute);
    }

    /// <summary>
    /// Fades out all SFX currently playing.
    /// </summary>
    public void FadeOutAllSfx(float time = 0.3f)
    {
        foreach (var child in GetChildren())
        {
            if (child is AudioStreamPlayer p && p.Bus == BUS_SFX && p.Playing)
            {
                var t = CreateTween();
                t.TweenProperty(p, "volume_db", -80f, time);
                t.TweenCallback(Callable.From(() => p.Stop()));
            }
        }
    }


    [ExportGroup("FLAVOUR TEXT")]
    [Export] public AudioStreamPlayer2D question {get; set;}
    [Export] public AudioStreamPlayer2D exclamation {get; set;}

    [ExportGroup("CHOICE SOUNDS")]
    [Export] public AudioStreamPlayer2D soft {get; set;}
    [Export] public AudioStreamPlayer2D impact {get; set;}
    [Export] public AudioStreamPlayer2D outQuestion {get; set;}
    [Export] public AudioStreamPlayer2D flipCard {get; set;}


    [ExportGroup("DECISION MAKING")]
    [Export] public AudioStreamPlayer2D Hover {get; set;}
    [Export] public AudioStreamPlayer2D Press {get; set;}

    [ExportGroup("VSN SOUNDS")]
    [Export] public AudioStreamPlayer2D Shake {get; set;}
    [Export] public AudioStreamPlayer2D Talk {get; set;}
    [Export] public AudioStreamPlayer2D Impact {get; set;}
    [Export] public AudioStreamPlayer2D Flashback {get; set;}
    [Export] public AudioStreamPlayer2D Flash {get; set;}

    [ExportGroup("UI SOUNDS")]
    [Export] public AudioStreamPlayer2D NextSentence {get; set;}
    [Export] public AudioStreamPlayer2D Remember {get; set;}
    [Export] public AudioStreamPlayer2D NewItem {get; set;}
    [Export] public AudioStreamPlayer2D Spining {get; set;}
    [Export] public AudioStreamPlayer2D ClockDisplay {get; set;}
    [Export] public AudioStreamPlayer2D ClockHide {get; set;}
    [Export] public AudioStreamPlayer2D Chorus1 {get; set;}
    [Export] public AudioStreamPlayer2D Flipsound {get; set;}
    [Export] public AudioStreamPlayer2D WalkingSounds {get; set;}
    [Export] public AudioStreamPlayer2D DialogInteract {get; set;}




}
