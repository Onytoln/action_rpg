public class PlayerCombat : Combat {

    #region Hit related

    public override HitOutput HitEnemy(HitInput hitInput) {
        HitOutput hitOutput = base.HitEnemy(hitInput);

        EventManager.OnPlayerHit?.Invoke(hitOutput);

        if (hitOutput.TargetHealthRemaining <= 0f) { 
            EventManager.OnPlayerHitKill?.Invoke(hitOutput);
            EventManager.OnPlayerKill?.Invoke(hitOutput);
        }

        return hitOutput;
    }

    public override HitOutput TakeEnemyHit(HitInput enemyHitInput) {
        HitOutput hitOutput = base.TakeEnemyHit(enemyHitInput);

        EventManager.OnPlayerHitTaken?.Invoke(hitOutput);

        return hitOutput;
    }

    public override HitOutput DamageEnemyViaNonHit(HitInput hitInput, bool canEvade = false) {
        HitOutput hitOutput = base.DamageEnemyViaNonHit(hitInput, canEvade);

        EventManager.OnPlayerNonHitDamageDone?.Invoke(hitOutput);

        if (hitOutput.TargetHealthRemaining <= 0f) { EventManager.OnPlayerKill?.Invoke(hitOutput); }

        return hitOutput;
    }

    public override HitOutput TakeDamageFromEnemyViaNonHit(HitInput enemyHitInput, bool canEvade = false) {
        HitOutput hitOutput = base.TakeDamageFromEnemyViaNonHit(enemyHitInput, canEvade);

        EventManager.OnPlayerNonHitDamageTaken?.Invoke(hitOutput);

        return hitOutput;
    }

#endregion

    #region Status Effect related

    public override bool ApplyStatusEffectToTarget(Combat enemyCombat, StatusEffect statusEffect, Character applier, CharStatsValContainer applierStats,
        int stackCount, HitOutput hitOutput = null) {

        bool result = base.ApplyStatusEffectToTarget(enemyCombat, statusEffect, applier, applierStats, stackCount, out StatusEffect appliedEffect, hitOutput);

        EventManager.OnPlayerAppliedStatusEffect?.Invoke(appliedEffect);

        return result;
    }

    public override bool ApplyStatusEffectToTarget(Combat enemyCombat, StatusEffect statusEffect, Character applier, CharStatsValContainer applierStats,
        int stackCount, out StatusEffect appliedEffect, HitOutput hitOutput = null) {

        bool result = base.ApplyStatusEffectToTarget(enemyCombat, statusEffect, applier, applierStats, stackCount, out appliedEffect, hitOutput);

        EventManager.OnPlayerAppliedStatusEffect?.Invoke(appliedEffect);

        return result;
    }

    public override bool GetStatusEffectApplied(StatusEffect statusEffect, Character applier, CharStatsValContainer applierStats,
        int stackCount, HitOutput hitOutput = null) {

        bool result = base.GetStatusEffectApplied(statusEffect, applier, applierStats, stackCount, out StatusEffect appliedEffect, hitOutput);

        EventManager.OnAppliedStatusEffectToPlayer?.Invoke(appliedEffect);

        return result;
    }

    public override bool GetStatusEffectApplied(StatusEffect statusEffect, Character applier, CharStatsValContainer applierStats,
        int stackCount, out StatusEffect appliedEffect, HitOutput hitOutput = null) {

        bool result = base.GetStatusEffectApplied(statusEffect, applier, applierStats, stackCount, out appliedEffect, hitOutput);

        EventManager.OnAppliedStatusEffectToPlayer?.Invoke(appliedEffect);

        return result;
    }

    #endregion

    public override float RestoreHealth(float value, bool affectedByHealthEffectivity) {
        float realHealthRestored = base.RestoreHealth(value, affectedByHealthEffectivity);

        EventManager.OnPlayerHeal?.Invoke(realHealthRestored);
        return realHealthRestored;
    }

    public override float RestoreHealthPercentage(float value, bool affectedByHealthEffectivity) {
        float realHealthRestored = base.RestoreHealthPercentage(value, affectedByHealthEffectivity);

        EventManager.OnPlayerHeal?.Invoke(realHealthRestored);
        return realHealthRestored;
    }

    public override float RestoreByCurrentMissingHealthPercentage(float value, bool affectedByHealthEffectivity) {
        float realHealthRestored = base.RestoreByCurrentMissingHealthPercentage(value, affectedByHealthEffectivity);

        EventManager.OnPlayerHeal?.Invoke(realHealthRestored);
        return realHealthRestored;
    }

    public override float RestoreMana(float value) {
        float restored = base.RestoreMana(value);

        EventManager.OnPlayerManaRestored?.Invoke(restored);
        return restored;
    }

    public override float RestoreManaPercetage(float value) {
        float restored = base.RestoreManaPercetage(value);

        EventManager.OnPlayerManaRestored?.Invoke(restored);
        return restored;
    }

}
