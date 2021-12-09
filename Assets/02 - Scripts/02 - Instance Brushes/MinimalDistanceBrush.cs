using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MinimalDistanceBrush : InstanceBrush
{
    public int freeSpace = 5;

    public override void draw(float x, float z)
    {
        if (isPossible(x, z))
        {
            spawnObject(x, z);
        }
    }

    bool isPossible(float x, float z)
    {
        int nbrObjects = terrain.getObjectCount();

        bool res = true;
        for (int i = 0; i < nbrObjects; i++)
        {
            Vector3 loc = terrain.getObjectLoc(i);

            if ((Math.Pow(loc.x - x, 2) + Math.Pow(loc.z - z, 2)) < Math.Pow(freeSpace, 2))
            {
                res = false;
            }

        }
        return res;
    }
}