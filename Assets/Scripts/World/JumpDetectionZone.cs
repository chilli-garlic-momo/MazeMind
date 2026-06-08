using UnityEngine;
using MazeMind.Core;

public class JumpDetectionZone : MonoBehaviour
{
    public SlidingPlatform platformToSlide;

    private bool _triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        var controller = other.GetComponent<PlayerController>();
        if (controller == null) return;

        // Only trigger if the player is moving forward AND in the air (i.e. actually jumping)
        if (controller.IsInAir && controller.Velocity.y > 0)
        {
            _triggered = true;
            if (platformToSlide != null)
                platformToSlide.OnPlayerJumped();
        }
    }

    public void ResetTrigger()
    {
        _triggered = false;
    }
}