using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlgo : MonoBehaviour
{
    
    [Header("Genetic Algorithm parameters")]
    public int popSize = 40;
    public GameObject animalPrefab;
    public GameObject predatorPrefab;
    public bool showVision = false;
    public bool mutate = true;

    private SimpleNeuralNet globalNeuralNet;
    private SimpleNeuralNet globalPredNeuralNet;
    [Header("Dynamic elements")]
    public float vegetationGrowthRate = 1.0f;
    public float currentGrowth;
    public float maxVegetationHeight = 50f;
    public float maxVegetationSteep = 25f;

    private List<GameObject> animals;
    private List<GameObject> predators;
    private float totalSpeed;
    protected Terrain terrain;
    protected CustomTerrain customTerrain;
    protected float width;
    protected float height;

    void Start()
    {
        // Retrieve terrain.
        terrain = Terrain.activeTerrain;
        customTerrain = GetComponent<CustomTerrain>();
        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;

        // Initialize terrain growth.
        currentGrowth = 0.0f;

        // Initialize animals array.
        animals = new List<GameObject>();
        predators = new List<GameObject>();
        int[] networkStruct = new int[] { 5, 5, 1 };
        int[] predNetworkStruct = new int[] { 6, 6, 1 };
        globalNeuralNet = new SimpleNeuralNet( networkStruct);
        globalPredNeuralNet = new SimpleNeuralNet(predNetworkStruct);
        for (int i = 0; i < popSize; i++)
        {
            GameObject animal = makeAnimal();
            animals.Add(animal);
            totalSpeed += 0.5f;
        }
        GameObject predator = makePredator();
        predators.Add(predator);
       
    }

    void Update()
    {
        // Keeps animal to a minimum.
        while (animals.Count < popSize / 2)
        {
            animals.Add(makeAnimal());
            totalSpeed += 0.5f;
        }
        while (predators.Count < popSize / 10)
        {
            predators.Add(makePredator());
        }
        customTerrain.debug.text = "N° animals: " + animals.Count.ToString() +"\n" + "Avg Speed: " + (totalSpeed/animals.Count).ToString();

        // Update grass elements/food resources.
        updateResources();
    }

    /// <summary>
    /// Method to place grass or other resource in the terrain.
    /// </summary>
    public void updateResources()
    {
        Vector2 detail_sz = customTerrain.detailSize();
        int[,] details = customTerrain.getDetails();
        currentGrowth += vegetationGrowthRate;
        while (currentGrowth > 1.0f)
        {
            int x = (int)(UnityEngine.Random.value * detail_sz.x);
            int y = (int)(UnityEngine.Random.value * detail_sz.y);
            float tx = (float)x / detail_sz.x * width;
            float ty = (float)y / detail_sz.y * height;
            if (customTerrain.get(tx,ty) < maxVegetationHeight && customTerrain.getSteepness(tx,ty)<maxVegetationSteep)
            {
                details[y, x] = 1;
            }
            currentGrowth -= 1.0f;
        }
        customTerrain.saveDetails();
    }

    /// <summary>
    /// Method to instantiate an animal prefab. It must contain the animal.cs class attached.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public GameObject makeAnimal(Vector3 position)
    {
        GameObject animal = Instantiate(animalPrefab, transform);
        animal.GetComponent<Animal>().Setup(customTerrain, this);
        animal.transform.position = position;
        animal.transform.Rotate(0.0f, UnityEngine.Random.value * 360.0f, 0.0f);
        animal.GetComponent<Animal>().InheritBrain(globalNeuralNet, true);
        return animal;
    }

    /// <summary>
    /// If makeAnimal() is called without position, we randomize it on the terrain.
    /// </summary>
    /// <returns></returns>
    public GameObject makeAnimal()
    {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = UnityEngine.Random.value * width;
        float z = UnityEngine.Random.value * height;
        float y = customTerrain.getInterp(x / scale.x, z / scale.z);
        return makeAnimal(new Vector3(x, y, z));
    }
    public GameObject makePredator(Vector3 position)
    {
        GameObject predator = Instantiate(predatorPrefab, transform);
        predator.GetComponent<Predator>().Setup(customTerrain, this);
        predator.transform.position = position;
        predator.transform.Rotate(0.0f, UnityEngine.Random.value * 360.0f, 0.0f);
        predator.GetComponent<Predator>().InheritBrain(globalPredNeuralNet, true);
        return predator;
    }

    public GameObject makePredator()
    {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = UnityEngine.Random.value * width;
        float z = UnityEngine.Random.value * height;
        float y = customTerrain.getInterp(x / scale.x, z / scale.z);
        return makePredator(new Vector3(x, y, z));
    }

    /// <summary>
    /// Method to add an animal inherited from anothed. It spawns where the parent was.
    /// </summary>
    /// <param name="parent"></param>
    public void addOffspring(Animal parent)
    {
        GameObject animal = makeAnimal(parent.transform.position);
        animal.GetComponent<Animal>().InheritBrain(parent.GetBrain(), mutate);
        animal.GetComponent<Animal>().InheritAttributes(parent.GetSpeed(), parent.GetMaxVision(), mutate); ;
        animals.Add(animal);
        if (animal.GetComponent<Animal>().GetSpeed() < 0.1f)
        {
            totalSpeed += 0.1f;
        }else
         totalSpeed += animal.GetComponent<Animal>().GetSpeed();
        globalNeuralNet = animal.GetComponent<Animal>().GetBrain();
    }

    public void addPredatorOffspring(Predator parent)
    {
        GameObject predator = makePredator(parent.transform.position);
        predator.GetComponent<Predator>().InheritBrain(parent.GetBrain(), mutate);
        predator.GetComponent<Predator>().InheritAttributes(parent.GetSpeed(), mutate);
        predators.Add(predator);
        globalPredNeuralNet = predator.GetComponent<Predator>().GetBrain();
    }
    /// <summary>
    /// Remove instance of an animal.
    /// </summary>
    /// <param name="animal"></param>
    public void removeAnimal(Animal animal)
    {
        animals.Remove(animal.transform.gameObject);
        totalSpeed -= animal.GetComponent<Animal>().GetSpeed(); 
        Destroy(animal.transform.gameObject);
    }

    public void removePredator(Predator predator)
    {
        predators.Remove(predator.transform.gameObject);
        Destroy(predator.transform.gameObject);
    }

    public int getAnimalCount()
    {
        return animals.Count;
    }
    public int getPredatorCount()
    {
        return predators.Count;
    }
    public float getAverageSpeed()
    {
        return totalSpeed/animals.Count;
    }
}
