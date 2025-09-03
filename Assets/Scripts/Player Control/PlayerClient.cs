using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerClient : MonoBehaviour
{
    Vector3 hoveredLocation = Vector3.zero;
    public GameObject hoveredLocationCubePrefab;
    GameObject hoveredLocationCube;
    NavMeshAgent agent;
    private List<Vector3> path = new List<Vector3>();
    private int currentPathIndex = 0;
    
    [Header("Player Positioning")]
    public float yOffset = 0.1f;

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
        hoveredLocation = new Vector3(Mathf.Round(hoveredLocation.x), Mathf.Round(hoveredLocation.y), Mathf.Round(hoveredLocation.z));
        hoveredLocationCube.transform.position = hoveredLocation;

        if (Input.GetMouseButtonDown(0))
        {
            RequestNewPath(hoveredLocation);
        }
    }
    
    private void RequestNewPath(Vector3 destination)
    {
        Vector3 currentGridPos = new Vector3(Mathf.RoundToInt(transform.position.x), transform.position.y, Mathf.RoundToInt(transform.position.z));
        Vector3 destinationGridPos = new Vector3(Mathf.RoundToInt(destination.x), destination.y, Mathf.RoundToInt(destination.z));
        
        // Compare only X and Z for grid position equality
        if (Mathf.RoundToInt(currentGridPos.x) == Mathf.RoundToInt(destinationGridPos.x) && 
            Mathf.RoundToInt(currentGridPos.z) == Mathf.RoundToInt(destinationGridPos.z))
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
                // Compare only X and Z coordinates for grid position equality
                Vector3 firstPathPoint = path[0];
                if (Mathf.RoundToInt(firstPathPoint.x) == Mathf.RoundToInt(currentGridPos.x) && 
                    Mathf.RoundToInt(firstPathPoint.z) == Mathf.RoundToInt(currentGridPos.z))
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
            nextPoint.y += yOffset;
            transform.position = nextPoint;
            currentPathIndex++;
            
            print($"Moving to waypoint {currentPathIndex - 1}: {nextPoint}, remaining waypoints: {path.Count - currentPathIndex}");
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
                navMeshCorners[i].y, // Keep original Y height from NavMesh
                Mathf.RoundToInt(navMeshCorners[i].z)
            );
        }
        
        // Generate step-by-step path between corners
        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[i + 1];
            
            // Skip if start and end are the same (in X and Z)
            if (Mathf.RoundToInt(start.x) == Mathf.RoundToInt(end.x) && 
                Mathf.RoundToInt(start.z) == Mathf.RoundToInt(end.z)) continue;
            
            // Add intermediate steps from start to end (excluding the start point to avoid duplicates)
            List<Vector3> segmentPath = GenerateGridPath(start, end);
            
            // Add segment path, avoiding duplicates
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
        
        // Add the final destination if it's not already there
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
        
        // Convert to grid coordinates for pathfinding
        int currentX = Mathf.RoundToInt(current.x);
        int currentZ = Mathf.RoundToInt(current.z);
        int endX = Mathf.RoundToInt(end.x);
        int endZ = Mathf.RoundToInt(end.z);
        
        while (currentX != endX || currentZ != endZ)
        {
            // Move one step at a time towards the target
            int diffX = endX - currentX;
            int diffZ = endZ - currentZ;
            
            // Prioritize moving diagonally first, then cardinal directions
            int stepX = diffX != 0 ? (diffX > 0 ? 1 : -1) : 0;
            int stepZ = diffZ != 0 ? (diffZ > 0 ? 1 : -1) : 0;
            
            currentX += stepX;
            currentZ += stepZ;
            
            // Sample the NavMesh to get the correct Y position for this grid point
            NavMeshHit hit;
            Vector3 samplePosition = new Vector3(currentX, end.y, currentZ);
            if (NavMesh.SamplePosition(samplePosition, out hit, 1.0f, NavMesh.AllAreas))
            {
                path.Add(new Vector3(currentX, hit.position.y, currentZ));
            }
            else
            {
                // Fallback to interpolated Y if NavMesh sampling fails
                float progress = Vector2.Distance(new Vector2(currentX, currentZ), new Vector2(start.x, start.z)) / 
                               Vector2.Distance(new Vector2(end.x, end.z), new Vector2(start.x, start.z));
                float interpolatedY = Mathf.Lerp(start.y, end.y, progress);
                path.Add(new Vector3(currentX, interpolatedY, currentZ));
            }
            
            // Safety check to prevent infinite loops
            if (path.Count > 1000) break;
        }
        
        return path;
    }

    private List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        List<Vector3> path = new List<Vector3>();
        Vector3 current = start;
        
        int currentX = Mathf.RoundToInt(current.x);
        int currentZ = Mathf.RoundToInt(current.z);
        int endX = Mathf.RoundToInt(end.x);
        int endZ = Mathf.RoundToInt(end.z);
        
        while (currentX != endX || currentZ != endZ)
        {
            Vector3 next = GetNextStep(current, end);
            if (Mathf.RoundToInt(next.x) == currentX && Mathf.RoundToInt(next.z) == currentZ) break; // can't move
            path.Add(next);
            current = next;
            currentX = Mathf.RoundToInt(current.x);
            currentZ = Mathf.RoundToInt(current.z);
        }
        return path;
    }

    private Vector3 GetNextStep(Vector3 current, Vector3 end)
    {
        int currentX = Mathf.RoundToInt(current.x);
        int currentZ = Mathf.RoundToInt(current.z);
        int endX = Mathf.RoundToInt(end.x);
        int endZ = Mathf.RoundToInt(end.z);
        
        int diffX = endX - currentX;
        int diffZ = endZ - currentZ;
        int dx = diffX > 0 ? 1 : diffX < 0 ? -1 : 0;
        int dz = diffZ > 0 ? 1 : diffZ < 0 ? -1 : 0;
        
        int nextX = currentX + dx;
        int nextZ = currentZ + dz;
        
        // Sample NavMesh for correct Y position
        NavMeshHit hit;
        Vector3 samplePosition = new Vector3(nextX, end.y, nextZ);
        if (NavMesh.SamplePosition(samplePosition, out hit, 1.0f, NavMesh.AllAreas))
        {
            return new Vector3(nextX, hit.position.y, nextZ);
        }
        else
        {
            // Fallback to current Y if sampling fails
            return new Vector3(nextX, current.y, nextZ);
        }
    }

    void OnGUI()
    {
        Vector3 worldPosition = transform.position + Vector3.up * 2f; // 2 units above player
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        if (screenPosition.z > 0)
        {
            string labelText = $"{transform.position.x:F2}, {transform.position.z:F2}";
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(labelText));
            float padding = 16f;
            float width = size.x + padding;
            float height = size.y + padding / 2;
            float x = screenPosition.x - width / 2;
            float y = Screen.height - screenPosition.y - height / 2;

            GUI.Box(new Rect(x, y, width, height), labelText);
        }
    }

    void OnDrawGizmos()
    {
        if (path != null && path.Count > 1)
        {
            Gizmos.color = Color.red;
            
            // Draw lines between each point in the path
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 start = path[i];
                Vector3 end = path[i + 1];
                Gizmos.DrawLine(start, end);
            }
            
            // Draw spheres at each waypoint
            Gizmos.color = Color.yellow;
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 point = path[i];
                Gizmos.DrawWireSphere(point, 0.1f);
            }
            
            // Highlight current target point
            if (currentPathIndex < path.Count)
            {
                Gizmos.color = Color.green;
                Vector3 currentTarget = path[currentPathIndex];
                Gizmos.DrawWireSphere(currentTarget, 0.2f);
            }
        }
    }
}
