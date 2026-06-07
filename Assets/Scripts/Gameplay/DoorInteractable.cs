using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    private bool opened = false;

    public void TryOpenDoor()
    {
        if (opened)
            return;

        if (GameManager.Instance.hasKey)
        {
            transform.Rotate(0f, 90f, 0f);

            opened = true;

            Debug.Log("Door Opened");
        }
        else
        {
            Debug.Log("Need a Key");
        }
    }
}