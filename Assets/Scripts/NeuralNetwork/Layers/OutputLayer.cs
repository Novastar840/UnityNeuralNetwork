using UnityEngine;
[System.Serializable]
public class OutputLayer : Layer
{
    [SerializeField] private OutputNeuron[] OutputNeurons;

    public OutputNeuron[] GetNeurons()
    {
        return OutputNeurons;
    }

    public void InitializeLayer(int size, float[] previousLayerValues)
    {
        OutputNeurons = new OutputNeuron[size];
        for (int i = 0; i < size; i++)
        {
            OutputNeurons[i] = new OutputNeuron();
        }

        foreach (OutputNeuron neuron in OutputNeurons)
        {
            neuron.SetPreviousLayerValues(previousLayerValues);
            neuron.InitializeNeuron();
        }
    }

    public float[] GetAllNeuronValues()
    {
        float[] values = new float[OutputNeurons.Length];
        for (int i = 0; i < OutputNeurons.Length; i++)
        {
            values[i] = OutputNeurons[i].GetNeuronValue();
        }
        return values;
    }
}
