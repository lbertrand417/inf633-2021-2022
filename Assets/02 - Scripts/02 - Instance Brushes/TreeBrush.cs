using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeBrush : InstanceBrush
{
    public float maxHeight = 40f;
    public float maxAngle = 40f;
    public GameObject[] instances = new GameObject[3];
    public override void draw(float x, float z)
    {
        if (terrain.get(x, z) < maxHeight && terrain.getSteepness(x, z) < maxAngle)
        {
            int id = Random.Range(0, 1);
            terrain.object_prefab = instances[id];
           
        }
        else
            terrain.object_prefab = instances[2];
        spawnObject(x, z);
    }
}

