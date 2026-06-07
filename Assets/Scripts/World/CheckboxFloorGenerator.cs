using System.Collections.Generic;
using UnityEngine;
using MazeMind.Core;

/// <summary>
/// Section 1.3 laser-maze floor.
/// v11: exposes Regenerate() so Section13Director can shuffle the safe path
/// every time the player re-enters 1.3 (after a death OR after being
/// bounced back from 1.5 for missing the key).
/// </summary>
public class CheckboxFloorGenerator : MonoBehaviour
{
    [Header("Grid dimensions")]
    public int cols = 10;
    public int rows = 6;

    [Header("Entry / exit doorways (lists of tiles)")]
    public List<Vector2Int> entryCoords = new() { new Vector2Int(0, 0) };
    public List<Vector2Int> exitCoords  = new() { new Vector2Int(9, 5) };

    [Header("Path generation")]
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
    private GameObject _spawnedKey;

    void Start()
    {
        CollectTiles();
        Regenerate();
    }

    /// <summary>
    /// Public — clears existing key + bullet traps + materials, then
    /// generates a fresh safe path & re-spawns the key.
    /// Safe to call repeatedly (called by Section13Director on re-entry).
    /// </summary>
    public void Regenerate()
    {
        if (_tiles.Count == 0) CollectTiles();

        // Wipe previous key (key may have been picked up already — handle null)
        if (_spawnedKey != null) { Destroy(_spawnedKey); _spawnedKey = null; }

        // Wipe previous bullet traps so traps don't pile up across regens
        foreach (var t in _tiles) {
            var bt = t.GetComponent<BulletTrap>();
            if (bt != null) Destroy(bt);
        }

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

    Vector2Int InwardNeighbor(Vector2Int c)
    {
        int dx = 0, dz = 0;
        if (c.x == 0) dx = 1; else if (c.x == cols - 1) dx = -1;
        if (c.y == 0) dz = 1; else if (c.y == rows - 1) dz = -1;
        if (dx == 0 && dz == 0) return c;
        return new Vector2Int(c.x + dx, c.y + dz);
    }

    void GenerateSafePath()
    {
        _safeCoords.Clear();
        _orderedPath.Clear();

        if (entryCoords == null || entryCoords.Count == 0) entryCoords = new() { new Vector2Int(0, 0) };
        if (exitCoords  == null || exitCoords.Count  == 0) exitCoords  = new() { new Vector2Int(cols - 1, rows - 1) };

        float trapDensity = 1.0f;
        if (AIDirector.I != null && AIDirector.I.state != null)
            trapDensity = AIDirector.I.state.trapDensity;

        int targetLength = Mathf.RoundToInt((cols + rows) * basePathFactor * trapDensity);
        targetLength = Mathf.Clamp(targetLength, cols + rows - 2, cols * rows / 2);

        Vector2Int entry = ClampToGrid(entryCoords[Random.Range(0, entryCoords.Count)]);
        Vector2Int exit  = ClampToGrid(exitCoords [Random.Range(0, exitCoords.Count)]);

        float keyFrac = (AIDirector.I != null)
            ? AIDirector.I.ComputeRoom13KeyPathFraction()
            : 0.55f;

        Vector2 kf = Vector2.Lerp(entry, exit, keyFrac);
        kf += new Vector2(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
        _keyCoord = ClampToGrid(new Vector2Int(Mathf.RoundToInt(kf.x), Mathf.RoundToInt(kf.y)));
        if (_keyCoord == entry || _keyCoord == exit)
            _keyCoord = ClampToGrid(new Vector2Int(_keyCoord.x + 1, _keyCoord.y + 1));

        int leg1Target = Mathf.Max(2, Mathf.RoundToInt(targetLength * keyFrac));
        int leg2Target = Mathf.Max(2, targetLength - leg1Target);

        var visited = new HashSet<Vector2Int>();
        var leg1 = RandomWalkPath(entry, _keyCoord, leg1Target, visited);
        var visited2 = new HashSet<Vector2Int>(leg1);
        visited2.Remove(_keyCoord);
        var leg2 = RandomWalkPath(_keyCoord, exit, leg2Target, visited2);

        _orderedPath.AddRange(leg1);
        for (int i = 1; i < leg2.Count; i++) _orderedPath.Add(leg2[i]);
        foreach (var p in _orderedPath) _safeCoords.Add(p);

        foreach (var c in entryCoords) {
            var cc = ClampToGrid(c);
            _safeCoords.Add(cc);
            _safeCoords.Add(ClampToGrid(InwardNeighbor(cc)));
        }
        foreach (var c in exitCoords) {
            var cc = ClampToGrid(c);
            _safeCoords.Add(cc);
            _safeCoords.Add(ClampToGrid(InwardNeighbor(cc)));
        }

        Debug.Log($"[CheckboxFloor] Path generated. Length={_orderedPath.Count}, trapDensity={trapDensity:F2}, " +
                  $"entry={entry} (of {entryCoords.Count}), key={_keyCoord} (frac={keyFrac:F2}), exit={exit} (of {exitCoords.Count})");
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
            foreach (var d in dirs) {
                var next = current + d;
                if (next.x < 0 || next.x >= cols || next.y < 0 || next.y >= rows) continue;
                if (visited.Contains(next)) continue;
                options.Add(next);
            }
            if (options.Count == 0) break;

            Vector2Int chosen;
            if (path.Count >= targetLen) {
                chosen = options[0];
                int bestDist = ManhDist(chosen, end);
                foreach (var o in options) { int d = ManhDist(o, end); if (d < bestDist) { bestDist = d; chosen = o; } }
            } else {
                if (Random.value < 0.4f) {
                    chosen = options[0];
                    int bestDist = ManhDist(chosen, end);
                    foreach (var o in options) { int d = ManhDist(o, end); if (d < bestDist) { bestDist = d; chosen = o; } }
                } else chosen = options[Random.Range(0, options.Count)];
            }

            path.Add(chosen); visited.Add(chosen); current = chosen;
        }

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

    int ManhDist(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    void ApplyTilesTagsAndScripts()
    {
        foreach (var tile in _tiles) {
            var coord = TileCoord(tile);
            bool isSafe = _safeCoords.Contains(coord);

            if (isSafe) {
                tile.tag = "SafeTile";
                var bt = tile.GetComponent<BulletTrap>();
                if (bt != null) Destroy(bt);
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
            } else {
                tile.tag = "TrapTile";
                var col = tile.GetComponent<Collider>();
                if (col != null) col.isTrigger = true;
                if (tile.GetComponent<BulletTrap>() == null) tile.AddComponent<BulletTrap>();
                if (trapTileMat != null) {
                    var mr = tile.GetComponent<MeshRenderer>();
                    if (mr != null) mr.material = trapTileMat;
                }
            }
        }
    }

    void SpawnKey()
    {
        // If the player already picked up the key once, don't spawn again.
        if (GameManager.Instance != null && GameManager.Instance.hasKey) return;
        if (keyPrefab == null) { Debug.LogWarning("[CheckboxFloor] keyPrefab not set."); return; }
        var keyTile = TileAt(_keyCoord);
        if (keyTile == null) { Debug.LogWarning($"[CheckboxFloor] no tile at {_keyCoord}"); return; }
        Vector3 pos = keyTile.transform.position + Vector3.up * keyHeightOffset;
        Transform parent = keyParent != null ? keyParent : transform.parent;
        _spawnedKey = Instantiate(keyPrefab, pos, Quaternion.identity, parent);
        Debug.Log($"[CheckboxFloor] key spawned at {_keyCoord} pos {pos}");
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
