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

	private float TotalScore;

	[HideInInspector]
	public bool IsBestPerformingRagDoll = false;

	private void Awake()
	{
		RagDollController = GetComponent<RagdollController>();
		RagDollController.OnRagDollStatusUpdate += OnRagDollStatusUpdate;
	}

	private void Start()
	{
		if (RagDollController.IsTraining)
		{
			NeuralTrainerManager.Singleton.OnIterationStartDelegate += StartIteration;
		}
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
		float score = StandingTimer;
		if (Fallen)
		{
			score *= 0.5f;
		}
		return score;

		//--FINAL TRAINING SCORE--
		// float distanceToTarget = Vector3.Distance(
		// 	RagDollController.GetBodyPosition(),
		// 	RagDollController.WalkTarget.transform.position);
		//
		// // 1. Weight distance higher so movement matters
		// float distanceScore = Mathf.Max(0f, (30f - distanceToTarget) * 0.5f);
		//
		// // 2. Keep standing bonus, but cap it or reduce its weight
		// float standingBonus = Fallen ? 0f : Mathf.Min(StandingTimer, 5f);
		//
		// // 3. Optional: Penalize falling to discourage early collapse
		// float fallPenalty = Fallen ? -5f : 0f;
		//
		// return distanceScore + standingBonus + fallPenalty;
	}

	private void StartIteration()
	{
	}

	public float GetTotalScore()
	{
		return CalculateScore();
	}
}
