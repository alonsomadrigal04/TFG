using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Utility;


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

    [Export] CanvasLayer objectLayer;
    
    [Export] float yOffset = 25f;
    [Export]  float xOffset = 25f;
    [Export] PackedScene specialItemFrameScene;
    SpecialItemFrame specialItemFrame;
    public bool IsObjectInScene {get; private set;} = false;

    public void DisappearObject()
    {
        if (specialItemFrame == null) return;

        IsObjectInScene = false;
        AnimateObjectDisAppear(specialItemFrame.itemFrame);
    }

    void AnimateObjectDisAppear(TextureRect itemFrame)
    {
        if (itemFrame == null) return;

        Tween tween = CreateTween();
        tween.SetParallel(true);

        Vector2 originalPos = itemFrame.Position;

        tween.TweenDelegate<float>(
            value => {
                var c = itemFrame.Modulate;
                c.A = value;
                itemFrame.Modulate = c;
            },
            1f,
            0f,
            0.25f
        );

        tween.TweenDelegate<Vector2>(
            value => itemFrame.Position = value,
            originalPos,
            originalPos + new Vector2(0, 40),
            0.3f
        ).SetTrans(Tween.TransitionType.Cubic)
        .SetEase(Tween.EaseType.In);

        tween.TweenDelegate<Vector2>(
            value => itemFrame.Scale = value,
            itemFrame.Scale,
            new Vector2(0.6f, 0.6f),
            0.25f
        ).SetTrans(Tween.TransitionType.Quad)
        .SetEase(Tween.EaseType.In);

        tween.Chain().TweenCallback(Callable.From(() =>
        {
            specialItemFrame?.QueueFree();
            specialItemFrame = null;
        }));
    }

    public void AppearObject(Texture2D icon)
    {
        IsObjectInScene = true;
        //todo create a variable or smothing to store the new object created or make the object public
        specialItemFrame = specialItemFrameScene.Instantiate<SpecialItemFrame>();
        specialItemFrame.SetItemIcon(icon);
        TextureRect textureRectIcon = specialItemFrame.itemFrame;

        objectLayer.AddChild(specialItemFrame);
        textureRectIcon.AddPercentageOffset(xOffset, 0);
        textureRectIcon.AddPercentageOffset(0, yOffset, -1f);

        AnimateObjectAppear(textureRectIcon);
    }

    private void AnimateObjectAppear(TextureRect itemFrame)
    {
        //todo add cool sounds
        //todo this may be be an Action in a Actionbus?
        itemFrame.Modulate = new Color(1, 1, 1, 0);
        itemFrame.Scale = new Vector2(0.6f, 0.6f);
        itemFrame.PivotOffset = itemFrame.Size / 2f;
        
        Vector2 originalPos = itemFrame.Position;
        itemFrame.Position += new Vector2(0, 40);

        Tween tween = CreateTween();
        tween.SetParallel(true);

        tween.TweenDelegate<float>(
            value => {
                var c = itemFrame.Modulate;
                c.A = value;
                itemFrame.Modulate = c;
            },
            0f,
            1f,
            0.25f
        );

        tween.TweenDelegate<Vector2>(
            value => itemFrame.Position = value,
            itemFrame.Position,
            originalPos,
            0.35f
        ).SetTrans(Tween.TransitionType.Cubic)
        .SetEase(Tween.EaseType.Out);
        tween.TweenDelegate<Vector2>(
            value => itemFrame.Scale = value,
            new Vector2(0.6f, 0.6f),
            new Vector2(1.1f, 1.1f),
            0.25f
        ).SetTrans(Tween.TransitionType.Back)
        .SetEase(Tween.EaseType.Out);

        tween.Chain().TweenDelegate<Vector2>(
            value => itemFrame.Scale = value,
            new Vector2(1.1f, 1.1f),
            Vector2.One,
            0.12f
        ).SetTrans(Tween.TransitionType.Quad)
        .SetEase(Tween.EaseType.Out);
    }

    internal void CleanEffects()
    {
        DisappearObject();
        
    }
}