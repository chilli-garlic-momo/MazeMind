using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeMind.Core {
    [Serializable] public class DecisionEntry {
        public float t;
        public string trigger;   // "OnSectionEnter", etc
        public string section;
        public string ruleName;
        public string playerLine; // cryptic
        public string devLine;    // full
    }

    public class DecisionLogger : MonoBehaviour {
        public static DecisionLogger I { get; private set; }
        public readonly List<DecisionEntry> entries = new();
        public event Action<DecisionEntry> OnNew;

        void Awake() {
            if (I != null) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);
        }

        public void Log(string trigger, string section, string ruleName, string playerLine, string devLine) {
            var e = new DecisionEntry {
                t = Time.time, trigger = trigger, section = section,
                ruleName = ruleName, playerLine = playerLine, devLine = devLine
            };
            entries.Add(e);
            Debug.Log($"[AI] {trigger} {section} :: {ruleName} :: {devLine}");
            OnNew?.Invoke(e);
        }
    }
}