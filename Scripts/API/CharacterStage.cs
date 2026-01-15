using Godot;
using Godot.Collections;
using System;

public partial class CharacterStage : Control
{
    public static CharacterStage Instance {get; private set;}
    Dictionary<Character, TextureRect> charactersInScene = new Dictionary<Character, TextureRect>();

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
        if(charactersInScene.ContainsKey(newCharacter))
            GD.PushError($"[CharacterStage] {newCharacter} is already in the scene");
            
        TextureRect newPortrait = new(){
            Texture = newCharacter.Portraits[0],
            PivotOffset = new Vector2(Size.X/2, Size.Y/2),
            Position = summonPosition.Value,
            Modulate = new Color(1f, 1f, 1f, 0f)
            //TODO: Add Scale or  Size
        };

        AppearAnimation(newPortrait);

        charactersInScene[newCharacter] = newPortrait;

        //TODO: This will be in another Node container or smthing
        CallDeferred("add_child", newPortrait);

    }

    private void AppearAnimation(TextureRect newPortrait)
    {
        Tween tween = CreateTween();

        // TODO: finish this
        tween.SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(newPortrait, "modulate:a", 255f, 0.3f);
        tween.SetTrans(Tween.TransitionType.Elastic).SetParallel();

    }
}
