using System.Collections.Generic;

namespace MazeMind.Core {
    public class AdaptationState {
        public float trapDensity            = 1.0f;
        public float hazardSpeedMultiplier  = 1.0f;
        public int   dacoitGemDemand        = 10;
        public float dummyKeySubtlety       = 0.0f;
        public bool  give13VoiceHint        = false;
        public bool  spawn13GuideAnimal     = false;

        // 0.0 = key near entry (easy), 1.0 = key near exit (hard).
        // Section 1.3 reads this when laying out the safe path.
        public float room13KeyPathFraction  = 0.6f;

        // Section 1.2 gap-2 widening multiplier. 1.0 = honest single-jump-clearable,
        // >1.0 = forces double-jump, <1.0 = friendlier.
        public float room12Gap2WidenFactor  = 1.0f;

        // Section 1.4 variant the director picked for THIS run.
        public Section14Variant room14Variant = Section14Variant.Honest;

        public HashSet<string> activeTags = new();
    }

    public enum Section14Variant { Honest, SwitchMidway, SpikeRhythm }
}
