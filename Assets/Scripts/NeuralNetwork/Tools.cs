using System;
using System.Collections.Generic;
using UnityEngine;

public class Tools
{
    public static float Sigmoid(float x)
    {
        return 1f / (1f + Mathf.Exp(-x));
    }

    public static float GetTotalValue(Array array)
    {
        float total = 0f;
        foreach (float element in array)
        {
            total += element;
        }
        return total;
    }

    public static float GetTotalValue(List<float> array)
    {
        float total = 0f;
        foreach (float element in array)
        {
            total += element;
        }
        return total;
    }

    public static float ReLU(float value)
    {
        return Mathf.Max(0, value);
    }

}
