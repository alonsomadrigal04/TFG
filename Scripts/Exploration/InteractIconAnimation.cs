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

    public void InitializeValues(float startingY, bool isActive, Texture2D image)
    {
        minHeight = startingY + yOffset;
        maxHeight = minHeight + yDifference;

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
		Show();
	}

	public void Desactivate()
	{
		active = false;
		Hide();
	}
}
