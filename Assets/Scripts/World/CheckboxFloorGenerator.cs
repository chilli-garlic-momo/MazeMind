using System.Collections.Generic;
using UnityEngine;
using MazeMind.Core;

/// <summary>
/// Generates Section 1.3's laser-maze floor. Entry and exit are CONSTANT per scene
/// (set in inspector). A single safe path runs entry -> key tile -> exit, so the
/// player is forced through the key on the way out. Trap tiles get a BulletTrap.
/// Key placement along the path is chosen by AIDirector.ComputeRoom13KeyPathFraction().
/// </summary>
public class CheckboxFloorGenerator : MonoBehaviour
{
    [Header("Grid dimensions")]
    public int cols = 10;   // X axis
    public int rows = 6;    // Z axis

    [Header("Fixed entry / exit (in grid coords)")]
    public Vector2Int entryCoord = new Vector2Int(0, 0);
    public Vector2Int exitCoord  = new Vector2Int(9, 5);

    [Header("Path generation")]
    [Tooltip("Base path length as fraction of (cols+rows). 1.0 = straight, 2.0 = very winding.")]
    public float basePathFactor = 1.2f;

    [Header("Key on path")]
    public GameObject keyPrefab;
    public Transform  keyParent;
    public float      keyHeightOffset = 0.6f;

    [Header("Visual materials")]
    public Material safeTileMat;
    public Material trapTileMat;
    public bool revealSafePath = false;

    [Header("Debug")]
    public Color debugSafeColor = Color.green;
    public Material debugSafeMat;

    private List<GameObject> _tiles = new();
    private HashSet<Vector2Int> _safeCoords = new();
    private List<Vector2Int> _orderedPath = new();
    private Vector2Int _keyCoord;

    void Start()
    {
        CollectTiles();
        GenerateSafePath();
        ApplyTilesTagsAndScripts();
        SpawnKey();
    }

    void CollectTiles()
    {
        _tiles.Clear();
        foreach (Transform child in transform)
            _tiles.Add(child.gameObject);
    }

    Vector2Int TileCoord(GameObject tile)
    {
        var lp = tile.transform.localPosition;
        int x = Mathf.RoundToInt(lp.x + (cols - 1) / 2f);
        int z = Mathf.RoundToInt(lp.z + (rows - 1) / 2f);
        return new Vector2Int(x, z);
    }

    GameObject TileAt(Vector2Int c)
    {
        foreach (var t in _tiles)
            if (TileCoord(t) == c) return t;
        return null;
    }

    void GenerateSafePath()
    {
        _safeCoords.Clear();
        _orderedPath.Clear();

        float trapDensity = 1.0f;
        if (AIDirector.I != null && AIDirector.I.state != null)
            trapDensity = AIDirector.I.state.trapDensity;

        int targetLength = Mathf.RoundToInt((cols + rows) * basePathFactor * trapDensity);
        targetLength = Mathf.Clamp(targetLength, cols + rows - 2, cols * rows / 2);

        Vector2Int entry = ClampToGrid(entryCoord);
        Vector2Int exit  = ClampToGrid(exitCoord);

        // Pick key fraction from AIDirector
        float keyFrac = (AIDirector.I != null)
            ? AIDirector.I.ComputeRoom13KeyPathFraction()
            : 0.55f;

        // Pick an intermediate key target somewhere between entry and exit.
        // Linear interpolation across grid, then snap.
        Vector2 kf = Vector2.Lerp(entry, exit, keyFrac);
        // Nudge slightly off the straight line so the path bends through it
        kf += new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
        _keyCoord = ClampToGrid(new Vector2Int(Mathf.RoundToInt(kf.x), Mathf.RoundToInt(kf.y)));
        if (_keyCoord == entry || _keyCoord == exit)
            _keyCoord = ClampToGrid(new Vector2Int(_keyCoord.x + 1, _keyCoord.y + 1));

        // Two-leg walk: entry -> key, then key -> exit (force player through key).
        int leg1Target = Mathf.Max(2, Mathf.RoundToInt(targetLength * keyFrac));
        int leg2Target = Mathf.Max(2, targetLength - leg1Target);

        var visited = new HashSet<Vector2Int>();
        var leg1 = RandomWalkPath(entry, _keyCoord, leg1Target, visited);
        // Don't reuse the key tile as "already visited" for leg 2 except as start.
        var visited2 = new HashSet<Vector2Int>(leg1);
        visited2.Remove(_keyCoord);
        var leg2 = RandomWalkPath(_keyCoord, exit, leg2Target, visited2);

        // Stitch
        _orderedPath.AddRange(leg1);
        for (int i = 1; i < leg2.Count; i++) _orderedPath.Add(leg2[i]);
        foreach (var p in _orderedPath) _safeCoords.Add(p);

        Debug.Log($"[CheckboxFloor] Path generated. Length={_orderedPath.Count}, trapDensity={trapDensity:F2}, " +
                  $"entry={entry}, key={_keyCoord} (frac={keyFrac:F2}), exit={exit}");
    }

