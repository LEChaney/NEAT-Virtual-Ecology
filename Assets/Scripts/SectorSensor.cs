using MathNet.Numerics.LinearAlgebra.Complex;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.PlayerLoop;

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
    public bool senseAngles = true;

    // Double the number of sectors if angle sensing is on
    public int NumSenses { get; private set; }

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
        if (senseAngles)
            NumSenses = numSectors * 2;
        else
            NumSenses = numSectors;
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
        for (int i = 0; i < numSectors; ++i)
        {
            sectorObjs[i].Clear();
        }
    }

    // Returns the index of the sector containing the given position.
    // Note that bounds are not checked, so output could be negative,
    // or greater than the number of sectors in the sensor. This is
    // so we can check if something is outside of the sensors field of view.
    private int PosToSectorIdx(Vector3 position)
    {
        float fovStart = -fov / 2;
        float sectorAngleDelta = fov / numSectors;

        Vector3 relPosition = transform.InverseTransformPoint(position);
        float angleFromForward = Mathf.Atan2(relPosition.z, relPosition.x) * Mathf.Rad2Deg - 90;
        float angleFromFOVStart = angleFromForward - fovStart;
        int sectorIdx = (int)Mathf.Floor(angleFromFOVStart / sectorAngleDelta);

        return sectorIdx;
    }

    // Checks a sector index is valid.
    // Useful to apply to results of PosToSectorIdx to check position is inside sensor FOV.
    private bool IsValidSectorIdx(int idx)
    {
        return (idx >= 0 && idx < numSectors);
    }

    private void FixedUpdate()
    {
        // Avoid updating every physics frame
        timeAccum += Time.fixedDeltaTime;
        if (timeAccum >= updateInterval)
        {
            timeAccum -= updateInterval;
            // Get objects in each sector that are the type of object this sensor is
            // sensitive to.
            ClearSectorObjects();
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

            // Set sector senses to the distance to the first visible object
            for (int sectorIdx = 0; sectorIdx < numSectors; ++sectorIdx)
            {
                // Loop over objects in sector from near to far
                GameObject firstVisble = null;
                foreach (GameObject obj in sectorObjs[sectorIdx])
                {
                    // Return first visible
                    RaycastHit hitInfo;
                    Vector3 rayDir = obj.transform.position - transform.position;
                    if (Physics.Raycast(transform.position, rayDir, out hitInfo))
                    {
                        if (hitInfo.transform == obj.transform)
                        {
                            firstVisble = obj;
                            break;
                        }
                    }
                }

                // Sector sense should be 0 for NO or FAR objects in sector, and close to 1 for objects that are NEAR
                if (firstVisble != null)
                {
                    // Distance sensing for closest in sector
                    Vector3 vectorToFirstVisible = (firstVisble.transform.position - transform.position);
                    float distance = vectorToFirstVisible.magnitude;
                    sectorSenses[sectorIdx] = 1 - distance / range;

                    // Angle sensing for closest in sector
                    if (senseAngles)
                    {
                        Vector3 dirToFirstVisible = vectorToFirstVisible / distance;
                        float angleSense = Vector3.Dot(dirToFirstVisible, transform.right);
                        sectorSenses[numSectors + sectorIdx] = angleSense;
                    }

                    // Visualize sensor registration
                    if (displayDebug)
                    {
                        Color debugColor = Color.Lerp(Color.white, Color.red, sectorSenses[sectorIdx]);
                        Debug.DrawLine(transform.position, firstVisble.transform.position, debugColor, updateInterval - timeAccum);
                    }
                }
                else
                {
                    sectorSenses[sectorIdx] = 0;
                    if (senseAngles)
                    {
                        sectorSenses[numSectors + sectorIdx] = 0;
                    }
                }
            }
        }
    }
}
