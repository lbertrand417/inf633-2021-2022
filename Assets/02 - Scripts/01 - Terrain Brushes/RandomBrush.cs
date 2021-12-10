using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomBrush : TerrainBrush
{
    public enum shape_list { rectangle, circle };

    public float maximum = 5;
    public shape_list shape;

    public override void draw(int x, int z)
    {

        if (shape == shape_list.rectangle)
        {
            for (int zi = -radius; zi <= radius; zi++)
            {
                for (int xi = -radius; xi <= radius; xi++)
                {
                    terrain.set(x + xi, z + zi, UnityEngine.Random.Range(0, maximum));

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
                        terrain.set(x + xi, z + zi, UnityEngine.Random.Range(0, maximum));
                    }
                }
            }
        }
    }
}