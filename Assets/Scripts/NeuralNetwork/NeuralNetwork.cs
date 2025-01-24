using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

[System.Serializable]
public class NeuralNetwork
{
    [SerializeField] [HideInInspector] private Layer[] HiddenLayers;
    [SerializeField] private InputLayer InputLayer;
    [SerializeField] private Layer OutputLayer;

    private void SetInputLayerValues(float[] data)
    {
        for (int i = 0; i < InputLayer.GetNeurons().Length; ++i)
        {
            InputLayer.GetNeurons()[i].SetNeuronValue(data[i]);
        }
    }

    public float[] GetOutputLayerData()
    {
        return OutputLayer.GetAllNeuronValues();
    }

    public void InitializeNeuralNetwork(NeuralNetworkInitData initData)
    {
        InputLayer = new InputLayer();
        InputLayer.InitializeLayer(initData.InputCount);

        float[] previousLayerValues = InputLayer.GetAllNeuronValues();

        HiddenLayers = new Layer[initData.HiddenLayerSizes.Length];
        for (int i = 0; i < initData.HiddenLayerSizes.Length; i++)
        {
            Layer hiddenLayer = new Layer();
            hiddenLayer.InitializeLayer<HiddenNeuron>(initData.HiddenLayerSizes[i], previousLayerValues);
            HiddenLayers[i] = hiddenLayer;
            previousLayerValues = hiddenLayer.GetAllNeuronValues();
        }

        OutputLayer = new Layer();
        OutputLayer.InitializeLayer<OutputNeuron>(initData.OutputCount, previousLayerValues);
    }

    public void Save(NeuralNetworkSave saveFile)
    {
        saveFile.Save(this);
    }

    public void Load(NeuralNetwork loadedNeuralNetWork)
    {
        HiddenLayers = loadedNeuralNetWork.HiddenLayers;
        InputLayer = loadedNeuralNetWork.InputLayer;
        OutputLayer = loadedNeuralNetWork.OutputLayer;
    }

    public bool IsInitialized()
    {
        if (InputLayer == null && OutputLayer == null)
        {
            return false;
        }
        return true;
    }

    public float[] ProcessData(float[] data)
    {
        SetInputLayerValues(data);

        for (int i = 0; i < HiddenLayers.Length; i++)
        {
            foreach (NonInputNeuron neuron in HiddenLayers[i].GetNeurons())
            {
                if (i == 0) neuron.SetPreviousLayerValues(InputLayer.GetAllNeuronValues());
                else neuron.SetPreviousLayerValues(HiddenLayers[i - 1].GetAllNeuronValues());

                neuron.CalculateNeuronValue();
            }
        }

        NonInputNeuron[] outputNeurons = OutputLayer.GetNeurons() as NonInputNeuron[];
        for (int i = 0; i < OutputLayer.GetNeurons().Length; i++)
        {
            outputNeurons[i].SetPreviousLayerValues(HiddenLayers[^1].GetAllNeuronValues());
            outputNeurons[i].CalculateNeuronValue();
        }

        return OutputLayer.GetAllNeuronValues();
    }

    private int GetBiggestLayerSize()
    {
        int largestLayer = 0;
        if (InputLayer.GetNeurons().Length > OutputLayer.GetNeurons().Length)
        {
            largestLayer = InputLayer.GetNeurons().Length;
        }
        else largestLayer = OutputLayer.GetNeurons().Length;

        int largestHiddenLayer = HiddenLayers[0].GetNeurons().Length;
        for (int i = 1; i < HiddenLayers.Length; ++i)
        {
            if (HiddenLayers[i - 1].GetNeurons().Length < HiddenLayers[i].GetNeurons().Length)
            {
                largestHiddenLayer = HiddenLayers[i].GetNeurons().Length;
            }
        }

        if (largestLayer < largestHiddenLayer)
        {
            return largestHiddenLayer;
        }
        return largestLayer;
    }
}
