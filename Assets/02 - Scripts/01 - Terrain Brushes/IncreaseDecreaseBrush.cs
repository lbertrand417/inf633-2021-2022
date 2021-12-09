using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseDecreaseBrush : TerrainBrush
{

    public enum shape_list { rectangle, circle };

    public float increment = 5;
    public shape_list shape;

    public override void draw(int x, int z)
    {
        if (shape == shape_list.rectangle)
        {
            for (int zi = -radius; zi <= radius; zi++)
            {
                for (int xi = -radius; xi <= radius; xi++)
                {
                    terrain.set(x + xi, z + zi, Math.Max(0, terrain.get(x + xi, z + zi) + increment));

                }
            }
        }

        if (shape == shape_list.circle)
        {
            for (int zi = -radius; zi <= radius; zi++)
            {
                for (int xi = -radius; xi <= radius; xi++)
                {
                    if ((Math.Pow(xi, 2) + Math.Pow(zi, 2)) < Math.Pow(radius, 2))
                    {
                        terrain.set(x + xi, z + zi, Math.Max(0, terrain.get(x + xi, z + zi) + increment));
                    }
                }
            }
        }
    }
}