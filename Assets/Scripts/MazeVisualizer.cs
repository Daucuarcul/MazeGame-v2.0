using UnityEngine;

public class MazeVisualizer : MonoBehaviour
{
    public GameObject floorPrefab;
    public GameObject wallVerticalPrefab;
    public GameObject wallHorizontalPrefab;
    public float tileSize = 1f;

    public void Visualize(int[,] mazeGrid)
    {
        int width = mazeGrid.GetLength(0);
        int height = mazeGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Podea
                Vector3 floorPos = new Vector3(x * tileSize, 0f, y * tileSize);
                Instantiate(floorPrefab, floorPos, Quaternion.identity, transform);

                // Pereți între celule
                if (x < width - 1 && mazeGrid[x, y] != mazeGrid[x + 1, y])
                {
                    Vector3 wallVPos = new Vector3((x + 0.5f) * tileSize, -0.25f, y * tileSize);
                    Instantiate(wallVerticalPrefab, wallVPos, Quaternion.identity, transform);
                }

                if (y < height - 1 && mazeGrid[x, y] != mazeGrid[x, y + 1])
                {
                    Vector3 wallHPos = new Vector3(x * tileSize, -0.25f, (y + 0.5f) * tileSize);
                    Instantiate(wallHorizontalPrefab, wallHPos, Quaternion.identity, transform);
                }

                // Pereți margine dreapta
                if (x == width - 1)
                {
                    Vector3 wallVRight = new Vector3((x + 0.5f) * tileSize, -0.25f, y * tileSize);
                    Instantiate(wallVerticalPrefab, wallVRight, Quaternion.identity, transform);
                }

                // Pereți margine sus
                if (y == height - 1)
                {
                    Vector3 wallHTop = new Vector3(x * tileSize, -0.25f, (y + 0.5f) * tileSize);
                    Instantiate(wallHorizontalPrefab, wallHTop, Quaternion.identity, transform);
                }

                // Pereți margine stânga
                if (x == 0)
                {
                    Vector3 wallVLeft = new Vector3((x - 0.5f) * tileSize, -0.25f, y * tileSize);
                    Instantiate(wallVerticalPrefab, wallVLeft, Quaternion.identity, transform);
                }

                // Pereți margine jos
                if (y == 0)
                {
                    Vector3 wallHBottom = new Vector3(x * tileSize, -0.25f, (y - 0.5f) * tileSize);
                    Instantiate(wallHorizontalPrefab, wallHBottom, Quaternion.identity, transform);
                }
            }
        }
    }
}
