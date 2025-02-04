using UnityEngine;
[System.Serializable]
public class HiddenLayer : Layer
{
    [SerializeField] private HiddenNeuron[] HiddenNeurons;

    public HiddenNeuron[] GetNeurons()
    {
        return HiddenNeurons;
    }

    public void InitializeLayer(int size, float[] previousLayerValues)
    {
        HiddenNeurons = new HiddenNeuron[size];
        for (int i = 0; i < size; i++)
        {
            HiddenNeurons[i] = new HiddenNeuron(); 
        }

        foreach (HiddenNeuron neuron in HiddenNeurons)
        {
            neuron.SetPreviousLayerValues(previousLayerValues);
            neuron.InitializeNeuron();
        }
    }

    public float[] GetAllNeuronValues()
    {
        float[] values = new float[HiddenNeurons.Length];
        for (int i = 0; i < HiddenNeurons.Length; i++)
        {
            values[i] = HiddenNeurons[i].GetNeuronValue();
        }
        return values;
    }
}
