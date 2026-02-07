using System;
using UnityEngine;
using UnityEngine.Serialization;

public class NeuralTrainer : MonoBehaviour
{
	private float StandingTimer;
	private RagdollController RagDollController;
	public bool FeetOnGround = false;
	public bool Fallen = false;

	private NeuralNetwork NeuralNetwork;

	[SerializeField]
	private float StandingTimeImpactScalar = 1;

	[SerializeField]
	private float DistanceToObjectiveImpactScalar = 1;

	private float TotalScore;

	[SerializeField]
	private int GenerationSize = 10;

	[HideInInspector]
	public bool IsBestPerformingRagDoll = false;

	private void Awake()
	{
		RagDollController = GetComponent<RagdollController>();
		RagDollController.OnRagDollStatusUpdate += OnRagDollStatusUpdate;
	}

	private void Start()
	{
		NeuralTrainerManager.Singleton.OnIterationStartDelegate += StartIteration;
	}

	private void Update()
	{
		if (!Fallen)
		{
			StandingTimer += Time.deltaTime;
		}
	}

	private void OnRagDollStatusUpdate()
	{

	}

	private float CalculateScore()
	{
		float distanceScore = 1f / (1f + Vector3.Distance(
			RagDollController.GetBodyPosition(),
			RagDollController.WalkTarget.transform.position));

		float standingBonus = Fallen ? 0f : StandingTimer;

		return distanceScore + standingBonus;
	}

	private void StartIteration()
	{
	}

	public float GetTotalScore()
	{
		return CalculateScore();
	}
}
