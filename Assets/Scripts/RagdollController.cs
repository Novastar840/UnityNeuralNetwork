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

	[Header("Normalization Ranges")]
	[SerializeField] private float MaxLimbDistance = 4f;        // maximum expected distance of limb from body
	[SerializeField] private float MaxTargetDistance = 30f;     // maximum distance to walk target

	public GameObject WalkTarget;

	private Vector3 RightLegHighPosition;
	private Vector3 LeftLegHighPosition;
	private Vector3 RightLegLowPosition;
	private Vector3 LeftLegLowPosition;
	private Vector3 RightFootPosition;
	private Vector3 LeftFootPosition;
	private Vector3 BodyPosition;

	public NeuralNetwork NeuralNetwork;

	private Collider[] PositiveColliders;
	private Collider[] NegativeColliders;

	private NeuralTrainer Trainer;
	private int PositiveCollisionCounter = 0;
	private int NegativeCollisionCounter = 0;

	public delegate void NoParamDelegate();

	public NoParamDelegate OnRagDollStatusUpdate;

	[HideInInspector] public bool IsTraining;

	// For Multithreaded processing
	[HideInInspector] public float[] PendingOutputs;
	[HideInInspector] public bool HasPendingOutputs = false;

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

		if (IsTraining)
		{
			return;
		}

		if (NeuralNetwork == null)
		{
			NeuralNetwork = new NeuralNetwork();
			NeuralNetwork.InitializeNeuralNetwork(InitData);
			NeuralNetwork.Save(SaveFile);
		}
		NeuralNetwork = SaveFile.Load();
	}

	private void FixedUpdate()
	{
		if (IsTraining)
		{
			// For training, outputs are computed in parallel by NeuralTrainerManager
			// Apply pending outputs if available
			if (HasPendingOutputs && PendingOutputs != null)
			{
				SetRagDollJoints(PendingOutputs);
				HasPendingOutputs = false;
			}
		}
		else
		{
			// Non-training mode: process synchronously
			float[] inputs = NormalizeInputs(MakeInputList());
			NeuralNetwork.ProcessData(inputs);
			SetRagDollJoints(NeuralNetwork.GetOutputLayerData());
		}
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
		// Each group of 3 (x,y,z) gets normalized independently
		float[] normalized = new float[inputs.Length];
		for (int i = 0; i < inputs.Length; i += 3)
		{
			Vector3 vec = new Vector3(inputs[i], inputs[i + 1], inputs[i + 2]);
			vec = Vector3.ClampMagnitude(vec, 1f); // Just clamp magnitude
			normalized[i] = vec.x;
			normalized[i + 1] = vec.y;
			normalized[i + 2] = vec.z;
		}
		return normalized;
	}


	private float[] MakeInputList()
	{
		// Update positions
		UpdateBodyPositions();

		// Compute everything relative to body root
		Vector3 bodyPos = BodyPosition;
		Vector3 rLegHigh = (RightLegHighPosition - bodyPos) / MaxLimbDistance;
		Vector3 lLegHigh = (LeftLegHighPosition - bodyPos) / MaxLimbDistance;
		Vector3 rLegLow = (RightLegLowPosition - bodyPos) / MaxLimbDistance;
		Vector3 lLegLow = (LeftLegLowPosition - bodyPos) / MaxLimbDistance;
		Vector3 rFoot = (RightFootPosition - bodyPos) / MaxLimbDistance;
		Vector3 lFoot = (LeftFootPosition - bodyPos) / MaxLimbDistance;

		// Relative target direction normalized
		Vector3 toTarget = (WalkTarget.transform.position - bodyPos) / MaxTargetDistance;

		// Build input list with clamped values
		List<float> inputList = new List<float>();
		AddVector3Clamped(ref inputList, rLegHigh);
		AddVector3Clamped(ref inputList, lLegHigh);
		AddVector3Clamped(ref inputList, rLegLow);
		AddVector3Clamped(ref inputList, lLegLow);
		AddVector3Clamped(ref inputList, rFoot);
		AddVector3Clamped(ref inputList, lFoot);
		AddVector3Clamped(ref inputList, toTarget);

		return inputList.ToArray();
	}

	private void AddVector3Clamped(ref List<float> list, Vector3 vector)
	{
		list.Add(Mathf.Clamp(vector.x, -1f, 1f));
		list.Add(Mathf.Clamp(vector.y, -1f, 1f));
		list.Add(Mathf.Clamp(vector.z, -1f, 1f));
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
		float maxRotationAngle = 40f;

		float x = data[index] * maxRotationAngle;
		float y = data[index + 1] * maxRotationAngle;
		float z = data[index + 2] * maxRotationAngle;
		index += 3;
		return Quaternion.Euler(x, y, z);
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
		OnRagDollStatusUpdate.Invoke();
	}

	public NeuralNetworkSave GetSaveFile()
	{
		return SaveFile;
	}

	public Vector3 GetBodyPosition()
	{
		return BodyPosition;
	}

	/// <summary>
	/// Prepare input data for neural network processing (called from main thread)
	/// </summary>
	public float[] PrepareInputsForProcessing()
	{
		UpdateBodyPositions();
		return NormalizeInputs(MakeInputList());
	}

	/// <summary>
	/// Apply pre-computed neural network outputs to joints
	/// </summary>
	public void ApplyNetworkOutputs(float[] outputs)
	{
		PendingOutputs = outputs;
		HasPendingOutputs = true;
	}
}
