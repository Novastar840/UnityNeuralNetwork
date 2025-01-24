using System;
using UnityEngine;
[System.Serializable]
public class Layer
{
    [SerializeField] private NonInputNeuron[] Neurons;
    public Neuron[] GetNeurons()
    {
        return Neurons;
    }

    public void InitializeLayer<T>(int size, float[] previousLayerValues = null) where T : NonInputNeuron, new()
    {
        T[] array = new T[size];

        for (int i = 0; i < size; i++)
        {
            array[i] = new T(); 
        }
        Neurons = array;

        if (previousLayerValues != null)
        {
            foreach (NonInputNeuron neuron in Neurons)
            {
                neuron.SetPreviousLayerValues(previousLayerValues);
                neuron.InitializeNeuron();
            }
        }
    }

    public virtual float[] GetAllNeuronValues()
    {
        float[] values = new float[Neurons.Length];
        for (int i = 0; i < Neurons.Length; i++)
        {
            values[i] = Neurons[i].GetNeuronValue();
        }
        return values;
    }
}
