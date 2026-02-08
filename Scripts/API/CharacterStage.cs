using Godot;
using Godot.Collections;
using System;
using Utility;

public partial class CharacterStage : Node
{
    public static CharacterStage Instance {get; private set;}
    public static Dictionary<Character, TextureRect> CharactersInScene {get; private set;} = [];

    [Export(PropertyHint.Range, "0,20,0.1")]
    float appearingIntensity = 5f; // TODO: This variable not seems to do things propetly
    [Export] float appearTime = 0.3f; // is the time that takes for a character to appear
    [Export] float disAppearTime = 0.3f;
    [Export] int portraitLayer = -1;

    Dictionary<Character, Tween> activeTweens = [];
    [Export] float bobIntensity = 30f;
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
        Tween tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(portrait, "anchor_left", ToolKit.XPositions[newDirection], 0.3f);
        tween.SetParallel().TweenProperty(portrait, "anchor_right", ToolKit.GetAnchorPosition(newDirection), 0.3f);
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
        newPortrait.SetAnchorOffsetToZero();

        newPortrait.Scale = Vector2.One  * (1f / (1f + appearingIntensity));

        CharactersInScene[newCharacter] = newPortrait;

        CallDeferred(nameof(AddAndAnimate), newPortrait);

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
    }

    void AddAndAnimate(TextureRect portrait)
    {
        characterContainer.AddChild(portrait);
        AppearAnimation(portrait);
    }

    void AppearAnimation(TextureRect portrait)
    {
        Tween tween = CreateTween();

        Vector2 startPos = portrait.Position + new Vector2(0, 40);
        Vector2 endPos = portrait.Position;

        portrait.Position = startPos;
        portrait.Modulate = portrait.Modulate with { A = 0.4f };
        portrait.Scale = new Vector2(0.95f, 1.05f);

        tween.TweenProperty(portrait, "modulate:a", 1f, appearTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        tween.Parallel()
            .TweenProperty(portrait, "position", endPos + new Vector2(0, -6), appearTime * 0.5f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        tween.Parallel()
            .TweenProperty(portrait, "scale", Vector2.One, appearTime * 0.5f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(portrait, "position", endPos, appearTime * 0.5f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
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
        Tween tween = CreateTween();
        Vector2 originalPosition= portrait.Position;

        tween.SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(portrait, "position", originalPosition + new Vector2(0, -bobIntensity), 0.1f);

        tween.TweenProperty(portrait, "position", originalPosition, 0.1f);

        portrait.Position = originalPosition;

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
