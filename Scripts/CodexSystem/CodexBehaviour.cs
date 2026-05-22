using Godot;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

public partial class CodexBehaviour : Node
{
    [ExportGroup("UI Sounds")]
    [Export] AudioStream hoverSound;
    [Export] AudioStream clickSound;


    [ExportGroup("Displayer")]
    [Export] ContentDisplayer displayer;
    [Export] Container entryButtonContainer;

    [ExportGroup("Mode Buttons")]
    [Export] Button bCharacters;
    [Export] Button bTerms;
    [Export] Button bLocalizations;

    CodexLibrary library;
    CodexType    currentMode = CodexType.Character;

    public override void _Ready()
    {
        library = new CodexLibrary();
        library.LoadAll();



        bCharacters.Pressed    += () => SwitchMode(CodexType.Character);
        bTerms.Pressed         += () => SwitchMode(CodexType.Term);
        bLocalizations.Pressed += () => SwitchMode(CodexType.Localization);
        
        bCharacters.Resized += () => bCharacters.PivotOffset = bCharacters.Size / 2f;
        bTerms.Resized += () => bTerms.PivotOffset = bTerms.Size / 2f;
        bLocalizations.Resized += () => bLocalizations.PivotOffset = bLocalizations.Size / 2f;


        SwitchMode(CodexType.Character);
    }

    void SwitchMode(CodexType type)
    {
        currentMode = type;
        RebuildEntryButtons(library.GetList(type));
    }

    void RebuildEntryButtons(List<CoreInformation> entries)
    {
        foreach (Node child in entryButtonContainer.GetChildren())
            child.QueueFree();

        foreach (var entry in entries)
        {
            var capturedEntry = entry;

            var button = new ButtonFeedback
            {
                Text = entry.Title,
                HoverSound = hoverSound,
                ClickSound = clickSound
            };

            button.PivotOffset = new Vector2(button.Size.X / 2, button.Size.Y / 2);

            button.Pressed += () => displayer.DisplayContent(capturedEntry);
            entryButtonContainer.AddChild(button);
        }

        if (entries.Count > 0)
            displayer.DisplayContent(entries[0]);
    }
}