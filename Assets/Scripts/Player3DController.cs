using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Player3DController : MonoBehaviour
{
    NavMeshAgent agent;
    void Start()
    {
        GameManager.instance.OnPathNextTile += HandleNextTile;
        agent = GetComponent<NavMeshAgent>();
    }

    private void HandleNextTile(Vector3 tile)
    {
        agent.SetDestination(tile);
    }
}