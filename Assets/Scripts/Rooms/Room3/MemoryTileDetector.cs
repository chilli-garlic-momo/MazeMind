using UnityEngine;

public class MemoryTileDetector : MonoBehaviour
{
    private MemoryTile currentTile;
    private PlayerHealth health;

    private void Start()
    {
        health = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        Ray ray = new Ray(transform.position, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            MemoryTile tile = hit.collider.GetComponent<MemoryTile>();

            if (tile == null)
                return;

            if (tile == currentTile)
                return;

            currentTile = tile;

            if (!tile.isSafe)
            {
                Debug.Log("UNSAFE TILE!");

                health.Damage(tile.damage);
            }
            else
            {
                Debug.Log("SAFE TILE");
            }
        }
    }
}