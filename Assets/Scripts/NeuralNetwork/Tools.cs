using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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

    public static bool Roll(float chance)
    {
	    // Ensure chance is between 0 and 1
	    chance = Mathf.Clamp01(chance);
	    // Random.value returns a float in [0.0, 1.0)
	    return Random.value < chance;
    }
}
