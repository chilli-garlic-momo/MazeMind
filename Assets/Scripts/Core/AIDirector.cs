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
    }
}