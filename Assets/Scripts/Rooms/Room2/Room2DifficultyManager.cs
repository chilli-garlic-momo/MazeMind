using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MazeMind.Core;

public static class Room2DifficultyManager
{
    public struct Profile
    {
        public int laserCount;
        public float laserDamageMultiplier;
        public int gemCount;
        public int dacoitGemDemand;
    }

    public static Profile GetProfile()
    {
        if (AIDirector.I == null)
            return GetDefaultProfile();

        AIDirector.I.RecomputeTags();

        return GetProfileFromTags(
            AIDirector.I.state.activeTags
        );
    }

    public static int GetLaserCount(int maxPositions)
    {
        var profile = GetProfile();

        return Mathf.Clamp(
            profile.laserCount,
            0,
            maxPositions
        );
    }

    public static float GetLaserDamageMultiplier()
    {
        return GetProfile().laserDamageMultiplier;
    }

    public static int GetGemCount(int maxSpawnPoints)
    {
        var profile = GetProfile();

        return Mathf.Clamp(
            profile.gemCount,
            1,
            maxSpawnPoints
        );
    }

    public static int GetDacoitDemand()
    {
        return GetProfile().dacoitGemDemand;
    }

    static Profile GetProfileFromTags(IEnumerable<string> tags)
    {
        if (tags.Contains("Reckless"))
            return GetRecklessProfile();

        if (tags.Contains("Paranoid"))
            return GetParanoidProfile();

        if (tags.Contains("Collector"))
            return GetCollectorProfile();

        if (tags.Contains("Speedrunner"))
            return GetSpeedrunnerProfile();

        return GetDefaultProfile();
    }

    static Profile GetRecklessProfile()
    {
        return new Profile
        {
            laserCount = 6,
            laserDamageMultiplier = 1.5f,
            gemCount = 3,
            dacoitGemDemand = 5
        };
    }

    static Profile GetParanoidProfile()
    {
        return new Profile
        {
            laserCount = 2,
            laserDamageMultiplier = 0.75f,
            gemCount = 6,
            dacoitGemDemand = 2
        };
    }

    static Profile GetCollectorProfile()
    {
        return new Profile
        {
            laserCount = 4,
            laserDamageMultiplier = 1.0f,
            gemCount = 5,
            dacoitGemDemand = 4
        };
    }

    static Profile GetSpeedrunnerProfile()
    {
        return new Profile
        {
            laserCount = 5,
            laserDamageMultiplier = 1.25f,
            gemCount = 3,
            dacoitGemDemand = 4
        };
    }

    static Profile GetDefaultProfile()
    {
        return new Profile
        {
            laserCount = 4,
            laserDamageMultiplier = 1.0f,
            gemCount = 4,
            dacoitGemDemand = 3
        };
    }
}