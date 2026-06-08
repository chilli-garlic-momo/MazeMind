using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
    public GameObject laserPrefab;
    public Transform[] positions;

    private void Start()
    {
        int lasersToSpawn = Room2DifficultyManager.GetLaserCount(positions.Length);

        for (int i = 0; i < lasersToSpawn; i++)
        {
            Instantiate(
                laserPrefab,
                positions[i].position,
                positions[i].rotation
            );
        }

        Debug.Log($"Spawned {lasersToSpawn} lasers.");
    }
}