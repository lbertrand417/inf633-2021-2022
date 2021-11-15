using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseDecreaseBrush : TerrainBrush
{

    public float increment = 5;

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                terrain.set(x + xi, z + zi, terrain.get(x + xi, z + zi) + increment);
            }
        }
    }
}
