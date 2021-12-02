using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussianBrush : TerrainBrush
{
    public float amplitude = 1;

    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                if((Math.Pow(xi, 2) + Math.Pow(zi, 2) <= Math.Pow(radius, 2)))
                {
                    float oldHeight = terrain.get(x + xi, z + zi);
                    // ln(0.05) = -r² / 2*s²
                    //r² = -2ln(0.05)s² = 2ln(1/0.05)s²
                    //s = r / sqrt(2ln(1/0.05))
                    double sigma = radius / Math.Sqrt(2 * Math.Log(1 / 0.05)); // To have almost 0 at the boundary of the sphere
                    terrain.set(x + xi, z + zi, Math.Max(0, oldHeight + amplitude * (float)Math.Exp(-(Math.Pow(xi, 2) + Math.Pow(zi, 2)) / (2 * Math.Pow(sigma, 2)))));
                }

            }
        }
    }
}

