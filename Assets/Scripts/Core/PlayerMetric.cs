using System.Collections.Generic;
using UnityEngine;

namespace MazeMind.Core {
    public class PlayerMetrics : MonoBehaviour {
        public static PlayerMetrics I { get; private set; }

        public float avgMoveSpeed;
        public float explorationPct;
        public float avgHesitationSec;
        public int   damageTaken;
        public float gemCollectionPct;

        // Per-section bookkeeping
        public readonly Dictionary<string,int> deathsInSection = new();
        public int sectionsEntered = 0;

        // Internal accumulators
        readonly Queue<float> _speedSamples = new();
        const float SpeedWindowSec = 5f;
        float _hesitationTotal;
        readonly HashSet<Vector2Int> _visitedTiles = new();
        int _totalWalkableTiles = 200; // set per room from RoomConfigSO
        int _gemsCollected;
        int _gemsAvailable = 4; // Room 1: 3 in 1.5 + 1 elsewhere — set from RoomConfigSO

        Transform _player;
        Vector3 _lastPos;

        void Awake() {
            if (I != null) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);
        }

        public void Bind(Transform player) {
            _player = player; _lastPos = player.position;
        }

        public void ConfigureRoom(int totalWalkableTiles, int gemsAvailable) {
            _totalWalkableTiles = Mathf.Max(1, totalWalkableTiles);
            _gemsAvailable = Mathf.Max(1, gemsAvailable);
            _visitedTiles.Clear();
        }

        void FixedUpdate() {
            if (_player == null) return;
            float dt = Time.fixedDeltaTime;
            Vector3 p = _player.position;
            float v = (p - _lastPos).magnitude / dt;
            _lastPos = p;

            // rolling avg speed
            _speedSamples.Enqueue(v);
            if (_speedSamples.Count > Mathf.RoundToInt(SpeedWindowSec / dt))
                _speedSamples.Dequeue();
            float sum = 0; foreach (var s in _speedSamples) sum += s;
            avgMoveSpeed = sum / Mathf.Max(1,_speedSamples.Count);

            // hesitation
            if (v < 0.2f) _hesitationTotal += dt;
            avgHesitationSec = sectionsEntered > 0 ? _hesitationTotal / sectionsEntered : 0;

            // exploration
            var tile = new Vector2Int(Mathf.FloorToInt(p.x/2f), Mathf.FloorToInt(p.z/2f));
            _visitedTiles.Add(tile);
            explorationPct = (float)_visitedTiles.Count / _totalWalkableTiles;
        }

        public void OnDamage(int hp) => damageTaken += hp;
        public void OnGem()         { _gemsCollected++; gemCollectionPct = (float)_gemsCollected/_gemsAvailable; }
        public void OnEnterSection(string id) {
            sectionsEntered++;
            if (!deathsInSection.ContainsKey(id)) deathsInSection[id] = 0;
        }
        public void OnDeath(string sectionId) {
            damageTaken += 100;
            if (!deathsInSection.ContainsKey(sectionId)) deathsInSection[sectionId] = 0;
            deathsInSection[sectionId]++;
        }
    }
}