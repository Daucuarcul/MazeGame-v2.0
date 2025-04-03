using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public int width = 21;
    public int height = 21;
    public MazeVisualizer visualizer;

    private int[,] mazeGrid;

    void Start()
    {
        GenerateMaze();
    }

    public void GenerateMaze()
    {
        mazeGrid = new int[width, height];

        // Inițial toți pereți (0)
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                mazeGrid[x, y] = 0;

        int startX = Random.Range(0, width / 2) * 2 + 1;
        int startY = Random.Range(0, height / 2) * 2 + 1;

        RecursiveBacktrack(startX, startY);

        Debug.Log("✅ Maze logic generated.");
        visualizer.Visualize(mazeGrid);
    }

    private void RecursiveBacktrack(int x, int y)
    {
        mazeGrid[x, y] = 1;

        int[] dx = { 0, 0, -2, 2 };
        int[] dy = { -2, 2, 0, 0 };

        for (int i = 0; i < 4; i++)
        {
            int rand = Random.Range(i, 4);
            (dx[i], dx[rand]) = (dx[rand], dx[i]);
            (dy[i], dy[rand]) = (dy[rand], dy[i]);
        }

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (IsInBounds(nx, ny) && mazeGrid[nx, ny] == 0)
            {
                mazeGrid[x + dx[i] / 2, y + dy[i] / 2] = 1;
                RecursiveBacktrack(nx, ny);
            }
        }
    }

    private bool IsInBounds(int x, int y)
    {
        return x > 0 && x < width && y > 0 && y < height;
    }
}
