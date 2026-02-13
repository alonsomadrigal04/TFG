using Godot;
using Godot.Collections;
using System;
using System.Xml.Serialization;
using Utility;

public partial class CharacterStage : Node
{
    public static CharacterStage Instance {get; private set;}
    public static Dictionary<Character, TextureRect> CharactersInScene {get; private set;} = [];

    [ExportGroup("TIME ANIMATIONS")]
    [Export(PropertyHint.Range, "0,2,0.1")]
    float appearTime = 1.0f; // is the time that takes for a character to appear
    [Export(PropertyHint.Range, "0,2,0.1")]
    float disAppearTime = 1.0f;
    [Export(PropertyHint.Range, "0,2,0.1")]
    float moveTime = 1.0f;

    [ExportGroup("ANIMATIONS INTENSITY")]
    [Export] float bobIntensity = 15f;

    [ExportGroup("CHARACTER PORTRAIT")]
    Dictionary<Character, Tween> activeTweens = [];
    [Export] int portraitLayer = -1;
    [Export] Control characterContainer;

    public override void _Ready()
    {
        if(Instance != null && Instance != this){
            QueueFree();
            return;
        }
        Instance = this;
    }

    public void MovePortrait(Character character, ScreenPosition position)
    {
        if(!IsCharacterInScene(character)) {
            GD.PrintErr($"[CharacterStage] {character} not in the Scene");
            return;
        }

        TextureRect portrait = CharactersInScene[character];
        MoveAnimation(portrait, position);        
    }

    void MoveAnimation(TextureRect portrait, ScreenPosition newDirection)
    {
        ActionBus.ActionStarted();

        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(portrait, "position", ToolKit.GetPosition(newDirection) - new Vector2(0, portrait.Size.Y /2), moveTime);

        tween.Finished += ActionBus.ActionFinished;
    }

    /// <summary>
    /// Makes a character appear on the stage.
    /// </summary>
    /// <param name="newCharacter">The character to appear.</param>
    /// <param name="screenPosition">The position on the screen where the character should appear.</param>
    public void CharacterAppears(Character newCharacter, ScreenPosition screenPosition)
    {
        if (IsCharacterInScene(newCharacter))
        {
            GD.PrintErr($"[CharacterStage] {newCharacter.Name} is already in the scene");
            return;
        }

        TextureRect newPortrait = new()
        {
            Texture = newCharacter.Portraits[0],
            Modulate = new Color(1, 1, 1, 0)
        };

        newPortrait.ResetSize();

        newPortrait.PivotOffset = newPortrait.Size / 2;
        newPortrait.ZIndex = portraitLayer;
        ToolKit.SetPosition(newPortrait, screenPosition);
        newPortrait.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);

        CharactersInScene[newCharacter] = newPortrait;
        AddAndAnimate(newPortrait);
        // CallDeferred(nameof(AddAndAnimate), newPortrait);

    }

    public void CharacterDisappears(Character character)
    {
        if (!IsCharacterInScene(character))
        {
            GD.PrintErr($"[CharacterStage] {character.Name} is not in the Scene");
            return;
        }
        TextureRect textureToDestroy = CharactersInScene[character];
        DisappearAnimation(textureToDestroy, character);
    }

    void DisappearAnimation(TextureRect textureToDestroy, Character character)
    {
        ActionBus.ActionStarted();
        Tween tween = CreateTween();

        tween.SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(textureToDestroy, "scale", textureToDestroy.Scale * 0.75f, disAppearTime);
        tween.Parallel()
            .TweenProperty(textureToDestroy, "modulate:a", 0f, disAppearTime);

        tween.TweenCallback(Callable.From(() =>
        {
            CharactersInScene.Remove(character);
            textureToDestroy.QueueFree();
        }));
        tween.Finished += ActionBus.ActionFinished;
    }

    void AddAndAnimate(TextureRect portrait)
    {
        characterContainer.AddChild(portrait);
        AppearAnimation(portrait);
    }

    void AppearAnimation(TextureRect portrait)
    {
        ActionBus.ActionStarted();

        Tween tween = CreateTween();

        Vector2 endPos = portrait.Position;

        portrait.Modulate = new Color(0,0,0,0);
        portrait.Scale = new Vector2(0.5f, 0.5f);

        tween.TweenProperty(portrait, "modulate", Colors.White, appearTime)
        .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.In);

        tween.Parallel().TweenProperty(portrait, "scale", Vector2.One, appearTime)
                        .SetTrans(Tween.TransitionType.Quint).SetEase(Tween.EaseType.Out);

        tween.Finished += ActionBus.ActionFinished;

    }


    public bool IsCharacterInScene(Character character) => CharactersInScene.ContainsKey(character);

    public void AnimateTalking(Character speaker)
    {
        if (IsCharacterInScene(speaker))
        {
            TextureRect textureRect = CharactersInScene[speaker];
            TalkAnimation(textureRect);
            return;
        }
        GD.PrintErr($"[CharacterStage] {speaker.Name} is not on the scene");
    }

    private void TalkAnimation(TextureRect portrait)
    {
        ActionBus.ActionStarted();

        Tween tween = CreateTween();
        Vector2 originalPosition= portrait.Position;

        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(portrait, "position", originalPosition + new Vector2(0, -bobIntensity), 0.1f);

        tween.TweenProperty(portrait, "position", originalPosition, 0.1f);

        portrait.Position = originalPosition;

        tween.Finished += ActionBus.ActionFinished;
    }

    public static Vector2 GetCharacterPosition(CommandToken commandToken, Vector2 resetPosition)
    {
        if (CharacterDatabase.TryGetCharacter(commandToken.Arguments[0], out Character character))
        {
            TextureRect portraitZoomed = CharactersInScene[character];
            resetPosition = portraitZoomed.Position;
        }

        return resetPosition;
    }
    public TextureRect GetCharacterPortrait(Character character)
    {
        if(!IsCharacterInScene(character))
            GD.PrintErr($"[CharacterStage] {character} not in the Scene");
        return CharactersInScene[character];
    }

    public void HideAllCharacters()
    {
        foreach (TextureRect character in CharactersInScene.Values)
        {
            character.Hide();
        }
    }

    public void ShowAllCharacters()
    {
        foreach (TextureRect character in CharactersInScene.Values)
        {
            character.Show();
        }
    }
}
