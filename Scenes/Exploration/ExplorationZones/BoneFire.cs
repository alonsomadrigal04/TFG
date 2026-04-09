using Godot;
using System;

public partial class BoneFire : OmniLight3D
{
    FastNoiseLite noise = new();

    public override void _Process(double delta)
    {
        float t = Time.GetTicksMsec() * 0.1f;
        float flicker = noise.GetNoise1D(t) * 0.5f + 0.5f;
        LightEnergy = 3f - flicker * 2f;

        GD.Print(flicker);
    }
}
