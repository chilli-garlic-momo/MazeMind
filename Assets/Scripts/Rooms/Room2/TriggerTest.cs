using UnityEngine;

public class TriggerTest : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("ENTERED: " + other.name);
    }

    private void OnTriggerStay(Collider other)
    {
        Debug.Log("STAYING: " + other.name);
    }
}