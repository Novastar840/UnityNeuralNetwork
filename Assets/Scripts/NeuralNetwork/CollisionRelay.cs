using UnityEngine;

public class CollisionRelay : MonoBehaviour
{
    private RagdollController ParentController;

    private void Start()
    {
        ParentController = GetComponentInParent<RagdollController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        ParentController?.CollisionEnter(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        ParentController?.CollisionExit(collision);
    }
}
