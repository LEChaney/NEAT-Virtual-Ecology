using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour, InputController
{
    private float xInput = 0;
    private float yInput = 0;

    public float GetXInput()
    {
        return xInput;
    }

    public float GetYInput()
    {
        return yInput;
    }

    // Update is called once per frame
    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");
    }
}
