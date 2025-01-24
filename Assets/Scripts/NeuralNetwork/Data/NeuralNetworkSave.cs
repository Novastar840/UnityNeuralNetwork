using System.IO;
using UnityEngine;

[CreateAssetMenu(fileName = "NeuralNetworkSave", menuName = "Scriptable Objects/NeuralNetworkSave")]
public class NeuralNetworkSave : ScriptableObject
{
    private NeuralNetwork NeuralNetwork;

    [SerializeField] private string SaveFileName = "NeuralNetworkSave.json";
    private string SavesFolder = Path.Combine(Application.dataPath, "Saves");

    public void Save(NeuralNetwork neuralNetwork)
    {
        NeuralNetwork = neuralNetwork;

        string json = JsonUtility.ToJson(NeuralNetwork, true);

        if (!Directory.Exists(SavesFolder))
        {
            Directory.CreateDirectory(SavesFolder);
            Debug.Log("Created Saves folder at " + SavesFolder);
        }
        string saveFilePath = Path.Combine(SavesFolder, SaveFileName);
        File.WriteAllText(saveFilePath, json);

        Debug.Log("Neural Network saved to " + saveFilePath);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }

    public NeuralNetwork Load()
    {
        string filePath = Path.Combine(SavesFolder, SaveFileName);

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            NeuralNetwork = JsonUtility.FromJson<NeuralNetwork>(json);
            Debug.Log("Neural Network loaded from " + filePath);
            return NeuralNetwork;
        }
        Debug.LogWarning("No save file found at " + filePath);
        return null;
    }
}
