using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.Rendering.CameraUI;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private GameObject RightLegHigh;
    [SerializeField] private GameObject LeftLegHigh;
    [SerializeField] private GameObject RightLegLow;
    [SerializeField] private GameObject LeftLegLow;
    [SerializeField] private GameObject RightFoot;
    [SerializeField] private GameObject LeftFoot;
    [SerializeField] private GameObject Body;

    [SerializeField] private NeuralNetworkInitData InitData;
    [SerializeField] private NeuralNetworkSave SaveFile;

    [SerializeField] private ConfigurableJoint RightLegHighJoint;
    [SerializeField] private ConfigurableJoint LeftLegHighJoint;
    [SerializeField] private ConfigurableJoint RightLegLowJoint;
    [SerializeField] private ConfigurableJoint LeftLegLowJoint;
    [SerializeField] private ConfigurableJoint RightFootJoint;
    [SerializeField] private ConfigurableJoint LeftFootJoint;

    [SerializeField] private GameObject TargetObject;

    private Vector3 RightLegHighPosition;
    private Vector3 LeftLegHighPosition;
    private Vector3 RightLegLowPosition;
    private Vector3 LeftLegLowPosition;
    private Vector3 RightFootPosition;
    private Vector3 LeftFootPosition;
    private Vector3 BodyPosition;

    private NeuralNetwork NeuralNetwork;

    private void Start()
    {
        NeuralNetwork = SaveFile.Load();
        if (NeuralNetwork == null)
        {
            NeuralNetwork = new NeuralNetwork();
            NeuralNetwork.InitializeNeuralNetwork(InitData);
            NeuralNetwork.Save(SaveFile);
        }
    }

    private void FixedUpdate()
    {
        float[] inputs = NormalizeInputs(MakeInputList());
        NeuralNetwork.ProcessData(inputs);
        SetRagDollJoints(NeuralNetwork.GetOutputLayerData());
        float[] outputs = NeuralNetwork.GetOutputLayerData();
        Debug.Log($"Neural Network Outputs: {string.Join(", ", outputs)}");
    }

    private void UpdateBodyPositions()
    {
        RightLegHighPosition = RightLegHigh.transform.position;
        LeftLegHighPosition = LeftLegHigh.transform.position;
        BodyPosition = Body.transform.position;
        RightLegLowPosition = RightLegLow.transform.position;
        LeftLegLowPosition = LeftLegLow.transform.position;
        RightFootPosition = RightFoot.transform.position;
        LeftFootPosition = LeftFoot.transform.position;
    }


    [ContextMenu("Initialize network")]
    private void InitializeNeuralNetwork()
    {
        NeuralNetwork = new NeuralNetwork();
        NeuralNetwork.InitializeNeuralNetwork(InitData);
        NeuralNetwork.Save(SaveFile);
    }

    private float[] NormalizeInputs(float[] inputs)
    {
        float min = Mathf.Min(inputs); // Find the smallest value
        float max = Mathf.Max(inputs); // Find the largest value

        float[] normalized = new float[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            normalized[i] = (inputs[i] - min) / (max - min);
        }
        return normalized;
    }


    private float[] MakeInputList()
    {
        UpdateBodyPositions();

        List<float> inputList = new List<float>();
        BreakVectorsAndAdd(ref inputList, 
            RightLegHighPosition, 
            LeftLegHighPosition, 
            RightLegLowPosition, 
            LeftLegLowPosition, 
            RightFootPosition, 
            LeftFootPosition, 
            BodyPosition);

        inputList.Add(TargetObject.transform.position.x);
        inputList.Add(TargetObject.transform.position.z);
        return inputList.ToArray();
    }

    private void BreakVectorsAndAdd(ref List<float> inputList, params Vector3[] vectors)
    {
        foreach (Vector3 vector in vectors)
        {
            inputList.Add(vector.x);
            inputList.Add(vector.y);
            inputList.Add(vector.z);
        }
    }


    private void SetRagDollJoints(float[] data)
    {
        NormaliseOutput(ref data);

        int outputIterationIndex = 0;
        RightLegHighJoint.targetRotation = GetQuaternionFromNeuralOutput(ref data, ref outputIterationIndex);
        LeftLegHighJoint.targetRotation = GetQuaternionFromNeuralOutput(ref data, ref outputIterationIndex);
        RightLegLowJoint.targetRotation = GetQuaternionFromNeuralOutput(ref data, ref outputIterationIndex);
        LeftLegLowJoint.targetRotation = GetQuaternionFromNeuralOutput(ref data, ref outputIterationIndex);
        RightFootJoint.targetRotation = GetQuaternionFromNeuralOutput(ref data, ref outputIterationIndex);
        LeftFootJoint.targetRotation = GetQuaternionFromNeuralOutput(ref data, ref outputIterationIndex);
    }

    private Quaternion GetQuaternionFromNeuralOutput(ref float[] data, ref int index)
    {
        Quaternion rotation = new Quaternion(data[index], data[index + 1], data[index + 2], data[index + 3]);
        index += 4;
        return rotation.normalized;
    }

    private void NormaliseOutput(ref float[] data)
    {

    }
}
