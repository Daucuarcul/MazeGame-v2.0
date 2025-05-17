using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

public class HeroMovement : MonoBehaviour
{
    public float moveSpeed = 3.0f;
    public float detectionRadius = 2.5f;
    public float rotationSpeed = 720.0f;


    // Threshold for determining if hero has reached the target tile
    private const float ARRIVAL_THRESHOLD = 0.2f; // toleranță mai mare pentru viteze mici


    private List<Vector2Int> pathToExit;
    private List<BranchPath> branchPaths = new List<BranchPath>();
    private int currentPathIndex = 0;

    private bool isExploringBranch = false;
    private bool returningFromBranch = false;
    private bool isTileTransitionComplete = true; // Flag to ensure tile transitions are complete before state changes

    private Queue<Vector2Int> pathToExplore;   // drumul pe care merge Hero în ramură
    private Vector2Int returnPoint = new Vector2Int(-1, -1);  // tile-ul unde se va întoarce după ce adună PowerUp
    private HashSet<Vector2Int> collectedItems = new HashSet<Vector2Int>(); // Track collected items

    private BranchPath activeBranch;


    private void Start()
    {
        Debug.Log($"💨 moveSpeed in Start = {moveSpeed}");

        if (pathToExit != null && pathToExit.Count > 0)
        {
            Vector2Int startTile = pathToExit[0];
            transform.position = new Vector3(startTile.x, transform.position.y, startTile.y);
            currentPathIndex = 0;
        }
    }

    public void SetPathToExit(List<Vector2Int> path)
    {
        pathToExit = path;

        if (path != null && path.Count > 0)
        {
            Vector2Int start = path[0];
            Vector3 heroPos = transform.position;
            Vector3 targetPos = new Vector3(start.x, heroPos.y, start.y);

            float dist = Vector3.Distance(heroPos, targetPos);
            transform.position = targetPos;
            Debug.Log($"📌 Hero snapped at start to exact tile {targetPos}, dist={dist}");

            if (dist < ARRIVAL_THRESHOLD)
            {
                Debug.Log("⚠️ Hero already at first path tile — skipping it");
                currentPathIndex = 1; // skip current tile
            }
            else
            {
                currentPathIndex = 0;
            }

            isTileTransitionComplete = true;
            Debug.Log($"🧭 Hero positioned at start tile: {start}");
        }
        else
        {
            currentPathIndex = 0;
            Debug.LogWarning("⚠️ pathToExit is null or empty – Hero cannot start moving.");
        }
    }


    public void SetBranchPaths(List<BranchPath> branches)
    {
        branchPaths = branches;
    }
    public void AssignBranchIndicesByPath()
    {
        if (pathToExit == null || pathToExit.Count == 0 || branchPaths == null || branchPaths.Count == 0)
        {
            Debug.LogWarning("⚠️ Nu se poate face numerotarea ramurilor – lipsește pathToExit sau branchPaths.");
            return;
        }

        for (int i = 0; i < pathToExit.Count; i++)
        {
            Vector2Int tile = pathToExit[i];

            foreach (BranchPath branch in branchPaths)
            {
                if (branch.indexOnPath >= 0) continue; // deja numerotată

                if (IsNeighbor(branch.entryPoint, tile))
                {
                    branch.indexOnPath = i;
                    Debug.Log($"🔢 Ramura de la {branch.entryPoint} primeste index {i} (vecina cu tile {tile})");
                }
            }
        }

        // Sortăm ramurile în ordinea în care apar pe pathToExit
        branchPaths = branchPaths
            .Where(b => b.indexOnPath >= 0)
            .OrderBy(b => b.indexOnPath)
            .ToList();

        Debug.Log($"✅ S-au numerotat și sortat {branchPaths.Count} ramuri după pathToExit.");
    }

    public int Debug_GetBranchCount()
    {
        return branchPaths != null ? branchPaths.Count : -1;
    }

