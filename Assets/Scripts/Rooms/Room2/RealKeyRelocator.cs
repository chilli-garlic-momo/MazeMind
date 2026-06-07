// File: RealKeyRelocator.cs 
using UnityEngine;

public class RealKeyRelocator : MonoBehaviour
{
    [Tooltip("The real key GameObject — starts at easy near position.")]
    public Transform realKey;

    [Tooltip("Empty Transform at the hard far position it moves to if dummy is grabbed.")]
    public Transform farPosition;

    bool _moved;

    public void Relocate()
    {
        if (_moved || realKey == null || farPosition == null) return;
        _moved = true;

        // Detach from any parent so position is in world space
        realKey.SetParent(null);
        realKey.position = farPosition.position;
        realKey.rotation = farPosition.rotation;

        Debug.Log("[Relocator] Real key moved to far position.");
    }

    public bool HasMoved => _moved;
}