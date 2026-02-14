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

    public void MovePortrait(Character character, ScreenPosition targetPosition)
    {
        if (!IsCharacterInScene(character))
        {
            GD.PrintErr($"[CharacterStage] {character} not in the Scene");
            return;
        }

        TextureRect portrait = CharactersInScene[character];
        ScreenPosition currentSide = ToolKit.GetScreenSide(portrait.Position);

        bool currentIsLeft = currentSide == ScreenPosition.FarLeft
                        || currentSide == ScreenPosition.Left
                        || currentSide == ScreenPosition.Center;

        bool targetIsLeft = targetPosition == ScreenPosition.FarLeft
                        || targetPosition == ScreenPosition.Left
                        || targetPosition == ScreenPosition.Center;

        if (currentIsLeft != targetIsLeft)
            FlipCharacterHorizontally(character);

        MoveAnimation(portrait, targetPosition);
    }


    void MoveAnimation(TextureRect portrait, ScreenPosition newDirection)
    {
        ActionBus.ActionStarted();

        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(portrait, "position", ToolKit.GetPosition(newDirection) - new Vector2(portrait.Size.X /2, portrait.Size.Y /2), moveTime);

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

    public void ShakeCharacter(Character character)
    {
        if (!IsCharacterInScene(character))
            GD.PrintErr($"[CharacterScene] {character} is not in the scene");
        
        TextureRect portrait = GetCharacterPortrait(character);

        AnimateShake(portrait);
    }

    public void FlipCharacterHorizontally(Character character)
    {
        if (!IsCharacterInScene(character))
        {
            GD.PrintErr($"[CharacterScene] {character} is not in the scene");
            return;
        }
        ActionBus.ActionStarted();

        TextureRect portrait = GetCharacterPortrait(character);

        float duration = 0.5f;
        float half = duration * 0.5f;

        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.InOut);

        tween.TweenDelegate<Color>(
            v => portrait.Modulate = v,
            Colors.White,
            Colors.Black,
            0.1f
        );

        tween.TweenDelegate<Vector2>(
            v => portrait.Scale = v,
            Vector2.One,
            new Vector2(0f, 1f),
            half
        );

        tween.TweenCallback(Callable.From(() =>
        {
            portrait.FlipH = !portrait.FlipH;
        }));

        tween.TweenDelegate<Vector2>(
            v => portrait.Scale = v,
            new Vector2(0f, 1f),
            Vector2.One,
            half
        );

        tween.TweenDelegate<Color>(
            v => portrait.Modulate = v,
            Colors.Black,
            Colors.White,
            0.1f
        );

        tween.Finished += ActionBus.ActionFinished;
    }


    public async void AnimateShake(TextureRect portrait)
    {
        ActionBus.ActionStarted();
        Vector2 originalPos = portrait.Position;

        var tween = portrait.CreateTween();
        tween.SetTrans(Tween.TransitionType.Sine);
        tween.SetEase(Tween.EaseType.InOut);

        float strength = 4f;
        float duration = 0.05f;
        int shakes = 4;

        for (int i = 0; i < shakes; i++)
        {
            Vector2 offset = new Vector2(
                (float)GD.RandRange(-strength, strength),
                (float)GD.RandRange(-strength, strength)
            );

            tween.TweenProperty(portrait, "position", originalPos + offset, duration);
        }

        tween.TweenProperty(portrait, "position", originalPos, duration);

        tween.Finished += ActionBus.ActionFinished;
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
