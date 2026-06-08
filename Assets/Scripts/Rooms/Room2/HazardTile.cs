using System.Collections;
using UnityEngine;
using MazeMind.Core;

public class HazardTile : MonoBehaviour
{
    public int damageOnContact = 10;
    public float damageCooldown = 1f;

    private Coroutine _ticker;
    private bool _playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Hazard entered by Player");

        PlayerHealth hp = other.GetComponent<PlayerHealth>();

        if (hp == null)
        {
            Debug.Log("PlayerHealth not found");
            return;
        }

        Debug.Log("PlayerHealth found");

        _playerInside = true;

        if (_ticker != null)
            StopCoroutine(_ticker);

        _ticker = StartCoroutine(Tick(hp));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        _playerInside = false;

        if (_ticker != null)
        {
            StopCoroutine(_ticker);
            _ticker = null;
        }

        Debug.Log("Player left hazard");
    }

    private IEnumerator Tick(PlayerHealth hp)
    {
        while (_playerInside)
        {
            hp.Damage(damageOnContact);

            Debug.Log("Hazard Damage Applied");

            yield return new WaitForSeconds(damageCooldown);
        }
    }
}