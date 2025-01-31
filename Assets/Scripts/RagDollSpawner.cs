using System.Collections.Generic;
using UnityEngine;

public class RagDollSpawner : MonoBehaviour
{
    [SerializeField] private GameObject RagDollPrefab;
    private readonly List<GameObject> SpawnedRagDolls = new List<GameObject>();

    public void SpawnRagdoll(Vector3 position)
    {
        GameObject newRagdoll = Instantiate(RagDollPrefab, position, Quaternion.identity);

        // Ignore collisions with existing ragdolls
        foreach (GameObject ragDoll in SpawnedRagDolls)
        {
            IgnoreCollisions(ragDoll, newRagdoll);
        }

        SpawnedRagDolls.Add(newRagdoll);
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
}
