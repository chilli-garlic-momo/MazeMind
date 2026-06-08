using System.Collections.Generic;
using UnityEngine;

public class FakeGemSpawner : MonoBehaviour
{
    [Header("Fake Gem Prefab")]
    public GameObject fakeGemPrefab;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints = new();

    void Start()
    {
        int fakeGemCount = Room3DifficultyManager.GetProfile().fakeGemCount;

        SpawnFakeGems(fakeGemCount);
    }

    void SpawnFakeGems(int count)
    {
        List<Transform> available = new(spawnPoints);

        count = Mathf.Min(count, available.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);

            Instantiate(
                fakeGemPrefab,
                available[index].position,
                available[index].rotation);

            available.RemoveAt(index);
        }
    }
}