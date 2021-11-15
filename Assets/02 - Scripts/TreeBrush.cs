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
            float p = Random.Range(0f, 1f);
            if(p<0.5f)
                terrain.object_prefab = instances[0];
            else terrain.object_prefab = instances[1];
        }
        else
            terrain.object_prefab = instances[2];
        spawnObject(x, z);
    }
}

