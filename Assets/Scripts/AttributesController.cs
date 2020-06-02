using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttributesController : MonoBehaviour
{
    public bool needsFood = true;
    public bool needsWater = true;
    public float maxEnergy = 100;
    public float maxHydration = 100;
    public float energyBaseTickDownRate = 1;
    public float hydrationBaseTickDownRate = 1;
    // Modifier for base tick down rate when moving
    // this modifier will be maximally applied when moving at max speed.
    public float energyMovementTickDownModifier = 2;
    // Modifier for base tick down rate when moving
    // this modifier will be maximally applied when moving at max speed.
    public float hydrationMovementTickDownModifier = 2;
    public float drinkRate = 10;
    public string foodTag = "";
    public float foodValue = 10;
    public bool showAttributeBars = true;

    public AttributeBars attributeBarsPrefab;
    private AttributeBars attributeBars;
    private Rigidbody rb;
    private MovementController mv;

    private List<Water> overlappedWaterTiles;

    private float energy;
    private float hydration;

    public float Energy 
    { 
        get
        {
            return energy;
        }
        private set
        {
            energy = Mathf.Clamp(value, 0, maxEnergy);
        }
    }
    public float Hydration 
    { 
        get
        {
            return hydration;
        }
        private set
        {
            hydration = Mathf.Clamp(value, 0, maxHydration);
        } 
    }
    public bool Alive { get; private set; }

    // Fitness evaluation properties.
    // Normalized to each tick up at a maximum of 1 unit per second.
    public float TimeAlive { get; private set; }
    public float AccumEnergy { get; private set; }
    public float AccumHydration { get; private set; }


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mv = GetComponent<MovementController>();

        Energy = maxEnergy;
        Hydration = maxHydration;
        Alive = true;
        TimeAlive = 0;
        AccumEnergy = 0;
        AccumHydration = 0;
        overlappedWaterTiles = new List<Water>();

        // Create attribute bars
        attributeBars = null;
        if (showAttributeBars)
        {
            attributeBars = Instantiate(attributeBarsPrefab, Vector3.zero, Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);
            attributeBars.followTransform = transform;
            attributeBars.SetMaxEnergy(maxEnergy);
            attributeBars.SetMaxHydration(maxHydration);
        }
    }

    private void FixedUpdate()
    {
        float modifierActivity = rb.velocity.magnitude / mv.moveSpeed;
        float energyTickDownModifier = Mathf.Lerp(1, energyMovementTickDownModifier, modifierActivity);
        float hydrationTickDownModifier = Mathf.Lerp(1, hydrationMovementTickDownModifier, modifierActivity);

        // Tick down attributes
        if (needsFood)
            Energy -= energyTickDownModifier * energyBaseTickDownRate * Time.fixedDeltaTime;
        if (needsWater)
            Hydration -= hydrationTickDownModifier * hydrationBaseTickDownRate * Time.fixedDeltaTime;

        // Update overlapping resource lists in case any have been used up and destroyed
        for (int i = 0; i < overlappedWaterTiles.Count; ++i)
        {
            if (overlappedWaterTiles[i] == null)
            {
                overlappedWaterTiles.RemoveAt(i);
                --i;
            }
        }

        // Tick up attributes by collecting resources if available
        if (overlappedWaterTiles.Count > 0)
        {
            Hydration += overlappedWaterTiles[0].Take(drinkRate * Time.fixedDeltaTime);
        }

        // Update visual indicators
        if (attributeBars != null)
        {
            attributeBars.SetEnergy(Energy);
            attributeBars.SetHydration(Hydration);
        }

        // Update accumlation stats to use for fitness
        if (Alive)
        {
            TimeAlive += Time.fixedDeltaTime;
            AccumEnergy += Energy / maxEnergy * Time.fixedDeltaTime; // Normalized
            AccumHydration += Hydration / maxHydration * Time.fixedDeltaTime; // Normalized
        }

        // Death on running out of resources
        if (Energy <= 0 || Hydration <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        Alive = false;
        
        var evoAIController = GetComponent<EvoAIController>();
        if (evoAIController)
        {
            // Restore attributes and randomize position
            // Fitness attributes will no longer be modified
            // after this point.
            Energy = maxEnergy;
            Hydration = maxHydration;
            evoAIController.RandomizePosition();
        }
        else
        {
            gameObject.SetActive(false);
            if (attributeBars != null)
            {
                Destroy(attributeBars.gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            overlappedWaterTiles.Add(other.GetComponent<Water>());
        }
        if (other.tag == foodTag)
        {
            EatFood(other);
        }
    }

    private void EatFood(Collider foodCollider)
    {
        if (foodTag == "Prey")
        {
            var other = foodCollider.gameObject.GetComponent<AttributesController>();
            other.Kill();
            Energy += other.foodValue;
        }
        else
        {
            var food = foodCollider.gameObject.GetComponent<Food>();
            Energy += food.eat();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water")
            overlappedWaterTiles.Remove(other.gameObject.GetComponent<Water>());
    }

    private void OnCollisionEnter(Collision collision)
    {
        Collider collider = collision.collider;
        if (collider.tag == foodTag)
        {
            EatFood(collider);
        }
    }

    private void OnDestroy()
    {
        // Cleanup UI elements
        if (attributeBars != null)
        {
            Destroy(attributeBars.gameObject);
        }
    }
}
