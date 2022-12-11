using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public static class StatusEffectExtensions {
    public static void DestroyIfCopy(this StatusEffect statusEffect) {
        if (statusEffect == null) return;
        if (statusEffect.IsCopy) UnityEngine.Object.Destroy(statusEffect);
    }
}

public class StatusEffect : ScriptableObject {

    [field: System.NonSerialized] public bool IsInitialized { get; set; } = false;
    public bool IsCopy { get; set; } = false;

    //References
    [SerializeField] protected StatusEffectProperties statusEffectProperties;
    public StatusEffectProperties StatusEffectProperties { get => statusEffectProperties; }

    #region Manager references

    private TargetManager targetManager;
    public TargetManager TargetManager { get { if (targetManager == null) targetManager = TargetManager.Instance; return targetManager; } }

    private ObjectPoolManager objectPoolManager;
    public ObjectPoolManager ObjectPoolManager { get { if (objectPoolManager == null) objectPoolManager = ObjectPoolManager.Instance; return objectPoolManager; } }

    private EventManager eventManager;
    public EventManager EventManager { get { if (eventManager == null) eventManager = EventManager.Instance; return eventManager; } }

    private CooldownManager coolDownManager;
    public CooldownManager CoolDownManager { get { if (coolDownManager == null) coolDownManager = CooldownManager.Instance; return coolDownManager; } }

    private AbilityObjectManager abilityObjectManager;
    public AbilityObjectManager AbilityObjectManager { get { if (abilityObjectManager == null) abilityObjectManager = AbilityObjectManager.Instance; return abilityObjectManager; } }

    private AudioManager audioManager;
    public AudioManager AudioManager { get { if (audioManager == null) audioManager = AudioManager.Instance; return audioManager; } }

    private VfxManager vfxManager;
    public VfxManager VfxManager { get { if (vfxManager == null) vfxManager = VfxManager.Instance; return vfxManager; } }

    #endregion

    #region Applied To
    protected Character AppliedToCharacterComponent { get; set; }
    protected StatusEffectsManager AppliedToStatusEffectsManager { get; set; }

    protected NPCStats appliedToStats;
    public NPCStats AppliedToStats { get => appliedToStats; }
    #endregion

    #region Applier
    private Character applierCharacterComponent;
    public Character ApplierCharacterComponent { get => applierCharacterComponent; private set => applierCharacterComponent = value; }

    private IHitLayers applierHitLayers;
    public IHitLayers ApplierHitLayers { get => applierHitLayers; private set => applierHitLayers = value; }

    private CoreStatsValuesContainer applierStatsContainer;
    public CoreStatsValuesContainer ApplierStatsContainer { get => applierStatsContainer; set => applierStatsContainer = value; }
    protected CoreStatsValuesContainer prevAppliedStatsContainer;

    #endregion

    #region Control properties
    private float startingDuration;
    public float StartingDuration { get => startingDuration; }
    //Core params
    private float currentDuration;
    public float CurrentDuration {
        get => currentDuration;
        protected set {
            currentDuration = value;

            if (currentDuration <= 0f) {
                HasEnded = true;
            }
        }
    }

    private float currentTickTime;
    public float CurrentTickTime {
        get => currentTickTime;
        protected set {
            if (statusEffectProperties.tickRate.GetValue() <= 0f) return;

            currentTickTime = value;
        }
    }

    private int currentStacks;
    public int CurrentStacks {
        get => currentStacks;
        protected set {
            currentStacks = value;
            DirtiedCombatTooltip = true;

            MaxStacksVerify();

            OnStacksChanged?.Invoke(currentStacks);
        }
    }

    private bool hasEnded = false;
    public bool HasEnded { get => hasEnded; set => hasEnded = value; }


    public bool Stackable { get; set; }
    public bool Refreshable { get; set; }
    public bool Permanent { get; set; }

    protected bool DirtiedCombatTooltip { get; set; }
    #endregion

    public event Action<int> OnStacksChanged;
    public event Action<StatusEffect> OnStatusEffectEnd;

    public virtual void Awake() { }

    public virtual void StatusEffectHolderInitialized() { }

    public StatusEffect GetCopy() {
        if (IsCopy) {
            return this;
        } else {
            StatusEffect copied = Instantiate(this);
            copied.IsCopy = true;
            copied.IsInitialized = IsInitialized;
            copied.statusEffectProperties = statusEffectProperties;
            copied.statusEffectProperties.IsInitialized = statusEffectProperties.IsInitialized;
            return copied;
        }
    }

