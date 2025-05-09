using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// 🔵 Reprezintă o ramificație spre power-up-uri
[System.Serializable]
public class BranchPath
{
    public Vector2Int entryPoint; // Punctul de unde intrăm în ramificație
    public List<Vector2Int> path = new List<Vector2Int>(); // Lista de tile-uri din ramificație
    public List<Vector2Int> itemPositions = new List<Vector2Int>(); // Pozițiile unde sunt itemele în ramificație
    public bool alreadyExplored = false;
}



public class MazeGenerator : MonoBehaviour
{
    public int width = 21;
    public int height = 21;

    public GameObject floorPrefab;
    public GameObject wallVerticalPrefab;
    public GameObject wallHorizontalPrefab;
    public GameObject labelStartPrefab;
    public GameObject labelExitPrefab;
    public GameObject specialPrefab;
    public GameObject powerupPrefab;
    public GameObject entryPointLabelPrefab;

    public float tileSize = 1f;
    private List<Vector2Int> deadEnds = new List<Vector2Int>();
    private int[,] mazeGrid;
    private Vector2Int exitA, exitB;
    private List<Vector2Int> pathToExit;
    private List<BranchPath> branchPaths = new List<BranchPath>();
    public List<Vector2Int> GetPathToExit()
    {
        return pathToExit;
    }

    public List<BranchPath> GetBranchPaths()
    {
        return branchPaths;
    }
    public List<Vector3> entryPoints
    {
        get
        {
            List<Vector3> result = new List<Vector3>();
            foreach (BranchPath b in branchPaths)
            {
                Vector3 worldPos = new Vector3(b.entryPoint.x * tileSize, 0f, b.entryPoint.y * tileSize);
                result.Add(worldPos);
            }
            return result;
        }
    }

    void Start()
    {
        mazeGrid = new int[width, height];
        GenerateFloor();
        GenerateMazePath();
        FindLongestPathEnds();
        FindMainPath();
        IdentifyDeadEnds();
        PlacePowerupsOnDeadEnds();
        CutEntrancesFromOutside();
        VisualizeMaze();
        PlaceDebugLabels();
        FindBranchPaths();
        MarkIsolatedAreasWithPrefab();
       

    }

