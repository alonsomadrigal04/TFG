using Godot;
using System;

public partial class InteractIconAnimation : Sprite3D
{
	bool active = false;
	float minHeight;
	float maxHeight;
	float yOffset = 0.5f;
	float yDifference = 0.2f;

	float turnVelocity = 1.5f;
	float displacementVelocity = 1f;
	float t = 0f;
	Vector3 originalScale;

	Tween disappearTween;
	Tween appearTween;


    public void InitializeValues(float startingY, bool isActive, Texture2D image)
    {
        minHeight = startingY + yOffset;
        maxHeight = minHeight + yDifference;

		originalScale = Scale;

        Position = new(0f, minHeight, 0f);

		Texture = image;

		if (isActive) Activate();
		else Desactivate();
    }

    public override void _Process(double delta)
	{
		if (active)
            UpdateIconTransform(delta);
    }

    void UpdateIconTransform(double delta)
    {
        Rotation += new Vector3(0f, turnVelocity * (float)delta, 0f);
        t += (float)delta * displacementVelocity;

        float easedT = Mathf.SmoothStep(0f, 1f, Mathf.PingPong(t, 1f));

        float pos = Mathf.Lerp(minHeight, maxHeight, easedT);

        Position = new(0f, pos, 0f);
    }

    public void Activate()
	{
		active = true;
		AppearAnimation();
	}

    void AppearAnimation()
	{
		Show();
		appearTween?.Kill();

		appearTween = CreateTween();
		appearTween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Elastic);

		Scale = Vector3.Zero;

		appearTween.TweenProperty(this, "scale", originalScale, 0.9f);
	}

    public void Desactivate()
	{
		active = false;
        DisappearAnimation();
	}

    void DisappearAnimation()
	{
		disappearTween?.Kill();
		disappearTween = CreateTween();

		disappearTween.SetEase(Tween.EaseType.In).SetTrans(Tween.TransitionType.Sine);

		disappearTween.TweenProperty(this, "scale", new Vector3(0.01f, 0.01f, 0.01f), 0.2f);
		disappearTween.Finished += Hide;
	}

}
