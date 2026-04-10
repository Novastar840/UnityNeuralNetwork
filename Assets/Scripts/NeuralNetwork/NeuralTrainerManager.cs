using System;
using System.Collections.Generic;
using UnityEngine;

public class NeuralTrainerManager : MonoBehaviour
{
	private List<GameObject> Generation = new List<GameObject>(0);
	private int IterationCount;

	[SerializeField] private GameObject StartPositionObject;
	private Vector3 StartPosition;

	[SerializeField] private int GenerationSize = 5;
	[SerializeField] private float MutateStrength = 0.5f;
	[SerializeField] private float IterationTime = 20f;
	[SerializeField] private int GenerationCarryOverCount = 3;
	private float IterationCountDown;

	[SerializeField] private GameObject RagDollPrefabToTrain;

	[SerializeField] private GameObject WalkingDestinationPrefab;
	private GameObject WalkingDestinationInstance;

	public static NeuralTrainerManager Singleton;

	private GameObject BestPerformingRagDoll = null;

	private NeuralNetwork TrainingNeuralNetwork;

	public delegate void IterationDelegate();

	public IterationDelegate OnIterationStartDelegate;

	private NeuralNetworkSave SaveFile;

	private void Awake()
	{
		Application.runInBackground = true;
	}

	private void Start()
	{
		if (Singleton == null)
		{
			Singleton = this;
		}
		else
		{
			Destroy(this);
		}

		StartPosition = StartPositionObject.transform.position;

		SaveFile = RagDollPrefabToTrain.GetComponent<RagdollController>().GetSaveFile();
		TrainingNeuralNetwork = SaveFile.Load();
		IterationCount = TrainingNeuralNetwork.GetIterationCount();

		StartIteration(true);
	}

	private void Update()
	{
		if (IterationCountDown > 0)
		{
			IterationCountDown -= Time.deltaTime;
		}
		else
		{
			EndIteration();
		}
	}

	private void StartIteration(bool firstIterationOfTrainingSession)
	{
		IterationCount++;
		TrainingNeuralNetwork.SetIterationCount(IterationCount);
		Debug.Log(IterationCount);
		IterationCountDown = IterationTime;

		if (firstIterationOfTrainingSession)
		{
			Quaternion randRotation = GetRandomRotation();
			for (int i = 0; i < GenerationSize; i++)
			{
				GameObject ragDoll = SpawnRagDollAndAddToGeneration(StartPosition, randRotation);
				if (i != 0)
				{
					AssignAndMutateNeuralNetworkCopy(ragDoll);
				}
			}
		}
		else
		{
			CreateNextGeneration(BestPerformingRagDoll);
		}

		SpawnDestination();

		foreach (GameObject ragDoll in Generation)
		{
			ragDoll.GetComponent<RagdollController>().WalkTarget = WalkingDestinationInstance;
		}
	}

	private void AssignAndMutateNeuralNetworkCopy(GameObject ragDoll)
	{
		RagdollController controller = ragDoll.GetComponent<RagdollController>();
		NeuralNetwork neuralNetwork = TrainingNeuralNetwork.GetClone();

		controller.IsTraining = true;
		controller.NeuralNetwork = neuralNetwork;
		controller.NeuralNetwork.Mutate(MutateStrength);
	}

	private void EndIteration()
	{
		BestPerformingRagDoll = GetBestPerformingRagDoll();

		TrainingNeuralNetwork = BestPerformingRagDoll.GetComponent<RagdollController>().NeuralNetwork;
		TrainingNeuralNetwork.Save(SaveFile);

		Destroy(WalkingDestinationInstance);

		StartIteration(false);
	}

	private void SpawnDestination()
	{
		WalkingDestinationInstance = Instantiate(WalkingDestinationPrefab, GetRandomWalkTargetPosition(StartPosition), Quaternion.identity);
	}

