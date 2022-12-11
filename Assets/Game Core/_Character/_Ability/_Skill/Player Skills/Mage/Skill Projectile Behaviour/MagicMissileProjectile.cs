using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicMissileProjectile : ProjectileObject {

    private MagicMissilePropertiesValuesContainer magicMissileProperties;

    protected override void ExecuteIfCanHit(Collider other) {
        _ = HitEnemy(other, magicMissileProperties.AbilityDamage.Value, magicMissileProperties.HitInfoId);

        CoreAbilityData.SkillLogic.ApplyCooldown(CoreAbilityData.SkillLogic.CurrentCooldown + magicMissileProperties.CooldownOnHit.Value);

        CoreAbilityData.CharacterComponent.CharacterCombat.GetStatusEffectApplied(magicMissileProperties.BuffHolder[0].buffToApply, CoreAbilityData.CharacterComponent,
            CoreAbilityData.CoreStatsValues, magicMissileProperties.BuffHolder[0].stacksToApply.GetValue());
    }

    public override void OnEnable() {
        base.OnEnable();
        if (CoreAbilityData == null) return;
        magicMissileProperties = CoreAbilityData.AbilityPropertiesValuesContainer as MagicMissilePropertiesValuesContainer;
    }

    public override void Update() {
        if (!CanTravel) { return; }
        base.Update();
        transform.Translate(magicMissileProperties.TravelSpeed.Value * Time.deltaTime * Vector3.forward);
    }
}
