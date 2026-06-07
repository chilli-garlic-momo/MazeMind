using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MazeMind.Core {
    public class AIDirector : MonoBehaviour {
        public static AIDirector I { get; private set; }
        public List<AdaptationRuleSO> rules = new();
        public AdaptationState state = new();

        void Awake() {
            if (I != null) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);
        }

        public void RecomputeTags() {
            var m = PlayerMetrics.I; var s = state;
            s.activeTags.Clear();
            if (m.avgMoveSpeed > 4f && m.explorationPct < 0.45f) s.activeTags.Add("Speedrunner");
            if (m.gemCollectionPct > 0.8f)                       s.activeTags.Add("Collector");
            if (m.avgHesitationSec > 3.5f && m.damageTaken < 30) s.activeTags.Add("Paranoid");
            if (m.damageTaken > 80 && m.avgMoveSpeed > 4f)       s.activeTags.Add("Reckless");
        }

        public void Fire(TriggerKind t, string sectionId, int roomId) {
            RecomputeTags();
            var m = PlayerMetrics.I;
            var matched = rules
                .Where(r => r != null && r.Matches(m, state, t, sectionId, roomId))
                .OrderBy(r => r.priority).ToList();
            foreach (var r in matched) {
                r.Apply(state);
                DecisionLogger.I.Log(t.ToString(), sectionId, r.ruleName, r.playerLine, r.devLine);
            }
        }

        // -------- Room 1 specific director calls --------

        /// <summary>
        /// Picks the Section 1.5 dacoit gem demand. Varies per-run, ramps up if the
        /// player ignored gems (teach the lesson), ramps slightly for Collectors,
        /// and is clamped to never be literally unwinnable given current gem count.
        /// </summary>
        public int ComputeRoom1DacoitDemand() {
            var m = PlayerMetrics.I;
            int baseDemand = Random.Range(6, 11); // 6..10

            float collected = m != null ? m.gemCollectionPct : 0f;
            string reason;
            if (collected < 0.3f) {
                baseDemand += Random.Range(4, 8);
                reason = $"Ignored gems ({collected:P0}) -> ramp demand";
            } else if (collected > 0.85f) {
                baseDemand += Random.Range(2, 5);
                reason = $"Collector ({collected:P0}) -> mild ramp";
            } else {
                reason = $"Balanced ({collected:P0}) -> base demand";
            }

            int gemsNow = GameManager.Instance != null ? GameManager.Instance.gems : 0;
            int hardCap = Mathf.Max(5, gemsNow + 6);
            int demand  = Mathf.Clamp(baseDemand, 5, hardCap);

            state.dacoitGemDemand = demand;
            DecisionLogger.I?.Log("Room1DacoitDemand", "1.5", "ComputeRoom1Demand",
                $"The toll: {demand}.",
                $"{reason}. demand={demand} (raw {baseDemand}, cap {hardCap}, has {gemsNow}).");
            return demand;
        }

        /// <summary>
        /// Picks where along the safe path Section 1.3's key should sit.
        /// Friendlier players get the key early; collectors / clean runs get it deep.
        /// </summary>
        public float ComputeRoom13KeyPathFraction() {
            var m = PlayerMetrics.I;
            float frac;
            if (m == null) frac = 0.5f;
            else if (m.damageTaken > 60)         frac = 0.30f;  // struggling -> easy
            else if (m.gemCollectionPct > 0.85f) frac = 0.80f;  // collector -> hard
            else                                  frac = 0.55f + Random.Range(-0.1f, 0.15f);

            state.room13KeyPathFraction = Mathf.Clamp01(frac);
            DecisionLogger.I?.Log("Room1KeyPlace", "1.3", "ComputeKeyFraction",
                "The key is hiding.",
                $"keyPathFraction={state.room13KeyPathFraction:F2}");
            return state.room13KeyPathFraction;
        }

        /// <summary>
        /// Picks how much Section 1.2's second gap widens mid-jump.
        /// </summary>
        public float ComputeRoom12Gap2Widen() {
            var m = PlayerMetrics.I;
            float w = 1.0f;
            if (m != null) {
                if (m.gemCollectionPct < 0.3f) w = 0.85f;      // forgiving
                else if (m.gemCollectionPct > 0.8f) w = 1.25f; // punishing
                else w = 1.0f + Random.Range(-0.05f, 0.1f);
            }
            state.room12Gap2WidenFactor = w;
            return w;
        }

        /// <summary>
        /// Picks Section 1.4 variant based on trapDensity.
        /// </summary>
        public Section14Variant ComputeRoom14Variant() {
            float td = state.trapDensity;
            Section14Variant v;
            if (td < 0.85f)      v = Section14Variant.Honest;
            else if (td < 1.15f) v = Section14Variant.SwitchMidway;
            else                  v = Section14Variant.SpikeRhythm;
            state.room14Variant = v;
            DecisionLogger.I?.Log("Room1Section14", "1.4", "PickVariant",
                $"The floor: {v}.",
                $"trapDensity={td:F2} -> variant={v}");
            return v;
        }
    }
}
