using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;

public class SectorSensor : MonoBehaviour
{
    // Number of sectors in the sensor
    public int numSectors = 10;
    // Field of view containing all sectors
    public float fov = 180;
    // Sensor Range
    public float range = 30;
    // The Tag that an object requires to be detected by this sensor
    public string sensitiveTag = "";
    // How long to wait between updating the sensors (1 / sensorFrequency)
    public float updateInterval = 0.1f;
    public bool displayDebug = false;
    //public bool senseAngles = true;
    public LayerMask blocksLineOfSight = ~0;

    // Double the number of sectors if angle sensing is on
    public int NumSenses 
    { 
        get
        {
            return numSectors * 3;
        }
    }

    public int SensesPerSector
    {
        get
        {
            return NumSenses / numSectors;
        }
    }

    private  float timeAccum;

    private float[] sectorSenses;
    private List<GameObject>[] sectorObjs;

    // Gets the sensation felt in a particular sector
    public float GetSectorSense(int sectorIdx)
    {
        return sectorSenses[sectorIdx];
    }

    private void Start()
    {
        sectorSenses = new float[NumSenses];
        sectorObjs = new List<GameObject>[numSectors];
        for (int i = 0; i < numSectors; ++i)
        {
            sectorObjs[i] = new List<GameObject>();
        }
    }

    // Clears the current list of objects detected in each sector
    private void ClearSectorObjects()
    {
        Profiler.BeginSample("ClearSectorObjects");

        for (int i = 0; i < numSectors; ++i)
        {
            sectorObjs[i].Clear();
        }

        Profiler.EndSample();
    }

    // Returns the index of the sector containing the given position.
    // Note that bounds are not checked, so output could be negative,
    // or greater than the number of sectors in the sensor. This is
    // so we can check if something is outside of the sensors field of view.
    private int PosToSectorIdx(Vector3 position)
    {
        Profiler.BeginSample("PosToSectoIdx");

        float fovStart = -fov / 2;
        float sectorAngleDelta = fov / numSectors;

        Vector3 relPosition = transform.InverseTransformPoint(position);
        float angleFromForward = Mathf.Atan2(relPosition.z, relPosition.x) * Mathf.Rad2Deg - 90;
        if (angleFromForward < -180)
            angleFromForward += 360;
        float angleFromFOVStart = angleFromForward - fovStart;
        int sectorIdx = (int)Mathf.Floor(angleFromFOVStart / sectorAngleDelta);

        Profiler.EndSample();

        return sectorIdx;
    }

    // Checks a sector index is valid.
    // Useful to apply to results of PosToSectorIdx to check position is inside sensor FOV.
    private bool IsValidSectorIdx(int idx)
    {
        return (idx >= 0 && idx < numSectors);
    }

    // Sorts objects in each sector by their distance (from nearest to farthest)
    private void SortSectorObjects()
    {
        Profiler.BeginSample("SortSectorObjects");

        foreach (List<GameObject> sectorObjList in sectorObjs)
        {
            sectorObjList.Sort((a, b) =>
            {
                float sqrDistToA = (a.transform.position - transform.position).sqrMagnitude;
                float sqrDistToB = (b.transform.position - transform.position).sqrMagnitude;
                if (sqrDistToA < sqrDistToB)
                    return -1;
                else if (sqrDistToA > sqrDistToB)
                    return 1;
                else
                    return 0;
            });
        }

        Profiler.EndSample();
    }

    // Updates the list of objects in each sector
    private void UpdateSectorObjects()
    {
        Profiler.BeginSample("UpdateSectorObjects");

        ClearSectorObjects();

        // Get objects in each sector that are the type of object this sensor is
        // sensitive to.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range);
        foreach (Collider collider in hitColliders)
        {
            if (collider.tag == sensitiveTag && collider.gameObject != gameObject)
            {
                int sectorIdx = PosToSectorIdx(collider.transform.position);
                if (IsValidSectorIdx(sectorIdx)) // FOV check
                {
                    sectorObjs[sectorIdx].Add(collider.gameObject);
                }
            }
        }

        // Sort objects in each sector by their distance
        SortSectorObjects();

        Profiler.EndSample();
    }

