using MEC;
using System.Collections.Generic;
using UnityEngine;

public class FireballProjectile : ProjectileObject {
    private FireballPropertiesValuesContainer fireballProperties;

    protected override void ExecuteIfCanHit(Collider other) {
        _ = HitEnemy(other, fireballProperties.AbilityDamage.Value, fireballProperties.HitInfoId);
    }

    public override void OnEnable() {
        base.OnEnable();
        if (CoreAbilityData == null) return;
        fireballProperties = CoreAbilityData.AbilityPropertiesValuesContainer as FireballPropertiesValuesContainer;
    }

    public override void Update() {
        if (!CanTravel) { return; }
        base.Update();
        transform.Translate(fireballProperties.TravelSpeed.Value * Time.deltaTime * Vector3.forward);
    }

    public override void EndObjectsFunction() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, fireballProperties.ExplosionRadius.Value, CoreAbilityData.CharacterHitLayers.GetDirectHitLayer());
        for (int i = 0; i < colliders.Length; i++) {
            _ = HitEnemy(colliders[i], fireballProperties.ExplosionDamage.Value, fireballProperties.ExplosionHitInforId, fireballProperties.ExplosionDamageTypes);
        }

        base.EndObjectsFunction();
    }
}
