using Godot;
using System;
using System.Threading;

[GlobalClass]
public partial class GameStateManager : Node
{
    [Export] public PlayerBehaviour Player;
    State currentState = State.Explore;
    Node3D currentScene;
	InspectView inspectView;
    public static GameStateManager Instance { get; private set; }
    public override void _EnterTree() => Instance = this;
    public State GetState() => currentState;

    public void RegisterInspectView(InspectView iv) => inspectView ??= iv;

    public void ChangeState(State newState) => ChangeState<object>(newState, null);

    public void ChangeState<T>(State newState, T context = default)
    {
        if(currentState == newState) return;
        bool cancel = false;

        switch (newState)
        {
            case State.Dialog:
                EnterDialogState(context as string);
                break;
            case State.Explore:
                EnterExploreState();
                break;
            case State.Inspect:
                cancel = EnterInspectState(context as ObjectBehaviour);
                break;
        }
        if(!cancel)
            currentState = newState;
    }

    bool EnterInspectState(ObjectBehaviour obj)
    {
        if (inspectView != null && inspectView.CanInspect)
		{   
            if(!Player.isDebugging)
			    Player.SetInputBlocked(true);
			inspectView.DisplayObject(obj);
            return false;
		}
        return true;
    }


    private void EnterExploreState()
    {
        Player.SetInputBlocked(false);

        GD.Print("Sigamos explorando.");
    }

    void EnterDialogState(string dialogId)
    {
        Player.SetInputBlocked(true);

        GameManager.Instance.DialogManager.StartDialogScene(ConversationsDataBase.GetConversation(dialogId));
    }

}

public enum State
{
    Explore,
    Inspect,
    Dialog,
    Transition,
    Ending
}