using SharpNeat.Phenomes;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.XR;

public class EvoAIController : UnitController, InputController
{
    IBlackBox box;
    bool isRunning;
    float xInput;
    float yInput;
    HealthController health;

    void Start()
    {
    }

    private void FixedUpdate()
    {
        if (isRunning)
        {

        }
    }


    public override void Activate(IBlackBox box)
    {
        this.box = box;
        this.isRunning = true;

        xInput = 0;
        yInput = 0;
    }

    public override void Stop()
    {
        this.isRunning = false;
    }

    public override float GetFitness()
    {
        return health.health;
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
