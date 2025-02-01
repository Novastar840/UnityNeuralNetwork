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

    public static T[] GetComponentsFromObjects<T>(params GameObject[] objects) where T : Component
    {
        List<T> components = new List<T>();

        foreach (GameObject obj in objects)
        {
            T component = obj.GetComponent<T>();
            if (component != null)
            {
                components.Add(component);
            }
        }

        return components.ToArray();
    }

    public static T[] MakeArray<T>(params T[] objects)
    {
        return objects;
    }
}
