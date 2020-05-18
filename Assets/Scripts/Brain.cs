using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brain : MonoBehaviour, InputController
{
    private float xControlOutput;
    private float yControlOutput;

    private float numInputs;
    private float numHidden;
    private float numOutputs;

    public Brain()
    {
        numInputs = 4;
    }

    public float GetXInput()
    {
        return xControlOutput;
    }

    public float GetYInput()
    {
        return yControlOutput;
    }

    private void FixedUpdate()
    {
        
    }
}
