using MEC;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour {
    //Stat related
    public event Action<float, CharacterStats> CurrentHealthChange;
    public event Action<float, CharacterStats> CurrentManaChange;
    public delegate void OnStatChange(Stat stat);
    public event OnStatChange OnCharacterStatChange;
    public event Action OnCharacterStatsFullyLoaded;
    public bool CharacterStatsFullyLoaded { get; private set; } = false;

    //Combat related
    public delegate void PreDamageTaken(ref float damage, DamageType damageType, ref float penetrationValue);
    /// <summary>
    /// Delegate that gives last chance to modify input damage and penetration value
    /// before npc tied to this character takes damage
    /// </summary>
    public event PreDamageTaken PreCharacterDamageTaken;
    public event Action OnDeathInternal;

    //Core stats of the character  
    [field: SerializeField] public NPCStats CoreStats { get; protected set; }

    protected float currentHealth;
    public float CurrentHealth { get => currentHealth; }
    protected float currentMana;
    public float CurrentMana { get => currentMana; }

    [field: SerializeField] public StatusEffectsManager StatusEffectsManager { get; private set; }

    private IEnumerator<float> healthRegenerationCoroutine;
    private CoroutineHandle healthRegenerationCoroutineHandle;

    private IEnumerator<float> manaRegenerationCoroutine;
    private CoroutineHandle manaRegenerationCoroutineHandle;

    public virtual void Awake() {
        if (CoreStats == null) return;
        if(StatusEffectsManager == null) StatusEffectsManager = GetComponent<StatusEffectsManager>();
        healthRegenerationCoroutine = HealthRegeneration();
        manaRegenerationCoroutine = ManaRegeneration();
        StatusEffectsManager.OnCanRegenerateChanged += ChangeHealthRegenerationState;

        CoreStats.IsCopy = false;
        CoreStats = CoreStats.GetCopy();
        CoreStats.IsCopy = false;
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            CoreStats.Stats[i].Initialize();
            OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
        }

        currentHealth = CoreStats.HealthValue;
        currentMana = CoreStats.ManaValue;

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

    public void SetCurrentHealthToMax() {
        AddCurrentHealth(float.MaxValue, false);
    }

    //*********************************************************************************************************************//
    /// <summary>
    /// Sets health to value
    /// </summary>
    /// <param name="value">Value to which health will be set</param>
    public void SetCurrentHealth(float value) {
        currentHealth = Mathf.Clamp(value, 0, CoreStats.HealthValue);
        if (currentHealth <= 0) Die();
        CurrentHealthChange?.Invoke(CurrentHealth, this);
    }

    /// <summary>
    /// Adds value to current health negative/positive
    /// </summary>
    /// <param name="value">Value which will be added to current health</param>
    public float AddCurrentHealth(float value, bool affectedByHealingEffectivity) {
        if (value > 0 && affectedByHealingEffectivity) { value *= 1 + CoreStats.HealingEffectivityValue; }
        currentHealth = Mathf.Clamp(currentHealth + value, 0, CoreStats.HealthValue);
        if (currentHealth <= 0) Die();
        CurrentHealthChange?.Invoke(CurrentHealth, this);
        return value;
    }

    [Obsolete("Use CurrentHealth property")]
    public float GetCurrentHealth() {
        return currentHealth;
    }

    public float CurrentHealthPercentage => currentHealth / CoreStats.HealthValue;
    //*********************************************************************************************************************//

    /// <summary>
    /// Sets mana to value
    /// </summary>
    /// <param name="value">Value to which mana will be set</param>
    public void SetCurrentMana(float value) {
        currentMana = Mathf.Clamp(value, 0, CoreStats.ManaValue);
        CurrentManaChange?.Invoke(CurrentMana, this);
    }

    /// <summary>
    /// Adds value to current health negative/positive
    /// </summary>
    /// <param name="value">Value which will be added to current mana</param>
    public void AddCurrentMana(float value) {
        currentMana = Mathf.Clamp(currentMana + value, 0, CoreStats.ManaValue);
        CurrentManaChange?.Invoke(CurrentMana, this);
    }

    [Obsolete("Use CurrentMana property")]
    public float GetCurrentMana() {
        return currentMana;
    }

    public float CurrentManaPercentage => currentMana / CoreStats.ManaValue;

    //*********************************************************************************************************************//
    //GET STAT VALUE BY STAT TYPE
    [Obsolete("Use Properties")]
    public float GetStatValue(StatType statType) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                return CoreStats.Stats[i].GetValue();
            }
        }
        return 0;
    }

    public Stat GetStat(StatType statType) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                return CoreStats.Stats[i];
            }
        }

        return null;
    }
    //*********************************************************************************************************************//
    //SET PRIMARY VALUE
    public void SetPrimaryValue(StatType statType, float value) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].SetPrimaryValue(value);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }

    //*********************************************************************************************************************//
    //SET SCALE VALUE FOR SCALABLE STAT
    public void SetScaleValue(StatType statType, int value) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType && CoreStats.Stats[i] is ScalableStat) {
                CoreStats.Stats[i].SetScaleValue(value);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }
    //*********************************************************************************************************************//
    //ADD/REPLACE/REMOVE ABSOLUTE/RELATIVE/TOTAL STAT VALUES
    public void AddStat(StatType statType, float modifier, StatAddType statAddType, float replace = 0f) {
        switch (statAddType) {
            case StatAddType.Absolute:
                AddAbsoluteStat(statType, modifier, replace);
                break;
            case StatAddType.Relative:
                AddRelativeStat(statType, modifier, replace);
                break;
            case StatAddType.Total:
                AddTotalStat(statType, modifier, replace);
                break;
            default:
                throw new Exception("Stat Add Type when adding stat not recognized, must be Absolute/Relative/Total.");
        }
    }

    public void RemoveStat(StatType statType, float modifier, StatAddType statAddType) {
        switch (statAddType) {
            case StatAddType.Absolute:
                RemoveAbsoluteStat(statType, modifier);
                break;
            case StatAddType.Relative:
                RemoveRelativeStat(statType, modifier);
                break;
            case StatAddType.Total:
                RemoveTotalStat(statType, modifier);
                break;
            default:
                throw new Exception("Stat Add Type when removing stat not recognized, must be Absolute/Relative/Total.");
        }
    }

    //*********************************************************************************************************************//
    //ADD/REPLACE/REMOVE ABSOLUTE STAT VALUES
    public void AddAbsoluteStat(StatType statType, float absoluteModifier, float replace = 0f) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].AddAbsoluteModifier(absoluteModifier, replace);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }

    public void RemoveAbsoluteStat(StatType statType, float absoluteModifier) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].RemoveAbsoluteModifier(absoluteModifier);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }
    //*********************************************************************************************************************//
    //ADD/REPLACE/REMOVE RELATIVE STAT VALUES
    public void AddRelativeStat(StatType statType, float relativeModifier, float replace = 0f) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].AddRelativeModifier(relativeModifier, replace);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }

    public void RemoveRelativeStat(StatType statType, float relativeModifier) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].RemoveRelativeModifier(relativeModifier);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }
    //*********************************************************************************************************************//
    //ADD/REPLACE/REMOVE TOTAL STAT VALUES
    public void AddTotalStat(StatType statType, float totalModifier, float replace = 0f) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].AddTotalModifier(totalModifier, replace);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }

    public void RemoveTotalStat(StatType statType, float totalModifier) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].RemoveTotalModifier(totalModifier);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }
    //*********************************************************************************************************************//
    //UNCAP/CAP STAT VALUES
    public void UncapStat(StatType statType, float uncapValue) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if(CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].UncapStatValue(uncapValue);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }

    public void CapStat(StatType statType, float capValue) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType) {
                CoreStats.Stats[i].CapStatValue(capValue);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }

    //*********************************************************************************************************************//
    //UNCAP/CAP SCALABLE STAT VALUES
    public void UncapScalableStat(StatType statType, float uncapValue) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType && CoreStats.Stats[i] is ScalableStat) {
                CoreStats.Stats[i].UncapScalableStatValue(uncapValue);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
        }
    }

    public void CapScalableStat(StatType statType, float capValue) {
        for (int i = 0; i < CoreStats.Stats.Length; i++) {
            if (CoreStats.Stats[i].statType == statType && CoreStats.Stats[i] is ScalableStat) {
                CoreStats.Stats[i].CapScalableStatValue(capValue);
                OnCharacterStatChange?.Invoke(CoreStats.Stats[i]);
                break;
            }
            
        }
    }
    //*********************************************************************************************************************//

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
