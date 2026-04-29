using System;
using Godot;

public partial class ChapterEnv : Control
{
    [Export] Label chapterLabel;
    [Export] Label numberLabel;
    [Export] Label subTitleLabel;
    [Export] AnimationPlayer animationPlayer;
    [Export] AudioStreamPlayer introSound;

    public Action endAnimation;


    public override void _Ready()
    {
        string name = GameManager.Instance.CurrentChapterTitle;
        string subtitle = GameManager.Instance.CurrentChapterSubTitle;

        animationPlayer.AnimationFinished += OnAnimationFinished;


        StartChapter(name, subtitle);
    }

    void OnAnimationFinished(StringName anim)
    {
        if (anim == "introChapter")
            GameManager.Instance.chapterBehaviour.ChargeChapter();
    }

    void SetNumberLabel(int number)
    {
        if(number == 0)
        {
            numberLabel.Text = "";
            return;
        }
        numberLabel.Text = number.ToString();
        
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("skip"))
        {
            animationPlayer.Seek(animationPlayer.CurrentAnimationLength -0.1f, true);
        }
    }


    void SetNameLabel(string name) => chapterLabel.Text = name;
    void SetSubTitleLabel(string subtitle) => subTitleLabel.Text = subtitle;

    public void StartChapter(string name, string subtitle)
    {
        SetNumberLabel(GameManager.Instance.GetChapterNumber());
        SetSubTitleLabel(subtitle);
        SetNameLabel(name);
        animationPlayer.Play("introChapter");
        //introSound.Play();
    }
}