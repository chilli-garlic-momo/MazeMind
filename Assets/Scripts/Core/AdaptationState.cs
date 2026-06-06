using System.Collections.Generic;

namespace MazeMind.Core {
    public class AdaptationState {
        public float trapDensity = 1.0f;
        public float hazardSpeedMultiplier = 1.0f;
        public int   dacoitGemDemand = 10;
        public float dummyKeySubtlety = 0.0f;
        public bool  give13VoiceHint = false;
        public bool  spawn13GuideAnimal = false;
        public HashSet<string> activeTags = new();
    }
}