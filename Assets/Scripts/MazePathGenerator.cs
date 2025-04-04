using UnityEngine;
using System.Collections.Generic;

public class MazePathGenerator : MonoBehaviour
{
    public MazeBorderGenerator borderGenerator; // referință la scriptul care a generat marginile și ieșirile
    public MazeVisualizer visualizer;           // referință la scriptul de vizualizare

    private int[,] mazeGrid;
    private List<Vector2Int> exits;

    void Start()
    {
        // 1. Obține datele de la MazeBorderGenerator
        mazeGrid = borderGenerator.GetMazeGrid();
        exits = borderGenerator.GetExitPositions();

        // 2. Verifică dacă avem două ieșiri
        if (exits.Count < 2)
        {
            Debug.LogError("❌ Nu am găsit suficiente ieșiri pentru a genera drumul.");
            return;
        }

        // 3. Generează drumul principal între cele două ieșiri
        GenerateMainPath(exits[0], exits[1]);

        // 4. Vizualizează labirintul și afisează gridul în consolă
        visualizer.Visualize(mazeGrid);
        PrintMazeGrid();
    }

    void GenerateMainPath(Vector2Int start, Vector2Int end)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            mazeGrid[current.x, current.y] = 1; // marchează ca drum

            if (current == end)
                break;

            foreach (Vector2Int dir in GetShuffledDirections())
            {
                Vector2Int next = current + dir;

                if (IsInsideGrid(next) && !visited.Contains(next) && mazeGrid[next.x, next.y] == 0)
                {
                    stack.Push(next);
                    visited.Add(next);
                }
            }
        }

        Debug.Log("✅ Drum principal generat între cele două ieșiri.");
    }

    void PrintMazeGrid()
    {
        string debug = "";
        for (int y = mazeGrid.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = 0; x < mazeGrid.GetLength(0); x++)
            {
                debug += mazeGrid[x, y] == 1 ? "." : "#";
            }
            debug += "\n";
        }
        Debug.Log(debug);
    }

    bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.y >= 0 &&
               pos.x < mazeGrid.GetLength(0) &&
               pos.y < mazeGrid.GetLength(1);
    }

    List<Vector2Int> GetShuffledDirections()
    {
        List<Vector2Int> dirs = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        for (int i = 0; i < dirs.Count; i++)
        {
            Vector2Int temp = dirs[i];
            int randIndex = Random.Range(i, dirs.Count);
            dirs[i] = dirs[randIndex];
            dirs[randIndex] = temp;
        }

        return dirs;
    }
}
