using UnityEngine;

public class CameraController : MonoBehaviour
{
    public MazeGenerator maze; // referință la MazeGenerator din scenă
    public float padding = 2f;

    void Start()
    {
        CenterCamera();
    }

    void CenterCamera()
    {
        int width = maze.width;
        int height = maze.height;
        float tileSize = maze.tileSize;

        // Calculează centrul labirintului
        float centerX = (width - 1) * tileSize / 2f;
        float centerZ = (height - 1) * tileSize / 2f;

        // Mută camera deasupra centrului
        transform.position = new Vector3(centerX, 10f, centerZ);
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Ajustează orthographicSize pentru a cuprinde tot gridul
        Camera cam = GetComponent<Camera>();
        if (cam.orthographic)
        {
            float gridHeight = height * tileSize;
            float gridWidth = width * tileSize / cam.aspect;
            cam.orthographicSize = Mathf.Max(gridHeight, gridWidth) / 2f + padding;
        }
    }
}
