using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EAStartUI : MonoBehaviour
{
    public float UIOffset = 220;

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10 + UIOffset, 10, 100, 40), "Start All EA"))
        {
            foreach (var optmizer in FindObjectsOfType<Optimizer>())
                optmizer.StartEA();
        }

        if (GUI.Button(new Rect(10 + UIOffset, 60, 100, 40), "Stop EA"))
        {
            foreach (var optmizer in FindObjectsOfType<Optimizer>())
                optmizer.StopEA();
        }
        if (GUI.Button(new Rect(10 + UIOffset, 110, 100, 40), "Run best"))
        {
            foreach (var optmizer in FindObjectsOfType<Optimizer>())
                optmizer.RunBest();
        }
    }
}
