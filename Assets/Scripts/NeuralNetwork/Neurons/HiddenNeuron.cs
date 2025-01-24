using System;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class HiddenNeuron : NonInputNeuron
{
    public override void CalculateNeuronValue()
    {
        base.CalculateNeuronValue();

        SetNeuronValue(Tools.ReLU(GetNeuronValue()));
    }
}