	public GameObject SpawnRagDollAndAddToGeneration(Vector3 position, Quaternion rotation)
	{
		GameObject newRagDoll = Instantiate(RagDollPrefabToTrain, position, rotation);

		if (Generation != null)
		{
			// Ignore collisions with existing ragdolls
			foreach (GameObject ragDoll in Generation)
			{
				IgnoreCollisions(ragDoll, newRagDoll);
			}
		}

		Generation.Add(newRagDoll);
		return newRagDoll;
	}

	private void IgnoreCollisions(GameObject ragdollA, GameObject ragdollB)
	{
		Collider[] collidersA = ragdollA.GetComponentsInChildren<Collider>();
		Collider[] collidersB = ragdollB.GetComponentsInChildren<Collider>();

		foreach (Collider colliderA in collidersA)
		{
			foreach (Collider colliderB in collidersB)
			{
				Physics.IgnoreCollision(colliderA, colliderB);
			}
		}
	}

	private GameObject GetBestPerformingRagDoll()
	{
		GameObject bestPerformingRagDoll = null;
		float bestScore = float.MinValue;

		foreach (GameObject ragDoll in Generation)
		{
			NeuralTrainer trainer = ragDoll.GetComponent<NeuralTrainer>();
			if (trainer)
			{
				float score = trainer.GetTotalScore();
				if (score > bestScore)
				{
					bestScore = score;
					bestPerformingRagDoll = ragDoll;
				}
			}
		}

		if (bestPerformingRagDoll != null)
		{
			bestPerformingRagDoll.GetComponent<NeuralTrainer>().IsBestPerformingRagDoll = true;
		}
		return bestPerformingRagDoll;
	}

	public List<GameObject> GetBestPerformingRagdolls(int count)
	{
		List<KeyValuePair<GameObject, float>> ragdollScores = new List<KeyValuePair<GameObject, float>>();

		foreach (GameObject ragDoll in Generation)
		{
			NeuralTrainer trainer = ragDoll.GetComponent<NeuralTrainer>();
			if (trainer)
			{
				float score = trainer.GetTotalScore();
				ragdollScores.Add(new KeyValuePair<GameObject, float>(ragDoll, score));
			}
		}

		ragdollScores.Sort((a, b) => b.Value.CompareTo(a.Value));

		List<GameObject> bestRagdolls = new List<GameObject>();
		int actualCount = Mathf.Min(count, ragdollScores.Count);
		for (int i = 0; i < actualCount; i++)
		{
			bestRagdolls.Add(ragdollScores[i].Key);
		}

		return bestRagdolls;
	}

	private void CreateNextGeneration(GameObject ragDoll)
	{
		ClearGeneration();

		RagdollController controller = ragDoll.GetComponent<RagdollController>();
		Quaternion randRotation = GetRandomRotation();
		for (int i = 0; i < GenerationSize; i++)
		{
			NeuralNetwork neuralNetworkCopy = controller.NeuralNetwork.GetClone();
			if (i != 0)
			{
				neuralNetworkCopy.Mutate(MutateStrength);
			}

			GameObject newRagDoll = SpawnRagDollAndAddToGeneration(StartPosition, randRotation);
			RagdollController newController = newRagDoll.GetComponent<RagdollController>();
			newController.IsTraining = true;
			newController.NeuralNetwork = neuralNetworkCopy;
		}
	}

	private void ClearGeneration()
	{
		foreach (GameObject ragDoll in Generation)
		{
			Destroy(ragDoll);
		}
		Generation.Clear();
	}

	private Vector3 GetRandomWalkTargetPosition(Vector3 startPoint)
	{
		const float minDistance = 5f;
		const float maxDistance = 25f;
		float distance = UnityEngine.Random.Range(minDistance, maxDistance);

		float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
		float x = Mathf.Cos(angle) * distance;
		float z = Mathf.Sin(angle) * distance;

		return new Vector3(startPoint.x + x, startPoint.y, startPoint.z + z);
	}

	public float GetIterationTime()
	{
		return IterationTime;
	}

	private Quaternion GetRandomRotation()
	{
		// Generate a random rotation around the Y axis (upright)
		float yAngle = UnityEngine.Random.Range(0f, 360f);
		return Quaternion.Euler(0f, yAngle, 0f);
	}
}