    private void FixedUpdate()
    {
        Debug.Log($"🚶 Hero fixed update: index={currentPathIndex}, pathCount={pathToExit?.Count}, position={transform.position}");

        Vector2Int heroGridPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        // Handle branch exploration
        if (isExploringBranch && pathToExplore != null && pathToExplore.Count > 0)
        {
            HandleBranchExploration();
        }
        // Handle main path movement only when not in branch and transition is complete
        else if (!isExploringBranch && isTileTransitionComplete)
        {
            // Check for nearby entry points only if we've fully arrived at our current tile
            if (IsFullyOnTile(heroGridPos))
            {
                CheckForNearbyEntryPoints();
            }

            // Continue on main path if not exploring branch
            Debug.Log("🟩 Attempting MoveTowards from FixedUpdate");

            if (!isExploringBranch && currentPathIndex < pathToExit.Count)
            {
                Vector2Int target = pathToExit[currentPathIndex];
                MoveTowards(target, () =>
                {
                    currentPathIndex++;
                    isTileTransitionComplete = true;

                    // If reached the end of the main path
                    if (currentPathIndex >= pathToExit.Count)
                    {
                        Debug.Log("🏁 Hero a ajuns la finalul pathToExit. Pornim ramurile.");
                        ExploreNearestBranch();
                    }
                });
            }
        }
        UpdateMovement();

    }

    // Check if the hero is fully on a tile (to prevent diagonal entry)
    private bool IsFullyOnTile(Vector2Int tilePos)
    {
        Vector3 tileWorldPos = new Vector3(tilePos.x, transform.position.y, tilePos.y);
        return Vector3.Distance(transform.position, tileWorldPos) < ARRIVAL_THRESHOLD;
    }

    private void HandleBranchExploration()
    {
        Vector2Int nextTile = pathToExplore.Peek();
        Vector3 targetPos = new Vector3(nextTile.x, transform.position.y, nextTile.y);

        // Move towards the next tile in the branch path
        float step = moveSpeed * Time.fixedDeltaTime;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, step);

        // Rotate towards the movement direction
        Vector3 dir = targetPos - transform.position;
        if (dir.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // Check if arrived at the target tile
        float distance = Vector3.Distance(transform.position, targetPos);
        if (distance <= ARRIVAL_THRESHOLD)

        {
            transform.position = targetPos; // Snap exactly to the tile
            Debug.Log($"✅ [Branch] Reached tile {nextTile}, distance was {distance}");

            pathToExplore.Dequeue();

            // Check if this is an item position and mark it as collected
            if (activeBranch != null && activeBranch.itemPositions.Contains(nextTile))
            {
                collectedItems.Add(nextTile);
                Debug.Log($"🎁 Collected item at {nextTile}");
            }

            // Handle end of branch exploration
            if (pathToExplore.Count == 0)
            {
                if (!returningFromBranch)
                {
                    Debug.Log("✅ Hero a ajuns la finalul ramurii. Începe întoarcerea.");

                    if (activeBranch != null)
                    {
                        // Build return path ensuring we use the branch paths
                        List<Vector2Int> backtrack = new List<Vector2Int>();
                        Vector2Int current = nextTile;

                        // Find path back to entry point
                        List<Vector2Int> returnPath = FindPathInBranch(current, activeBranch.entryPoint, new HashSet<Vector2Int>(activeBranch.path));
                        pathToExplore = new Queue<Vector2Int>(returnPath);

                        returningFromBranch = true;
                        Debug.Log($"↩️ Drumul de întoarcere are {pathToExplore.Count} tile-uri.");
                    }
                }
                else
                {
                    Debug.Log($"✅ Hero s-a întors la entryPoint {activeBranch?.entryPoint}. Ramura marcată ca explorată.");

                    if (activeBranch != null)
                        activeBranch.alreadyExplored = true;

                    isExploringBranch = false;
                    returningFromBranch = false;
                    activeBranch = null;

                    ResumeMainPathFromCurrentPosition();
                }
            }
        }
    }
    private Vector3 movementTarget;
    private bool isMoving = false;
    private Action currentMoveCallback = null;

