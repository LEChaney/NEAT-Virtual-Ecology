using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Water : MonoBehaviour
{
    public float startQuantity = 100;
    public float refillRate = 1;
    private float quantity;
    private float startAlpha;
    Renderer renderer;

    private void Start()
    {
        quantity = startQuantity;
        renderer = GetComponent<Renderer>();
        startAlpha = renderer.material.color.a;
    }

    public void FixedUpdate()
    {
        if (quantity < startQuantity)
            Quantity += refillRate * Time.deltaTime;
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

            Color color = renderer.material.color;
            color.a = startAlpha * quantity / startQuantity;
            renderer.material.color = color;
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
