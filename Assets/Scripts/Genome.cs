using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class Genome
{
    private List<float> weights;
    private float numANNInputs;
    private float numANNHidden;
    private float numANNOutputs;

    public Genome(float numANNInputs, float numANNHidden, float numANNOutputs)
    {
        this.numANNInputs = numANNInputs;
        this.numANNHidden = numANNHidden;
        this.numANNOutputs = numANNOutputs;

        for (int i = 0; i < numANNInputs; ++i)
            for (int j = 0; j < numANNHidden; ++j)
                weights.Add(Random.value);

        for (int i = 0; i < numANNHidden; ++i)
            for (int j = 0; j < numANNOutputs; ++j)
                weights.Add(Random.value);
    }

    public static Genome CrossOver(Genome a, Genome b)
    {
        Genome child = new Genome(a.numANNInputs, a.numANNHidden, a.numANNOutputs);
        float crossoverPoint = Random.Range(0, a.weights.Count);
        for (int i = 0; i < child.weights.Count; ++i)
        {
            if (i < crossoverPoint)
                child.weights[i] = a.weights[i];
            else
                child.weights[i] = b.weights[i];
        }

        return child;
    }

    public Genome CrossOver(Genome that)
    {
        return CrossOver(this, that);
    }

    public void RandomizeWeights()
    {
        for (int i = 0; i < weights.Count; ++i)
            weights[i] = Random.value;
    }

    // mutatePercent in range [0 - 1].
    public void Mutate(float mutatePercent)
    {
        for (int i = 0; i < weights.Count; ++i)
        {
            if (Random.value < mutatePercent)
                weights[i] = Random.value;
        }
    }
}
