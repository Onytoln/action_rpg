using System.Text;
using UnityEngine;


public class StatusEffectProperties : AbilityProperties {

    [field: System.NonSerialized] public bool IsInitialized { get; set; } = false;

    [Header("Status Effect properties")]
    public StatFloat duration;
    public StatFloat tickRate;

    public StatInt maxStacks;

    [field: SerializeField, Header("Core - dynamic")] public bool Stackable { get; protected set; } = true;
    [field: SerializeField] public bool Refreshable { get; protected set; } = true;
    [field: SerializeField] public bool Permanent { get; protected set; } = false;
    [field: SerializeField, Header("Core - static")] public bool MultiInstanced { get; protected set; } = false;
    [field: SerializeField] public bool SurvivesDeath { get; protected set; } = false;
    [field: SerializeField] public bool SurvivesSceneLoad { get; protected set; } = false;
    [field: SerializeField] public bool SurvivesGameSessionEnd { get; protected set; } = false;

    //Tooltip
    protected StringBuilder combatTooltip;

    public override void OnValidate() {
        base.OnValidate();
    }

    public override void Initialize() {
        if (IsInitialized) return;
        IsInitialized = true;
        IsCopy = false;
        SetTooltipIsDirty();
        base.Initialize();
    }

    public override void AssignReferences() {
        base.AssignReferences();
        duration.SetTooltipDirtyMethod = SetTooltipIsDirty;
        tickRate.SetTooltipDirtyMethod = SetTooltipIsDirty;
        maxStacks.setTooltipDirtyMethod = SetTooltipIsDirty;
    }

    protected virtual void BuildCombatTooltip(StatusEffect se) { }

    public StringBuilder GetCombatTooltip(StatusEffect se, bool rebuild) {
        if (rebuild) BuildCombatTooltip(se);

        return combatTooltip;
    }

}
