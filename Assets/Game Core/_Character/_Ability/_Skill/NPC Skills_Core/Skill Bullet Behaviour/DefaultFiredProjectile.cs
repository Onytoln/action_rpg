using MEC;
using System.Collections.Generic;
using UnityEngine;

public class DefaultFiredProjectile : ProjectileObject {

    private DefaultNPCFireProjectilePropertiesValuesContainer defaultFireProjectileProperties;

    public override void OnEnable() {
        base.OnEnable();
        if (CoreAbilityData == null) return;
        defaultFireProjectileProperties = CoreAbilityData.AbilityPropertiesValuesContainer as DefaultNPCFireProjectilePropertiesValuesContainer;
    }

    protected override void ExecuteIfCanHit(Collider other) {
        _ = HitEnemy(other, defaultFireProjectileProperties.AbilityDamage.Value, defaultFireProjectileProperties.HitInfoId);
    }

    public override void Update() {
        if (!CanTravel) { return; }
        base.Update();
        transform.Translate(defaultFireProjectileProperties.TravelSpeed.Value * Time.deltaTime * Vector3.forward);
    }
}
