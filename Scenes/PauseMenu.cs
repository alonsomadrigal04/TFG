using Godot;
using System;

public partial class PauseMenu : Control
{
    [ExportGroup("Sliders")]
    [Export] HSlider musicSlider;
    [Export] HSlider sfxSlider;

    [ExportGroup("Buttons")]

    [Export] Button continueButton;
    [Export] Button exitButton;

    public override void _Ready()
    {
        continueButton.Pressed += ResumeGame;
        exitButton.Pressed += ExitGame;

        sfxSlider.ValueChanged += UpdateSFXVolume;
        musicSlider.ValueChanged += UpdateMusicVolume;

        InitializeSliders();
    }

    void InitializeSliders()
    {
        SetSliderToBusValue(sfxSlider, "SFX");
        SetSliderToBusValue(musicSlider, "Music");
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
    }

    void UpdateMusicVolume(double value)
    {
        SetBusVolume("Music", (float)value);
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
