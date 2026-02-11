using System;
using Godot;
public static class ActionBus
{
    private static int runningActions = 0;
    public static event Action AllActionsFinished;
    public static bool IsBusy => runningActions > 0;
    public static void ActionStarted()
    {
        runningActions++;
        GD.Print("START");
    } 
        
    public static void ActionFinished()
    {
        GD.Print($"ENDED");

        if (runningActions <= 0)
        {
            GD.PrintErr("ActionFinished called without matching ActionStarted");
            return;
        }

        runningActions--;
        if (runningActions == 0)
            AllActionsFinished?.Invoke();
    }


    public static Action RunAfterActions(Action action){
        if(!IsBusy){
            action();
            return () => {};
        }

        void Callback(){
            AllActionsFinished -= Callback;
            action();
        }
        
        AllActionsFinished += Callback;
        return ()=>{
            AllActionsFinished -= Callback;
        };
    }
}