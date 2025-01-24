using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class NeuralNetwork
{
    [SerializeField] [HideInInspector] private Layer[] HiddenLayers;
    [SerializeField] private InputLayer InputLayer;
    [SerializeField] private Layer OutputLayer;

    private void ImportData(float[] data)
    {
        for (int i = 0; i > InputLayer.GetNeurons().Length; ++i)
        {
            InputLayer.GetNeurons()[i].SetNeuronValue(data[i]);
        }
    }

    public void InitializeNeuralNetwork(NeuralNetworkInitData initData)
    {
        InputLayer = new InputLayer();
        InputLayer.InitializeLayer(initData.InputCount);

        float[] previousLayerValues = InputLayer.GetAllNeuronValues();

        HiddenLayers = new Layer[initData.HiddenLayerCount];
        for (int i = 0; i < initData.HiddenLayerCount; i++)
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

    public void Load(NeuralNetworkSave saveFile)
    {
        NeuralNetwork loadedNeuralNetWork = saveFile.Load();
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
