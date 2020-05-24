using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.PlayerLoop;

public class DistanceSensor : MonoBehaviour
{
    // Number of sectors in the sensor
    public int numSensors = 5;
    // Field of view containing all sectors
    public float fov = 180;
    // Sensor Range
    public float range = 30;
    // How long to wait between updating the sensors (1 / sensorFrequency)
    public float updateInterval = 0.06f;
    public bool displayDebug = false;

    private float timeAccum;
    private float fovStart;
    private float deltaAngle;

    private float[] senses;

    NativeArray<RaycastCommand> raycastCommands;
    NativeArray<RaycastHit> raycastResults;

    // Gets the sensation felt in a particular sector
    public float GetSense(int sensorIdx)
    {
        return senses[sensorIdx];
    }

    private void Start()
    {
        senses = new float[numSensors];
        raycastCommands = new NativeArray<RaycastCommand>(numSensors, Allocator.Persistent);
        raycastResults = new NativeArray<RaycastHit>(numSensors, Allocator.Persistent);

        fovStart = 90 - fov / 2;
        deltaAngle = fov / (numSensors - 1);
    }

    private void FixedUpdate()
    {
        // Avoid updating every physics frame
        timeAccum += Time.fixedDeltaTime;
        if (timeAccum >= updateInterval)
        {
            timeAccum -= updateInterval;

            // Fill raycast command buffer
            for (int i = 0; i < numSensors; ++i)
            {
                float angle = (fovStart + deltaAngle * i) * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                direction = transform.TransformDirection(direction);
                raycastCommands[i] = new RaycastCommand(transform.position, direction, range);
            }

            // Perform raycasts
            JobHandle handle = RaycastCommand.ScheduleBatch(raycastCommands, raycastResults, 1, default(JobHandle));
            handle.Complete();

            // Read results into senses
            for (int i = 0; i < numSensors; ++i)
            {
                RaycastHit hit = raycastResults[i];
                if (hit.collider != null)
                {
                    senses[i] = 1 - hit.distance / range;

                    if (displayDebug)
                    {
                        Debug.DrawLine(transform.position, hit.point, Color.Lerp(Color.white, Color.red, senses[i]), updateInterval - timeAccum);
                    }
                }
                else
                {
                    senses[i] = 0;

                    if (displayDebug)
                    {
                        float angle = (fovStart + deltaAngle * i) * Mathf.Deg2Rad;
                        Vector3 direction = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                        direction = transform.TransformDirection(direction);
                        Debug.DrawRay(transform.position, direction * range, Color.white, updateInterval - timeAccum);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        raycastCommands.Dispose();
        raycastResults.Dispose();
    }
}
