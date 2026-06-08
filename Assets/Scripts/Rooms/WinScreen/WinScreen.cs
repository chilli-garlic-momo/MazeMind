using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using MazeMind.Core;

public class WinScreen : MonoBehaviour
{
    public string firstLevelScene = "Boot";

    [Header("UI")]
    public TMP_Text profileTitleText;
    public TMP_Text profileDescriptionText;
    public TMP_Text timelineText;

    void Start()
    {
        BuildPlayerProfile();
        BuildTimeline();
    }

    void BuildPlayerProfile()
    {
        if (AIDirector.I == null)
            return;

        var tags = AIDirector.I.state.activeTags;

        string title = "Balanced Adventurer";
        string description =
            "You showed a balanced playstyle with no dominant behaviour pattern.";

        if (tags.Contains("Collector") && tags.Contains("Explorer"))
        {
            title = "The Treasure Hunter";
            description =
                "You explored thoroughly and collected resources whenever possible. " +
                "The AI Director responded by increasing collection-focused challenges.";
        }
        else if (tags.Contains("Speedrunner") && tags.Contains("Reckless"))
        {
            title = "The Daredevil";
            description =
                "You moved fast, accepted danger and rarely hesitated. " +
                "The AI Director increased pressure and hazard intensity.";
        }
        else if (tags.Contains("Paranoid"))
        {
            title = "The Strategist";
            description =
                "You frequently paused before decisions and approached the maze cautiously. " +
                "The AI Director adjusted pacing and challenge presentation.";
        }
        else if (tags.Contains("Collector"))
        {
            title = "The Hoarder";
            description =
                "You prioritised collecting everything you could find. " +
                "The AI Director increased resource-related objectives.";
        }
        else if (tags.Contains("Explorer"))
        {
            title = "The Explorer";
            description =
                "You preferred discovering new paths and uncovering hidden areas. " +
                "The AI Director encouraged exploration-driven gameplay.";
        }
        else if (tags.Contains("Speedrunner"))
        {
            title = "The Speedrunner";
            description =
                "You rushed through sections with minimal hesitation. " +
                "The AI Director responded with faster-paced encounters.";
        }
        else if (tags.Contains("Reckless"))
        {
            title = "The Risk Taker";
            description =
                "You willingly accepted damage and dangerous routes. " +
                "The AI Director increased hazard pressure.";
        }

        if(profileTitleText != null)
            profileTitleText.text = title;

        if(profileDescriptionText != null)
            profileDescriptionText.text = description;
    }

    void BuildTimeline()
    {
        if (timelineText == null)
            return;

        if (DecisionLogger.I == null)
        {
            timelineText.text = "No adaptation data available.";
            return;
        }

        StringBuilder sb = new StringBuilder();

        sb.AppendLine("AI ADAPTATION TIMELINE");
        sb.AppendLine();

        foreach (var entry in DecisionLogger.I.entries)
        {
            if (string.IsNullOrEmpty(entry.devLine))
                continue;

            sb.AppendLine(
                $"[{entry.t:F1}s] {entry.ruleName}\n" +
                $"→ {entry.devLine}\n");
        }

        timelineText.text = sb.ToString();
    }

    public void PlayAgain()
    {
        Debug.Log("PLAY AGAIN CLICKED");
        SceneManager.LoadScene(firstLevelScene);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT CLICKED");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}