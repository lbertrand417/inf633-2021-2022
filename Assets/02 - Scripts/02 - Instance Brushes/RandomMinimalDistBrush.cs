using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RandomMinimalDistBrush : InstanceBrush
{
    public enum shape_list { rectangle, circle };
    public shape_list shape;

    public int freeSpace = 50;

    public override void draw(float x, float z)
    {
        float dx = UnityEngine.Random.Range(-radius, radius);
        float dz = UnityEngine.Random.Range(-radius, radius);

        if (shape == shape_list.rectangle)
        {
            Debug.Log("rectangle");
            if(isPossible(x + dx, z + dz))
            {
                spawnObject(x + dx, z + dz);
            }
        }

        if (shape == shape_list.circle)
        {
            Debug.Log("circle");
            if ((Math.Pow(dx, 2) + Math.Pow(dz, 2)) < Math.Pow(radius, 2) && isPossible(x + dx, z + dz))
            {
                spawnObject(x + dx, z + dz);
            }
            
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
