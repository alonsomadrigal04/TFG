using Godot;
using System;

public partial class DustParticles : GpuParticles3D
{

    /// <summary>
    /// Changes the direction of the particles to match the player's movement direction.
    /// </summary>
    /// <param name="newDirection">The new direction for the particles.</param>
    public void ChangeDirection(Direction newDirection)
    {
       switch(newDirection)
       {
           case Direction.Front:
               ProcessMaterial.Set("direction", new Vector3(0, 0, -1));
               break;
           case Direction.Back:
               ProcessMaterial.Set("direction", new Vector3(0, 0, 1));
               break;
           case Direction.Right:
               ProcessMaterial.Set("direction", new Vector3(-1, 0, 0));
               break;
           case Direction.Left:
               ProcessMaterial.Set("direction", new Vector3(1, 0, 0));
               break;
       }
    }
}
