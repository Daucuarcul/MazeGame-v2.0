using System.Collections.Generic;
using UnityEngine;

public class HeroMovement : MonoBehaviour
{
    public float speed = 3.0f;
    public float detectionRadius = 2.5f;

    private List<Vector2Int> pathToExit;
    private List<BranchPath> branchPaths = new List<BranchPath>();
    private int currentPathIndex = 0;

    private bool isExploringBranch = false;
    private bool returningFromBranch = false;

    private List<Vector2Int> currentBranchPath = new List<Vector2Int>();
    private int branchStepIndex = 0;
    private Vector2Int returnPoint = new Vector2Int(-1, -1);

    private BranchPath activeBranch;

    private bool isMovingToTile = false;

    public void SetPathToExit(List<Vector2Int> path)
    {
        pathToExit = path;
    }

    public void SetBranchPaths(List<BranchPath> branches)
    {
        branchPaths = branches;
    }

    public int Debug_GetBranchCount()
    {
        return branchPaths != null ? branchPaths.Count : -1;
    }

    private void FixedUpdate()
    {
        if (isExploringBranch)
        {
            FollowBranchPath();
        }
        else
        {
            FollowMainPath();
            CheckForNearbyEntryPoints();
        }
    }

    void FollowMainPath()
    {
        if (pathToExit == null || currentPathIndex >= pathToExit.Count)
            return;

        MoveTowards(pathToExit[currentPathIndex], () =>
        {
            currentPathIndex++;
        });
    }

    void FollowBranchPath()
    {
        if (!returningFromBranch)
        {
            if (branchStepIndex >= currentBranchPath.Count)
            {
                returningFromBranch = true;
                return;
            }

            Vector2Int target = currentBranchPath[branchStepIndex];
            MoveTowards(target, () =>
            {
                branchStepIndex++;
            });
        }
        else
        {
            MoveTowards(returnPoint, () =>
            {
                isExploringBranch = false;
                returningFromBranch = false;

                if (activeBranch != null)
                {
                    activeBranch.alreadyExplored = true;
                    activeBranch = null;
                }
            });
        }
    }

    void MoveTowards(Vector2Int targetCell, System.Action onArrived)
    {
        if (isMovingToTile)
            return;

        Vector3 targetWorldPos = new Vector3(targetCell.x, transform.position.y, targetCell.y);

        transform.position = new Vector3(
            Mathf.Round(transform.position.x * 100f) / 100f,
            transform.position.y,
            Mathf.Round(transform.position.z * 100f) / 100f
        );

        float distanceToTarget = Vector3.Distance(transform.position, targetWorldPos);

        if (distanceToTarget <= 0.05f)
        {
            transform.position = targetWorldPos;
            isMovingToTile = true;
            onArrived?.Invoke();
            Invoke(nameof(UnlockTileMovement), Time.fixedDeltaTime);
            return;
        }

        Vector3 direction = (targetWorldPos - transform.position);
        direction.y = 0f;
        direction.Normalize();

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, 10 * Time.fixedDeltaTime);
            transform.position += transform.forward * speed * Time.fixedDeltaTime;
        }
    }

    void UnlockTileMovement()
    {
        isMovingToTile = false;
    }

    private void CheckForNearbyEntryPoints()
    {
        Vector2Int heroGridPos = new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z)
        );

        // Dacă Hero explorează o ramificație, verificăm dacă a trecut de entryPoint
        if (isExploringBranch && activeBranch != null)
        {
            if (heroGridPos == activeBranch.entryPoint)
            {
                Debug.Log("✅ Hero a trecut de entryPoint. Stingem butonul și marcăm ramura.");
                UIManager.Instance.SetPowerUpButtonActive(false);
                activeBranch.alreadyExplored = true;
                isExploringBranch = false;
            }
        }

        foreach (BranchPath branch in branchPaths)
        {
            if (branch.alreadyExplored)
                continue;

            Vector2Int entry = branch.entryPoint;
            List<Vector2Int> neighbors = new List<Vector2Int>
        {
            new Vector2Int(entry.x + 1, entry.y),
            new Vector2Int(entry.x - 1, entry.y),
            new Vector2Int(entry.x, entry.y + 1),
            new Vector2Int(entry.x, entry.y - 1)
        };

            Vector2Int? vecinDrumPrincipal = null;

            foreach (var n in neighbors)
            {
                if (pathToExit.Contains(n))
                {
                    vecinDrumPrincipal = n;
                    break;
                }
            }

            if (vecinDrumPrincipal == null)
            {
                Debug.LogWarning($"⚠️ Nu am găsit vecin drum principal pentru entryPoint: {entry}");
                continue;
            }

            int index = pathToExit.IndexOf(vecinDrumPrincipal.Value);
            if (index == -1)
            {
                Debug.LogWarning($"⚠️ Vecinul nu este în pathToExit: {vecinDrumPrincipal}");
                continue;
            }

            for (int offset = -2; offset <= 2; offset++)
            {
                int checkIndex = index + offset;

                if (checkIndex >= 0 && checkIndex < pathToExit.Count)
                {
                    if (pathToExit[checkIndex] == heroGridPos)
                    {
                        UIManager.Instance.SetPowerUpButtonActive(true);
                        Debug.Log($"✅ HERO detectat în fereastra logică pentru ramura EP: {entry}");
                        return;
                    }
                }
            }
        }

        UIManager.Instance.SetPowerUpButtonActive(false);
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
        currentBranchPath = new List<Vector2Int>(closest.path);
        branchStepIndex = 0;
        returnPoint = closest.entryPoint;
        activeBranch = closest;

        isExploringBranch = true;
        returningFromBranch = false;
    }
}
}
