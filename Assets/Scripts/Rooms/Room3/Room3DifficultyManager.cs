using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MazeMind.Core;

public static class Room3DifficultyManager
{
    public struct Profile
    {
        public int fakeGemCount;
        public float fakeGemPenalty;
    }

    static Profile GetDefaultProfile()
    {
        return new Profile
        {
            fakeGemCount = 4,
            fakeGemPenalty = 1f
        };
    }

    static Profile GetProfileFromTags(IEnumerable<string> tags)
    {
        Profile p = GetDefaultProfile();

        if (tags == null)
            return p;

        // Collectors get punished harder
        if (tags.Contains("Collector"))
        {
            p.fakeGemCount += 3;
        }

        // Explorers see more deception
        if (tags.Contains("Explorer"))
        {
            p.fakeGemCount += 2;
        }

        // Speedrunners get fewer fake gems
        if (tags.Contains("Speedrunner"))
        {
            p.fakeGemCount -= 1;
        }

        // Reckless players get stronger penalties
        if (tags.Contains("Reckless"))
        {
            p.fakeGemPenalty = 2f;
        }

        p.fakeGemCount = Mathf.Clamp(p.fakeGemCount, 1, 10);

        return p;
    }

    public static Profile GetProfile()
    {
        if (AIDirector.I == null)
            return GetDefaultProfile();

        return GetProfileFromTags(
            AIDirector.I.state.activeTags
        );
    }

    public static void LogProfile()
    {
        Profile p = GetProfile();

        Debug.Log(
            $"[Room3DifficultyManager] FakeGems={p.fakeGemCount}, " +
            $"Penalty={p.fakeGemPenalty}"
        );
    }
}