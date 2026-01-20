using Godot;
using Godot.Collections;
using System;

public partial class CharacterStage : Node
{
    public static CharacterStage Instance {get; private set;}
    Dictionary<Character, TextureRect> charactersInScene = [];

    [Export(PropertyHint.Range, "0,20,0.1")]
    float appearingIntensity = 5f;
    [Export] float appearTime = 0.3f;
    [Export] float disAppearTime = 0.3f;
    [Export] int portraitLayer = -1;

    Dictionary<Character, Tween> activeTweens = [];

    public override void _Ready()
    {
        if(Instance != null && Instance != this){
            QueueFree();
            return;
        }
        Instance = this;
    }

    public void MovePortrait(Character character, Vector2 newDirection)
    {
        if(!charactersInScene.ContainsKey(character)) {
            GD.PrintErr($"[CharacterStage] {character} not in the Scene");
            return;
        }

        TextureRect portrait = charactersInScene[character];
        MoveAnimation(portrait, newDirection);        
    }

    void MoveAnimation(TextureRect portrait, Vector2 newDirection)
    {
        Tween tween = CreateTween();

        tween
        .SetTrans(Tween.TransitionType.Back)
        .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(portrait, "position", newDirection, 0.2f);
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
            Modulate = new Color(1, 1, 1, 0),
            Scale = Vector2.One  * (1f / (1f + appearingIntensity))
        };
        newPortrait.ResetSize();

        newPortrait.PivotOffset = newPortrait.Size / 2;
        newPortrait.ZIndex = portraitLayer;
        ToolKit.SetPosition(newPortrait, screenPosition);


        charactersInScene[newCharacter] = newPortrait;

        CallDeferred(nameof(AddAndAnimate), newPortrait);
    }

    public void CharacterDisappears(Character character)
    {
        if (!IsCharacterInScene(character))
        {
            GD.PrintErr($"[CharacterStage] {character.Name} is not in the Scene");
            return;
        }
        TextureRect textureToDestroy = charactersInScene[character];
        DisapearAnimation(textureToDestroy, character);
    }

    void DisapearAnimation(TextureRect textureToDestroy, Character character)
    {
        Tween tween = CreateTween();

        tween.SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(textureToDestroy, "scale", textureToDestroy.Scale * 0.75f, disAppearTime);
        tween.Parallel()
            .TweenProperty(textureToDestroy, "modulate:a", 0f, disAppearTime);

        tween.TweenCallback(Callable.From(() =>
        {
            charactersInScene.Remove(character);
            textureToDestroy.QueueFree();
        }));
    }


    void AddAndAnimate(TextureRect portrait)
    {
        AddChild(portrait);
        AppearAnimation(portrait);
    }


    void AppearAnimation(TextureRect portrait)
    {
        Tween tween = CreateTween();

        tween.TweenProperty(portrait, "modulate:a", 1f, appearTime);

        tween.Parallel()
            .TweenProperty(portrait, "scale", Vector2.One, appearTime)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
    }

    public bool IsCharacterInScene(Character character) => charactersInScene.ContainsKey(character);

}
