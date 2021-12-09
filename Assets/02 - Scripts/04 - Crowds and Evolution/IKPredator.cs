using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IKPredator : MonoBehaviour
{

    [Header("Animal parameters")]
    public float swapRate = 0.01f;
    public float mutateRate = 0.01f;
    public float swapStrength = 10.0f;
    public float mutateStrength = 0.5f;
    public float maxAngle = 10.0f;

    [Header("Energy parameters")]
    public float maxEnergy = 30.0f;
    public float lossEnergy = 0.3f;
    public float gainEnergy = 30.0f;
    private float energy;
    private float speed = 0f;

    [Header("Sensor - Vision")]
    public float maxVision = 60.0f;
    public float stepAngle = 5.0f;
    public int nEyes = 12;

    private int[] networkStruct;
    private SimpleNeuralNet brain = null;

    // Terrain.
    private CustomTerrain terrain = null;
    private int[,] details = null;
    private Vector2 detailSize;
    private Vector2 terrainSize;
    private Vector2Int lastPos = new Vector2Int(0, 0);

    // Animal.
    private Transform tfm;
    private float[] vision;
    //private CapsuleAutoController controller;

    private CapsuleCollider collider;

    // Genetic alg.
    private IKGeneticAlgo genetic_algo = null;

    // Renderer.
    private Material mat = null;
    private Color matColor;

    void Start()
    {
        // Network: 1 input per receptor, 1 output per actuator.
        vision = new float[nEyes];
        networkStruct = new int[] { nEyes, 5, 1 };
        energy = maxEnergy;
        tfm = transform;

        Vector3 objectScale = tfm.localScale;
        // Sets the local scale of game object
        //tfm.localScale = new Vector3(objectScale.x * 3, objectScale.y * 3, objectScale.z * 3);

        // Renderer used to update animal color.
        // It needs to be updated for more complex models.
        //controller = GetComponent<CapsuleAutoController>();

        collider = GetComponent<CapsuleCollider>();

        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            mat = renderer.material;
            matColor = renderer.material.color;
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

        //controller.max_speed = speed;
        // Retrieve animal location in the heighmap
        int dx = (int)((tfm.position.x / terrainSize.x) * detailSize.x);
        int dy = (int)((tfm.position.z / terrainSize.y) * detailSize.y);

        // For each frame, we lose lossEnergy
        energy -= lossEnergy * speed;

        // Update terrain info
        terrain.setPredatorPos(lastPos.x, lastPos.y, false);
        terrain.setPredatorPos(dx, dy, true);
        lastPos.x = dx;
        lastPos.y = dy;

        // To clean the animal pos array just in case
        if (terrain.getAnimalPos(dx, dy))
        {
            terrain.setAnimalPos(dx, dy, false);
        }

        // If the energy is below 0, the animal dies.
        if (energy < 0)
        {
            energy = 0.0f;
            terrain.setPredatorPos(dx, dy, false);
            genetic_algo.removePredator(this);
        }

        // Update the color of the animal as a function of the energy that it contains.
        if (mat != null)
            mat.color = matColor * (energy / maxEnergy);

        // 1. Update receptor.
        UpdateVision();

        // 2. Use brain.
        float[] output = brain.getOutput(vision);

        // 3. Act using actuators.
        float angle = (output[0] * 2.0f - 1.0f) * maxAngle;
        tfm.Rotate(0.0f, angle * speed, 0.0f);



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

                if ((int)px >= 0 && (int)px < details.GetLength(1) && (int)py >= 0 && (int)py < details.GetLength(0) && terrain.getAnimalPos((int)px, (int)py))
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
    public void InheritAttributes(float parentSpeed, bool mutate)
    {
        speed = parentSpeed;
        if (mutate)
            speed += (2.0f * UnityEngine.Random.value - 1.0f) * 0.1f;
    }
    public SimpleNeuralNet GetBrain()
    {
        return brain;
    }
    public float GetSpeed()
    {
        return speed;
    }
    public float GetHealth()
    {
        return energy / maxEnergy;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Animal"))
        {
            energy += gainEnergy;
            if (energy > maxEnergy)
                energy = maxEnergy;


            genetic_algo.addPredatorOffspring(this);

            IKAnimal animal = other.gameObject.GetComponent<IKAnimal>();
            int dx = (int)((tfm.position.x / terrainSize.x) * detailSize.x);
            int dy = (int)((tfm.position.z / terrainSize.y) * detailSize.y);
            animal.GetEaten(dx, dy);
        }
    }

}