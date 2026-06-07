// File: RealKeyRelocator.cs
using UnityEngine;

public class RealKeyRelocator : MonoBehaviour {
    public Transform realKey;
    public Transform farPosition;   // drag the far-away empty Transform here

    bool _moved;

    public void Relocate() {
        if (_moved || realKey == null || farPosition == null) return;
        _moved = true;
        realKey.position = farPosition.position;
        realKey.rotation = farPosition.rotation;
    }
}