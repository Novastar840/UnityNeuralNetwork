using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [SerializeField] private GameObject RightLegHigh;
    [SerializeField] private GameObject LeftLegHigh;
    [SerializeField] private GameObject RightLegLow;
    [SerializeField] private GameObject LeftLegLow;
    [SerializeField] private GameObject RightFoot;
    [SerializeField] private GameObject LeftFoot;
    [SerializeField] private GameObject Body;
    private GameObject[] BodyParts;

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

    private Collider[] PositiveColliders;
    private Collider[] NegativeColliders;

    private NeuralTrainer Trainer;
    private int PositiveCollisionCounter = 0;
    private int NegativeCollisionCounter = 0;
    private void Awake()
    {
        PositiveColliders = Tools.GetComponentsFromObjects<Collider>(LeftFoot, RightFoot);
        NegativeColliders = Tools.GetComponentsFromObjects<Collider>(RightLegHigh, LeftLegHigh, LeftLegLow, RightLegLow, Body);
        Trainer = GetComponent<NeuralTrainer>();
    }

    private void Start()
    {
        BodyParts = Tools.MakeArray(RightLegHigh, LeftLegHigh, LeftLegLow, RightLegLow, Body, LeftFoot, RightFoot);
        SetupCollisionRelay(BodyParts);
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

    private void OnCollisionEnter(Collision other)
    {
        CollisionEnter(other);
    }

    private void OnCollisionExit(Collision other)
    {
       CollisionExit(other);
    }

    public void CollisionEnter(Collision other)
    {
        Collider otherCollider = other.collider;

        foreach (Collider collider in PositiveColliders)
        {
            if (collider == otherCollider)
            {
                PositiveCollisionCounter++;
                return;
            }
        }

        foreach (Collider collider in NegativeColliders)
        {
            if (collider == otherCollider)
            {
                NegativeCollisionCounter++;
                return;
            }
        }
        
        UpdateStatusBooleans();
    }

    public void CollisionExit(Collision other)
    {
        Collider otherCollider = other.collider;

        foreach (Collider collider in PositiveColliders)
        {
            if (collider == otherCollider)
            {
                PositiveCollisionCounter--;
                return;
            }
        }

        foreach (Collider collider in NegativeColliders)
        {
            if (collider == otherCollider)
            {
                NegativeCollisionCounter--;
                return;
            }
        }
        
        UpdateStatusBooleans();
    }

    private void SetupCollisionRelay(GameObject[] objects)
    {
        foreach (GameObject gameObject in objects)
        {
            gameObject.AddComponent<CollisionRelay>();
        }
    }

    private void UpdateStatusBooleans()
    {
        if (NegativeCollisionCounter <= 0 && PositiveCollisionCounter > 0)
        {
            Trainer.Fallen = false;
            Trainer.FeetOnGround = true;
        }
        else if (NegativeCollisionCounter <= 0 && PositiveCollisionCounter <= 0)
        {
            Trainer.Fallen = false;
            Trainer.FeetOnGround = false;
        }
        else if (NegativeCollisionCounter > 0 && PositiveCollisionCounter > 0)
        {
            Trainer.Fallen = true;
            Trainer.FeetOnGround = true;
        }
        else
        {
            Trainer.Fallen = true;
            Trainer.FeetOnGround = false;
        }
    }
}
