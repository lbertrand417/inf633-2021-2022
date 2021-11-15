using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class RandomInstanceBrush : InstanceBrush
{
    public enum shape_list { rectangle, circle };
    public shape_list shape;

    public override void draw(float x, float z)
    {
        float dx = UnityEngine.Random.Range(-radius, radius);
        float dz = UnityEngine.Random.Range(-radius, radius);

        if (shape == shape_list.rectangle)
        {
            Debug.Log("rectangle");
            spawnObject(x + dx, z + dz);
        }

        if (shape == shape_list.circle)
        {
            Debug.Log("circle");

            if ((Math.Pow(dx, 2) + Math.Pow(dz, 2)) > Math.Pow(radius, 2))
            {
                spawnObject(x + dx, z + dz);
            }

            
        }
    }
}