    void MoveTowards(Vector2Int targetTile, Action onReached)
    {
        movementTarget = new Vector3(targetTile.x, transform.position.y, targetTile.y);
        isMoving = true;
        isTileTransitionComplete = false;
        currentMoveCallback = onReached;

        Debug.Log($"➡️ Starting movement to: {targetTile}, from {transform.position}");
    }
    private void UpdateMovement()
    {
        if (!isMoving || currentMoveCallback == null) return;

        Vector3 dir = (movementTarget - transform.position).normalized;
        transform.position = Vector3.MoveTowards(transform.position, movementTarget, moveSpeed * Time.fixedDeltaTime);

        // Rotează către direcția de mers
        if (dir.magnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        float distance = Vector3.Distance(transform.position, movementTarget);
        if (distance <= ARRIVAL_THRESHOLD)
        {
            transform.position = movementTarget;
            isMoving = false;
            isTileTransitionComplete = true;

            var callback = currentMoveCallback;
            currentMoveCallback = null;

            Debug.Log($"✅ Reached target position, distance: {distance}");
            callback?.Invoke();
        }
    }




    private void CheckForNearbyEntryPoints()
    {
        if (!isTileTransitionComplete) return; // Don't check during tile transitions

        Vector2Int heroGridPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        // Don't check if already exploring a branch
        if (isExploringBranch || pathToExplore != null && pathToExplore.Count > 0)
            return;

        foreach (BranchPath branch in branchPaths)
        {
            if (branch.alreadyExplored || branch == activeBranch)
                continue;

            Vector2Int entry = branch.entryPoint;

            // Check if Hero is exactly on entry point or on a neighbor tile
            if (heroGridPos == entry || (IsNeighbor(heroGridPos, entry) && IsFullyOnTile(heroGridPos)))
            {
                Debug.Log($"🚨 Hero este pe sau lângă entryPoint-ul {entry} – începe explorarea ramurii");

                // We need to build a path that covers all items in the branch
                BuildBranchExplorationPath(branch);
                return;
            }
        }
    }

    private void BuildBranchExplorationPath(BranchPath branch)
    {
        if (branch == null || branch.fullBranchPath == null || branch.fullBranchPath.Count == 0)
        {
            Debug.LogWarning("⚠️ Ramură invalidă sau fără fullBranchPath.");
            return;
        }

        pathToExplore = new Queue<Vector2Int>(branch.fullBranchPath);
        returnPoint = branch.entryPoint;

        activeBranch = branch;
        isExploringBranch = true;
        returningFromBranch = false;

        Debug.Log($"🚶‍♂️ Hero începe explorarea ramurii de la {branch.entryPoint} cu {branch.fullBranchPath.Count} tile-uri.");
    }


    public void ExploreNearestBranch()
    {
        if (isExploringBranch || branchPaths == null || branchPaths.Count == 0)
            return;

        Vector2Int heroPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        BranchPath closest = null;
        float closestDistance = float.MaxValue;

        foreach (BranchPath branch in branchPaths)
        {
            if (branch.alreadyExplored)
                continue;

            float dist = Vector2.Distance(heroPos, branch.entryPoint);
            if (dist < closestDistance)
            {
                closest = branch;
                closestDistance = dist;
            }
        }

        if (closest != null && closest.path.Count > 0)
        {
            BuildBranchExplorationPath(closest);
        }
    }

    private void ResumeMainPathFromCurrentPosition()
    {
        Vector2Int heroGrid = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        int resumeIndex = pathToExit.FindIndex(pos => pos == heroGrid);
        if (resumeIndex >= 0)
        {
            for (int i = resumeIndex + 1; i < pathToExit.Count; i++)
            {
                pathToExplore.Enqueue(pathToExit[i]);
            }

            Debug.Log($"▶️ Reluat pathToExit de la indexul {resumeIndex + 1}");
        }



        activeBranch = null;
    }




    private List<Vector2Int> FindPathInBranch(Vector2Int from, Vector2Int to, HashSet<Vector2Int> validArea)
    {
        Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        cameFrom[from] = from;
        queue.Enqueue(from);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == to)
                break;

            foreach (var dir in GetCardinalDirections())
            {
                Vector2Int neighbor = current + dir;

                if (validArea.Contains(neighbor) && !cameFrom.ContainsKey(neighbor))
                {
                    cameFrom[neighbor] = current;
                    queue.Enqueue(neighbor);
                }
            }
        }

        List<Vector2Int> path = new List<Vector2Int>();
        if (!cameFrom.ContainsKey(to)) return path;

        Vector2Int step = to;
        while (step != from)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(from);
        path.Reverse();
        return path;
    }

    private List<Vector2Int> GetCardinalDirections()
    {
        return new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };
    }

    private bool IsNeighbor(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y)) == 1;
    }
}
    