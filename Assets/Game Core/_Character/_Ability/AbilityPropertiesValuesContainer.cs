using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for ability objects to avoid copying AbilityProperties scriptable object, which has insane performance cost.
/// </summary>
public class AbilityPropertiesValuesContainer : ICoreAbilityPropertiesProvider {
    public GameObject AbilityPrefab { get; private set; }
    public string AbilityId { get; private set; }
    public string Name { get; private set; }
    public string HitInfoId { get; private set; }
    public Sprite Icon { get; private set; }
    public SkillStatContainer AbilityDamage { get; private set; }
    public List<DamageTypeWeight> DamageTypes { get; private set; }

    #region Crit benefit

    public CritBenefitContainer[] BenefitFromCriticalStrike { get; private set; }

    public float AbsoluteCritChanceBenefit => BenefitFromCriticalStrike[0].CriticalStrikeBenefit.Value;
    public float RelativeCritChanceBenefit => BenefitFromCriticalStrike[1].CriticalStrikeBenefit.Value;
    public float AbsoluteCritDamageBenefit => BenefitFromCriticalStrike[2].CriticalStrikeBenefit.Value;
    public float RelativeCritDamageBenefit => BenefitFromCriticalStrike[3].CriticalStrikeBenefit.Value;
    #endregion

    #region Penetration benefit

    public PeneBenefitContainer[] BenefitFromPenetration { get; private set; }

    public float PhysicalPenetrationBenefit => BenefitFromPenetration[0].PenetrationBenefit.Value;
    public float FirePenetrationBenefit => BenefitFromPenetration[1].PenetrationBenefit.Value;
    public float IcePenetrationBenefit => BenefitFromPenetration[2].PenetrationBenefit.Value;
    public float LightningPenetrationBenefit => BenefitFromPenetration[3].PenetrationBenefit.Value;
    public float PoisonPenetrationBenefit => BenefitFromPenetration[4].PenetrationBenefit.Value;

    #endregion

    public SkillStatContainer TravelSpeed { get; private set; }
    public SkillStatContainer MinCastRange { get; private set; }
    public SkillStatContainer MaxCastRange { get; private set; }
    public SkillStatContainer HitRange { get; private set; }
    public SkillStatContainer HitAngle { get; private set; }
    public SkillStatContainer Cooldown { get; private set; }

    public AbilityPropertiesValuesContainer(AbilityProperties abilityProp) {
        AbilityPrefab = abilityProp.abilityPrefab;
        AbilityId = abilityProp.abilityId;
        Name = abilityProp.name;
        HitInfoId = abilityProp.hitInfoId;
        Icon = abilityProp.icon;
        AbilityDamage = new SkillStatContainer(abilityProp.abilityDamage);

        DamageTypes = CopyDamageTypes(abilityProp.DamageTypes);

        BenefitFromCriticalStrike = new CritBenefitContainer[abilityProp.BenefitFromCriticalStrike.Length];
        for (int i = 0; i < abilityProp.BenefitFromCriticalStrike.Length; i++) {
            BenefitFromCriticalStrike[i] = new CritBenefitContainer(abilityProp.BenefitFromCriticalStrike[i]);
        }

        BenefitFromPenetration = new PeneBenefitContainer[abilityProp.BenefitFromPenetration.Length];
        for (int i = 0; i < abilityProp.BenefitFromPenetration.Length; i++) {
            BenefitFromPenetration[i] = new PeneBenefitContainer(abilityProp.BenefitFromPenetration[i]);
        }

        TravelSpeed = new SkillStatContainer(abilityProp.travelSpeed);
        MinCastRange = new SkillStatContainer(abilityProp.minCastRange);
        MaxCastRange = new SkillStatContainer(abilityProp.maxCastRange);
        HitRange = new SkillStatContainer(abilityProp.hitRange);
        HitAngle = new SkillStatContainer(abilityProp.hitAngle);
        Cooldown = new SkillStatContainer(abilityProp.cooldown);
    }

    public virtual IAoEValues TryGetAoEPropertiesValues() { return null; }
    public virtual IChargeValues TryGetChargePropertiesValues() { return null; }
    public virtual IProjectileValues TryGetProjectilePropertiesValues() { return null; }

    public float GetPenetrationBenefitValueByDamageType(DamageType damageType) => damageType switch {
        DamageType.Physical => PhysicalPenetrationBenefit,
        DamageType.Fire => FirePenetrationBenefit,
        DamageType.Ice => IcePenetrationBenefit,
        DamageType.Lightning => LightningPenetrationBenefit,
        DamageType.Poison => PoisonPenetrationBenefit,
        DamageType.Magical => PhysicalPenetrationBenefit,
        _ => 0f,
    };

    protected List<DamageTypeWeight> CopyDamageTypes(List<DamageTypeWeight> damageTypes) {
        if (damageTypes == null) return null;

        List<DamageTypeWeight> newDamageTypes = new List<DamageTypeWeight>(damageTypes.Count);

        for (int i = 0; i < damageTypes.Count; i++) {
            newDamageTypes.Add(new DamageTypeWeight(
                damageTypes[i].damageType,
                damageTypes[i].damageWeight,
                damageTypes[i].isMainDamageType));
        }

        return newDamageTypes;
    }
}
