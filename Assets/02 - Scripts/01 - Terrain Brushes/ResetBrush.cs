using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetBrush : TerrainBrush
{

   

    public override void draw(int x, int z)
    {
        terrain.reset();
    }
}
