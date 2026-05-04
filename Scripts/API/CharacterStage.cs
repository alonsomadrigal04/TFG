using Godot;
using Godot.Collections;
using Utility;

public partial class CharacterStage : Node
{
    public static CharacterStage Instance { get; private set; }
    public static bool IsThinking { get; private set; } = false;

    public static Dictionary<Character, CharacterActor> CharactersInScene { get; private set; } = [];

    [ExportGroup("TIME ANIMATIONS")]
    [Export(PropertyHint.Range, "0,2,0.1")] float appearTime = 1.0f;
    [Export(PropertyHint.Range, "0,2,0.1")] float disAppearTime = 1.0f;
    [Export(PropertyHint.Range, "0,2,0.1")] float moveTime = 1.0f;

    [ExportGroup("ANIMATIONS INTENSITY")]
    [Export] float bobIntensity = 15f;

    [ExportGroup("CHARACTER ACTORS")]
    [Export] int actorLayer = -1;
    [Export] Control characterContainer;

    [ExportGroup("FILTERS")]
    [Export] ColorRect blurshader;

    CharacterActor currentThinkingActor;

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;
    }

    public bool IsCharacterInScene(Character character)
        => CharactersInScene.ContainsKey(character);

    public CharacterActor GetActor(Character character)
    {
        if (!IsCharacterInScene(character))
        {
            GD.PrintErr($"[CharacterStage] Actor {character.Name} not in scene");
            return null;
        }

        return CharactersInScene[character];
    }

    // =========================
    // SPAWN / DESPAWN
    // =========================

    public void CharacterAppears(Character character, ScreenPosition screenPosition)
    {
        if (IsCharacterInScene(character))
            return;

        if (character.ActorScene == null)
        {
            GD.PrintErr($"[CharacterStage] {character.Name} has no ActorScene");
            return;
        }

        CharacterActor actor = character.ActorScene.Instantiate<CharacterActor>();

        CharactersInScene[character] = actor;

        AddAndAnimate(actor, screenPosition);
    }

    void AddAndAnimate(CharacterActor actor, ScreenPosition screenPosition)
    {
        characterContainer.AddChild(actor);

        actor.ZIndex = actorLayer;
        actor.Position = ToolKit.GetPosition(screenPosition);

        actor.SetFacing(screenPosition);

        AppearAnimation(actor);
    }

    void AppearAnimation(CharacterActor actor)
    {
        ActionBus.ActionStarted();

        Tween tween = CreateTween();

        actor.Modulate = new Color(1, 1, 1, 0);
        actor.Scale = Vector2.One * 0.5f;

        tween.TweenProperty(actor, "modulate:a", 1f, appearTime)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);

        tween.Parallel().TweenProperty(actor, "scale", Vector2.One, appearTime)
            .SetTrans(Tween.TransitionType.Quint)
            .SetEase(Tween.EaseType.Out);

        tween.Finished += ActionBus.ActionFinished;
    }

    public void CharacterDisappears(Character character)
    {
        if (!IsCharacterInScene(character))
        {
            GD.PrintErr($"[CharacterStage] {character.Name} not in scene");
            return;
        }

        CharacterActor actor = CharactersInScene[character];

        DisappearAnimation(actor, character);
    }

    void DisappearAnimation(CharacterActor actor, Character character)
    {
        ActionBus.ActionStarted();

        Tween tween = CreateTween();

        tween.SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(actor, "scale", actor.Scale * 0.75f, disAppearTime);
        tween.Parallel().TweenProperty(actor, "modulate:a", 0f, disAppearTime);

        tween.TweenCallback(Callable.From(() =>
        {
            CharactersInScene.Remove(character);
            actor.QueueFree();
        }));

        tween.Finished += ActionBus.ActionFinished;
    }

    // =========================
    // MOVEMENT
    // =========================

    public void MovePortrait(Character character, ScreenPosition targetPosition)
    {
        CharacterActor actor = GetActor(character);
        if (actor == null) return;

        MoveAnimation(actor, targetPosition);
    }

    void MoveAnimation(CharacterActor actor, ScreenPosition newDirection)
    {
        ActionBus.ActionStarted();

        Tween tween = CreateTween();

        tween.SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);

        tween.TweenProperty(
            actor,
            "position",
            ToolKit.GetPosition(newDirection),
            moveTime
        );

        tween.Finished += ActionBus.ActionFinished;
    }
    public void FlipCharacterHorizontally(Character character)
    {
        CharacterActor actor = GetActor(character);
        if (actor == null)
        {
            GD.PrintErr($"[CharacterStage] {character.Name} is not in the scene");
            return;
        }

        actor.FlipHorizontal();
    }

    public void AnimateTalking(Character speaker)
    {
        CharacterActor actor = GetActor(speaker);
        if (actor == null)
            return;

        actor.PlayTalk();
    }

    // =========================
    // EMOTIONS
    // =========================

    public void SetEmotion(Character character, string emotion)
    {
        CharacterActor actor = GetActor(character);
        if (actor == null) return;
        GD.Print($"Change emotion to  {emotion}");

        actor.PlayEmotion(emotion);
    }

    // =========================
    // THINKING MODE
    // =========================

    public void SetThinkingLayout(Character character, bool start)
    {
        CharacterActor actor = GetActor(character);
        if (actor == null) return;

        if (start)
        {
            IsThinking = true;
            ApplyBackgroundBlur(actor);
            return;
        }

        IsThinking = false;
        HideBackgroundBlur();
    }

    void ApplyBackgroundBlur(CharacterActor actor)
    {
        _ = BackgroundStage.Instance.AnimateFlash();

        actor.ZIndex = blurshader.ZIndex + 1;
        currentThinkingActor = actor;

        characterContainer.RemoveChild(actor);
        blurshader.AddChild(actor);

        blurshader.Show();
    }

    void HideBackgroundBlur()
    {
        if (currentThinkingActor == null)
            return;

        currentThinkingActor.ZIndex = actorLayer;

        blurshader.RemoveChild(currentThinkingActor);
        characterContainer.AddChild(currentThinkingActor);

        currentThinkingActor = null;

        blurshader.Hide();
    }

    // =========================
    // UTILITIES
    // =========================

    public void HideAllCharacters()
    {
        foreach (var actor in CharactersInScene.Values)
            actor.Hide();
    }

    public void ShowAllCharacters()
    {
        foreach (var actor in CharactersInScene.Values)
            actor.Show();
    }

    public void CleanEffects()
    {
        foreach (var kv in CharactersInScene)
            kv.Value.QueueFree();

        CharactersInScene.Clear();
        IsThinking = false;
    }
}