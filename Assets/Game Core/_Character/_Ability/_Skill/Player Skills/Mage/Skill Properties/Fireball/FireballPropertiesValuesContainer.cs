using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballPropertiesValuesContainer : ProjectileSkillPropertiesValuesContainer {
    public string ExplosionHitInforId { get; private set; }

    public SkillStatContainer ExplosionRadius { get; private set; }
    public SkillStatContainer ExplosionDamage { get; private set; }
    public List<DamageTypeWeight> ExplosionDamageTypes { get; private set; }

    public FireballPropertiesValuesContainer(FireballProperties ab) : base(ab) {
        ExplosionHitInforId = ab.explosionHitInforId;
        ExplosionRadius = new SkillStatContainer(ab.explosionRadius);
        ExplosionDamage = new SkillStatContainer(ab.explosionDamage);
        ExplosionDamageTypes = CopyDamageTypes(ab.ExplosionDamageTypes);
    }
}
