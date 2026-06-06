using System.Collections.Generic;
using UnityEngine;
using MazeMind.Core;

public class CheckboxFloorGenerator : MonoBehaviour
{
    [Header("Grid dimensions")]
    public int cols = 10;   // X axis
    public int rows = 6;    // Z axis

    [Header("Path generation")]
    [Tooltip("Base path length as fraction of (cols+rows). 1.0 = straight, 2.0 = very winding.")]
    public float basePathFactor = 1.2f;

    [Header("Visual materials")]
    public Material safeTileMat;   // optional — leave null to keep red/black look
    public Material trapTileMat;   // optional
    public bool revealSafePath = false;  // debug: show safe tiles in green

    [Header("Debug")]
    public Color debugSafeColor = Color.green;
    public Material debugSafeMat;   // optional override for debug viz

    private List<GameObject> _tiles = new();
    private HashSet<Vector2Int> _safeCoords = new();

    void Start()
    {
        CollectTiles();
        GenerateSafePath();
        ApplyTilesTagsAndScripts();
    }

    void CollectTiles()
    {
        _tiles.Clear();
        foreach (Transform child in transform)
        {
            _tiles.Add(child.gameObject);
        }
    }

    Vector2Int TileCoord(GameObject tile)
    {
        // Convert world position back to grid coord
        // Assumes tiles are placed at X = -4.5..4.5 and Z = -2.5..2.5 (1m spacing)
        var lp = tile.transform.localPosition;
        int x = Mathf.RoundToInt(lp.x + (cols - 1) / 2f);
        int z = Mathf.RoundToInt(lp.z + (rows - 1) / 2f);
        return new Vector2Int(x, z);
    }

    GameObject TileAt(Vector2Int c)
    {
        foreach (var t in _tiles)
        {
            if (TileCoord(t) == c) return t;
        }
        return null;
    }

    void GenerateSafePath()
    {
        _safeCoords.Clear();

        // Read AI Director knob — affects path length
        float trapDensity = 1.0f;
        if (AIDirector.I != null && AIDirector.I.state != null)
            trapDensity = AIDirector.I.state.trapDensity;

        // Target path length scales with trap density:
        // trapDensity = 1.0 → ~14 tiles (manageable)
        // trapDensity = 1.25 → ~18 tiles (winding, harder)
        // trapDensity = 0.7 → ~10 tiles (more direct)
        int targetLength = Mathf.RoundToInt((cols + rows) * basePathFactor * trapDensity);
        targetLength = Mathf.Clamp(targetLength, cols + rows - 2, cols * rows / 2);

        // Pick a random entry on the bottom row (Z=0) and exit on top row (Z=rows-1)
        int entryX = Random.Range(0, cols);
        int exitX = Random.Range(0, cols);
        Vector2Int entry = new(entryX, 0);
        Vector2Int exit = new(exitX, rows - 1);

        // Random walk with bias toward exit
        List<Vector2Int> path = RandomWalkPath(entry, exit, targetLength);

        foreach (var p in path) _safeCoords.Add(p);

        Debug.Log($"[CheckboxFloor] Path generated. Length={path.Count}, trapDensity={trapDensity:F2}, " +
                  $"entry={entry}, exit={exit}");
    }

    List<Vector2Int> RandomWalkPath(Vector2Int start, Vector2Int end, int targetLen)
    {
        var path = new List<Vector2Int> { start };
        var visited = new HashSet<Vector2Int> { start };
        var current = start;

        int safety = 200;
        while (current != end && safety-- > 0)
        {
            // Possible moves
            var options = new List<Vector2Int>();
            Vector2Int[] dirs = {
                new(0, 1), new(0, -1), new(1, 0), new(-1, 0)
            };

            foreach (var d in dirs)
            {
                var next = current + d;
                if (next.x < 0 || next.x >= cols || next.y < 0 || next.y >= rows) continue;
                if (visited.Contains(next)) continue;
                options.Add(next);
            }

            if (options.Count == 0)
            {
                // dead end — break and force connect to end
                break;
            }

            Vector2Int chosen;
            // If path is already long enough, push toward exit; else wander
            if (path.Count >= targetLen)
            {
                // pick option closest to end
                chosen = options[0];
                int bestDist = ManhDist(chosen, end);
                foreach (var o in options)
                {
                    int d = ManhDist(o, end);
                    if (d < bestDist) { bestDist = d; chosen = o; }
                }
            }
            else
            {
                // wander, slight bias toward end (50/50 random vs. toward-end)
                if (Random.value < 0.4f)
                {
                    chosen = options[0];
                    int bestDist = ManhDist(chosen, end);
                    foreach (var o in options)
                    {
                        int d = ManhDist(o, end);
                        if (d < bestDist) { bestDist = d; chosen = o; }
                    }
                }
                else
                {
                    chosen = options[Random.Range(0, options.Count)];
                }
            }

            path.Add(chosen);
            visited.Add(chosen);
            current = chosen;
        }

        // If we didn't reach end via walk, add a direct line from current to end
        if (current != end)
        {
            while (current.x != end.x)
            {
                current.x += (end.x > current.x) ? 1 : -1;
                if (!visited.Contains(current)) { path.Add(current); visited.Add(current); }
            }
            while (current.y != end.y)
            {
                current.y += (end.y > current.y) ? 1 : -1;
                if (!visited.Contains(current)) { path.Add(current); visited.Add(current); }
            }
        }

        return path;
    }

    int ManhDist(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    void ApplyTilesTagsAndScripts()
    {
        foreach (var tile in _tiles)
        {
            var coord = TileCoord(tile);
            bool isSafe = _safeCoords.Contains(coord);

            if (isSafe)
            {
                tile.tag = "SafeTile";
                // Remove BulletTrap if present
                var bt = tile.GetComponent<BulletTrap>();
                if (bt != null) Destroy(bt);

                if (revealSafePath && debugSafeMat != null)
                {
                    var mr = tile.GetComponent<MeshRenderer>();
                    if (mr != null) mr.material = debugSafeMat;
                }
            }
            else
            {
                tile.tag = "TrapTile";
                // Ensure trigger
                var col = tile.GetComponent<Collider>();
                if (col != null) col.isTrigger = true;
                // Add BulletTrap
                if (tile.GetComponent<BulletTrap>() == null)
                    tile.AddComponent<BulletTrap>();
            }
        }
    }
}