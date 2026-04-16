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

	//private GameObject BestPerformingRagDoll = null;
	private GameObject[] BestPerformingRagdolls = null;

	private NeuralNetwork TrainingNeuralNetwork;

	public delegate void IterationDelegate();

	public IterationDelegate OnIterationStartDelegate;

	private NeuralNetworkSave SaveFile;

	// Multithreading support
	private System.Threading.Tasks.Task<NeuralNetworkParallelProcessor.NetworkProcessingResult[]> PendingProcessingTask;
	private List<NeuralNetworkParallelProcessor.NetworkProcessingResult> CurrentGenerationResults;

	private void Awake()
	{
		Application.runInBackground = true;
	}

	private void Start()
	{
		InitAndStartIteration();
	}

	public void InitAndStartIteration()
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

			ProcessGenerationInParallel();
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
				if (i == 0)
				{
					var controller  = ragDoll.GetComponent<RagdollController>();
					
					controller.NeuralNetwork = TrainingNeuralNetwork.GetClone();
					controller.IsTraining = true;
				}
				else
				{
					AssignAndMutateNeuralNetworkCopy(ragDoll);
				}
			}
		}
		else
		{
			CreateNextGeneration(BestPerformingRagdolls);
		}

		SpawnDestination();

		foreach (GameObject ragDoll in Generation)
		{
			ragDoll.GetComponent<RagdollController>().WalkTarget = WalkingDestinationInstance;
		}

		InitializeParallelProcessing();
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
		BestPerformingRagdolls = GetBestPerformingRagdolls(GenerationCarryOverCount).ToArray();

		TrainingNeuralNetwork = BestPerformingRagdolls[0].GetComponent<RagdollController>().NeuralNetwork;
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

	//private GameObject GetBestPerformingRagDoll()
	//{
	//	GameObject bestPerformingRagDoll = null;
	//	float bestScore = float.MinValue;

	//	foreach (GameObject ragDoll in Generation)
	//	{
	//		NeuralTrainer trainer = ragDoll.GetComponent<NeuralTrainer>();
	//		if (trainer)
	//		{
	//			float score = trainer.GetTotalScore();
	//			if (score > bestScore)
	//			{
	//				bestScore = score;
	//				bestPerformingRagDoll = ragDoll;
	//			}
	//		}
	//	}

	//	if (bestPerformingRagDoll != null)
	//	{
	//		bestPerformingRagDoll.GetComponent<NeuralTrainer>().IsBestPerformingRagDoll = true;
	//	}
	//	return bestPerformingRagDoll;
	//}

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

	//private void CreateNextGeneration(GameObject ragDoll)
	//{
	//	ClearGeneration();

	//	RagdollController controller = ragDoll.GetComponent<RagdollController>();
	//	Quaternion randRotation = GetRandomRotation();
	//	for (int i = 0; i < GenerationSize; i++)
	//	{
	//		NeuralNetwork neuralNetworkCopy = controller.NeuralNetwork.GetClone();
	//		if (i != 0)
	//		{
	//			neuralNetworkCopy.Mutate(MutateStrength);
	//		}

	//		GameObject newRagDoll = SpawnRagDollAndAddToGeneration(StartPosition, randRotation);
	//		RagdollController newController = newRagDoll.GetComponent<RagdollController>();
	//		newController.IsTraining = true;
	//		newController.NeuralNetwork = neuralNetworkCopy;
	//	}
	//}

	private void CreateNextGeneration(GameObject[] ragDolls)
	{
		ClearGeneration();
		Quaternion randRotation = GetRandomRotation();
		foreach (GameObject ragDoll in ragDolls)
		{
			RagdollController controller = ragDoll.GetComponent<RagdollController>();
			for (int i = 0; i < (GenerationSize / ragDolls.Length) - 1; i++)
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

			GameObject carryOverRagdoll = SpawnRagDollAndAddToGeneration(StartPosition, randRotation);
			RagdollController caryOverController = ragDoll.GetComponent<RagdollController>();
			caryOverController.IsTraining = true;
			caryOverController.NeuralNetwork = controller.NeuralNetwork;
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

	/// <summary>
	/// Initialize data structures for parallel processing
	/// </summary>
	private void InitializeParallelProcessing()
	{
		CurrentGenerationResults = new List<NeuralNetworkParallelProcessor.NetworkProcessingResult>();
		PendingProcessingTask = null;
	}

	/// <summary>
	/// Process all ragdoll neural networks in parallel using multiple CPU cores
	/// </summary>
	private void ProcessGenerationInParallel()
	{
		// Check if previous task is complete and apply results
		if (PendingProcessingTask != null && PendingProcessingTask.IsCompleted)
		{
			ApplyParallelProcessingResults(PendingProcessingTask.Result);
			PendingProcessingTask = null;
		}

		// Only start new task if no pending task and we have generation to process
		if (PendingProcessingTask == null && Generation.Count > 0)
		{
			var processingInputs = new List<NeuralNetworkParallelProcessor.NetworkProcessingResult>();

			foreach (GameObject ragdoll in Generation)
			{
				RagdollController controller = ragdoll.GetComponent<RagdollController>();

				if (controller != null && controller.NeuralNetwork != null)
				{
					float[] inputs = controller.PrepareInputsForProcessing();
					processingInputs.Add(new NeuralNetworkParallelProcessor.NetworkProcessingResult
					{
						NeuralNetwork = controller.NeuralNetwork,
						Inputs = inputs,
						RagdollID = ragdoll.GetInstanceID()
					});
				}
			}

			if (processingInputs.Count > 0)
			{
				// Start parallel processing on background threads
				var inputData = processingInputs.ToArray();
				PendingProcessingTask = NeuralNetworkParallelProcessor.ProcessNetworksInParallel(inputData);
			}
		}
	}

	/// <summary>
	/// Apply the results from parallel processing back to ragdolls
	/// </summary>
	private void ApplyParallelProcessingResults(NeuralNetworkParallelProcessor.NetworkProcessingResult[] results)
	{
		foreach (var result in results)
		{
			// Find the corresponding ragdoll by instance ID
			foreach (GameObject ragDoll in Generation)
			{
				if (ragDoll.GetInstanceID() == result.RagdollID)
				{
					RagdollController controller = ragDoll.GetComponent<RagdollController>();
					if (controller != null)
					{
						controller.ApplyNetworkOutputs(result.Outputs);
					}
					break;
				}
			}
		}
	}
}
