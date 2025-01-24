using UnityEngine;

[CreateAssetMenu(fileName = "NeuralNetworkInitData", menuName = "Scriptable Objects/NeuralNetworkInitData")]
public class NeuralNetworkInitData : ScriptableObject
{
    public int InputCount;
    public int OutputCount;
    public int[] HiddenLayerSizes;
}
