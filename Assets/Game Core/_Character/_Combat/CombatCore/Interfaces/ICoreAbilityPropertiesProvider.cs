using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICoreAbilityPropertiesProvider {
    public List<DamageTypeWeight> DamageTypes { get; }

    public float AbsoluteCritChanceBenefit { get; }
    public float RelativeCritChanceBenefit { get; }
    public float AbsoluteCritDamageBenefit { get; }
    public float RelativeCritDamageBenefit { get; }

    public float PhysicalPenetrationBenefit { get; }
    public float FirePenetrationBenefit { get; }
    public float IcePenetrationBenefit { get; }
    public float LightningPenetrationBenefit { get; }
    public float PoisonPenetrationBenefit { get; }

    float GetPenetrationBenefitValueByDamageType(DamageType damageType);
}
