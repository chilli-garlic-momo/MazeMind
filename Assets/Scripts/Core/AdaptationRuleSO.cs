using UnityEngine;

namespace MazeMind.Core {
    public enum TriggerKind { OnSectionEnter, OnSectionDeath, OnSectionExit, OnRoomExit }

    [CreateAssetMenu(menuName="MazeMind/Adaptation Rule")]
    public class AdaptationRuleSO : ScriptableObject {
        public string ruleName;
        public int priority = 10;
        public TriggerKind trigger;
        public string sectionId;     // "" = any
        public int roomId = -1;      // -1 = any

        [Header("Conditions")]
        public bool requireSpeedrunner;
        public bool requireCollector;
        public bool requireParanoid;
        public bool requireReckless;
        public int  minDeathsInSection = 0;
        public float minHesitation = -1;

        [Header("Effects")]
        public float multTrapDensity = 1f;
        public float multHazardSpeed = 1f;
        public int   setDacoitDemand = -1;
        public bool  setVoiceHint13;
        public bool  setGuideAnimal13;

        [Header("Log")]
        [TextArea] public string playerLine;
        [TextArea] public string devLine;

        public bool Matches(PlayerMetrics m, AdaptationState s, TriggerKind t, string secId, int rId) {
            if (t != trigger) return false;
            if (!string.IsNullOrEmpty(sectionId) && sectionId != secId) return false;
            if (roomId >= 0 && roomId != rId) return false;
            if (requireSpeedrunner && !s.activeTags.Contains("Speedrunner")) return false;
            if (requireCollector   && !s.activeTags.Contains("Collector"))   return false;
            if (requireParanoid    && !s.activeTags.Contains("Paranoid"))    return false;
            if (requireReckless    && !s.activeTags.Contains("Reckless"))    return false;
            if (minDeathsInSection > 0) {
                if (!m.deathsInSection.TryGetValue(secId, out var d) || d < minDeathsInSection) return false;
            }
            if (minHesitation >= 0 && m.avgHesitationSec < minHesitation) return false;
            return true;
        }

        public void Apply(AdaptationState s) {
            s.trapDensity *= multTrapDensity;
            s.hazardSpeedMultiplier *= multHazardSpeed;
            if (setDacoitDemand >= 0) s.dacoitGemDemand = setDacoitDemand;
            if (setVoiceHint13)   s.give13VoiceHint = true;
            if (setGuideAnimal13) s.spawn13GuideAnimal = true;
        }
    }
}