using Microsoft.Win32;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttributesController : MonoBehaviour
{
    public float maxEnergy = 100;
    public float maxHydration = 100;
    public float energyBaseTickDownRate = 0.1f;
    public float hydrationBaseTickDownRate = 0.7f;
    // Modifier for base tick down rate when moving
    // this modifier will be maximally applied when moving at max speed.
    public float energyMovementTickDownModifier = 10;
    // Modifier for base tick down rate when moving
    // this modifier will be maximally applied when moving at max speed.
    public float hydrationMovementTickDownModifier = 10;
    public float drinkRate = 10;

    public AttributeBars attributeBarsPrefab;
    private AttributeBars attributeBars;
    private Rigidbody rb;
    private MovementController mv;

    private List<Water> overlappedWaterTiles;

    public float Energy { get; private set; }
    public float Hydration { get; private set; }
    public bool Alive { get; private set; }
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
        attributeBars = GameObject.Instantiate(attributeBarsPrefab, Vector3.zero, Quaternion.identity, GameObject.FindGameObjectWithTag("Canvas").transform);
        attributeBars.followTransform = transform;
        attributeBars.SetMaxEnergy(maxEnergy);
        attributeBars.SetMaxHydration(maxHydration);
    }

    private void FixedUpdate()
    {
        float modifierActivity = rb.velocity.magnitude / mv.moveSpeed;
        float energyTickDownModifier = Mathf.Lerp(1, energyMovementTickDownModifier, modifierActivity);
        float hydrationTickDownModifier = Mathf.Lerp(1, hydrationMovementTickDownModifier, modifierActivity);

        // Tick down attributes
        Energy -= energyTickDownModifier * energyBaseTickDownRate * Time.fixedDeltaTime;
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
            //Hydration += overlappedWaterTiles[0].Take(drinkRate * Time.fixedDeltaTime);
            Hydration += drinkRate * Time.fixedDeltaTime;
            Hydration = Mathf.Clamp(Hydration, 0, maxHydration);
        }

        // Update visual indicators
        attributeBars.SetEnergy(Energy);
        attributeBars.SetHydration(Hydration);

        // Update accumlation stats to use for fitness
        TimeAlive += Time.fixedDeltaTime;
        AccumEnergy += Energy * Time.fixedDeltaTime;
        AccumHydration += Hydration * Time.fixedDeltaTime;

        if (Energy <= 0 || Hydration <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        Alive = false;
        gameObject.SetActive(false);
        attributeBars.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Water")
        {
            overlappedWaterTiles.Add(other.GetComponent<Water>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Water")
            overlappedWaterTiles.Remove(other.gameObject.GetComponent<Water>());
    }

    private void OnDestroy()
    {
        // Cleanup UI elements
        if (attributeBars != null)
        {
            Destroy(attributeBars);
        }
    }
}
