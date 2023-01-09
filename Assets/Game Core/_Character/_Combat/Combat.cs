using System;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class Combat : MonoBehaviour {

    #region Events
    public event Action<HitInput> PreHitSent;
    public event Action<HitInput> PreHitTaken;

    public event Action<HitOutput> OnHitTaken;
    public event Action<HitOutput> OnHitDone;

    public event Action<HitOutput> OnNonHitDamageTaken;
    public event Action<HitOutput> OnNonHitDamageDone;

    public event Action<float> OnHeal;
    public event Action<float> OnManaRestored;

    #endregion

    public CharacterStats MyCharacterStats { get; private set; }
    public StatusEffectsManager StatusEffectsManager { get; private set; }
    public EventManager EventManager { get; private set; }

    private void Awake() {
        MyCharacterStats = GetComponent<CharacterStats>();
        StatusEffectsManager = GetComponent<StatusEffectsManager>();
        EventManager = EventManager.Instance;
        OnHitDone += LifeSteal;
    }

    #region Hit/Damage Related

    public virtual HitOutput HitEnemy(HitInput hitInput) {
        if (hitInput == null) { return null; }

        PreHitSent?.Invoke(hitInput);

        HitOutput hitOutput = hitInput.TargetCombatComponent.TakeEnemyHit(hitInput);

        OnHitDone?.Invoke(hitOutput);

        ApplyStatusEffectsOnHitAutomatically(hitInput.TargetCombatComponent, hitOutput);

        return hitOutput;
    }

    public virtual HitOutput TakeEnemyHit(HitInput enemyHitInput) {
        if (enemyHitInput == null) return null;

        //call pre-hit taken event
        PreHitTaken?.Invoke(enemyHitInput);

        //make new hit output 
        HitOutput hitOutput = new HitOutput();
        //assign hitInput to hitOutput 
        hitOutput.hitInput = enemyHitInput;

        //check if is invulnerable
        if (StatusEffectsManager.IsInvulnerable) {
            hitOutput.wasInvulnerable = true;
            OnHitTaken?.Invoke(hitOutput);
            return hitOutput;
        }

        //randomize attack evasion
        float randomNum = GetRandomNumber(0f, 1f);
        if (randomNum <= MyCharacterStats.CoreStats.EvasionChanceValue) {
            hitOutput.wasEvaded = true;
            OnHitTaken?.Invoke(hitOutput);
            return hitOutput;
        }

        float finalDamageToDeal = enemyHitInput.GetTotalDamage();

        //randomize attack crit
        randomNum = GetRandomNumber(0f, 1f);
        if (randomNum <= enemyHitInput.GetTotalCritChance()) {
            hitOutput.wasCrit = true;
            finalDamageToDeal *= 1 + enemyHitInput.GetTotalCritDamage();
        }

        //randomize block
        randomNum = GetRandomNumber(0f, 1f);
        if (randomNum <= MyCharacterStats.CoreStats.BlockChanceValue) {
            hitOutput.wasBlock = true;
            float damageBlocked = finalDamageToDeal * MyCharacterStats.CoreStats.BlockStrenghtValue;
            hitOutput.DamageBlocked = damageBlocked;
            finalDamageToDeal -= damageBlocked;
        }

        hitOutput.TotalPreReductionsDamage = finalDamageToDeal;

        //initialize hitoutput damage array
        hitOutput.OutputByDamageType = new OutputByDamageType[enemyHitInput.CurrentDamageTypes.Count];

        //process damage
        hitOutput.TotalDamageTakenPostReductions = 0f;
        hitOutput.TargetHealthRemaining = 0f;
        for (int i = 0; i < enemyHitInput.CurrentDamageTypes.Count; i++) {
            var results = MyCharacterStats.TakeDamage(finalDamageToDeal * enemyHitInput.CurrentDamageTypes[i].damageWeight,
                enemyHitInput.CurrentDamageTypes[i].damageType, enemyHitInput.GetTotalPenetration(i));

            hitOutput.OutputByDamageType[i] = new OutputByDamageType();
            hitOutput.OutputByDamageType[i].damageType = enemyHitInput.CurrentDamageTypes[i].damageType;
            hitOutput.OutputByDamageType[i].reductionModifier = results.finalDamageReductionValue;
            hitOutput.OutputByDamageType[i].damageByThisType = results.finalDamage;
            hitOutput.OutputByDamageType[i].percentOutOfMaxDamage = hitOutput.OutputByDamageType[i].damageByThisType / finalDamageToDeal;

            hitOutput.TotalDamageTakenPostReductions += results.finalDamage;
            hitOutput.TargetHealthRemaining = results.healthRemaining;
        }

        OnHitTaken?.Invoke(hitOutput);
        return hitOutput;
    }

    public virtual HitOutput DamageEnemyViaNonHit(HitInput hitInput, bool canEvade = false) {
        if (hitInput == null) { return null; }

        HitOutput hitOutput = hitInput.TargetCombatComponent.TakeDamageFromEnemyViaNonHit(hitInput, canEvade);

        OnNonHitDamageDone?.Invoke(hitOutput);

        return hitOutput;
    }

    public virtual HitOutput TakeDamageFromEnemyViaNonHit(HitInput enemyHitInput, bool canEvade = false) {
        if (enemyHitInput == null) { return null; }

        //make new hit output 
        HitOutput hitOutput = new HitOutput {
            //assign hitInput to hitOutput 
            hitInput = enemyHitInput
        };

        //check if is invulnerable
        if (StatusEffectsManager.IsInvulnerable) {
            hitOutput.wasInvulnerable = true;
            OnHitTaken?.Invoke(hitOutput);
            return hitOutput;
        }

        float randomNum = 0f;

        if (canEvade) {
            //randomize attack evasion
            randomNum = GetRandomNumber(0f, 1f);
            if (randomNum <= MyCharacterStats.CoreStats.EvasionChanceValue) {
                hitOutput.wasEvaded = true;
                OnHitTaken?.Invoke(hitOutput);
                return hitOutput;
            }
        }

        float finalDamageToDeal = enemyHitInput.GetTotalDamage();

        //randomize attack crit
        randomNum = GetRandomNumber(0f, 1f);
        if (randomNum <= enemyHitInput.GetTotalCritChance()) {
            hitOutput.wasCrit = true;
            finalDamageToDeal *= 1 + enemyHitInput.GetTotalCritDamage();
        }
        //randomize block
        randomNum = GetRandomNumber(0f, 1f);
        if (randomNum <= MyCharacterStats.CoreStats.BlockChanceValue) {
            hitOutput.wasBlock = true;
            float damageBlocked = finalDamageToDeal * MyCharacterStats.CoreStats.BlockStrenghtValue;
            hitOutput.DamageBlocked = damageBlocked;
            finalDamageToDeal -= damageBlocked;
        }
        hitOutput.TotalPreReductionsDamage = finalDamageToDeal;

        //initialize hitoutput damage array
        hitOutput.OutputByDamageType = new OutputByDamageType[enemyHitInput.CurrentDamageTypes.Count];
        //process damage
        hitOutput.TotalDamageTakenPostReductions = 0f;
        hitOutput.TargetHealthRemaining = 0f;
        for (int i = 0; i < enemyHitInput.CurrentDamageTypes.Count; i++) {
            var results = MyCharacterStats.TakeDamage(finalDamageToDeal * enemyHitInput.CurrentDamageTypes[i].damageWeight,
                enemyHitInput.CurrentDamageTypes[i].damageType, enemyHitInput.GetTotalPenetration(i));

            hitOutput.OutputByDamageType[i] = new OutputByDamageType();
            hitOutput.OutputByDamageType[i].damageType = enemyHitInput.CurrentDamageTypes[i].damageType;
            hitOutput.OutputByDamageType[i].reductionModifier = results.finalDamageReductionValue;
            hitOutput.OutputByDamageType[i].damageByThisType = results.finalDamage;
            hitOutput.OutputByDamageType[i].percentOutOfMaxDamage = results.finalDamage / finalDamageToDeal;

            hitOutput.TotalDamageTakenPostReductions += results.finalDamage;
            hitOutput.TargetHealthRemaining = results.healthRemaining; 
        }

        OnNonHitDamageTaken?.Invoke(hitOutput);

        return hitOutput;
    }

    #endregion

    #region Status Effects Related
    protected void ApplyStatusEffectsOnHitAutomatically(Combat target, HitOutput hitOutput) {
        if (!(hitOutput.HitSourceAbilityProperties is SkillProperties skillProperties)) return;

        if (skillProperties.debuffHolder == null) return;

        CharStatsValContainer coreStats = hitOutput.HitSourceCoreStatsValues ?? hitOutput.HitSourceCharacterComponent.CharacterStats.CoreStats.GetCurrentStatsValuesCopy();

        for (int i = 0; i < skillProperties.debuffHolder.Length; i++) {
            if (skillProperties.debuffHolder[i].ApplyManually) continue;
            if (!string.IsNullOrEmpty(skillProperties.debuffHolder[i].ApplyOnHitInfoId) && !string.IsNullOrEmpty(hitOutput.CurrentHitInforId) &&
                  !hitOutput.CurrentHitInforId.Equals(skillProperties.debuffHolder[i].ApplyOnHitInfoId)) continue;

            _ = ApplyStatusEffectToTarget(target, skillProperties.debuffHolder[i].debuffToApply,
               hitOutput.HitSourceCharacterComponent, coreStats, skillProperties.debuffHolder[i].stacksToApply.GetValue(), hitOutput);
        }
    }

    public virtual bool ApplyStatusEffectToTarget(Combat enemyCombat, StatusEffect statusEffect, Character applier, CharStatsValContainer applierStats,
        int stackCount, HitOutput hitOutput = null) {

        return enemyCombat.GetStatusEffectApplied(statusEffect, applier, applierStats, stackCount, hitOutput);
    }

    public virtual bool ApplyStatusEffectToTarget(Combat enemyCombat, StatusEffect statusEffect, Character applier, CharStatsValContainer applierStats,
        int stackCount, out StatusEffect appliedEffect, HitOutput hitOutput = null) {

        return enemyCombat.GetStatusEffectApplied(statusEffect, applier, applierStats, stackCount, out appliedEffect, hitOutput);
    }

    public virtual bool GetStatusEffectApplied(StatusEffect statusEffect, Character applier, CharStatsValContainer applierStats,
        int stackCount, HitOutput hitOutput = null) {

        return StatusEffectsManager.ApplyStatusEffect(statusEffect, applier, applierStats, stackCount, out var _, hitOutput);
    }

    public virtual bool GetStatusEffectApplied(StatusEffect statusEffect, Character applier, CharStatsValContainer applierStats,
        int stackCount, out StatusEffect appliedEffect, HitOutput hitOutput = null) {

        return StatusEffectsManager.ApplyStatusEffect(statusEffect, applier, applierStats, stackCount, out appliedEffect, hitOutput);
    }

    #endregion

    public virtual float RestoreHealth(float value, bool affectedByHealthEffectivity) {
        float realHealthRestored = MyCharacterStats.AddCurrentHealth(value, affectedByHealthEffectivity);

        OnHeal?.Invoke(realHealthRestored);
        return realHealthRestored;
    }

    public virtual float RestoreHealthPercentage(float value, bool affectedByHealthEffectivity) {
        float realHealthRestored = MyCharacterStats.AddCurrentHealth(value * MyCharacterStats.CoreStats.HealthValue, affectedByHealthEffectivity);

        OnHeal?.Invoke(realHealthRestored);
        return realHealthRestored;
    }

    public virtual float RestoreByCurrentMissingHealthPercentage(float value, bool affectedByHealthEffectivity) {
        float realHealthRestored = MyCharacterStats.AddCurrentHealth(value * (MyCharacterStats.CoreStats.HealthValue - MyCharacterStats.CurrentHealth), affectedByHealthEffectivity);

        OnHeal?.Invoke(realHealthRestored);
        return realHealthRestored;
    }

    public virtual void LifeSteal(HitOutput hitOutput) {
        RestoreHealth(hitOutput.TotalDamageTakenPostReductions * MyCharacterStats.CoreStats.LifeStealValue, true);
    }

    public virtual float RestoreMana(float value) {
        MyCharacterStats.AddCurrentMana(value);
        OnManaRestored?.Invoke(value);
        return value;
    }

    public virtual float RestoreManaPercetage(float value) {
        float manaToRestore = value * MyCharacterStats.CoreStats.ManaValue;
        MyCharacterStats.AddCurrentMana(manaToRestore);

        OnManaRestored?.Invoke(manaToRestore);
        return manaToRestore;
    }

    public float GetRandomNumber(float minRange, float maxRange) {
        return UnityEngine.Random.Range(minRange, maxRange);
    }

}
