using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour {
    //Stat related
    public event Action<float, CharacterStats> OnCurrentHealthChange;
    public event Action<float, CharacterStats> OnCurrentManaChange;
    public event Action<ICharacterStatReadonly> OnCharacterStatChange;
    public event Action OnCharacterStatsFullyLoaded;
    public bool CharacterStatsFullyLoaded { get; private set; } = false;

    //Combat related
    public delegate void PreDamageTaken(ref float damage, DamageType damageType, ref float penetrationValue);
    /// <summary>
    /// Delegate that gives last chance to modify input damage and penetration value
    /// before npc tied to this character takes damage
    /// </summary>
    public event PreDamageTaken PreCharacterDamageTaken;
    public event Action<CharacterStats> OnPreDeath;
    public event Action OnDeathInternal;

    //Core stats of the character  
    [field: SerializeField] public StatValues CoreStats { get; protected set; }

    protected float _currentHealth;
    public float CurrentHealth { 
        get => _currentHealth;
        private set {
            _currentHealth = value;
            if (_currentHealth <= 0f) Die();
            OnCurrentHealthChange?.Invoke(CurrentHealth, this);
        }
    }

    protected float _currentMana;
    public float CurrentMana { 
        get => _currentMana;
        private set {
            _currentMana = value;
            OnCurrentManaChange?.Invoke(CurrentMana, this);
        }
    }

    [field: SerializeField] public StatusEffectsManager StatusEffectsManager { get; private set; }

    private IEnumerator<float> healthRegenerationCoroutine;
    private CoroutineHandle healthRegenerationCoroutineHandle;

    private IEnumerator<float> manaRegenerationCoroutine;
    private CoroutineHandle manaRegenerationCoroutineHandle;

    public virtual void Awake() {
        if (CoreStats == null) return;

        if(StatusEffectsManager == null) StatusEffectsManager = GetComponent<StatusEffectsManager>();

        healthRegenerationCoroutine = HealthRegeneration();
        StatusEffectsManager.OnCanRegenerateChanged += ChangeHealthRegenerationState;
        manaRegenerationCoroutine = ManaRegeneration();

        CoreStats = CoreStats
            .SetIsCopy(false)
            .GetCopy()
            .SetIsCopy(false)
            .SetOnStatChangeCallback(OnCharacterStatChange)
            .RaiseStatChangedCallbackForEveryStat();

        CurrentHealth = CoreStats.HealthValue;
        CurrentMana = CoreStats.ManaValue;

        CharacterStatsFullyLoaded = true;
        OnCharacterStatsFullyLoaded?.Invoke();
        OnCharacterStatsFullyLoaded = null;
    }

    public virtual void Start() {
        
    }

    public (float finalDamage, float finalDamageReductionValue, float healthRemaining) TakeDamage(float damage, DamageType damageType, float penetrationValue) {
        damage = Mathf.Clamp(damage, 0, float.MaxValue);
        PreCharacterDamageTaken?.Invoke(ref damage, damageType, ref penetrationValue); 

        var (reduction, min) = CoreStats.GetResistanceValueByDamageType(damageType);

        float finalDamageReductionValue = reduction;

        if (finalDamageReductionValue > 0) {
            finalDamageReductionValue -= Mathf.Clamp(penetrationValue + (finalDamageReductionValue - penetrationValue), 0f, penetrationValue); ;
        }

        finalDamageReductionValue = Mathf.Clamp(finalDamageReductionValue, min, 0.999f);

        float finalDamage = damage * (1 - finalDamageReductionValue);

        _ = AddCurrentHealth(-finalDamage, false);
        return (finalDamage, finalDamageReductionValue, CurrentHealth);
    }

    public virtual void Die() {
        if (StatusEffectsManager.IsDead) return;
        OnPreDeath?.Invoke(this);

        if (CurrentHealth > 0f) return;

        OnDeathInternal?.Invoke();
        OnDeathInternal = null;
    }

    public void ChangeHealthRegenerationState(bool state) {
        if (state) {
            RunHealthRegenerationCoroutine();
        } else {
            StopHealthRegenerationCoroutine();
        }
    }

    public void RunHealthRegenerationCoroutine() {
        //Debug.Log("running hp regen coroutine");
        Timing.KillCoroutines(healthRegenerationCoroutineHandle);
        if (StatusEffectsManager.CanRegenerate) {
            healthRegenerationCoroutineHandle = Timing.RunCoroutine(healthRegenerationCoroutine);
        }
    }

    public void StopHealthRegenerationCoroutine() {
        Timing.KillCoroutines(healthRegenerationCoroutineHandle);
    }

    public void RunManaRegenerationCoroutine() {
        //Debug.Log("running mana regen coroutine");
        Timing.KillCoroutines(manaRegenerationCoroutineHandle);
        manaRegenerationCoroutineHandle = Timing.RunCoroutine(manaRegenerationCoroutine);
    }

    public void StopManaRegenerationCoroutine() {
        Timing.KillCoroutines(manaRegenerationCoroutineHandle);
    }

    #region Current health operations

    public void SetCurrentHealthToMax() {
        AddCurrentHealth(float.MaxValue, false);
    }

    public void SetCurrentHealth(float value) {
        CurrentHealth = Mathf.Clamp(value, 0f, CoreStats.HealthValue);
    }

    public float AddCurrentHealth(float value, bool affectedByHealingEffectivity) {
        if (value > 0f && affectedByHealingEffectivity) { value *= 1f + CoreStats.HealingEffectivityValue; }
        CurrentHealth = Mathf.Clamp(CurrentHealth + value, 0f, CoreStats.HealthValue);
        return value;
    }

    [Obsolete("Use CurrentHealth property")]
    public float GetCurrentHealth() {
        return CurrentHealth;
    }

    public float CurrentHealthPercentage => CurrentHealth / CoreStats.HealthValue;

    #endregion

    #region Current mana operations

    public void SetCurrentMana(float value) {
        CurrentMana = Mathf.Clamp(value, 0, CoreStats.ManaValue);
        
    }

    public void AddCurrentMana(float value) {
        CurrentMana = Mathf.Clamp(CurrentMana + value, 0, CoreStats.ManaValue);
    }

    [Obsolete("Use CurrentMana property")]
    public float GetCurrentMana() {
        return CurrentMana;
    }

    public float CurrentManaPercentage => CurrentMana / CoreStats.ManaValue;

    #endregion

    #region Add/Remove stat values operations

    public ICharacterStatReadonly GetStat(CharacterStatType statType) => CoreStats.GetStat(statType);

    public void SetPrimaryValue(CharacterStatType statType, float value) => CoreStats.SetPrimaryValue(statType, value);

    public void SetScaleValue(ScalableCharacterStatType statType, float value) => CoreStats.SetScaleValue(statType, value);

    public void AddStatModifier(CharacterStatType statType, float modifier, StatValueType statValueType, float replace = 0f)
        => CoreStats.AddStatModifier(statType, modifier, statValueType, replace);

    public void RemoveStatModifier(CharacterStatType statType, float modifier, StatValueType statValueType)
        => CoreStats.RemoveStatModifier(statType, modifier, statValueType); 

    public void UncapStatValue(CharacterStatType statType, float uncapValue, float replace = 0f) => CoreStats.UncapStatValue(statType, uncapValue, replace);

    public void RemoveStatUncapValue(CharacterStatType statType, float capValue) => CoreStats.RemoveStatUncap(statType, capValue);

    #endregion

    IEnumerator<float> HealthRegeneration() {
        while (true) {
            if (StatusEffectsManager.CanRegenerate) {
                _ = AddCurrentHealth(CoreStats.HealthRegenerationValue * Time.deltaTime, true);
            }
            yield return Timing.WaitForOneFrame;
        }
    }

    IEnumerator<float> ManaRegeneration() {
        while (true) {
            AddCurrentMana(CoreStats.ManaRegenerationValue * Time.deltaTime);
            yield return Timing.WaitForOneFrame;
        }
    }
}
