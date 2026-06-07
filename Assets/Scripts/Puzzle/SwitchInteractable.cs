using UnityEngine;

public class SwitchInteractable : Interactable
{
    public bool isOn = false;

    public override void Interact()
    {
        isOn = !isOn;

        Debug.Log("Switch " + (isOn ? "ON" : "OFF"));
    }
}