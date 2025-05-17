using UnityEngine;
using System.Collections;

public class HeroSpawner : MonoBehaviour
{
    public GameObject heroPrefab;
    public MazeGenerator mazeGenerator;

    private void Start()
    {
        Debug.Log("🧩 HeroSpawner: Start() a fost apelat.");
        StartCoroutine(SpawnHeroAfterMaze());
    }

    IEnumerator SpawnHeroAfterMaze()
    {
        // Așteptăm până când MazeGenerator a generat ramificațiile
        yield return new WaitForSeconds(0.1f);
        Debug.Log("✅ Am trecut de yield, urmează instanțierea Hero.");




        // Adăugăm o mică întârziere pentru siguranță
        yield return new WaitForSeconds(0.1f);

        Vector3 startPosition = mazeGenerator.GetStartWorldPosition();
        Debug.Log("🚀 Hero va fi instanțiat la: " + startPosition);

        GameObject hero = Instantiate(heroPrefab, startPosition, Quaternion.identity);
        hero.SetActive(false); // 🔴 oprim temporar

        HeroMovement movement = hero.GetComponent<HeroMovement>();
        if (movement != null)
        {
            var branches = mazeGenerator.GetBranchPaths();
            Debug.Log("🧪 Verificare finală: transmit " + branches.Count + " ramificații către Hero.");

            movement.SetPathToExit(mazeGenerator.GetPathToExit());
            movement.SetBranchPaths(branches);
            movement.AssignBranchIndicesByPath();
            Debug.Log("🧪 HeroMovement intern are " + movement.Debug_GetBranchCount() + " ramificații după setare.");
        }

       

        hero.SetActive(true); // ✅ abia acum îl pornim
    }
}
