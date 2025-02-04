using UnityEngine;

[System.Serializable]
public class InputLayer : Layer
{
    [SerializeField] protected InputNeuron[] InputNeurons;

    public InputNeuron[] GetNeurons()
    {
        return InputNeurons;
    }

    public void InitializeLayer(int size)
    {
        InputNeurons = new InputNeuron[size];
        for (int i = 0; i < size; i++)
        {
            InputNeurons[i] = new InputNeuron();
            InputNeurons[i].SetNeuronValue(Random.Range(-1f, 1f));
        }
    }

    public float[] GetAllNeuronValues()
    {
        float[] values = new float[InputNeurons.Length];
        for (int i = 0; i < InputNeurons.Length; i++)
        {
            values[i] = InputNeurons[i].GetNeuronValue();
        }
        return values;
    }
}
