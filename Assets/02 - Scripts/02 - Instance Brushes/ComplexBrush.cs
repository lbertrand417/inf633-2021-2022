using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ComplexBrush : InstanceBrush
{
    public enum shape_list { rectangle, circle };
    public shape_list shape;

    public int freeSpace = 5; // Space around trees
    public float maxHeight = 40; // Max height where the trees can appear
    public float maxAngle = 40; // Max steepness 
    [Range(0, 1)]
    public float sparsity = 0;

    [Range(1, 50)]
    public float grovesSize = 2; // Size of the tree

    public GameObject[] instances = new GameObject[1]; // Instances list


    public override void draw(float x, float z)
    {

        float dx = 0;
        float dz = 0;

        // Choose instance
        terrain.object_prefab = instances[UnityEngine.Random.Range(1, instances.Length)];
        terrain.max_scale = grovesSize;

        // Use Perlin noise for object position
        float scale = radius / 2f;
        bool perlin = true;

        int k = 0;
        while (perlin && k < 100)
        {
            dx = UnityEngine.Random.Range(-radius, radius);
            dz = UnityEngine.Random.Range(-radius, radius);

            float p = Mathf.PerlinNoise((x + dx) / scale, (z + dz) / scale) - sparsity;
            if (UnityEngine.Random.Range(0f, 1f) < p)
            {
                perlin = false;
            }
            k++;
        }



        if (shape == shape_list.rectangle)
        {
            //Debug.Log("rectangle");
            if (isPossible(x + dx, z + dz))
            {
                spawnObject(x + dx, z + dz);
            }
        }

        if (shape == shape_list.circle)
        {
            //Debug.Log("circle");
            if ((Math.Pow(dx, 2) + Math.Pow(dz, 2)) < Math.Pow(radius, 2) && isPossible(x + dx, z + dz))
            {
                spawnObject(x + dx, z + dz);
            }

        }
    }

    bool isPossible(float x, float z)
    {
        bool res = true;

        // Check neighborhood
        int nbrObjects = terrain.getObjectCount();
        for (int i = 0; i < nbrObjects; i++)
        {
            Vector3 loc = terrain.getObjectLoc(i);

            if ((Math.Pow(loc.x - x, 2) + Math.Pow(loc.z - z, 2)) < Math.Pow(freeSpace, 2))
            {
                res = false;
            }
        }

        // Check height and steepness
        if (!(terrain.get(x, z) < maxHeight && terrain.getSteepness(x, z) < maxAngle))
        {
            res = false;
        }

        return res;
    }
}