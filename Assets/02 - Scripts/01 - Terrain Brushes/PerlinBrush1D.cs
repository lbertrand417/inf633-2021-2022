using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PerlinBrush1D : TerrainBrush
{

   
    public override void draw(int x, int z)
    {
        
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                float oldHeight = terrain.get(x + xi, z + zi);
                terrain.set(x + xi, z + zi, oldHeight+ noise(x+xi));
            }
        }
    }

    private float grad(float x)
    {
        System.Random rnd = new System.Random();
        int sgn = rnd.Next(2);
        if (sgn == 1)
        {
            return 1;
        }
        else return -1;

    }

    private float noise(int x)
    {
        float x0 = Mathf.Floor(x);
        float x1 = x0 + 1;

       

        float g0 = grad(x0);
        float g1 = grad(x1);
    
        return g0 * (x - x0) +  g1 * (x - x1);
    }

}
