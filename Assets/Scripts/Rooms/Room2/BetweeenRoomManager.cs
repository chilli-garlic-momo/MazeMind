// File: BetweenRoomManager.cs — PATCHED (guards against double-fire)
using System.Collections; using System.Text;
using TMPro; using UnityEngine; using UnityEngine.SceneManagement;
using MazeMind.Core;

public class BetweenRoomManager : MonoBehaviour {
    public static BetweenRoomManager I { get; private set; }

    [Header("Between-room screen root")]
    public GameObject screen;
    public TMP_Text   headerText;
    public TMP_Text   metricsText;
    public TMP_Text   adaptationsText;
    public TMP_Text   aiMessageText;

    [Header("Timing")]
    public float displaySeconds = 6f;

    bool _loading; // NEW: prevents multiple ShowScreen calls from queueing extra scene loads

    void Awake() {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        if (screen != null) screen.SetActive(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Reset the guard when a new scene actually finishes loading so future
    // transitions (e.g. Play Again) still work.
    void OnSceneLoaded(Scene s, LoadSceneMode mode) {
        _loading = false;
        if (screen != null) screen.SetActive(false);
    }

    public void ShowScreen(string nextScene) {
        if (_loading) {
            Debug.Log($"[BetweenRoomManager] Ignoring duplicate ShowScreen({nextScene}); already loading.");
            return;
        }
        _loading = true;
        if (screen != null) screen.SetActive(true);
        PopulateUI();
        StartCoroutine(LoadAfterDelay(nextScene));
    }

    void PopulateUI() {
        var m = PlayerMetrics.I;
        var s = AIDirector.I?.state;
        var log = DecisionLogger.I;

        if (headerText != null)
            headerText.text = "MAZE ANALYSIS COMPLETE";

        if (metricsText != null && m != null) {
            var sb = new StringBuilder();
            sb.AppendLine($"Speed:       {m.avgMoveSpeed:F1} u/s");
            sb.AppendLine($"Exploration: {m.explorationPct:P0}");
            sb.AppendLine($"Hesitation:  {m.avgHesitationSec:F1} s");
            sb.AppendLine($"Damage:      {m.damageTaken} HP");
            sb.AppendLine($"Gems:        {m.gemCollectionPct:P0}");
            if (s != null) {
                sb.AppendLine("─────────────────────");
                sb.AppendLine($"Tags: {(s.activeTags.Count > 0 ? string.Join(", ", s.activeTags) : "none")}");
            }
            metricsText.text = sb.ToString();
        }

        if (adaptationsText != null && log != null) {
            var sb = new StringBuilder();
            sb.AppendLine("ADAPTATIONS APPLIED:");
            int shown = 0;
            for (int i = log.entries.Count - 1; i >= 0 && shown < 5; i--) {
                var e = log.entries[i];
                if (!string.IsNullOrEmpty(e.devLine)) {
                    sb.AppendLine($"• {e.ruleName}: {e.devLine}");
                    shown++;
                }
            }
            if (shown == 0) sb.AppendLine("• No adaptations this room.");
            adaptationsText.text = sb.ToString();
        }

        if (aiMessageText != null && s != null)
            aiMessageText.text = PickAtmosphericLine(s);
    }

    string PickAtmosphericLine(AdaptationState s) {
        if (s.activeTags.Contains("Speedrunner"))
            return "\"You move fast. The next room was designed for that.\"";
        if (s.activeTags.Contains("Collector"))
            return "\"Every gem. Every one. The maze noticed.\"";
        if (s.activeTags.Contains("Paranoid"))
            return "\"Your hesitation will cost you. Or save you. I haven't decided.\"";
        if (s.activeTags.Contains("Reckless"))
            return "\"You are not careful. The maze appreciates the entertainment.\"";
        return "\"Observation complete. Adjustments made. Continue.\"";
    }

    IEnumerator LoadAfterDelay(string nextScene) {
        yield return new WaitForSeconds(displaySeconds);
        if (screen != null) screen.SetActive(false);
        SceneManager.LoadScene(nextScene);
    }
}