    Vector2Int ClampToGrid(Vector2Int c) =>
        new Vector2Int(Mathf.Clamp(c.x, 0, cols - 1), Mathf.Clamp(c.y, 0, rows - 1));

    List<Vector2Int> RandomWalkPath(Vector2Int start, Vector2Int end, int targetLen, HashSet<Vector2Int> alreadyVisited)
    {
        var path = new List<Vector2Int> { start };
        var visited = new HashSet<Vector2Int>(alreadyVisited) { start };
        var current = start;

        int safety = 250;
        while (current != end && safety-- > 0)
        {
            var options = new List<Vector2Int>();
            Vector2Int[] dirs = { new(0, 1), new(0, -1), new(1, 0), new(-1, 0) };

            foreach (var d in dirs)
            {
                var next = current + d;
                if (next.x < 0 || next.x >= cols || next.y < 0 || next.y >= rows) continue;
                if (visited.Contains(next)) continue;
                options.Add(next);
            }

            if (options.Count == 0) break;

            Vector2Int chosen;
            if (path.Count >= targetLen)
            {
                chosen = options[0];
                int bestDist = ManhDist(chosen, end);
                foreach (var o in options) {
                    int d = ManhDist(o, end);
                    if (d < bestDist) { bestDist = d; chosen = o; }
                }
            }
            else
            {
                if (Random.value < 0.4f)
                {
                    chosen = options[0];
                    int bestDist = ManhDist(chosen, end);
                    foreach (var o in options) {
                        int d = ManhDist(o, end);
                        if (d < bestDist) { bestDist = d; chosen = o; }
                    }
                }
                else chosen = options[Random.Range(0, options.Count)];
            }

            path.Add(chosen);
            visited.Add(chosen);
            current = chosen;
        }

        // Force-connect to end if walk didn't reach
        while (current.x != end.x) {
            current.x += (end.x > current.x) ? 1 : -1;
            if (!visited.Contains(current)) { path.Add(current); visited.Add(current); }
        }
        while (current.y != end.y) {
            current.y += (end.y > current.y) ? 1 : -1;
            if (!visited.Contains(current)) { path.Add(current); visited.Add(current); }
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
                var bt = tile.GetComponent<BulletTrap>();
                if (bt != null) Destroy(bt);

                // Safe tile colliders must stay solid so the player can stand.
                var safeCol = tile.GetComponent<Collider>();
                if (safeCol != null) safeCol.isTrigger = false;

                if (safeTileMat != null) {
                    var mr = tile.GetComponent<MeshRenderer>();
                    if (mr != null) mr.material = safeTileMat;
                }
                if (revealSafePath && debugSafeMat != null) {
                    var mr = tile.GetComponent<MeshRenderer>();
                    if (mr != null) mr.material = debugSafeMat;
                }
            }
            else
            {
                tile.tag = "TrapTile";
                var col = tile.GetComponent<Collider>();
                if (col != null) col.isTrigger = true;
                if (tile.GetComponent<BulletTrap>() == null)
                    tile.AddComponent<BulletTrap>();
                if (trapTileMat != null) {
                    var mr = tile.GetComponent<MeshRenderer>();
                    if (mr != null) mr.material = trapTileMat;
                }
            }
        }
    }

    void SpawnKey()
    {
        if (keyPrefab == null) {
            Debug.LogWarning("[CheckboxFloor] keyPrefab not set — skipping key spawn.");
            return;
        }
        var keyTile = TileAt(_keyCoord);
        if (keyTile == null) {
            Debug.LogWarning($"[CheckboxFloor] no tile at key coord {_keyCoord}");
            return;
        }
        Vector3 pos = keyTile.transform.position + Vector3.up * keyHeightOffset;
        Transform parent = keyParent != null ? keyParent : transform.parent;
        Instantiate(keyPrefab, pos, Quaternion.identity, parent);
        Debug.Log($"[CheckboxFloor] key spawned at coord {_keyCoord} pos {pos}");
    }

    void OnDrawGizmosSelected()
    {
        if (_orderedPath == null || _orderedPath.Count < 2) return;
        Gizmos.color = debugSafeColor;
        for (int i = 1; i < _orderedPath.Count; i++) {
            var a = TileAt(_orderedPath[i - 1]);
            var b = TileAt(_orderedPath[i]);
            if (a != null && b != null)
                Gizmos.DrawLine(a.transform.position + Vector3.up * 0.2f,
                                b.transform.position + Vector3.up * 0.2f);
        }
        Gizmos.color = Color.yellow;
        var k = TileAt(_keyCoord);
        if (k != null) Gizmos.DrawWireSphere(k.transform.position + Vector3.up * 0.6f, 0.4f);
    }
}
