using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultChargeCollision : AoEObject {
    [SerializeField] private ParticleSystem hitParticle;

    protected override void OnTriggerEnter(Collider other) {
        base.OnTriggerEnter(other);

        if (!other.CompareTag("Terrain")) {
            _ = HitEnemy(other, CoreAbilityData.AbilityPropertiesValuesContainer.AbilityDamage.Value, CoreAbilityData.AbilityPropertiesValuesContainer.HitInfoId);

            if (hitParticle != null) {
                VfxManager.PlayOneShotParticle(hitParticle, other.transform.position, transform.rotation);
            }
        }
    }
}
