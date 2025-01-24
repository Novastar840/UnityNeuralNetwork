using UnityEngine;

[System.Serializable]
public class InputLayer : Layer
{
    [SerializeField] protected InputNeuron[] InputNeurons;

    public void InitializeLayer(int size)
    {
        InputNeuron[] array = new InputNeuron[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = new InputNeuron();
        }
        InputNeurons = array;
    }

    public override float[] GetAllNeuronValues()
    {
        float[] values = new float[InputNeurons.Length];
        for (int i = 0; i < InputNeurons.Length; i++)
        {
            values[i] = InputNeurons[i].GetNeuronValue();
        }
        return values;
    }

    public new InputNeuron[] GetNeurons()
    {
        return InputNeurons;
    }
}
