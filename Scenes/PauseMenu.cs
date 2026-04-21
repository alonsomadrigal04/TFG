using Godot;
using System;

public partial class PauseMenu : Control
{
    [ExportGroup("Sliders")]
    [Export] HSlider musicSlider;
    [Export] HSlider sfxSlider;
    [Export] HSlider voiceSlider;

    [Export] AudioStreamPlayer indicatorSoundMusic;
    [Export] AudioStreamPlayer indicatorSoundSFX;
    [Export] AudioStreamPlayer indicatorSoundVoices;



    [ExportGroup("Buttons")]

    [Export] Button continueButton;
    [Export] Button exitButton;

    public override void _Ready()
    {
        continueButton.Pressed += ResumeGame;
        exitButton.Pressed += ExitGame;

        sfxSlider.ValueChanged += UpdateSFXVolume;
        musicSlider.ValueChanged += UpdateMusicVolume;
        voiceSlider.ValueChanged += UpdateVoicesVolume;

        InitializeSliders();
    }



    void InitializeSliders()
    {
        SetSliderToBusValue(sfxSlider, "SFX");
        SetSliderToBusValue(musicSlider, "Music");
        SetSliderToBusValue(voiceSlider, "Voices");

    }

    void SetSliderToBusValue(HSlider slider, string busName)
    {
        int index = AudioServer.GetBusIndex(busName);

        if (index == -1)
        {
            GD.PrintErr($"[Pause Menu] Bus {busName} does not exist");
            return;
        }

        float db = AudioServer.GetBusVolumeDb(index);
        float linear = Mathf.DbToLinear(db);

        slider.SetValueNoSignal(linear);
    }

    void UpdateSFXVolume(double value)
    {
        SetBusVolume("SFX", (float)value);
        indicatorSoundSFX.Play();
    }

    void UpdateVoicesVolume(double value)
    {
        SetBusVolume("Voices", (float)value);
        indicatorSoundVoices.Play();
        
    }
    void UpdateMusicVolume(double value)
    {
        SetBusVolume("Music", (float)value);
        indicatorSoundMusic.Play();

    }

    void SetBusVolume(string busName, float value)
    {
        int index = AudioServer.GetBusIndex(busName);

        if (index == -1)
        {
            GD.PrintErr($"[Pause Menu] Bus {busName} does not exist");
            return;
        }

        AudioServer.SetBusVolumeDb(index, LinearToDb(value));
    }

    static float LinearToDb(float linear) =>(linear <= 0) ? -80f : 20f * Mathf.Log(linear);

    void ResumeGame() => GameManager.Instance.PauseGame();


    void ExitGame() => GameManager.Instance.ExitGame();
}
