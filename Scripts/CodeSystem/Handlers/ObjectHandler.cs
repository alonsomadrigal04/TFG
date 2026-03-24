using Godot;
using System;
using System.Collections.Generic;

public class ObjectHandler : ICommandHandler
{
    public HashSet<string> Supportedverbs => supportedVerbs;
    public HashSet<string> supportedVerbs = [
        "shows",
        "hides"
    ];

    public void Execute(CommandToken commandToken)
    {
        switch (commandToken.Verb)
        {
            case "shows":
                ObjectAppear(commandToken);
                break;
            case "hides":
                ObjectDissappear(commandToken);
                break;
            default:
                break;
        }
    }

    private void ObjectDissappear(CommandToken commandToken)
    {
        if(!ObjectStage.Instance.IsObjectInScene)
        {
            GD.PrintErr("[ObjectStage] SpecialItem there is no objects in the scene");
            return;
        }
        string item = commandToken.Subject.ToLower();
        if (!ObjectDataBase.LoadedSpecialItems.TryGetValue(item, out ObjectData objectData))
            GD.PrintErr($"[ObjectStage] {item} is not registered");
        Texture2D icon = objectData.Icon;
        ObjectStage.Instance.DisappearObject(icon);
    }

    void ObjectAppear(CommandToken commandToken)
    {
        if(ObjectStage.Instance.IsObjectInScene)
        {
            GD.PrintErr("[ObjectStage] SpecialItem already in the scene");
            return;
        }
        string item = commandToken.Subject.ToLower();
        if (!ObjectDataBase.LoadedSpecialItems.TryGetValue(item, out ObjectData objectData))
            GD.PrintErr($"[ObjectStage] {item} is not registered");
        Texture2D icon = objectData.Icon;
        ObjectStage.Instance.AppearObject(icon);
    }

    public void CleanEffects()
    {
        ObjectStage.Instance.CleanEffects();
    }
}