using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
    public float moveSpeed = 16f;
    public float swimSpeed = 2f;
    public float acceleration = 5f;
    public float turnSpeed = 10f;
    public float slideRatioMin = 0.2f;
    public float slideRatioMax = 1f;
    private Rigidbody rb;
    private InputController input;

    private List<Water> overlappedWaterTiles;
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

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        input = GetComponent<InputController>();
        overlappedWaterTiles = new List<Water>();
    }

    private void FixedUpdate()
    {
        // Update overlapping water tiles list in case any have been used up and destroyed
        for (int i = 0; i < overlappedWaterTiles.Count; ++i)
        {
            if (overlappedWaterTiles[i] == null)
            {
                overlappedWaterTiles.RemoveAt(i);
                --i;
            }
        }

        // Apply water tile movement modifier
        float speed;
        if (overlappedWaterTiles.Count > 0)
            speed = swimSpeed;
        else
            speed = moveSpeed;

        // Get input
        if (input == null)
            input = GetComponent<InputController>();

        float xInput;
        float yInput;
        if (input == null)
        {
            xInput = 0;
            yInput = 0;
        }
        else
        {
            xInput = input.GetXInput();
            yInput = input.GetYInput();
        }

        // Calculate forces
        Vector3 targetForwardVelocity = transform.forward * speed * yInput;
        Vector3 forwardVelocity = Vector3.Project(rb.velocity, transform.forward);
        // Multiply by 2 so that AVERAGE acceleration (when going from 0 to full speed) is equal to user specified acceleration.
        Vector3 forwardAcceleration = (targetForwardVelocity - forwardVelocity) / speed * 2 * acceleration;

        rb.AddForceAtPosition(forwardAcceleration, transform.position, ForceMode.Acceleration);

        Vector3 velRight = Vector3.Project(rb.velocity, transform.right);
        Vector3 velForward = Vector3.Project(rb.velocity, transform.forward);
        float slideDenominator = (velRight.magnitude + velForward.magnitude);
        if (slideDenominator >= 0.01f)
        {
            float slideRatio = velRight.magnitude / slideDenominator;
            slideRatio = Mathf.Lerp(slideRatioMax, slideRatioMin, slideRatio);
            rb.AddForce(-slideRatio * velRight, ForceMode.VelocityChange);
        }

        Vector3 targetAngularVelocity = transform.up * turnSpeed * xInput;
        Vector3 turnTorque = targetAngularVelocity - rb.angularVelocity;

        rb.AddTorque(turnTorque, ForceMode.VelocityChange);
    }
}
