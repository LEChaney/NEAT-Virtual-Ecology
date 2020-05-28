using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WaterTransient : Water
{
    private float zMin = -8;
    private float zMax = 106;

    private float xMin = -17;
    private float xMax = 96;

    public static float WaterSpawnQuantity = 10;

    private void Start()
    {

        quantity = 0;
    }

    public void FixedUpdate()
    {
        if (quantity <= 0)
        {
            transform.position = new Vector3(Random.Range(xMin, xMax), transform.position.y, Random.Range(zMin, zMax));
            Quantity = WaterSpawnQuantity;
        }
    }

    public float Quantity
    {
        get
        {
            return quantity;
        }
        set
        {
            quantity = value;

            if (quantity < 0)
                quantity = 0;
        }
    }

    public float Take(float amount)
    {
        if (amount > quantity)
            amount = quantity;

        Quantity -= amount;
        return quantity;
    }
}
