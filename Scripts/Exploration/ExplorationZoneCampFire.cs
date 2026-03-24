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

    public void SpawnCharacters()
    {
        // NpcBehaviour NuriaNpc = new(NuriaData);
        // // if cool logic
        // NuriaNpc.Position = AlonsoPosition1;
        NpcBehaviour AlonsoNpc = npcScene.Instantiate<NpcBehaviour>();
        AlonsoNpc.Initialize(AlonsoData);
        AddChild(AlonsoNpc);
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
    }

    void GetPlayer()
    {
        GameStateManager.Instance.Player = player;
    }
}