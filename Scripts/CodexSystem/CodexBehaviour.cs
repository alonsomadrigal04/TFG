using Godot;
using System.Collections.Generic;

public partial class CodexBehaviour : Node
{
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

            var button = new Button
            {
                Text = entry.Title,
                // Opcional: si tienes un tema/estilo definido, asígnalo aquí
                // Theme = myButtonTheme
            };

            button.Pressed += () => displayer.DisplayContent(capturedEntry);
            entryButtonContainer.AddChild(button);
        }

        if (entries.Count > 0)
            displayer.DisplayContent(entries[0]);
    }
}