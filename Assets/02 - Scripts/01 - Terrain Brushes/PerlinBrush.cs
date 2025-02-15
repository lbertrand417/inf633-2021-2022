using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinBrush : TerrainBrush
{

    public bool useRoundBrush = false;
    public bool lowerTerrain = false;
    [Range(1, 5)]
    public int LOD = 3;
    [Range(0, 1)]
    public float persistance = 0.25f;
    [Range(1, 5)]
    public float lacunarity = 2f;
    [Range(0, 5)]
    public float strength = 1f;
    [Range(0, 5)]
    public float scaling = 1f;

    public override void draw(int x, int z)
    {
       float scale = radius / 2f*scaling;
        for (int zi = -radius; zi <= radius; zi++)
        {
            for (int xi = -radius; xi <= radius; xi++)
            {
                if (!useRoundBrush || dist(xi, zi) <= radius)
                {
                    float oldHeight = terrain.get(x + xi, z + zi);
                    float sigma = radius / Mathf.Sqrt(2 * Mathf.Log(1f / 0.01f));
                    //float borderFactor =  (float)Mathf.Exp(-(Mathf.Pow(Mathf.Max(Mathf.Abs(zi),Mathf.Abs(xi)), 2))/ (sigma*sigma));
                    float borderFactor = Mathf.Pow(Mathf.Min((float)radius - Mathf.Abs(zi), (float)radius - Mathf.Abs(xi)),2) / (radius*radius);

                    if (!lowerTerrain)
                        terrain.set(x+xi,z+zi, oldHeight + calculateNoise(x+xi,z+zi,scale,LOD)*strength*borderFactor);
                    else
                        terrain.set(x + xi, z + zi, Math.Max(0, oldHeight - calculateNoise(x + xi, z + zi, scale, LOD) * strength * borderFactor));
                }
            }
        }
    }
    
    private float dist(int x, int z)
    {
        return Mathf.Sqrt(x * x + z * z);
    }

    private float calculateNoise(int x, int z, float scale_, int lod)
    {
        float noiseSum = 0f;
        for (int i =0; i < lod; i++)
        {
            noiseSum += Mathf.PerlinNoise(x / (scale_ / Mathf.Pow(lacunarity, i)), z / (scale_ / Mathf.Pow(lacunarity, i)))* Mathf.Pow(persistance,i);
        }
        return noiseSum-0.1f;
    }
}
