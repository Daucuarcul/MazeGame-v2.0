using UnityEngine;

public class MazeVisualizer : MonoBehaviour
{
    public GameObject floorPrefab;
    public GameObject wallHorizontalPrefab;
    public GameObject wallVerticalPrefab;
    public float tileSize = 1f;

    public void Visualize(int[,] mazeGrid)
    {
        int width = mazeGrid.GetLength(0);
        int height = mazeGrid.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * tileSize, 0f, y * tileSize);

                if (mazeGrid[x, y] == 1)
                {
                    Instantiate(floorPrefab, position, Quaternion.identity, transform);
                }
                else
                {
                    // Poziții de pereți pe marginea celulei curente
                    bool wallRight = (x < width - 1 && mazeGrid[x + 1, y] == 1);
                    bool wallTop = (y < height - 1 && mazeGrid[x, y + 1] == 1);

                    // Instanțiere perete vertical între celula curentă și dreapta
                    if (wallRight)
                    {
                        Vector3 wallVPos = new Vector3((x + 0.5f) * tileSize, -0.25f, y * tileSize);
                        Instantiate(wallVerticalPrefab, wallVPos, Quaternion.identity, transform);
                    }

                    // Instanțiere perete orizontal între celula curentă și sus
                    if (wallTop)
                    {
                        Vector3 wallHPos = new Vector3(x * tileSize, -0.25f, (y + 0.5f) * tileSize);
                        Instantiate(wallHorizontalPrefab, wallHPos, Quaternion.identity, transform);
                    }
                }
            }
        }

        Debug.Log("✅ Vizualizarea labirintului completă.");
    }
}
