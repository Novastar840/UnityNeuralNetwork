using System;
using UnityEngine;

public class OutputNeuron : NonInputNeuron
{
    public override void CalculateNeuronValue()
    {
        base.CalculateNeuronValue();

        SetNeuronValue(Tools.Sigmoid(GetNeuronValue()));
    }
}
