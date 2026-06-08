using UnityEngine; using UnityEngine.AI; using MazeMind.Core;

public class GhostController : MonoBehaviour {
    public Transform player;
    public NavMeshAgent agent;
    public float baseSpeed = 2.0f;
    public float dpsContact = 15f;

    void Update() {
        if (AIDirector.I == null || AIDirector.I.state == null) return;
        if (agent == null || player == null) return;
        float mult = AIDirector.I?.state?.hazardSpeedMultiplier ?? 1f;
        agent.speed = baseSpeed * mult;
        agent.SetDestination(player.position);
    }

    void OnTriggerStay(Collider o) {
        if (!o.CompareTag("Player")) return;
        var hp = o.GetComponent<PlayerHealth>();
        if (hp == null) return;
        hp.Damage(Mathf.RoundToInt(dpsContact * Time.deltaTime));
    }
}
