using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AttributeBars : MonoBehaviour
{
    public Slider energySlider;
    public Slider hydrationSlider;
    public Transform followTransform;
    public Vector2 offset;
    public Camera cam;

    private void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    public void LateUpdate()
    {
        transform.position = cam.WorldToScreenPoint(followTransform.position);
        transform.position = new Vector3(transform.position.x + offset.x, transform.position.y + offset.y, transform.position.z);
    }

    public void SetMaxEnergy(float maxEnergy)
    {
        energySlider.maxValue = maxEnergy;
        energySlider.value = maxEnergy;
    }

    public void SetMaxHydration(float maxHydration)
    {
        hydrationSlider.maxValue = maxHydration;
        hydrationSlider.value = maxHydration;
    }

    public void SetEnergy(float energy)
    {
        energySlider.value = energy;
    }

    public void SetHydration(float hydration)
    {
        hydrationSlider.value = hydration;
    }
}
