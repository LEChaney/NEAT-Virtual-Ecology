using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Food : MonoBehaviour
{
    public float foodValue = 50;
    public bool respawns = true;
    public FoodSpawner foodSpawner;

    public float eat()
    {
        if (respawns && foodSpawner)
        {
            foodSpawner.SpawnFood();
        }
        Destroy(gameObject);
        return foodValue;
    }
}
