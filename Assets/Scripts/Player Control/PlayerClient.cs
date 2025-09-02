using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerClient : MonoBehaviour
{
    Vector3 hoveredLocation = Vector3.zero;
    public GameObject hoveredLocationCubePrefab;
    GameObject hoveredLocationCube;
    NavMeshAgent agent;
    private List<Vector3Int> path = new List<Vector3Int>();
    private int currentPathIndex = 0;

    void Start()
    {
        GameManager.instance.OnTick += HandleTick;
        hoveredLocationCube = Instantiate(hoveredLocationCubePrefab, hoveredLocation, Quaternion.identity);
        agent = GetComponent<NavMeshAgent>();
        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            hoveredLocation = hit.point;
        }
        hoveredLocation = new Vector3(Mathf.Round(hoveredLocation.x), 0, Mathf.Round(hoveredLocation.z));
        hoveredLocationCube.transform.position = hoveredLocation;

        if (Input.GetMouseButtonDown(0))
        {
            RequestNewPath(hoveredLocation);
        }
    }
    
    private void RequestNewPath(Vector3 destination)
    {
        Vector3Int currentGridPos = new Vector3Int(Mathf.RoundToInt(transform.position.x), 0, Mathf.RoundToInt(transform.position.z));
        Vector3Int destinationGridPos = new Vector3Int(Mathf.RoundToInt(destination.x), 0, Mathf.RoundToInt(destination.z));
        
        if (currentGridPos == destinationGridPos)
        {
            path.Clear();
            currentPathIndex = 0;
            return;
        }
        
        agent.Warp(transform.position);
        
        NavMeshPath navPath = new NavMeshPath();
        if (agent.CalculatePath(destination, navPath))
        {
            path = ConvertNavMeshPathToGridPath(navPath.corners);
            currentPathIndex = 0;
            
            if (path.Count > 0)
            {
                if (path[0] == currentGridPos)
                {
                    path.RemoveAt(0);
                }
            }
        }
        else
        {
            path.Clear();
            currentPathIndex = 0;
        }
    }
    
    private void HandleTick()
    {
        print("Tick! From PlayerClient");
        
        agent.Warp(transform.position);
        
        if (path.Count > 0 && currentPathIndex >= 0 && currentPathIndex < path.Count)
        {
            Vector3 nextPoint = path[currentPathIndex];
            transform.position = nextPoint;
            currentPathIndex++;
            
            print($"Moving to waypoint {currentPathIndex - 1}: {nextPoint}, remaining waypoints: {path.Count - currentPathIndex}");
        }
    }

    private List<Vector3Int> ConvertNavMeshPathToGridPath(Vector3[] navMeshCorners)
    {
        List<Vector3Int> gridPath = new List<Vector3Int>();
        
        if (navMeshCorners.Length == 0) return gridPath;
        
        Vector3Int[] corners = new Vector3Int[navMeshCorners.Length];
        for (int i = 0; i < navMeshCorners.Length; i++)
        {
            corners[i] = new Vector3Int(
                Mathf.RoundToInt(navMeshCorners[i].x),
                0,
                Mathf.RoundToInt(navMeshCorners[i].z)
            );
        }
        
        // Generate step-by-step path between corners
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3Int start = corners[i];
            Vector3Int end = corners[i + 1];
            
            // Skip if start and end are the same
            if (start == end) continue;
            
            // Add intermediate steps from start to end (excluding the start point to avoid duplicates)
            List<Vector3Int> segmentPath = GenerateGridPath(start, end);
            
            // Add segment path, avoiding duplicates
            foreach (Vector3Int point in segmentPath)
            {
                if (gridPath.Count == 0 || gridPath[gridPath.Count - 1] != point)
                {
                    gridPath.Add(point);
                }
            }
        }
        
        // Add the final destination if it's not already there
        if (corners.Length > 0)
        {
            Vector3Int finalPoint = corners[corners.Length - 1];
            if (gridPath.Count == 0 || gridPath[gridPath.Count - 1] != finalPoint)
            {
                gridPath.Add(finalPoint);
            }
        }
        
        return gridPath;
    }

    private List<Vector3Int> GenerateGridPath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = start;
        
        while (current != end)
        {
            // Move one step at a time towards the target
            Vector3Int diff = end - current;
            
            // Prioritize moving diagonally first, then cardinal directions
            int stepX = diff.x != 0 ? (diff.x > 0 ? 1 : -1) : 0;
            int stepZ = diff.z != 0 ? (diff.z > 0 ? 1 : -1) : 0;
            
            current = new Vector3Int(current.x + stepX, 0, current.z + stepZ);
            path.Add(current);
            
            // Safety check to prevent infinite loops
            if (path.Count > 1000) break;
        }
        
        return path;
    }

    private List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int current = start;
        while (current != end)
        {
            Vector3Int next = GetNextStep(current, end);
            if (next == current) break; // can't move
            path.Add(next);
            current = next;
        }
        return path;
    }

    private Vector3Int GetNextStep(Vector3Int current, Vector3Int end)
    {
        Vector3Int diff = end - current;
        int dx = diff.x > 0 ? 1 : diff.x < 0 ? -1 : 0;
        int dz = diff.z > 0 ? 1 : diff.z < 0 ? -1 : 0;
        return new Vector3Int(current.x + dx, 0, current.z + dz);
    }

    void OnDrawGizmos()
    {
        if (path != null && path.Count > 1)
        {
            Gizmos.color = Color.red;
            
            // Draw lines between each point in the path
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 start = new Vector3(path[i].x, path[i].y, path[i].z);
                Vector3 end = new Vector3(path[i + 1].x, path[i + 1].y, path[i + 1].z);
                Gizmos.DrawLine(start, end);
            }
            
            // Draw spheres at each waypoint
            Gizmos.color = Color.yellow;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 point = new Vector3(path[i].x, path[i].y, path[i].z);
                Gizmos.DrawWireSphere(point, 0.1f);
            }
            
            // Highlight current target point
            if (currentPathIndex < path.Count)
            {
                Gizmos.color = Color.green;
                Vector3 currentTarget = new Vector3(path[currentPathIndex].x, path[currentPathIndex].y, path[currentPathIndex].z);
                Gizmos.DrawWireSphere(currentTarget, 0.2f);
            }
        }
    }
}
