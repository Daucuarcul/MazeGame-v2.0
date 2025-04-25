using UnityEngine;
using System.Collections;

public class HeroSpawner : MonoBehaviour
{
    public GameObject heroPrefab;
    public FixedJoystick joystick;
    public MazeGenerator mazeGenerator;

    private void Start()
    {
        StartCoroutine(SpawnHeroAfterMaze());
    }

    IEnumerator SpawnHeroAfterMaze()
    {
        // Așteptăm 1 frame ca MazeGenerator să termine Start()
        yield return null;

        Vector3 startPosition = mazeGenerator.GetStartWorldPosition();
        Debug.Log("🚀 Hero va fi instanțiat la: " + startPosition);

        GameObject hero = Instantiate(heroPrefab, startPosition, Quaternion.identity);

        HeroMovement movement = hero.GetComponent<HeroMovement>();
        if (movement != null)
        {
            movement.joystick = joystick;
        }
    }
}
