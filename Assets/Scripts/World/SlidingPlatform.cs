using System.Collections;
using UnityEngine;
using MazeMind.Core;

public class SlidingPlatform : MonoBehaviour
{
    [Header("Slide behavior")]
    [Tooltip("How far the platform slides backward (Z direction) during the jump.")]
    public float slideDistance = 4.5f;  // bumped — was too subtle in playtest

    [Tooltip("How fast the platform slides. Higher = harder to reach.")]
    public float slideSpeed = 6f;

    [Tooltip("If true, this is the unjumpable Gap C. Slides infinitely, no return.")]
    public bool isForcedFall = false;

    [Tooltip("How fast the forced-fall platform retreats. Higher = more obviously unreachable.")]
    public float forcedFallSpeed = 14f;

    [Header("Trigger zone (when player jumps off this near platform, slide begins)")]
    public Transform nearPlatform;
    public Collider jumpDetectionZone;

    [Header("Reset behavior")]
    public float resetDelay = 2f;

    [Header("AI Director modulation")]
    public bool aiModulated = true;

    private Vector3 _originalPos;
    private bool _hasTriggered = false;

    void Start() { _originalPos = transform.position; }

    public void OnPlayerJumped()
    {
        if (_hasTriggered) return;
        _hasTriggered = true;

        float finalSlideDistance = ComputeSlideDistance();
        float finalSlideSpeed = ComputeSlideSpeed();

        if (isForcedFall)
            StartCoroutine(SlideForever(forcedFallSpeed));
        else
            StartCoroutine(SlideAndHold(finalSlideDistance, finalSlideSpeed));
    }

    float ComputeSlideDistance()
    {
        if (!aiModulated || AIDirector.I == null) return slideDistance;
        var tags = AIDirector.I.state.activeTags;
        float multiplier = 1.0f;
        if (tags.Contains("Reckless") || tags.Contains("Speedrunner")) multiplier = 1.4f;
        else if (tags.Contains("Paranoid")) multiplier = 0.7f;
        if (PlayerMetrics.I != null && PlayerMetrics.I.deathsInSection.TryGetValue("1.2", out int deaths))
            multiplier *= Mathf.Max(0.5f, 1f - (deaths * 0.15f));
        return slideDistance * multiplier;
    }

    float ComputeSlideSpeed()
    {
        if (!aiModulated || AIDirector.I == null) return slideSpeed;
        var tags = AIDirector.I.state.activeTags;
        float multiplier = 1.0f;
        if (tags.Contains("Reckless") || tags.Contains("Speedrunner")) multiplier = 1.3f;
        else if (tags.Contains("Paranoid")) multiplier = 0.8f;
        return slideSpeed * multiplier;
    }

    IEnumerator SlideAndHold(float distance, float speed)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(0, 0, distance);

        float elapsed = 0f;
        float duration = Mathf.Max(0.05f, distance / speed);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }
        transform.position = targetPos;

        yield return new WaitForSeconds(resetDelay);
        ResetPlatform();
    }

    IEnumerator SlideForever(float speed)
    {
        while (true)
        {
            transform.position += new Vector3(0, 0, speed * Time.deltaTime);
            yield return null;
        }
    }

    public void ResetPlatform()
    {
        StopAllCoroutines();
        transform.position = _originalPos;
        _hasTriggered = false;
    }
}
