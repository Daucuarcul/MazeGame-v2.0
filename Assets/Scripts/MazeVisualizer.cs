using UnityEngine;

public class MazeVisualizer : MonoBehaviour
{
    public GameObject floorPrefab;               // Nu va mai fi folosit aici
    public GameObject wallHorizontalPrefab;
    public GameObject wallVerticalPrefab;
    public float tileSize = 1f;

    public void Visualize(int[,] mazeGrid)
    {
        int width = mazeGrid.GetLength(0);
        int height = mazeGrid.GetLength(1);

        // Instanțiază doar pereții între celule
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mazeGrid[x, y] == 1)
                {
                    // Verificăm dacă există perete la dreapta
                    if (x + 1 < width && mazeGrid[x + 1, y] == 0)
                    {
                        Vector3 wallPos = new Vector3((x + 0.5f) * tileSize, -0.25f, y * tileSize);
                        Instantiate(wallVerticalPrefab, wallPos, Quaternion.identity, transform);
                    }

                    // Verificăm dacă există perete sus
                    if (y + 1 < height && mazeGrid[x, y + 1] == 0)
                    {
                        Vector3 wallPos = new Vector3(x * tileSize, -0.25f, (y + 0.5f) * tileSize);
                        Instantiate(wallHorizontalPrefab, wallPos, Quaternion.identity, transform);
                    }
                }
            }
        }

        Debug.Log("✅ Vizualizarea labirintului completă (doar pereți).");
    }
}
