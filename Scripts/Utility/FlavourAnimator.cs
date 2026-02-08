using Components;
using Godot;
using System;


[GlobalClass]
public partial class FlavourAnimator : Node
{

    [Export] AnimationPlayer animationPlayer;

    [ExportGroup("FlavourAudios")]
    [Export] AudioStream questionAudio;


    public static FlavourAnimator Instance {get; private set;}

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if(Instance == this)
            Instance = null;
    }

    /// <summary>
    /// Plays an small animation with or without sound 
    /// </summary>
    /// <param name="type">Type of the Flavour Animation in string</param>
    public void PlayFlavour(string type)
    {
        switch (type.ToLower())
        {
            case "question":
            animationPlayer.Play("question");
            //AudioManager.PlayAudio(questionAudio);
            break;
            default:
                GD.PrintErr($"Flavour Type: {type} Not recognised");
            break;
        }
    }
}