    public StringBuilder GetTooltip() {
        return statusEffectProperties.GetTooltip();
    }

    public StringBuilder GetCombatTooltip() {
        bool rebuild = DirtiedCombatTooltip;
        DirtiedCombatTooltip = false;
        return statusEffectProperties.GetCombatTooltip(this, rebuild);
    }

    public virtual bool CanApply(HitOutput hitOutput) {
        return true;
    }

    public virtual bool CanRefresh() {
        return (Refreshable || Stackable) && !HasEnded;
    }

    public virtual void StartupInitialization() {
        if (IsInitialized) return;
        IsInitialized = true;
        IsCopy = false;
        statusEffectProperties.Initialize();
    }

    public virtual void PreApply(Character appliedTo, StatusEffectsManager appliedToStatusEffectsManager, Character applier, CoreStatsValuesContainer applierStatsContainer) {
        AppliedToCharacterComponent = appliedTo;
        AppliedToStatusEffectsManager = appliedToStatusEffectsManager;
        ApplierCharacterComponent = applier;
        ApplierHitLayers = applier;
        ApplierStatsContainer = applierStatsContainer;
        appliedToStats = AppliedToCharacterComponent.CharacterStats.CoreStats;
        hasEnded = false;
        currentTickTime = 0;
        currentStacks = 0;
        currentDuration = 0;

        Stackable = statusEffectProperties.Stackable;
        Refreshable = statusEffectProperties.Refreshable;
        Permanent = statusEffectProperties.Permanent;
    }

    public virtual void Apply(int stacksCount, HitOutput hitOutput) {
        currentDuration = statusEffectProperties.duration.GetValue();
        startingDuration = currentDuration;
        if (Stackable) {
            currentStacks += stacksCount;
            MaxStacksVerify();
        } else {
            currentStacks = 1;
        }

        DirtiedCombatTooltip = true;
    }

    public virtual void Refresh(CoreStatsValuesContainer applierStatsContainer, int stacksCount, HitOutput hitOutput) {
        if (Refreshable) {
            currentDuration = statusEffectProperties.duration.GetValue();
            if (applierStatsContainer != null) {
                prevAppliedStatsContainer = ApplierStatsContainer;
                ApplierStatsContainer = applierStatsContainer;
            }
        }

        if (Stackable) {
            CurrentStacks += stacksCount;
        }
    }

    public virtual void Tick(float deltaTime) {
        if (!Permanent) {
            CurrentDuration -= deltaTime;
        }

        CurrentTickTime += deltaTime;
    }

    public virtual void End() {
        OnStatusEffectEnd?.Invoke(this);
    }

    public virtual void OnStartVfx() { }

    public virtual void OnEndVfx() { }

    public void DecreaseDuration(float decreaseBy) {
        currentDuration -= decreaseBy;
        startingDuration -= decreaseBy;
    }

    private void MaxStacksVerify() {
        int maxStacks = statusEffectProperties.maxStacks.GetValue();
        if (currentStacks >= maxStacks) {
            currentStacks = maxStacks;
            Stackable = false;
        }
    }

    protected void CompareAndSetStats(CoreStatsValuesContainer coreStatsValuesContainer, StatsCompareType statsCompareType) {
        ApplierStatsContainer = statsCompareType switch {
            StatsCompareType.Offensive => prevAppliedStatsContainer.CompareStatsByOffensiveValues(coreStatsValuesContainer),
            StatsCompareType.Defensive => prevAppliedStatsContainer.CompareStatsDefensiveValues(coreStatsValuesContainer),
            _ => prevAppliedStatsContainer.CompareStats(coreStatsValuesContainer),
        };
    }


    public HitOutput HitEnemy(float damageValue, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemy(AppliedToCharacterComponent.CharacterCombat, damageValue, currentHitInfoId, ApplierCharacterComponent, StatusEffectProperties,
            null, ApplierStatsContainer, null, null, AppliedToCharacterComponent.transform.position, AppliedToCharacterComponent.gameObject, preferedDamageTypes);
    }

    public HitOutput HitEnemyExactDamage(float exactDamage, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        return HitInput.HitEnemyExactDamage(AppliedToCharacterComponent.CharacterCombat, exactDamage, currentHitInfoId, ApplierCharacterComponent, StatusEffectProperties,
            null, ApplierStatsContainer, null, null, AppliedToCharacterComponent.transform.position, AppliedToCharacterComponent.gameObject, preferedDamageTypes);
    }

}