    GameObject GetFirstVisible(int sectorIdx)
    {
        // Loop over objects in sector from near to far
        GameObject firstVisible = null;
        foreach (GameObject obj in sectorObjs[sectorIdx])
        {
            // Return first visible
            RaycastHit hitInfo;
            Vector3 rayDir = obj.transform.position - transform.position;
            if (Physics.Raycast(transform.position, rayDir, out hitInfo, range, blocksLineOfSight))
            {
                if (hitInfo.transform == obj.transform)
                {
                    firstVisible = obj;
                    break;
                }
                else if (displayDebug)
                {
                    Debug.DrawLine(transform.position, obj.transform.position, Color.magenta);
                }
            }
        }
        return firstVisible;
    }

    private void FixedUpdate()
    {
        // Avoid updating every physics frame
        timeAccum += Time.fixedDeltaTime;
        if (timeAccum >= updateInterval)
        {
            timeAccum -= updateInterval;

            UpdateSectorObjects();

            // Set sector senses to the distance to the first visible object
            for (int sectorIdx = 0; sectorIdx < numSectors; ++sectorIdx)
            {
                GameObject firstVisible = GetFirstVisible(sectorIdx);

                // Sector sense should be clost to 0 for FAR objects in sector, and close to 1 for objects that are NEAR
                if (firstVisible != null)
                {
                    // Distance sense for nearest in sector
                    Vector3 vectorToFirstVisible = (firstVisible.transform.position - transform.position);
                    //float distance = vectorToFirstVisible.magnitude;
                    //sectorSenses[sectorIdx] = 1 - distance / range;

                    Vector3 relDisplacement = transform.InverseTransformDirection(vectorToFirstVisible);
                    Vector3 senseVector = relDisplacement.normalized - relDisplacement / range;
                    sectorSenses[sectorIdx * SensesPerSector + 0] = senseVector.x;
                    sectorSenses[sectorIdx * SensesPerSector + 1] = senseVector.y;
                    sectorSenses[sectorIdx * SensesPerSector + 2] = senseVector.z;

                    // Angle sense for nearest in sector
                    //if (senseAngles)
                    //{
                    //    Vector3 dirToFirstVisible = vectorToFirstVisible / distance;
                    //    float angleSense = Vector3.Dot(dirToFirstVisible, transform.right);
                    //    sectorSenses[numSectors + sectorIdx] = angleSense;
                    //}

                    // Visualize sensor registration
                    //if (displayDebug)
                    //{
                    //    Color debugColor = Color.Lerp(Color.white, Color.red, sectorSenses[sectorIdx]);
                    //    Debug.DrawLine(transform.position, firstVisible.transform.position, debugColor);
                    //}

                    // Visualize sensor registration
                    if (displayDebug)
                    {
                        float debugColorR = Mathf.Lerp(0, 1, sectorSenses[sectorIdx * SensesPerSector + 0]);
                        float debugColorG = Mathf.Lerp(0, 1, sectorSenses[sectorIdx * SensesPerSector + 1]);
                        float debugColorB = Mathf.Lerp(0, 1, sectorSenses[sectorIdx * SensesPerSector + 2]);
                        Color debugColor = new Color(debugColorR, debugColorG, debugColorB);
                        Debug.DrawLine(transform.position, firstVisible.transform.position, debugColor);
                    }
                }
                else
                {
                    sectorSenses[sectorIdx * SensesPerSector + 0] = 0;
                    sectorSenses[sectorIdx * SensesPerSector + 1] = 0;
                    sectorSenses[sectorIdx * SensesPerSector + 2] = 0;
                    //sectorSenses[sectorIdx] = 0;
                    //if (senseAngles)
                    //{
                    //    sectorSenses[numSectors + sectorIdx] = 0;
                    //}
                }
            }
        }
    }
}
