using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrostChargePropertiesValuesContainer : ChargeSkillPropertiesValuesContainer {
    public string PostChargeExplosionHitInforId { get; private set; }
    public SkillStatContainer PostChargeExplosionRadius { get; private set; }
    public SkillStatContainer PostChargeExplosionDamage { get; private set; }
    public List<DamageTypeWeight> PostChargeExplosionDamageTypes { get; private set; }
  
    public FrostChargePropertiesValuesContainer(FrostChargeProperties ab) : base(ab) {
        PostChargeExplosionHitInforId = ab.postChargeExplosionHitInforId;
        PostChargeExplosionRadius = new SkillStatContainer(ab.postChargeExplosionRadius);
        PostChargeExplosionDamage = new SkillStatContainer(ab.postChargeExplosionDamage);
        PostChargeExplosionDamageTypes = CopyDamageTypes(ab.PostChargeExplosionDamageTypes);
    }
}
