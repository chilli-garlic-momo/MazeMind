using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room3MemoryManager : MonoBehaviour
{
    [Header("Tiles")]
    public List<MemoryTile> safeTiles = new();
    public List<MemoryTile> unsafeTiles = new();

    [Header("Materials")]
    public Material safeMaterial;
    public Material unsafeMaterial;
    public Material hiddenMaterial;

    [Header("Settings")]
    public float previewTime = 0.5f;

    private void Start()
    {
        AssignTiles();
        StartCoroutine(ShowPattern());
    }

    void AssignTiles()
    {
        foreach (var tile in safeTiles)
        {
            if (tile == null)
                continue;

            tile.isSafe = true;
        }

        foreach (var tile in unsafeTiles)
        {
            if (tile == null)
                continue;

            tile.isSafe = false;
        }
    }

    IEnumerator ShowPattern()
    {
        foreach (var tile in safeTiles)
        {
            if (tile != null)
                tile.GetComponent<Renderer>().material =
                    safeMaterial;
        }

        foreach (var tile in unsafeTiles)
        {
            if (tile != null)
                tile.GetComponent<Renderer>().material =
                    unsafeMaterial;
        }

        yield return new WaitForSeconds(previewTime);

        foreach (var tile in safeTiles)
        {
            if (tile != null)
                tile.GetComponent<Renderer>().material =
                    hiddenMaterial;
        }

        foreach (var tile in unsafeTiles)
        {
            if (tile != null)
                tile.GetComponent<Renderer>().material =
                    hiddenMaterial;
        }
    }
}