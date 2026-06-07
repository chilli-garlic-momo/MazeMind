using UnityEngine;

public class Dacoit : MonoBehaviour
{
    public int gemDemand = 3;

    private bool paid = false;

    private void OnTriggerEnter(Collider other)
    {
        if (paid) return;

        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.gems >= gemDemand)
            {
                GameManager.Instance.gems -= gemDemand;

                Debug.Log("Dacoit accepted payment");

                paid = true;
            }
            else
            {
                Debug.Log("Need " + gemDemand + " gems");
            }
        }
    }
}