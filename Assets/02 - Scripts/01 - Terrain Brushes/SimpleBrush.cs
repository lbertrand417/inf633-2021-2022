using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBrush : TerrainBrush {


    [Range(0, 5)]
    public float strength = 1f;
    public override void draw(int x, int z)
    {
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                    float borderFactor = Mathf.Sqrt(Mathf.Min((float)radius - Mathf.Abs(zi), (float)radius - Mathf.Abs(xi)))/ Mathf.Sqrt(radius);
                  
                    terrain.set(x + xi, z + zi, terrain.get(x + xi, z + zi) + 0.1f * strength*borderFactor);
                
            }
        }
    }
  
}



  