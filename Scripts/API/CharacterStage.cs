using Godot;
using Godot.Collections;
using System;

public partial class CharacterStage : Node
{
    public static CharacterStage Instance {get; private set;}
    Dictionary<Character, TextureRect> charactersInScene = new Dictionary<Character, TextureRect>();
    [Export] float appearingIntensity = 20f;
    [Export] float appearTime = 0.3f;

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
        if(!charactersInScene.ContainsKey(character))
            GD.PushError($"[CharacterStage] {character} not in the Scene");

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

    public void CharacterAppears(Character newCharacter, Vector2? summonPosition = null)
    {
        summonPosition ??= ToolKit.Center;

        if (charactersInScene.ContainsKey(newCharacter))
        {
            GD.PushError($"[CharacterStage] {newCharacter} is already in the scene");
            return;
        }

        TextureRect newPortrait = new()
        {
            Texture = newCharacter.Portraits[0],
            //PivotOffset = new Vector2(Size.X / 2, Size.Y / 2),
            Position = summonPosition.Value,
            Modulate = new Color(1, 1, 1, 0),
            Scale = Vector2.One * 0.05f
        };

        charactersInScene[newCharacter] = newPortrait;

        CallDeferred(nameof(AddAndAnimate), newPortrait);
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

}
