using UnityEngine;
using System.Collections.Generic;

public class GemSpawner : MonoBehaviour
{
    public GameObject gemPrefab;
    public Transform[] spawnPoints;

    [Header("Fallback")]
    public int gemsToSpawn = 4;

    private void Start()
    {
        int spawnCount = Room2DifficultyManager.GetGemCount(spawnPoints.Length);

        List<int> used = new List<int>();

        for (int i = 0; i < spawnCount; i++)
        {
            int index;

            do
            {
                index = Random.Range(0, spawnPoints.Length);
            }
            while (used.Contains(index));

            used.Add(index);

            Instantiate(
                gemPrefab,
                spawnPoints[index].position,
                Quaternion.identity
            );
        }

        Debug.Log($"Spawned {spawnCount} gems.");
    }
}