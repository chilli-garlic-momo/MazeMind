using UnityEngine;

public class LaserHazard : MonoBehaviour
{
    public int baseDamage = 10;
    public float damageInterval = 0.5f;

    private float lastDamageTime;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time < lastDamageTime + damageInterval)
            return;

        PlayerHealth hp =
            other.GetComponentInParent<PlayerHealth>();

        if (hp == null)
            return;

        float multiplier = Room2DifficultyManager.GetLaserDamageMultiplier();
        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

        hp.Damage(finalDamage);
        lastDamageTime = Time.time;

        Debug.Log($"Laser Damage Applied: {finalDamage}");
    }
}