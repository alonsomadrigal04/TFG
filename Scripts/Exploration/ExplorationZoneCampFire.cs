using Godot;
using System;

[GlobalClass]
public partial class ExplorationZoneCampFire : Node, IExplorationZone
{
    [Export] PackedScene npcScene;

    [ExportGroup("NPCS DATA")]
    [Export] NpcData alonsoData;

    [ExportGroup("NPC POSITIONS")]
    [Export] Vector3 alonsoPosition1;
    [Export] Vector3 alonsoPosition2;

    [ExportGroup("STAGE SETTINGS")]
    [Export] CameraBehaviour cameraBehaviour;
    [Export] PlayerBehaviour player;
    [Export] StaticBody3D stage;

    [ExportGroup("ANIMATIONS")]
    [Export] AnimationPlayer cinematicAnimationPlayer;
    [Export] AnimationPlayer uiAnimationPlayer;

    [ExportGroup("SOUNDS")]
    [Export] AudioStreamPlayer3D fireSounds;
    [Export] AudioStreamPlayer ambienceSounds;

    [ExportGroup("LORE")]
    [Export] LorePoint lorePoint;

    [ExportGroup("UI")]
    [Export] CanvasLayer dialogFrameLayer;

    DialogManager dialogManager;

    bool introPlayed;

    public override void _Ready()
    {
        GetPlayer();
        ValidateFields();
        SpawnCharacters();

        dialogFrameLayer.Hide();

        GetDialogManager();

        cinematicAnimationPlayer.AnimationFinished += OnCinematicFinished;

        if (GameManager.Instance.enabledDebugMode)
        {
            SkipToGameplay();
        }
    }

    public override void _ExitTree()
    {
        cinematicAnimationPlayer.AnimationFinished -= OnCinematicFinished;

        if (dialogManager != null)
        {
            dialogManager.DialogEnded -= OnDialogEnded;
            dialogManager.DialogStarted -= OnDialogStarted;
        }
    }

    public override void _Process(double delta)
    {
        DebugDraw3D.DrawSphere(alonsoPosition1);
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("skip"))
        {
            SkipCurrentAnimation();
        }
    }

    void GetPlayer()
    {
        GameStateManager.Instance.Player = player;
    }

    void GetDialogManager()
    {
        dialogManager = GameManager.Instance.DialogManager;

        dialogManager.DialogStarted += OnDialogStarted;
        dialogManager.DialogEnded += OnDialogEnded;
    }

    void SpawnNpc(NpcData npcData, Vector3 position)
    {
        NpcBehaviour npc = npcScene.Instantiate<NpcBehaviour>();

        npc.Initialize(npcData);

        AddChild(npc);

        npc.Position = position;
    }

    void OnDialogStarted()
    {
        // if (GameManager.Instance.IsDialogueActive)
        //     return;

        PlayUIAnimation("EnterDialog");

        dialogFrameLayer.Show();

        GD.Print("Dialog Enter Animation");
    }

    void OnDialogEnded()
    {
        if (!introPlayed)
        {
            StartIntroSequence();
            return;
        }

        EndDialogUI();
    }

    void StartIntroSequence()
    {
        introPlayed = true;

        GameManager.Instance.IsDialogueActive = true;

        GD.Print("Intro Started");

        cinematicAnimationPlayer.Play("Post_Prologue");
    }

    void EndDialogUI()
    {
        GD.Print("Dialog Exit Animation");

        PlayUIAnimation("ExitDialog");
    }

    void OnCinematicFinished(StringName animationName)
    {
        switch (animationName)
        {
            case "Post_Prologue":

                StartSounds();

                cinematicAnimationPlayer.Play("In_Gameplay");

                break;

            case "In_Gameplay":

                EnableGameplay();

                break;

        }
    }

    public void HideLayer()
    {
        dialogFrameLayer.Hide();
    }

    void EnableGameplay()
    {
        cameraBehaviour.IsActive = true;

        GameManager.Instance.IsDialogueActive = false;

        GD.Print("Gameplay Enabled");
    }

    void PlayUIAnimation(string animationName)
    {
        if (uiAnimationPlayer.CurrentAnimation == animationName)
            return;

        uiAnimationPlayer.Play(animationName);
    }

    void SkipCurrentAnimation()
    {
        AnimationPlayer currentPlayer = cinematicAnimationPlayer.IsPlaying()
            ? cinematicAnimationPlayer
            : uiAnimationPlayer;

        if (!currentPlayer.IsPlaying())
            return;

        string currentAnimation = currentPlayer.CurrentAnimation;

        currentPlayer.Stop();

        GD.Print($"Skipped animation: {currentAnimation}");

        if (currentPlayer == cinematicAnimationPlayer)
        {
            OnCinematicFinished(currentAnimation);
        }
    }

    void SkipToGameplay()
    {
        cinematicAnimationPlayer.Play("In_Gameplay");

        cinematicAnimationPlayer.Seek(
            cinematicAnimationPlayer.CurrentAnimationLength,
            true
        );

        EnableGameplay();
    }

    void StartSounds()
    {
        fireSounds?.Play();
        ambienceSounds?.Play();
    }

    public void SpawnCharacters()
    {
        SpawnNpc(alonsoData, alonsoPosition1);
    }

    public void ValidateFields()
    {
        if (cameraBehaviour == null)
            GD.PushError("[EXPLORATION CAMPFIRE] CameraBehaviour is missing.");

        if (stage == null)
            GD.PushError("[EXPLORATION CAMPFIRE] Stage is missing.");

        if (cinematicAnimationPlayer == null)
            GD.PushError("[EXPLORATION CAMPFIRE] Cinematic AnimationPlayer is missing.");

        if (uiAnimationPlayer == null)
            GD.PushError("[EXPLORATION CAMPFIRE] UI AnimationPlayer is missing.");
    }
}