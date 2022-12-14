using System;
using UnityEngine;

[System.Serializable]
public class BuffHolder {
    public Buff buffToApply;
    public StatInt stacksToApply = new StatInt(null, 1, 1, int.MaxValue);
    [field: SerializeField] public bool ApplyManually { get; set; } = false;
    [field: SerializeField] public bool ShowInTooltip { get; set; } = true;

    private Action<AbilityProperties> setTooltipDirtyAction;

    public void Initialize(Action setDirtyMethod) {
        buffToApply.Initialize();

        setTooltipDirtyAction = (a) => setDirtyMethod?.Invoke();
        buffToApply.StatusEffectProperties.OnTooltipDirtied += setTooltipDirtyAction;
    }

    public void HolderFullyInitialized() {
        buffToApply.StatusEffectHolderInitialized();
    }

    public void OnDestroy() {
        buffToApply.StatusEffectProperties.OnTooltipDirtied -= setTooltipDirtyAction;
    }
}
