using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class NonInputNeuron : Neuron
{
    [SerializeField] protected float[] PreviousLayerNeuronValues;
    [SerializeField] protected float[] Weights;
    [SerializeField] protected float Bias = 0;
    protected float GetWeight()
    {
        return Tools.Sigmoid(Tools.GetTotalValue(Weights));
    }

    protected float GetBias()
    {
        return Bias;
    }

    protected void SetBias(float value)
    {
        Bias = value;
    }

    public void SetPreviousLayerValues(float[] values)
    {
        PreviousLayerNeuronValues = values;
    }

    public void InitializeNeuron()
    {
        Weights = new float[PreviousLayerNeuronValues.Length];

        for (int i = 0; i < Weights.Length; i++)
        {
            Weights[i] = Random.Range(-1f, 1f);
        }
        Bias = Random.Range(-1f, 1f);

        CalculateNeuronValue();
    }

    public void CalculateNeuronValue()
    {
        List<float> calculatedWeightValues = new List<float>();
        for (int i = 0; i < PreviousLayerNeuronValues.Length; i++)
        {
            float result = PreviousLayerNeuronValues[i] * Weights[i];
            calculatedWeightValues.Add(result);
        }

        Value = Tools.Sigmoid(Tools.GetTotalValue(calculatedWeightValues) + Bias);
    }
}
