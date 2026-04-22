using System;
using System.Diagnostics;
using Godot;
[GlobalClass]
partial class ExplorationZoneCampFire : Node, IExplorationZone
{
    [Export] PackedScene npcScene;
    [ExportGroup("NPCS DATA")]
    [Export] NpcData AlonsoData;

    [ExportGroup("NPC POSITIONS")]
    [Export] Vector3 AlonsoPosition1;
    [Export] Vector3 AlonsoPosition2;
    // [Export] Vector3 NuriaPosition1;

    [ExportGroup("STAGE SETTINGS")] 
    [Export] CameraBehaviour cameraBehaviour;
    [Export] PlayerBehaviour player;
    [Export] StaticBody3D Stage;
    [Export] AnimationPlayer animationPlayer;
    DialogManager dialogManager;
    [ExportGroup("Sounds")]
    [Export] AudioStreamPlayer3D firesounds;
    [Export] AudioStreamPlayer ambiencesounds;



    public void SpawnCharacters()
    {
        // NpcBehaviour NuriaNpc = new(NuriaData);
        // // if cool logic
        // NuriaNpc.Position = AlonsoPosition1;
        NpcBehaviour AlonsoNpc = npcScene.Instantiate<NpcBehaviour>();
        AlonsoNpc.Initialize(AlonsoData);
        AddChild(AlonsoNpc);

        AlonsoNpc.Position = AlonsoPosition1;
        // if cool logic
        // if(CharacterDatabase.GetCharacter("nuria").characterState.Afinity < 70)
        //     AlonsoNpc.GlobalPosition = AlonsoPosition2;
    }
    public void ValidateFields()
    {
        if(cameraBehaviour == null || Stage == null )
            GD.Print("[EXPLORATION CAMPFIRE] There are fields missing");
    }

    public override void _Process(double delta)
    {
        DebugDraw3D.DrawSphere(AlonsoPosition1);
    }

    public override void _Ready()
    {
        GetPlayer();
        ValidateFields();
        SpawnCharacters();

        GetDialogManager();

        if (GameManager.Instance.enabledDebugMode)
        {
            animationPlayer.Play("In_Gameplay");
            animationPlayer.Seek(animationPlayer.CurrentAnimationLength -0.1f, false);
        }


        animationPlayer.AnimationFinished += HandleAnimations;
        
    }

    private void HandleAnimations(StringName animName)
    {
        switch (animName)
        {
            case "Post_Prologue":
                animationPlayer.Play("In_Gameplay");
                StartSounds();
                break;
            case "In_Gameplay":
                SetControlTrue();
                break;

        }
    }

    void SetControlTrue()
    {
        cameraBehaviour.IsActive = true;
        GameManager.Instance.IsDialogueActive = false;
    }

    void StartSounds()
    {
        firesounds.Play();
        ambiencesounds.Play();
    }

    void GetDialogManager()
    {
        dialogManager = GameManager.Instance.DialogManager;
        dialogManager.DialogEnded += SetAnimation;
    }

    void SetAnimation()
    {
        GameManager.Instance.IsDialogueActive = true;
        animationPlayer.Play("Post_Prologue");
        dialogManager.DialogEnded -= SetAnimation;
    }

    void GetPlayer()
    {
        GameStateManager.Instance.Player = player;
    }
}