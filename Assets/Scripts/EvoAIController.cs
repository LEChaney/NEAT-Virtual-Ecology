using MathNet.Numerics;
using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.XR;

public class EvoAIController : UnitController, InputController
{
    public Rect spawnRect = new Rect(0, 0, 100, 100);

    IBlackBox box;
    bool isRunning;
    float xInput;
    float yInput;
    AttributesController attributes;
    SectorSensor[] sectorSensors;
    DistanceSensor[] distanceSensors;
    MovementController movementController;
    Rigidbody rb;

    void Start()
    {
        attributes = GetComponent<AttributesController>();
        sectorSensors = GetComponents<SectorSensor>();
        distanceSensors = GetComponents<DistanceSensor>();
        movementController = GetComponent<MovementController>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (isRunning)
        {
            // Pass sensor inputs to neural network
            ISignalArray inputArr = box.InputSignalArray;
            int i = 0;
            foreach (SectorSensor sectorSensor in sectorSensors)
            {
                for (int j = 0; j < sectorSensor.NumSenses; ++j)
                {
                    inputArr[i] = sectorSensor.GetSectorSense(j);
                    ++i;
                }
            }

            foreach (DistanceSensor distanceSensor in distanceSensors)
            {
                for (int j = 0; j < distanceSensor.numSensors; ++j)
                {
                    inputArr[i] = distanceSensor.GetSense(j);
                    ++i;
                }
            }

            if (attributes.needsFood)
            {
                // Response high at low energy
                inputArr[i] = 1 - attributes.Energy / attributes.maxEnergy;
                ++i;
            }

            if (attributes.needsWater)
            {
                // Response high at low hydration
                inputArr[i] = 1 - attributes.Hydration / attributes.maxHydration;
                ++i;
            }

            if (movementController.senseSpeed)
            {
                inputArr[i] = rb.velocity.magnitude / movementController.moveSpeed;
                ++i;
            }

            if (movementController.senseSwimming)
            {
                inputArr[i] = movementController.IsSwimming ? 1 : 0;
                ++i;
            }

            // Activate neural network
            box.Activate();

            // Read outputs from neural network into movement input.
            ISignalArray outputArr = box.OutputSignalArray;
            xInput = (float)outputArr[0] * 2 - 1;
            yInput = (float)outputArr[1] * 2 - 1;
        }
    }


    public override void Activate(IBlackBox box)
    {
        this.box = box;
        this.isRunning = true;

        xInput = 0;
        yInput = 0;

        // Activate / spawn at random location
        float x = Random.Range(spawnRect.xMin, spawnRect.xMax);
        float z = Random.Range(spawnRect.yMin, spawnRect.yMax);
        float y = transform.position.y;
        transform.position = new Vector3(x, y, z);
    }

    public override void Stop()
    {
        this.isRunning = false;
    }

    public override float GetFitness()
    {
        if (!attributes.needsFood && !attributes.needsWater)
        {
            return attributes.TimeAlive;
        }
        else
        {
            float healthyness = 0;
            if (attributes.needsFood)
                healthyness += attributes.AccumEnergy;
            if (attributes.needsWater)
                healthyness += attributes.AccumHydration;
            return healthyness;
        }
    }

    public float GetXInput()
    {
        return xInput;
    }

    public float GetYInput()
    {
        return yInput;
    }
}
