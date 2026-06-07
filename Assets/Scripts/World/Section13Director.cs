using UnityEngine; using MazeMind.Core;

public class Section13Director : MonoBehaviour {
    public GameObject guideCat;
    public AudioSource voiceHint;

    void Update() {
        if (AIDirector.I == null || AIDirector.I.state == null) return;
        var s = AIDirector.I.state;
        if (guideCat != null && s.spawn13GuideAnimal && !guideCat.activeSelf)
            guideCat.SetActive(true);
        if (voiceHint != null && s.give13VoiceHint && !voiceHint.isPlaying)
            voiceHint.Play();
    }
}
