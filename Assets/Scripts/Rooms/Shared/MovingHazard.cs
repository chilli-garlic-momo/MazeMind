// File: MovingHazard.cs
using UnityEngine; using MazeMind.Core;

public class MovingHazard : MonoBehaviour {
    public enum Axis { X, Y, Z }

    [Header("Movement")]
    public Axis axis = Axis.X;
    public float baseSpeed = 2f;
    public float range = 4f;           // half-extent from origin in both directions

    [Header("Damage")]
    public int damageOnContact = 20;

    Vector3 _origin;
    int     _dir = 1;

    void Start() => _origin = transform.position;

    void Update() {
        float speed = baseSpeed;
        if (AIDirector.I?.state != null)
            speed *= AIDirector.I.state.hazardSpeedMultiplier;

        transform.position += Dir3() * (speed * _dir * Time.deltaTime);

        float dist = Vector3.Dot(transform.position - _origin, Dir3());
        if (dist >= range)  { _dir = -1; transform.position = _origin + Dir3() *  range; }
        if (dist <= -range) { _dir =  1; transform.position = _origin + Dir3() * -range; }
    }

    void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        var hp = other.GetComponent<PlayerHealth>();
        hp?.Damage(damageOnContact);
    }

    Vector3 Dir3() => axis switch {
        Axis.X => Vector3.right,
        Axis.Y => Vector3.up,
        _      => Vector3.forward
    };
}