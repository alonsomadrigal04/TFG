using Components;
using Godot;
using System;

public partial class BackgroundStage : Node
{
    [ExportGroup("TRANSITIONS")]
    [Export] float timeTransition = 1.5f; 
    [Export] ColorRect blurTransition;
    [Export] ScreenFlash flashTransition;
    [Export] TextureRect imageFrame;
    [Export] ColorRect sepiaFilter;
    [Export] AudioManager sounds;

    public static BackgroundStage Instance {get; private set;}
    float progress = 0f;
    bool isTransitioning = false;
    ShaderMaterial transitionBlurShader;


    public override void _Ready()
    {
        if(Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;
        var mat = blurTransition.Material as ShaderMaterial;
        transitionBlurShader = mat.Duplicate() as ShaderMaterial;
        blurTransition.Material = transitionBlurShader;

    }
    public override void _Process(double delta)
    {
        if (!isTransitioning)
            return;

        progress += (float)(delta / timeTransition);
        progress = Mathf.Min(progress, 1f);

        transitionBlurShader.SetShaderParameter("progress", progress);
        GD.Print(transitionBlurShader.GetShaderParameter("progress"));

        if (progress >= 1f)
        {
            progress = 0f;
            isTransitioning = false;
            imageFrame.Texture = 
                (Texture2D)(GodotObject)transitionBlurShader.GetShaderParameter("to_tex");
            blurTransition.Hide();
            
        }
    }


    public void SetBackground(Texture2D newBg)
    {        
        imageFrame.Texture = newBg;
        //TODO: make cool animation transition with non previus bg
    }

    public void BlurTransition(Texture2D newBg)
    {
        //TODO: add sounds
        Texture2D actualBg = imageFrame.Texture;

        if (actualBg == null)
        {
            GD.PrintErr("[BackgroundStage] no active Bg to make a transition");
            return;
        }

        transitionBlurShader.SetShaderParameter("from_tex", actualBg);
        transitionBlurShader.SetShaderParameter("to_tex", newBg);
        blurTransition.Show();

        progress = 0f;
        transitionBlurShader.SetShaderParameter("progress", progress);

        isTransitioning = true;
    }

    public async void FlashTransition(Texture2D newBg)
    {
        //TODO: Cool sounds
        sounds.Flash.Play();
        await flashTransition.PlayFlash(() =>
        {
            imageFrame.Texture = newBg;
        });
    }

    public async void MakeFlashback(Texture2D flashbackImg, float duration = 1f)
    {
        // TODO: cool flashcak sound
        Texture2D oldBg = imageFrame.Texture;
        sounds.Flashback.Play();
        
        await flashTransition.PlayFlash(() =>
        {
            imageFrame.Texture = flashbackImg;
            UiStage.Instance.HideTextBox();
            CharacterStage.Instance.HideAllCharacters();
            sepiaFilter.Show();
        });

        await ToSignal(GetTree().CreateTimer(duration), "timeout");

        await flashTransition.PlayFlash(() =>
        {
            imageFrame.Texture = oldBg;
            CharacterStage.Instance.ShowAllCharacters();
            sepiaFilter.Hide();
        });


        UiStage.Instance.AnimateShowTextBox();
    }

}