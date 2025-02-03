using System;
using UnityEngine;

public class NeuralTrainer : MonoBehaviour
{
    private float StandingTimer;
    private RagdollController RagdollController;
    public bool FeetOnGround = false;
    public bool Fallen = false;

    private void Awake()
    {
        RagdollController = GetComponent<RagdollController>();
    }

    private void Update()
    {
        if (!Fallen)
        {
            StandingTimer += Time.deltaTime;
        }
    }
}
