using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(QuadrupedProceduralMotion))]
public class IKAnimal : MonoBehaviour
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
    protected float energy;
    protected float speed = 0f;

    [Header("Sensor - Vision")]
    public float maxVision = 20.0f;
    public float stepAngle = 10.0f;
    public int nEyes = 5;

    protected int[] networkStruct;
    protected SimpleNeuralNet brain = null;

    // Terrain.
    protected CustomTerrain terrain = null;
    protected int[,] details = null;
    protected Vector2 detailSize;
    protected Vector2 terrainSize;
    protected Vector2Int lastPos = new Vector2Int(0, 0);

    // Animal.
    protected Transform tfm;
    protected float[] vision;
    //protected CapsuleAutoController controller;
    protected int eatCount = 0;
    // Genetic alg.
    protected IKGeneticAlgo genetic_algo = null;

    // Renderer.
    protected Material[] mat = null;
    protected Color[] matColors = null;

    protected QuadrupedProceduralMotion ik;
    protected GameObject emptyGO; // For goal transform

    void Start()
    {
        // Network: 1 input per receptor, 1 output per actuator.
        vision = new float[nEyes];
        networkStruct = new int[] { nEyes, 5, 1 };
        energy = maxEnergy;
        
        tfm = transform;

        // Renderer used to update animal color.
        // It needs to be updated for more complex models.
        //controller = GetComponent<CapsuleAutoController>();

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

        // Set goal
        ik = this.GetComponent<QuadrupedProceduralMotion>();
        emptyGO = new GameObject();
        ik.goal = emptyGO.transform;

        Vector3 goalPos = tfm.TransformPoint(new Vector3(ik.minDistToGoal * Vector3.forward.x, 0, ik.minDistToGoal * Vector3.forward.z));
        ik.goal.position = new Vector3(goalPos.x, terrain.get(goalPos.x, goalPos.z), goalPos.z);
    }

    void Update()
    {
        // In case something is not initialized...
        if (brain == null)
            brain = new SimpleNeuralNet(networkStruct);
        if (speed == 0f)
            speed = 0.2f;
        if (speed < 0.05f)
            speed = 0.05f;
        if (terrain == null)
            return;
        if (details == null)
        {
            UpdateSetup();
            return;
        }

        //controller.max_speed = speed;
        // Retrieve animal location in the heighmap
        int dx = (int)((tfm.position.x / terrainSize.x) * detailSize.x);
        int dy = (int)((tfm.position.z / terrainSize.y) * detailSize.y);

        // For each frame, we lose lossEnergy
        energy -= lossEnergy * speed;

        // Update terrain info
        if ((lastPos.x >= 0) && lastPos.x < (details.GetLength(1)) && (lastPos.y >= 0) && (lastPos.y < details.GetLength(0)))
        {
            terrain.setAnimalPos(lastPos.x, lastPos.y, false);
        } else
        {
            genetic_algo.removeAnimal(this);
        }

        if ((dx >= 0) && dx < (details.GetLength(1)) && (dy >= 0) && (dy < details.GetLength(0)))
        {
            terrain.setAnimalPos(dx, dy, true);
        }
        else
        {
            genetic_algo.removeAnimal(this);
        }


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
            for (int i = 0; i < mat.Length; i++)
                mat[i].color = matColors[i] * (energy / maxEnergy);
        }


        // 1. Update receptor.
        UpdateVision();

        // 2. Use brain.
        float[] output = brain.getOutput(vision);

        // 3. Act using actuators.
        float angle = (output[0] * 2.0f - 1.0f) * maxAngle;
        //tfm.Rotate(0.0f, angle * speed, 0.0f);
        //ik.goal.Translate(0.1f, 0.0f, 0.2f);
        ik.goal.Rotate(0.0f, angle * speed, 0.0f);

        Terrain leterrain = Terrain.activeTerrain;
        Vector3 scale = leterrain.terrainData.heightmapScale;

        Vector3 v = ik.goal.rotation * Vector3.forward * speed;
        Vector3 loc = ik.goal.position + v;
        if (loc.x < 0)
            loc.x = 0;
        else if (loc.x > leterrain.terrainData.size.x)
            loc.x = leterrain.terrainData.size.x;
        if (loc.z < 0)
            loc.z = 0;
        else if (loc.z > leterrain.terrainData.size.z)
            loc.z = leterrain.terrainData.size.z;
        loc.y = terrain.getInterp(loc.x / scale.x, loc.z / scale.z);
        ik.goal.position = loc;
        //Vector3 global_line_dir = tfm.TransformPoint(new Vector3(speed * ik.goal.forward.x, 0, speed * ik.goal.forward.z));
        //Vector3 goalPos = tfm.TransformPoint(new Vector3(maxVision * Vector3.forward.x, 0, maxVision * Vector3.forward.z));
        //ik.goal.position = new Vector3(global_line_dir.x, terrain.get(global_line_dir.x, global_line_dir.z), global_line_dir.z);
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
                Debug.DrawLine(ik.headBone.position, new Vector3(global_line_dir.x,
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
                        Debug.DrawLine(ik.headBone.position, new Vector3(global_line_dir.x,
                            terrain.get(global_line_dir.x, global_line_dir.z),
                            global_line_dir.z), Color.red);
                    }
                    break;
                }

                if ((int)px >= 0 && (int)px < details.GetLength(1) && (int)py >= 0 && (int)py < details.GetLength(0) && terrain.GetPredatorPos((int)px, (int)py))
                {
                    vision[i] = -distance / maxVision;
                    if (genetic_algo.showVision)
                    {
                        Vector3 line_dir = Quaternion.Euler(0.0f, startingAngle + (stepAngle * i), 0.0f) * Vector3.forward;
                        Vector3 global_line_dir = tfm.TransformPoint(new Vector3(distance * line_dir.x, 0, distance * line_dir.z));
                        Debug.DrawLine(ik.headBone.position, new Vector3(global_line_dir.x,
                            terrain.get(global_line_dir.x, global_line_dir.z),
                            global_line_dir.z), Color.blue);
                    }
                    break;
                }

            }
        }
    }

    public void Setup(CustomTerrain ct, IKGeneticAlgo ga)
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
    public void InheritAttributes(float parentSpeed, float parentMaxVision, bool mutate)
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

    public GameObject GetGoal()
    {
        return emptyGO;
    }

    public void GetEaten(int dx, int dy)
    {
        energy = 0.0f;
        genetic_algo.removeAnimal(this);
        terrain.setAnimalPos(dx, dy, false);
        terrain.setAnimalPos(lastPos.x, lastPos.y, false);
    }
}