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
    } 
        
    public static void ActionFinished()
    {
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