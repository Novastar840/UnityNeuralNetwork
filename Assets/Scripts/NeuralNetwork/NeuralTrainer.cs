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

	private float CumulativePostureScore;
	private int PostureSampleCount;

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
			if (FeetOnGround)
			{
				UpdatePostureScore();
			}
		}
	}

	private void UpdatePostureScore()
	{
		float bodyAngle = RagDollController.GetBodyAngleFromLevel();
		float postureFactor = Mathf.Max(0f, 1f - (bodyAngle / 90f));
		CumulativePostureScore += postureFactor;
		PostureSampleCount++;
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

		float averagePostureScore = 1f;
		if (PostureSampleCount > 0)
		{
			averagePostureScore = CumulativePostureScore / PostureSampleCount;
		}
		
		score *= averagePostureScore;
		
		// Distance is irrelevant for now
		// float distanceToTarget = Vector3.Distance(
		// 	RagDollController.GetBodyPosition(),
		// 	RagDollController.WalkTarget.transform.position);
		//
		// float distanceScoreFactor = 0.1f;
		// float distanceScore = Mathf.Max(0f, (30f - distanceToTarget) * distanceScoreFactor);
		//
		// return score + distanceScore;
		return score;
	}

	private void StartIteration()
	{
	}

	public float GetTotalScore()
	{
		return CalculateScore();
	}
}
