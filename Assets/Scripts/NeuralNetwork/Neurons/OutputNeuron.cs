using System;
using UnityEngine;
[System.Serializable]
public class OutputNeuron : NonInputNeuron
{
    public override void CalculateNeuronValue()
    {
        base.CalculateNeuronValue();

        SetNeuronValue(Tools.Sigmoid(GetNeuronValue()));
    }
}
