using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using MazeMind.Core;
using System.Text;

public class DevPanelUI : MonoBehaviour {
    public GameObject panel;
    public TMP_Text body;

    void Update() {
        if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
            panel.SetActive(!panel.activeSelf);
        if (!panel.activeSelf) return;

        var sb = new StringBuilder();
        var s = AIDirector.I.state; var m = PlayerMetrics.I;
        sb.AppendLine($"tags: {string.Join(",", s.activeTags)}");
        sb.AppendLine($"speed={m.avgMoveSpeed:F2} expl={m.explorationPct:P0} hes={m.avgHesitationSec:F1}s dmg={m.damageTaken} gem={m.gemCollectionPct:P0}");
        sb.AppendLine($"knobs: trap={s.trapDensity:F2} hazSpd={s.hazardSpeedMultiplier:F2} demand={s.dacoitGemDemand} dummy={s.dummyKeySubtlety:F2}");
        sb.AppendLine("---");
        foreach (var e in DecisionLogger.I.entries)
            sb.AppendLine($"{e.t,6:F1}  {e.trigger,-18} {e.section,-4} {e.ruleName,-22} {e.devLine}");
        body.text = sb.ToString();
    }
}