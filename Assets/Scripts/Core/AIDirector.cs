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
            if (m == null) return;
            if (m.avgMoveSpeed > 4f && m.explorationPct < 0.45f) s.activeTags.Add("Speedrunner");
            if (m.gemCollectionPct > 0.8f)                       s.activeTags.Add("Collector");
            if (m.avgHesitationSec > 3.5f && m.damageTaken < 30) s.activeTags.Add("Paranoid");
            if (m.damageTaken > 80 && m.avgMoveSpeed > 4f)       s.activeTags.Add("Reckless");
        }

        public void Fire(TriggerKind t, string sectionId, int roomId) {
            RecomputeTags();
            var m = PlayerMetrics.I;
            if (m == null || state == null) return;

            var matched = rules
                .Where(r => r != null && r.Matches(m, state, t, sectionId, roomId))
                .OrderBy(r => r.priority).ToList();
            foreach (var r in matched) {
                r.Apply(state);
                DecisionLogger.I?.Log(t.ToString(), sectionId, r.ruleName, r.playerLine, r.devLine);
            }
        }

        // -------- Room 1 specific director calls --------

        public int ComputeRoom1DacoitDemand() {
            var m = PlayerMetrics.I;
            int baseDemand = Random.Range(3, 7); // 3..6 before adaptation

            float collected = m != null ? m.gemCollectionPct : 0f;
            string reason;
            if (collected < 0.3f) {
                baseDemand += Random.Range(1, 3);
                reason = $"Ignored gems ({collected:P0}) -> mild ramp";
            } else if (collected > 0.85f) {
                baseDemand += Random.Range(1, 3);
                reason = $"Collector ({collected:P0}) -> mild ramp";
            } else {
                reason = $"Balanced ({collected:P0}) -> base demand";
            }

            int gemsNow = GameManager.EnsureExists().gems;
            int demand = Mathf.Clamp(baseDemand, 0, Mathf.Max(0, gemsNow));

            state.dacoitGemDemand = demand;
            DecisionLogger.I?.Log("Room1DacoitDemand", "1.5", "ComputeRoom1Demand",
                demand <= 0 ? "Show me the key." : $"The toll: {demand}.",
                $"{reason}. demand={demand} (raw {baseDemand}, player has {gemsNow}).");
            return demand;
        }

        public float ComputeRoom13KeyPathFraction() {
            var m = PlayerMetrics.I;
            float frac;
            if (m == null) frac = 0.5f;
            else if (m.damageTaken > 60)         frac = 0.30f;
            else if (m.gemCollectionPct > 0.85f) frac = 0.80f;
            else                                  frac = 0.55f + Random.Range(-0.1f, 0.15f);

            state.room13KeyPathFraction = Mathf.Clamp01(frac);
            DecisionLogger.I?.Log("Room1KeyPlace", "1.3", "ComputeKeyFraction",
                "The key is hiding.",
                $"keyPathFraction={state.room13KeyPathFraction:F2}");
            return state.room13KeyPathFraction;
        }

        public float ComputeRoom12Gap2Widen() {
            var m = PlayerMetrics.I;
            float w = 1.0f;
            if (m != null) {
                if (m.gemCollectionPct < 0.3f) w = 0.85f;
                else if (m.gemCollectionPct > 0.8f) w = 1.25f;
                else w = 1.0f + Random.Range(-0.05f, 0.1f);
            }
            state.room12Gap2WidenFactor = w;
            return w;
        }

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
