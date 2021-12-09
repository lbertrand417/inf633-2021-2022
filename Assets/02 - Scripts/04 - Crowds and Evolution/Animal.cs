using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animal : MonoBehaviour
{

    [Header("Animal parameters")]
    public float swapRate = 0.01f;
    public float mutateRate = 0.01f;
    public float swapStrength = 10.0f;
    public float mutateStrength = 0.5f;
    public float maxAngle = 10.0f;

    [Header("Energy parameters")]
    public float maxEnergy = 10.0f;
    public float lossEnergy = 0.2f;
    public float gainEnergy = 10.0f;
    private float energy;
    private float speed = 0f;

    [Header("Sensor - Vision")]
    public float maxVision = 20.0f;
    public float stepAngle = 10.0f;
    public int nEyes = 5;

    private int[] networkStruct;
    private SimpleNeuralNet brain = null;

    // Terrain.
    private CustomTerrain terrain = null;
    private int[,] details = null;
    private Vector2 detailSize;
    private Vector2 terrainSize;
    private Vector2Int lastPos = new Vector2Int(0,0);

    // Animal.
    private Transform tfm;
    private float[] vision;
    private CapsuleAutoController controller;
    private int eatCount = 0;
    // Genetic alg.
    private GeneticAlgo genetic_algo = null;

    // Renderer.
    private Material[] mat = null;
    private Color[] matColors = null;

    void Start()
    {
        // Network: 1 input per receptor, 1 output per actuator.
        vision = new float[nEyes];
        networkStruct = new int[] { nEyes, 5, 1 };
        energy = maxEnergy;
        tfm = transform;

        // Renderer used to update animal color.
        // It needs to be updated for more complex models.
        controller = GetComponent<CapsuleAutoController>();

        MeshRenderer[] renderer = GetComponentsInChildren<MeshRenderer>();
        mat = new Material[renderer.Length];
        matColors = new Color[renderer.Length];
        if (renderer != null)
        {
            int i = 0;
            foreach (MeshRenderer r in renderer)
            {
                mat[i] = r.material;
                matColors[i] = r.material.color;
                i++;
            }
        }
            
    }

    void Update()
    {
        // In case something is not initialized...
        if (brain == null)
            brain = new SimpleNeuralNet(networkStruct);
        if (speed == 0f)
            speed = 0.5f;
        if (speed < 0.1f)
            speed = 0.1f;
        if (terrain == null)
            return;
        if (details == null)
        {
            UpdateSetup();
            return;
        }

        controller.max_speed = speed;
        // Retrieve animal location in the heighmap
        int dx = (int)((tfm.position.x / terrainSize.x) * detailSize.x);
        int dy = (int)((tfm.position.z / terrainSize.y) * detailSize.y);

        // For each frame, we lose lossEnergy
        energy -= lossEnergy * speed;

        // Update terrain info
        terrain.setAnimalPos(lastPos.x, lastPos.y, false);
        terrain.setAnimalPos(dx, dy, true);
       

        // If the animal is located in the dimensions of the terrain and over a grass position (details[dy, dx] > 0), it eats it, gain energy and spawn an offspring.
        if ((dx >= 0) && dx < (details.GetLength(1)) && (dy >= 0) && (dy < details.GetLength(0)) && details[dy, dx] > 0)
        {
            // Eat (remove) the grass and gain energy.
            details[dy, dx] = 0;
            energy += gainEnergy;
            if (energy > maxEnergy)
                energy = maxEnergy;
            eatCount++;
           // if(eatCount%3==0 || eatCount ==1)
              genetic_algo.addOffspring(this);
        }

        // If the energy is below 0, the animal dies.
        if (energy < 0)
        {
            GetEaten(dx, dy);
        }

        // Update the color of the animal as a function of the energy that it contains.
        if (mat != null)
        {
            for(int i = 0; i<mat.Length; i++)
                mat[i].color = matColors[i]* (energy / maxEnergy);
        }
            

        // 1. Update receptor.
        UpdateVision();

        // 2. Use brain.
        float[] output = brain.getOutput(vision);

        // 3. Act using actuators.
        float angle = (output[0] * 2.0f - 1.0f) * maxAngle;
        tfm.Rotate(0.0f, angle*speed, 0.0f);
        lastPos.x = dx;
        lastPos.y = dy;

    }

    /// <summary>
    /// Calculate distance to the nearest food resource, if there is any.
    /// </summary>
    private void UpdateVision()
    {
        float startingAngle = -((float)nEyes / 2.0f) * stepAngle;
        Vector2 ratio = detailSize / terrainSize;

        for (int i = 0; i < nEyes; i++)
        {
            Quaternion rotAnimal = tfm.rotation * Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f);
            Vector3 forwardAnimal = rotAnimal * Vector3.forward;
            float sx = tfm.position.x * ratio.x;
            float sy = tfm.position.z * ratio.y;
            vision[i] = 1.0f;

            if (genetic_algo.showVision)
            {
                Vector3 line_dir = Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f) * Vector3.forward;
                Vector3 global_line_dir = tfm.TransformPoint(new Vector3(maxVision * line_dir.x, 0, maxVision * line_dir.z));
                Debug.DrawLine(tfm.position, new Vector3(global_line_dir.x,
                    terrain.get(global_line_dir.x, global_line_dir.z),
                    global_line_dir.z));
            }

            // Interate over vision length.
            for (float distance = 1.0f; distance < maxVision; distance += 0.5f)
            {
                // Position where we are looking at.
                float px = (sx + (distance * forwardAnimal.x * ratio.x));
                float py = (sy + (distance * forwardAnimal.z * ratio.y));

                if (px < 0)
                    px += detailSize.x;
                else if (px >= detailSize.x)
                    px -= detailSize.x;
                if (py < 0)
                    py += detailSize.y;
                else if (py >= detailSize.y)
                    py -= detailSize.y;

                if ((int)px >= 0 && (int)px < details.GetLength(1) && (int)py >= 0 && (int)py < details.GetLength(0) && details[(int)py, (int)px] > 0)
                {
                    vision[i] = distance / maxVision;
                    if (genetic_algo.showVision)
                    {
                        Vector3 line_dir = Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f) * Vector3.forward;
                        Vector3 global_line_dir = tfm.TransformPoint(new Vector3(distance * line_dir.x, 0, distance * line_dir.z));
                        Debug.DrawLine(tfm.position, new Vector3(global_line_dir.x,
                            terrain.get(global_line_dir.x, global_line_dir.z),
                            global_line_dir.z), Color.red);
                    }
                    break;
                }

                if ((int)px >= 0 && (int)px < details.GetLength(1) && (int)py >= 0 && (int)py < details.GetLength(0) && terrain.GetPredatorPos((int)px,(int)py))
                {
                    vision[i] = -distance / maxVision;
                    if (genetic_algo.showVision)
                    {
                        Vector3 line_dir = Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f) * Vector3.forward;
                        Vector3 global_line_dir = tfm.TransformPoint(new Vector3(distance * line_dir.x, 0, distance * line_dir.z));
                        Debug.DrawLine(tfm.position, new Vector3(global_line_dir.x,
                            terrain.get(global_line_dir.x, global_line_dir.z),
                            global_line_dir.z), Color.blue);
                    }
                    break;
                }

            }
        }
    }

    public void Setup(CustomTerrain ct, GeneticAlgo ga)
    {
        terrain = ct;
        genetic_algo = ga;
        UpdateSetup();
    }

    private void UpdateSetup()
    {
        detailSize = terrain.detailSize();
        Vector3 gsz = terrain.terrainSize();
        terrainSize = new Vector2(gsz.x, gsz.z);
        details = terrain.getDetails();
    }

    public void InheritBrain(SimpleNeuralNet other, bool mutate)
    {
        brain = new SimpleNeuralNet(other);
        if (mutate)
            brain.mutate(swapRate, mutateRate, swapStrength, mutateStrength);
    }
    public void InheritAttributes(float parentSpeed, float parentMaxVision ,bool mutate)
    {
        speed = parentSpeed;
        maxVision = parentMaxVision;
        if (mutate)
        {
            speed += (2.0f * UnityEngine.Random.value - 1.0f) * 0.15f;
            maxVision += 2.0f * UnityEngine.Random.value - 1.0f;
            
        }
    }
    public SimpleNeuralNet GetBrain()
    {
        return brain;
    }
    public float GetSpeed()
    {
        return speed;
    }
    public float GetMaxVision()
    {
        return maxVision;
    }
    public float GetHealth()
    {
        return energy / maxEnergy;
    }

    public void GetEaten(int dx, int dy)
    {
        energy = 0.0f;
        genetic_algo.removeAnimal(this);
        terrain.setAnimalPos(dx, dy, false);
        terrain.setAnimalPos(lastPos.x, lastPos.y, false);
    }
}