using UnityEngine;
using System.Collections.Generic;

public class MazeBorderGenerator : MonoBehaviour
{
    public int width = 21;
    public int height = 21;

    public GameObject floorPrefab;
    public GameObject wallVerticalPrefab;
    public GameObject wallHorizontalPrefab;

    public float tileSize = 1f;

    private int[,] mazeGrid;
    private List<Vector2Int> exitPositions = new List<Vector2Int>();

    void Start()
    {
        GenerateBorderWithFloor();
    }

    void GenerateBorderWithFloor()
    {
        mazeGrid = new int[width, height];

        // 1. Podea completă
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0f, y * tileSize);
                Instantiate(floorPrefab, pos, Quaternion.identity, transform);
                mazeGrid[x, y] = 0; // inițial, totul este perete (0)
            }
        }

        // 2. Generează 2 ieșiri aleatoare
        GenerateRandomExits();

        // 3. Pereți orizontali (sus și jos)
        for (int x = 0; x < width; x++)
        {
            // Jos
            if (!exitPositions.Contains(new Vector2Int(x, 0)))
                Instantiate(wallHorizontalPrefab, new Vector3(x * tileSize, -0.25f, -0.5f * tileSize), Quaternion.identity, transform);
            else
                mazeGrid[x, 0] = 1;

            // Sus
            if (!exitPositions.Contains(new Vector2Int(x, height - 1)))
                Instantiate(wallHorizontalPrefab, new Vector3(x * tileSize, -0.25f, (height - 0.5f) * tileSize), Quaternion.identity, transform);
            else
                mazeGrid[x, height - 1] = 1;
        }

        // 4. Pereți verticali (stânga și dreapta)
        for (int y = 0; y < height; y++)
        {
            // Stânga
            if (!exitPositions.Contains(new Vector2Int(0, y)))
                Instantiate(wallVerticalPrefab, new Vector3(-0.5f * tileSize, -0.25f, y * tileSize), Quaternion.identity, transform);
            else
                mazeGrid[0, y] = 1;

            // Dreapta
            if (!exitPositions.Contains(new Vector2Int(width - 1, y)))
                Instantiate(wallVerticalPrefab, new Vector3((width - 0.5f) * tileSize, -0.25f, y * tileSize), Quaternion.identity, transform);
            else
                mazeGrid[width - 1, y] = 1;
        }

        Debug.Log("✅ Podeaua, pereții și ieșirile au fost generate.");
    }

    void GenerateRandomExits()
    {
        List<string> sides = new List<string> { "Top", "Bottom", "Left", "Right" };
        string firstSide = sides[Random.Range(0, sides.Count)];
        string secondSide;

        do
        {
            secondSide = sides[Random.Range(0, sides.Count)];
        } while (secondSide == firstSide);

        exitPositions.Add(GetRandomEdgePosition(firstSide));
        exitPositions.Add(GetRandomEdgePosition(secondSide));
    }

    Vector2Int GetRandomEdgePosition(string side)
    {
        switch (side)
        {
            case "Top":
                return new Vector2Int(Random.Range(1, width - 1), height - 1);
            case "Bottom":
                return new Vector2Int(Random.Range(1, width - 1), 0);
            case "Left":
                return new Vector2Int(0, Random.Range(1, height - 1));
            case "Right":
                return new Vector2Int(width - 1, Random.Range(1, height - 1));
            default:
                return new Vector2Int(0, 0);
        }
    }

    // ✅ Metode pentru acces extern din MazePathGenerator
    public int[,] GetMazeGrid()
    {
        return mazeGrid;
    }

    public List<Vector2Int> GetExitPositions()
    {
        return exitPositions;
    }
}
