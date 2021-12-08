using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothBrush : TerrainBrush
{

    [Range(0, 5)]
    public float strength = 1f;


    public override void draw(int x, int z)
    {
        float mean = 0f;
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                mean += terrain.get(x + xi, z + zi);
            }
        }
        mean /=4* radius * radius;
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                float borderFactor = Mathf.Pow(Mathf.Min((float)radius - Mathf.Abs(zi), (float)radius - Mathf.Abs(xi)), 2) / (radius * radius);

                terrain.set(x + xi, z + zi, terrain.get(x+xi,z+zi)+(mean- terrain.get(x + xi, z + zi))*0.05f*strength*borderFactor);
            }
        }
    }
}
