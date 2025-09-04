using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Unit : MonoBehaviour
{
    private List<Vector3> path = new();
    private int currentPathIndex = 0;
    private Vector3 currentDestination = Vector3.zero;
    private bool hasDestination = false;
    NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    public void SetDestination(Vector3 destination)
    {
        currentDestination = destination;
        hasDestination = true;
        print($"New destination set: {destination}");
    }

    private void RecalculatePath()
    {
        if (!hasDestination) return;
        
        Vector3 currentGridPos = new(Mathf.RoundToInt(transform.position.x), transform.position.y, Mathf.RoundToInt(transform.position.z));
        Vector3 destinationGridPos = new(Mathf.RoundToInt(currentDestination.x), currentDestination.y, Mathf.RoundToInt(currentDestination.z));
        
        if (Mathf.RoundToInt(currentGridPos.x) == Mathf.RoundToInt(destinationGridPos.x) && 
            Mathf.RoundToInt(currentGridPos.z) == Mathf.RoundToInt(destinationGridPos.z))
        {
            path.Clear();
            currentPathIndex = 0;
            hasDestination = false;
            print("Reached destination!");
            return;
        }
        
        NavMeshPath navPath = new NavMeshPath();
        if (agent.CalculatePath(currentDestination, navPath))
        {
            path = ConvertNavMeshPathToGridPath(navPath.corners);
            currentPathIndex = 0;
            
            if (path.Count > 0)
            {
                Vector3 firstPathPoint = path[0];
                if (Mathf.RoundToInt(firstPathPoint.x) == Mathf.RoundToInt(currentGridPos.x) && 
                    Mathf.RoundToInt(firstPathPoint.z) == Mathf.RoundToInt(currentGridPos.z))
                {
                    path.RemoveAt(0);
                }
            }
            
            print($"Path recalculated with {path.Count} waypoints");
        }
        else
        {
            path.Clear();
            currentPathIndex = 0;
            print("Failed to calculate path to destination");
        }
    }
    
    public void MoveAlongPath()
    {
        print("Tick! From Unit");
        
        agent.Warp(transform.position);
        
        RecalculatePath();
        
        if (path.Count > 0 && currentPathIndex >= 0 && currentPathIndex < path.Count)
        {
            Vector3 nextPoint = path[currentPathIndex];
            transform.position = nextPoint;
            GameManager.instance.TriggerOnPathNextTile(nextPoint);
            currentPathIndex++;
        }
    }

    private List<Vector3> ConvertNavMeshPathToGridPath(Vector3[] navMeshCorners)
    {
        List<Vector3> gridPath = new List<Vector3>();
        
        if (navMeshCorners.Length == 0) return gridPath;
        
        Vector3[] corners = new Vector3[navMeshCorners.Length];
        for (int i = 0; i < navMeshCorners.Length; i++)
        {
            corners[i] = new Vector3(
                Mathf.RoundToInt(navMeshCorners[i].x),
                navMeshCorners[i].y,
                Mathf.RoundToInt(navMeshCorners[i].z)
            );
        }
        
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[i + 1];
            
            if (Mathf.RoundToInt(start.x) == Mathf.RoundToInt(end.x) && 
                Mathf.RoundToInt(start.z) == Mathf.RoundToInt(end.z)) continue;
            
            List<Vector3> segmentPath = GenerateGridPath(start, end);
            
            foreach (Vector3 point in segmentPath)
            {
                if (gridPath.Count == 0 || 
                    Mathf.RoundToInt(gridPath[gridPath.Count - 1].x) != Mathf.RoundToInt(point.x) ||
                    Mathf.RoundToInt(gridPath[gridPath.Count - 1].z) != Mathf.RoundToInt(point.z))
                {
                    gridPath.Add(point);
                }
            }
        }
        
        if (corners.Length > 0)
        {
            Vector3 finalPoint = corners[corners.Length - 1];
            if (gridPath.Count == 0 || 
                Mathf.RoundToInt(gridPath[gridPath.Count - 1].x) != Mathf.RoundToInt(finalPoint.x) ||
                Mathf.RoundToInt(gridPath[gridPath.Count - 1].z) != Mathf.RoundToInt(finalPoint.z))
            {
                gridPath.Add(finalPoint);
            }
        }
        
        return gridPath;
    }

    private List<Vector3> GenerateGridPath(Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 current = start;
        
        int currentX = Mathf.RoundToInt(current.x);
        int currentZ = Mathf.RoundToInt(current.z);
        int endX = Mathf.RoundToInt(end.x);
        int endZ = Mathf.RoundToInt(end.z);
        
        while (currentX != endX || currentZ != endZ)
        {
            int diffX = endX - currentX;
            int diffZ = endZ - currentZ;
            
            int stepX = diffX != 0 ? (diffX > 0 ? 1 : -1) : 0;
            int stepZ = diffZ != 0 ? (diffZ > 0 ? 1 : -1) : 0;
            
            currentX += stepX;
            currentZ += stepZ;
            
            NavMeshHit hit;
            Vector3 samplePosition = new Vector3(currentX, end.y, currentZ);
            if (NavMesh.SamplePosition(samplePosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                path.Add(new Vector3(currentX, hit.position.y, currentZ));
            }
            else
            {
                float progress = Vector2.Distance(new Vector2(currentX, currentZ), new Vector2(start.x, start.z)) / 
                               Vector2.Distance(new Vector2(end.x, end.z), new Vector2(start.x, start.z));
                float interpolatedY = Mathf.Lerp(start.y, end.y, progress);
                path.Add(new Vector3(currentX, interpolatedY, currentZ));
            }
            
            if (path.Count > 1000) break;
        }
        
        return path;
    }

    public void ClearPath()
    {
        path.Clear();
        currentPathIndex = 0;
        hasDestination = false;
        print("Path cleared");
    }

    void OnDrawGizmos()
    {
        if (path != null && path.Count > 1)
        {
            Gizmos.color = Color.red;
            
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 start = path[i];
                Vector3 end = path[i + 1];
                Gizmos.DrawLine(start, end);
            }
            
            Gizmos.color = Color.yellow;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 point = path[i];
                Gizmos.DrawWireSphere(point, 0.1f);
            }
            
            if (currentPathIndex < path.Count)
            {
                Gizmos.color = Color.green;
                Vector3 currentTarget = path[currentPathIndex];
                Gizmos.DrawWireSphere(currentTarget, 0.2f);
            }
        }
    }
}