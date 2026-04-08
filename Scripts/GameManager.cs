using Game;
using Godot;
using Microsoft.VisualBasic.FileIO;
using System;
using System.IO;
using System.Threading;
using Utility;
[GlobalClass]
public partial class GameManager : Node
{
    [ExportSubgroup("DEBUG MODE")]
    [Export] public bool enabledDebugMode = false;
    [Export] string debugDialogName = "";

    [ExportSubgroup("Launch")]
    [Export] PackedScene firstEnvironment;

    [ExportSubgroup("Dialog Path")]
    [Export] NodePath dialogManagerPath;
    [ExportSubgroup("Pause Menu")]
    [Export] AnimationPlayer pauseAnimationPlayer;


    [ExportSubgroup("Loading Screen")]
    [Export] ScreenTransition screenTransition;
    [Export] ColorRect safeScreen;
    [Export] float defaultLoadScreenSeconds = 1;


    [Export] public ChapterBehaviour chapterBehaviour;
    public string CurrentChapterTitle => chapterBehaviour.GetChapterName();
    public string CurrentChapterSubTitle => chapterBehaviour.GetChapterSubName();

    public DialogManager dialogManager;
    public bool IsDialogueActive {get; private set;}= false;

    static CancellationTokenSource changeEnvCts;
    static Node currentEnvironment;

    public static bool GlobalInputsEnabled { get => globalInputs; set => globalInputs = value; }
    public static bool GameStarted = false;


    static bool globalInputs = true;
    public static GameManager Instance { get => instance; set => instance = value; }

    private static GameManager instance;
    public override void _EnterTree() => Instance = this;


    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public override void _Ready()
    {
        InitializeGameData();
        safeScreen.Show();
        GD.Print("Launching the game...\n");
        GodotExtensions.CallDeferred(LaunchGame);
    }

    void InitializeGameData()
    {
        CharacterDatabase.LoadFromJson("Data\\Characters.json");
        BackgroundDataBase.Load();
        ObjectDataBase.Load();
        ToolKit.InitializePositions();
        ConversationsDataBase.Load();
        dialogManager = GetNode<DialogManager>(dialogManagerPath);

    }

    static void LaunchGame()
    {
        GD.Print("============ SCENE TREE LOADED: GAME LAUNCHED ============\n");

        instance.GetTree().Root.ProcessMode = ProcessModeEnum.Always; // Default processing is Always. Set Pausable on desired nodes
        if (instance.enabledDebugMode)
        {
            StartDebug();
            return;
        }
        ChangeEnvironment(GameEnvironments.MainMenu);
    }

    static void StartDebug()
    {
        GameStarted = true;
        ChangeEnvironment(GameEnvironments.OnlyTextBox);
        instance.dialogManager.StartDialogScene(ConversationsDataBase.GetConversation(instance.debugDialogName));
    }

    public async static void ChangeEnvironment(PackedScene newEnv, float loadTimeFactor = 1)
        => GodotExtensions.CallDeferred(() => ChangeEnvironmentDeferred(newEnv, loadTimeFactor));
    async static void ChangeEnvironmentDeferred(PackedScene newEnv, float loadTimeFactor = 1)
    {
        changeEnvCts?.Cancel();
        changeEnvCts?.Dispose();
        changeEnvCts = new CancellationTokenSource();
        CancellationToken token = changeEnvCts.Token;
        GlobalInputsEnabled = false;

        await Instance.screenTransition.FadeIn(token);
        if (token.IsCancellationRequested) return;

        // Old environment removal
        currentEnvironment?.QueueFree();

        // New environment creation
        currentEnvironment = newEnv.Instantiate();
        Instance.GetTree().CurrentScene.AddChild(currentEnvironment);

        // Restore global game state
        Engine.TimeScale = 1f;

        GD.Print($"Switched ENV to {currentEnvironment.GetType()}\n");

        await GodotExtensions.Delay(Instance.defaultLoadScreenSeconds * loadTimeFactor);
        if (token.IsCancellationRequested) return;

        if(GameStateManager.Instance.GetState() == State.Explore)
            DesactivateSafeScreen();
        await Instance.screenTransition.FadeOut(token);

        Instance.screenTransition.Hide();
        GlobalInputsEnabled = true;
    }

    static void DesactivateSafeScreen()
    {
        instance.safeScreen.Hide();
    }


    public void PauseGame()
    {
        if(!GameStarted) return;
        PauseManager.GamePaused = !PauseManager.GamePaused;
        if(PauseManager.GamePaused)
            pauseAnimationPlayer.Play("OnPause");
        else
            pauseAnimationPlayer.Play("OutPause");
    }

    public override void _Input(InputEvent e)
    {
        if(Input.IsAnythingPressed() && !GlobalInputsEnabled) return;

        if (e.IsActionPressed("pause"))
        {
            PauseGame();
        }
    }


    public override void _Process(double delta)
    {
        if (dialogManager.IsSpeaking && Input.IsActionJustPressed("ui_accept") && !ActionBus.IsBusy)
        {
            dialogManager.OnNextRequested();
        }
        if(dialogManager.IsSpeaking && Input.IsActionPressed("fasterDialog"))
        {
            Engine.TimeScale *= 2;
        }
        if (dialogManager.IsSpeaking && Input.IsActionJustReleased("fasterDialog"))
        {
            Engine.TimeScale = 1;
        }
    }

    public void StartNewChapter() => chapterBehaviour.ChangeChapter();
    public int GetChapterNumber() => chapterBehaviour.CurrentChapter;

    public void ExitGame()
    {
        GetTree().Quit();
    }

}
