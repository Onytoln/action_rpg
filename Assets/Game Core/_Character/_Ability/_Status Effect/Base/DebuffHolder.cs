using System;
using UnityEngine;

[System.Serializable]
public class DebuffHolder {
    public Debuff debuffToApply;
    public StatInt stacksToApply = new StatInt(null, 1, 1, int.MaxValue);
    [field: SerializeField] public bool ApplyManually { get; set; } = false;
    [field: SerializeField] public string ApplyOnHitInfoId { get; set; }
    [field: SerializeField] public bool ShowInTooltip { get; set; } = true;

    private Action<AbilityProperties> setTooltipDirtyAction;

    public void Initialize(Action setDirtyMethod) {
        debuffToApply.Initialize();

        setTooltipDirtyAction = (a) => setDirtyMethod?.Invoke();
        debuffToApply.StatusEffectProperties.OnTooltipDirtied += setTooltipDirtyAction;
    }

    public void HolderFullyInitialized() {
        debuffToApply.StatusEffectHolderInitialized();
    }

    public void OnDestroy() {
        debuffToApply.StatusEffectProperties.OnTooltipDirtied -= setTooltipDirtyAction;
    }
}
