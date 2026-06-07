using Godot;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Versioning;

public partial class VideoLayout : Control
{
    [Export] VideoStreamPlayer player;
    [Export] Label supportText;
    Vector2 originalPoition;
    bool videoPlaying;


    public override void _Ready()
    {
        supportText.Hide();
        player.Finished += OnVideoEnded;

        originalPoition = supportText.Position;

    }

    void OnVideoEnded()
    {
        AnimateSupportText(false);
        videoPlaying = false;
    }

    public void EndVideo()
    {
        player.Stop();
        player.Hide();
        OnVideoEnded();
        player.MouseFilter = MouseFilterEnum.Ignore;
    }

    public void StartVideo(VideoStream video)
    {
        player.Show();
        player.Stream = video;
        player.Play();
        player.MouseFilter = MouseFilterEnum.Stop;
        videoPlaying = true;
        AnimateSupportText(true);
    }

    void AnimateSupportText(bool isIn = false)
    {
        Vector2 targetPosition = isIn ? originalPoition : originalPoition - new Vector2(0, 20);
        supportText.Position = isIn ? originalPoition - new Vector2(0, 20) : originalPoition;

        supportText.Show();
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.InOut).SetParallel();
        tween.TweenProperty(supportText, "modulate:a", isIn ? 0.5f : 0f, 0.5f);
        tween.TweenProperty(supportText, "position", targetPosition, 0.5f);
    }

    public override void _Input(InputEvent @event)
    {
        if(videoPlaying && Input.IsAnythingPressed())
        {
            EndVideo();
        }

        // if (Input.IsActionJustPressed("AddRandomEntry"))
        // {
        //     CodexLibrary.ChargeEntry("Treya Meiden");
        // }
    }


}
