using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject food;
    public Rect spawnRect = new Rect(0, 0, 100, 100);
    public int foodAmount = 100;

    public void Start()
    {
        for (int i = 0; i < foodAmount; ++i)
        {
            SpawnFood();
        }
    }

    public void SpawnFood()
    {
        // Avoid spawning ontop of other objects
        Vector3 foodExtents = food.GetComponent<Renderer>().bounds.extents;
        float y = foodExtents.y + 5e-3f;
        Vector3 foodPos = new Vector3(0, y, 0);
        for (int i = 0; i < 100; ++i)
        {
            float x = Random.Range(spawnRect.xMin, spawnRect.xMax);
            float z = Random.Range(spawnRect.yMin, spawnRect.yMax);
            foodPos = new Vector3(x, y, z);
            Collider[] overlaps = Physics.OverlapBox(foodPos, foodExtents);
            if (overlaps.Length == 0)
                break;
        }

        // Spawn food
        var newFood = Instantiate(food, foodPos, Quaternion.identity).GetComponent<Food>();
        newFood.foodSpawner = this;
    }
}

