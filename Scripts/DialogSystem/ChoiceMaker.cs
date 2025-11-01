using Game.Common.Modules;
using Godot;
using System;

public partial class ChoiceMaker : Node
{
    [Export] AnimationPlayer animationPlayer;
    [Export] MultiaudioPlayerModule multiaudioPlayerModule;
    [Export] AudioStream impact;

    [ExportGroup("Options")]
    [Export] RichTextLabel Option1;
    [Export] RichTextLabel Option2;

    [ExportGroup("Animated Objects")]
    [Export] ColorRect shaderBack;
    [Export] ColorRect grayBack;

    // Guarda las líneas siguientes asociadas a cada opción
    private string[] nextOptions;

    // Llama al DialogManager para avanzar a la línea escogida
    public Action<string> OnOptionSelected;

    public void ShowChoices(DialogLine line)
    {
        ActivateChoiceUI();
        multiaudioPlayerModule.PlaySound(impact);

        SetUpOptions(line);

        animationPlayer.Play("IntroQuestion");
    }

    void ActivateChoiceUI()
    {
        shaderBack.Visible = true;
        grayBack.Visible = true;
    }

    void SetUpOptions(DialogLine line)
    {
        string[] texts = line.Text.Split('|', StringSplitOptions.TrimEntries);
        Option1.Text = texts.Length > 0 ? texts[0] : "";
        Option2.Text = texts.Length > 1 ? texts[1] : "";

        nextOptions = line.Next?.Split('|', StringSplitOptions.TrimEntries) ?? Array.Empty<string>();

        Option1.Connect("gui_input", new Callable(this, nameof(OnOption1Selected)));
        Option2.Connect("gui_input", new Callable(this, nameof(OnOption2Selected)));
    }

    void OnOption1Selected(InputEvent @event)
    {
        if (!IsClick(@event)) return;
        ProcessSelection(0);
    }

    void OnOption2Selected(InputEvent @event)
    {
        if (!IsClick(@event)) return;
        ProcessSelection(1);
    }

    bool IsClick(InputEvent @event)
    {
        return @event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left;
    }

    void ProcessSelection(int optionIndex)
    {
        //animationPlayer.Play("OutQuestion");

        string nextUid = nextOptions.Length > optionIndex ? nextOptions[optionIndex] : null;

        HideChoiceUI();

        if (!string.IsNullOrEmpty(nextUid))
            OnOptionSelected?.Invoke(nextUid);
    }

    void HideChoiceUI()
    {
        Option1.Visible = false;
        Option2.Visible = false;
        shaderBack.Visible = false;
        grayBack.Visible = false;

        Option1.Disconnect("gui_input", new Callable(this, nameof(OnOption1Selected)));
        Option2.Disconnect("gui_input", new Callable(this, nameof(OnOption2Selected)));
    }
}
