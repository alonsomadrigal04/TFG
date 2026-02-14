using Godot;
using Godot.Collections;


public partial class ObjectStage : Node
{
    public static ObjectStage Instance {get; private set;}
    public override void _Ready()
    {
        if(Instance != null && Instance != this){
            QueueFree();
            return;
        }
        Instance = this;
    }

    [Export] Control objectLayer;
    [Export] Material frameShader;
    public static Dictionary<Character, TextureRect> CharactersInScene {get; private set;} = [];


    public void AppearObject(CommandToken commandToken)
    {
        TextureRect newObject = new TextureRect()
        {
            
        };
    }
}