    void GenerateFloor()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0f, y * tileSize);
                Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                mazeGrid[x, y] = 0;
            }
    }

    void GenerateMazePath()
    {
        Vector2Int start = new Vector2Int(width / 2, height / 2);
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        start = MakeOdd(start);
        stack.Push(start);
        visited.Add(start);
        mazeGrid[start.x, start.y] = 1;

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Peek();
            List<Vector2Int> neighbors = new List<Vector2Int>();

            foreach (var dir in GetCardinalDirections(2))
            {
                Vector2Int neighbor = current + dir;
                Vector2Int between = current + dir / 2;
                if (IsInside(neighbor) && !visited.Contains(neighbor) && mazeGrid[neighbor.x, neighbor.y] == 0)
                    neighbors.Add(dir);
            }

            if (neighbors.Count > 0)
            {
                Vector2Int chosenDir = neighbors[Random.Range(0, neighbors.Count)];
                Vector2Int between = current + chosenDir / 2;
                Vector2Int next = current + chosenDir;

                mazeGrid[between.x, between.y] = 1;
                mazeGrid[next.x, next.y] = 1;
                visited.Add(next);
                stack.Push(next);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    void FindLongestPathEnds()
    {
        Vector2Int from = MakeOdd(new Vector2Int(width / 2, height / 2));
        Vector2Int furthestA = BFSFarthest(from);
        Vector2Int furthestB = BFSFarthest(furthestA);
        exitA = furthestA;
        exitB = furthestB;

        Debug.Log("🟢 exitA: " + exitA + "  |  exitB: " + exitB);
    }
    void FindMainPath()
    {
        pathToExit = FindPath(exitA, exitB);
        Debug.Log("📌 Path to Exit generat cu " + pathToExit.Count + " pași.");
        List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal)
        {
            Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            cameFrom[start] = start;

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                if (current == goal)
                {
                    break;
                }

                foreach (var dir in GetCardinalDirections(1))
                {
                    Vector2Int neighbor = current + dir;
                    if (IsInside(neighbor) && mazeGrid[neighbor.x, neighbor.y] == 1 && !cameFrom.ContainsKey(neighbor))
                    {
                        queue.Enqueue(neighbor);
                        cameFrom[neighbor] = current;
                    }
                }
            }

            List<Vector2Int> path = new List<Vector2Int>();
            Vector2Int temp = goal;
            while (temp != start)
            {
                path.Add(temp);
                temp = cameFrom[temp];
            }
            path.Add(start);
            path.Reverse();

            return path;
        }

    }
    void CutEntrancesFromOutside()
    {
        mazeGrid[exitA.x, exitA.y] = 1;
        mazeGrid[exitB.x, exitB.y] = 1;
    }

    void VisualizeMaze()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (x < width - 1 && mazeGrid[x, y] != mazeGrid[x + 1, y])
                {
                    Vector3 pos = new Vector3((x + 0.5f) * tileSize, -0.25f, y * tileSize);
                    Instantiate(wallVerticalPrefab, pos, Quaternion.identity, transform);

                    // 🔲 Adăugăm pereții de margine complet
                    for (int i = 0; i < width; i++)
                    {
                        // jos
                        Vector3 bottom = new Vector3(i * tileSize, -0.25f, -0.5f * tileSize);
                        Instantiate(wallHorizontalPrefab, bottom, Quaternion.identity, transform);

                        // sus
                        Vector3 top = new Vector3(i * tileSize, -0.25f, (height - 0.5f) * tileSize);
                        Instantiate(wallHorizontalPrefab, top, Quaternion.identity, transform);
                    }

                    for (int j = 0; j < height; j++)
                    {
                        // stanga
                        Vector3 left = new Vector3(-0.5f * tileSize, -0.25f, j * tileSize);
                        Instantiate(wallVerticalPrefab, left, Quaternion.identity, transform);

                        // dreapta
                        Vector3 right = new Vector3((width - 0.5f) * tileSize, -0.25f, j * tileSize);
                        Instantiate(wallVerticalPrefab, right, Quaternion.identity, transform);
                    }
                }

                if (y < height - 1 && mazeGrid[x, y] != mazeGrid[x, y + 1])
                {
                    Vector3 pos = new Vector3(x * tileSize, -0.25f, (y + 0.5f) * tileSize);
                    Instantiate(wallHorizontalPrefab, pos, Quaternion.identity, transform);
                }
            }
    }

    void PlaceDebugLabels()
    {
        Vector3 posA = new Vector3(exitA.x * tileSize, 0.1f, exitA.y * tileSize);
        Vector3 posB = new Vector3(exitB.x * tileSize, 0.1f, exitB.y * tileSize);
        Instantiate(labelStartPrefab, posA, Quaternion.identity, transform);
        Instantiate(labelExitPrefab, posB, Quaternion.identity, transform);
    }

    void MarkIsolatedAreasWithPrefab()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (mazeGrid[x, y] == 0)
                {
                    Vector3 pos = new Vector3(x * tileSize, 0.15f, y * tileSize);
                    Instantiate(specialPrefab, pos, Quaternion.identity, transform);
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            if (mazeGrid[x, 0] == 0)
                Instantiate(specialPrefab, new Vector3(x * tileSize, 0.15f, 0 * tileSize), Quaternion.identity, transform);
            if (mazeGrid[x, height - 1] == 0)
                Instantiate(specialPrefab, new Vector3(x * tileSize, 0.15f, (height - 1) * tileSize), Quaternion.identity, transform);
        }

        for (int y = 0; y < height; y++)
        {
            if (mazeGrid[0, y] == 0)
                Instantiate(specialPrefab, new Vector3(0 * tileSize, 0.15f, y * tileSize), Quaternion.identity, transform);
            if (mazeGrid[width - 1, y] == 0)
                Instantiate(specialPrefab, new Vector3((width - 1) * tileSize, 0.15f, y * tileSize), Quaternion.identity, transform);
        }

        Debug.Log("✨ Prefaburi plasate pe zonele care nu sunt drum.");
    }

    void PlacePowerupsOnDeadEnds()
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (mazeGrid[x, y] == 1 && !(exitA.x == x && exitA.y == y) && !(exitB.x == x && exitB.y == y))
                {
                    int neighbors = 0;
                    if (mazeGrid[x + 1, y] == 1) neighbors++;
                    if (mazeGrid[x - 1, y] == 1) neighbors++;
                    if (mazeGrid[x, y + 1] == 1) neighbors++;
                    if (mazeGrid[x, y - 1] == 1) neighbors++;

                    if (neighbors == 1)
                    {
                        Vector3 pos = new Vector3(x * tileSize, 0.15f, y * tileSize);
                        Instantiate(powerupPrefab, pos, Quaternion.identity, transform);
                    }
                }
            }
        }

        Debug.Log("⭐ Powerup-uri plasate pe toate fundăturile.");
    }
    private void IdentifyDeadEnds()
    {
        deadEnds.Clear();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (mazeGrid[x, y] == 1)
                {
                    int openNeighbors = 0;
                    foreach (var dir in GetCardinalDirections(1))
                    {
                        Vector2Int neighbor = new Vector2Int(x + dir.x, y + dir.y);
                        if (IsInside(neighbor) && mazeGrid[neighbor.x, neighbor.y] == 1)
                        {
                            openNeighbors++;
                        }
                    }

                    if (openNeighbors == 1)
                    {
                        deadEnds.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        Debug.Log("🟡 DeadEnds identificate: " + deadEnds.Count);
    }
    private List<Vector2Int> GetAvailableNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        foreach (var dir in GetCardinalDirections(1))
        {
            Vector2Int neighbor = cell + dir;
            if (IsInside(neighbor) && mazeGrid[neighbor.x, neighbor.y] == 1)
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    private void FindBranchPaths()
    {
        branchPaths.Clear();
        int id = 0;
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (Vector2Int start in deadEnds)
        {
            if (visited.Contains(start))
                continue;

            Queue<Vector2Int> queue = new Queue<Vector2Int>();
            HashSet<Vector2Int> branchArea = new HashSet<Vector2Int>();
            Vector2Int entryPoint = Vector2Int.zero;
            bool foundEntryPoint = false;

            queue.Enqueue(start);
            visited.Add(start);
            branchArea.Add(start);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();
                List<Vector2Int> neighbors = GetAvailableNeighbors(current);

                foreach (var neighbor in neighbors)
                {
                    if (visited.Contains(neighbor))
                        continue;

                    if (IsPartOfMainPath(neighbor))
                    {
                        entryPoint = current;
                        foundEntryPoint = true;
                        continue;
                    }

                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                    branchArea.Add(neighbor);
                }
            }

            if (!foundEntryPoint)
            {
                Debug.LogWarning($"⚠️ Ramură de la {start} ignorată — fără conectare la pathToExit.");
                continue;
            }

            BranchPath branch = new BranchPath();
            branch.path = new List<Vector2Int>(branchArea);
            branch.entryPoint = entryPoint;

            // Detectăm PowerUps cu metodă robustă
            foreach (var pos in branch.path)
            {
                if (DetectPowerUpAt(pos))
                {
                    branch.itemPositions.Add(pos);
                    Debug.Log($"💎 PowerUp detectat la {pos} pentru ramura EP {branch.entryPoint}");
                }
            }

            if (branch.itemPositions.Count > 0)
            {
                branchPaths.Add(branch);
                Debug.Log($"🌱 Ramificație validă #{id} de la {branch.entryPoint} (len: {branch.path.Count})");

                // Etichetă vizuală EP
                if (entryPointLabelPrefab != null)
                {
                    Vector3 labelPos = new Vector3(branch.entryPoint.x * tileSize, 0.6f, branch.entryPoint.y * tileSize);
                    GameObject label = Instantiate(entryPointLabelPrefab, labelPos, Quaternion.identity, transform);

                    TextMesh text = label.GetComponent<TextMesh>();
                    if (text != null)
                    {
                        text.text = $"EP {id}";
                        text.characterSize = 0.2f;
                        text.color = Color.yellow;
                    }
                }

                id++;
            }
            else
            {
                Debug.LogWarning($"❌ Ramură fără PowerUps de la {start}");
            }
        }

        Debug.Log("🌟 Total ramuri detectate: " + branchPaths.Count);
    }

    private bool DetectPowerUpAt(Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x * tileSize, 0.5f, pos.y * tileSize);

        // 1. OverlapSphere cu rază mai mare
        Collider[] sphereHits = Physics.OverlapSphere(worldPos, 0.6f);
        foreach (var col in sphereHits)
        {
            if (col.CompareTag("PowerUp"))
                return true;
        }

        // 2. OverlapBox pe volum 3D
        Collider[] boxHits = Physics.OverlapBox(worldPos, new Vector3(0.6f, 1f, 0.6f));
        foreach (var col in boxHits)
        {
            if (col.CompareTag("PowerUp"))
                return true;
        }

        // 3. Raycast în jos din aer
        RaycastHit hit;
        if (Physics.Raycast(worldPos + Vector3.up * 2f, Vector3.down, out hit, 5f))
        {
            if (hit.collider.CompareTag("PowerUp"))
                return true;
        }

        // 4. Căutare globală ca fallback
        GameObject[] allPowerUps = GameObject.FindGameObjectsWithTag("PowerUp");
        foreach (GameObject powerUp in allPowerUps)
        {
            Vector3 p = powerUp.transform.position;
            Vector2Int gridPos = new Vector2Int(
                Mathf.RoundToInt(p.x / tileSize),
                Mathf.RoundToInt(p.z / tileSize)
            );
            if (gridPos == pos)
                return true;
        }

        return false;
    }


    Vector2Int BFSFarthest(Vector2Int start)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        bool[,] visited = new bool[width, height];
        int[,] dist = new int[width, height];

        q.Enqueue(start);
        visited[start.x, start.y] = true;
        Vector2Int farthest = start;
        int maxD = 0;

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();
            foreach (var dir in GetCardinalDirections(1))
            {
                Vector2Int nb = cur + dir;
                if (IsInside(nb) && mazeGrid[nb.x, nb.y] == 1 && !visited[nb.x, nb.y])
                {
                    visited[nb.x, nb.y] = true;
                    dist[nb.x, nb.y] = dist[cur.x, cur.y] + 1;
                    if (dist[nb.x, nb.y] > maxD)
                    {
                        maxD = dist[nb.x, nb.y];
                        farthest = nb;
                    }
                    q.Enqueue(nb);
                }
            }
        }

        return farthest;
    }

    Vector2Int MakeOdd(Vector2Int v)
    {
        int x = (v.x % 2 == 0) ? v.x + 1 : v.x;
        int y = (v.y % 2 == 0) ? v.y + 1 : v.y;
        return new Vector2Int(Mathf.Clamp(x, 1, width - 2), Mathf.Clamp(y, 1, height - 2));
    }

    List<Vector2Int> GetCardinalDirections(int step)
    {
        return new List<Vector2Int> {
            new Vector2Int(step, 0),
            new Vector2Int(-step, 0),
            new Vector2Int(0, step),
            new Vector2Int(0, -step)
        };
    }

    bool IsInside(Vector2Int v)
    {
        return v.x >= 1 && v.y >= 1 && v.x < width - 1 && v.y < height - 1;
    }
    public Vector3 GetStartWorldPosition()
    {
        Vector3 pos = new Vector3(exitA.x * tileSize, 0.5f, exitA.y * tileSize);
        Debug.Log("📍 Pozitie START calculata pentru Hero: " + pos);
        return pos;
    }
    public Vector3 GetExitWorldPosition()
    {
        return new Vector3(exitB.x * tileSize, 0.5f, exitB.y * tileSize);
    }
    private bool IsPartOfMainPath(Vector2Int cell)

    {
        return pathToExit != null && pathToExit.Contains(cell);
    }
}