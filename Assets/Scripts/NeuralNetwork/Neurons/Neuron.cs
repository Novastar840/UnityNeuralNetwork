using UnityEngine;
[System.Serializable]
public class Neuron 
{
    [SerializeField] protected float Value = 0;

    public float GetNeuronValue()
    {
        return Value;
    }

    public void SetNeuronValue(float value)
    {
        Value = value;
    }
}
