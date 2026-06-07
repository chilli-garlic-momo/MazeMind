// File: Room2ObserverMessages.cs 
using System.Collections;
using UnityEngine;
using MazeMind.Core;

public class Room2ObserverMessages : MonoBehaviour
{

    [System.Serializable]
    public struct TimedMessage
    {
        public float afterSeconds;
        [TextArea] public string playerLine;
        public string devNote;
    }

    [Header("Messages sent at fixed times after room start")]
    public TimedMessage[] scheduled = new TimedMessage[] {
        new() { afterSeconds = 2f,
                playerLine = "Session started. Observing.",
                devNote    = "Room2: observer opened" },
        new() { afterSeconds = 12f,
                playerLine = "You seem impatient.",
                devNote    = "Room2: speed-taunt at 12s" },
        new() { afterSeconds = 25f,
                playerLine = "Interesting choice.",
                devNote    = "Room2: neutral taunt at 25s" },
        new() { afterSeconds = 40f,
                playerLine = "The maze noticed.",
                devNote    = "Room2: ambient taunt at 40s" },
    };

    void Start()
    {
        foreach (var msg in scheduled)
            StartCoroutine(SendAt(msg));
    }

    IEnumerator SendAt(TimedMessage msg)
    {
        yield return new WaitForSeconds(msg.afterSeconds);
        DecisionLogger.I?.Log(
            "Observer", "2.ambient", "AmbientMessage",
            msg.playerLine, msg.devNote);
    }
}