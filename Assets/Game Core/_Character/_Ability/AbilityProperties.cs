using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class AbilityPropertiesExtensions {
    public static void DestroyIfCopy(this AbilityProperties abilityProperties) {
        if (abilityProperties == null) return;
        if (abilityProperties.IsCopy) UnityEngine.Object.Destroy(abilityProperties);
    }
}

public class AbilityProperties : ScriptableObject, ICoreAbilityPropertiesProvider {
    public bool IsCopy { get; set; } = false;
    //--------Required-references--------
    [field: Header("References of the ability - assigned on runtime")]
    public Character CharacterComponent { protected get; set; } = null;
    //Reference to casters data object (Player | Enemy)
    [Header("Ability Properties")]
    public GameObject abilityPrefab;
    public string abilityId;
    new public string name = "new skill";
    public string hitInfoId;
    public Sprite icon;
    public SkillStat abilityDamage;
    [SerializeField]
    private List<DamageTypeWeight> damageTypes = new List<DamageTypeWeight>();
    public List<DamageTypeWeight> DamageTypes {
        get => damageTypes;
    }

    #region Crit benefit

    [SerializeField] private CritBenefit[] benefitFromCriticalStrike;
    public CritBenefit[] BenefitFromCriticalStrike { get => benefitFromCriticalStrike; set => benefitFromCriticalStrike = value; }

    public float AbsoluteCritChanceBenefit => BenefitFromCriticalStrike[0].CriticalStrikeBenefit.GetValue();
    public float RelativeCritChanceBenefit => BenefitFromCriticalStrike[1].CriticalStrikeBenefit.GetValue();
    public float AbsoluteCritDamageBenefit => BenefitFromCriticalStrike[2].CriticalStrikeBenefit.GetValue();
    public float RelativeCritDamageBenefit => BenefitFromCriticalStrike[3].CriticalStrikeBenefit.GetValue();
    #endregion

    #region Penetration benefit

    [SerializeField] private PeneBenefit[] benefitFromPenetration;
    public PeneBenefit[] BenefitFromPenetration { get => benefitFromPenetration; set => benefitFromPenetration = value; }

    public float PhysicalPenetrationBenefit => BenefitFromPenetration[0].PenetrationBenefit.GetValue();
    public float FirePenetrationBenefit => BenefitFromPenetration[1].PenetrationBenefit.GetValue();
    public float IcePenetrationBenefit => BenefitFromPenetration[2].PenetrationBenefit.GetValue();
    public float LightningPenetrationBenefit => BenefitFromPenetration[3].PenetrationBenefit.GetValue();
    public float PoisonPenetrationBenefit => BenefitFromPenetration[4].PenetrationBenefit.GetValue();

    #endregion

    public SkillStat travelSpeed;
    public SkillStat minCastRange;
    public SkillStat maxCastRange;
    public SkillStat hitRange;
    public SkillStat hitAngle;

    public SkillStat cooldown;

    //Tooltip
    protected StringBuilder abilityTooltip;
    protected bool isDirtyTooltip = true;
    public event Action<AbilityProperties> OnTooltipDirtied;

    public AbilityProperties GetCopy() {
        if (IsCopy) {
            return this;
        } else {
            Debug.LogError("You should not copy Ability Properties into Ability Objects anymore. Instead use value containers.");
            AbilityProperties copy = Instantiate(this);
            copy.IsCopy = true;
            return copy;
        }
    }

    public T GetCopy<T>() {
        if (IsCopy) {
            return (T)(object)this;
        } else {
            AbilityProperties copy = Instantiate(this);
            copy.IsCopy = true;
            return (T)(object)copy;
        }
    }

    public virtual AbilityPropertiesValuesContainer GetValuesCopy() {
        return new AbilityPropertiesValuesContainer(this);
    }

    public virtual void Awake() { }

    public virtual void PropertiesUserStartInitialized() { }

    public virtual void OnDisable() {
        //Debug.Log("Ability properties disabled.");
    }

    public virtual void OnValidate() {
        bool rebuildPene = false;

        if (BenefitFromPenetration != null) {
            for (int i = 0; i < BenefitFromPenetration.Length; i++) {
                if (!BenefitFromPenetration[i].PenetrationBenefitType.IsPenetration()) {
                    rebuildPene = true;
                    break;
                }
            }
        } else {
            rebuildPene = true;
        }

        if (rebuildPene) {
            List<SkillStat> backup = new List<SkillStat>();

            for (int i = 0; i < BenefitFromPenetration?.Length; i++) {
                backup.Add(BenefitFromPenetration[i].PenetrationBenefit);
            }

            BenefitFromPenetration = new PeneBenefit[5];

            List<StatType> penetrations = BasicMyEnumExtensions.GetPenetrationsList();

            for (int i = 0; i < benefitFromPenetration.Length; i++) {
                if (backup.Count > i) {
                    BenefitFromPenetration[i] = new PeneBenefit(penetrations[i], new SkillStat(backup[i].GetValue(), backup[i].GetMinValue(), backup[i].GetMaxValue() <= 0f ? 1f : backup[i].GetMaxValue()));
                } else {
                    BenefitFromPenetration[i] = new PeneBenefit(penetrations[i], new SkillStat(0f, 0f, 1f));
                }
            }
        }
    }


    public virtual void Initialize() {
        AssignReferences();
        CheckProperties();
        SetUpListeners();
    }

    public virtual void AssignReferences() {
        abilityDamage.SetTooltipDirtyMethod = SetTooltipIsDirty;
        for (int i = 0; i < BenefitFromCriticalStrike.Length; i++) {
            BenefitFromCriticalStrike[i].CriticalStrikeBenefit.SetTooltipDirtyMethod = SetTooltipIsDirty;
        }
        for (int i = 0; i < benefitFromPenetration.Length; i++) {
            benefitFromPenetration[i].PenetrationBenefit.SetTooltipDirtyMethod = SetTooltipIsDirty;
        }
        travelSpeed.SetTooltipDirtyMethod = SetTooltipIsDirty;
        maxCastRange.SetTooltipDirtyMethod = SetTooltipIsDirty;
        hitRange.SetTooltipDirtyMethod = SetTooltipIsDirty;
        hitAngle.SetTooltipDirtyMethod = SetTooltipIsDirty;
        cooldown.SetTooltipDirtyMethod = SetTooltipIsDirty;
    }

    public virtual void CheckProperties() {
        if (!CheckDamageTypesIntegrity(damageTypes)) {
            Debug.LogError("Damage types addition is not equal to 1!!!!!");
        }

        if (string.IsNullOrEmpty(abilityId)) {
            Debug.LogError($"Ability {nameof(abilityId)} of {ToString()} is null or empty!");
        }

        if (string.IsNullOrEmpty(hitInfoId)) {
            Debug.LogError($"Ability {nameof(hitInfoId)} of {ToString()} is null or empty!");
        }
    }

    public virtual void SetUpListeners() {
        if (CharacterComponent == null) return;

        CharacterComponent.CharacterStats.OnCharacterStatChange += SetTooltipRebuildIfRequired;
    }

    public virtual void SetTooltipRebuildIfRequired(Stat stat) { }

    public bool CheckDamageTypesIntegrity(List<DamageTypeWeight> damageTypes) {
        if (damageTypes == null || damageTypes.Count == 0) return true;

        float totalDamageTypeWeight = 0;
        for (int i = 0; i < damageTypes.Count; i++) {
            totalDamageTypeWeight += damageTypes[i].damageWeight;
        }
        return totalDamageTypeWeight == 1;
    }

    public float GetPenetrationBenefitValueByDamageType(DamageType damageType) => damageType switch {
        DamageType.Physical => PhysicalPenetrationBenefit,
        DamageType.Fire => FirePenetrationBenefit,
        DamageType.Ice => IcePenetrationBenefit,
        DamageType.Lightning => LightningPenetrationBenefit,
        DamageType.Poison => PoisonPenetrationBenefit,
        DamageType.Magical => PhysicalPenetrationBenefit,
        _ => 0f,
    };

    public virtual void AddDamageType(DamageType modifyTo, float value) {
        CalculateAddedDamageType(DamageTypes, modifyTo, value);
    }

    public virtual void RemoveDamageType(DamageType typeToRemove) {
        CalculateRemovedDamageType(DamageTypes, typeToRemove);
    }

    protected void CalculateAddedDamageType(List<DamageTypeWeight> _damageTypes, DamageType modifyTo, float value) {
        if (_damageTypes.FindIndex(x => x.damageType == modifyTo && x.isMainDamageType) != -1) return;

        int mainDamageTypesCount = 0;
        for (int i = 0; i < _damageTypes.Count; i++) {
            if (_damageTypes[i].isMainDamageType)
                mainDamageTypesCount += 1;
        }

        float modifyMainTypesBy = value / mainDamageTypesCount;
        for (int i = 0; i < _damageTypes.Count; i++) {
            if (_damageTypes[i].isMainDamageType)
                _damageTypes[i].damageWeight -= modifyMainTypesBy;
        }
        _damageTypes.Add(new DamageTypeWeight(modifyTo, value, false));
    }

    protected void CalculateRemovedDamageType(List<DamageTypeWeight> _damageTypes, DamageType typeToRemove) {
        if (_damageTypes.Count < 1) { return; }
        if (_damageTypes.FindIndex(x => x.damageType == typeToRemove && x.isMainDamageType) != -1) return;

        int damageTypeIndex = _damageTypes.FindIndex(x => x.damageType == typeToRemove);
        if (damageTypeIndex == -1) { return; }

        float modifyMainTypesBy = _damageTypes[damageTypeIndex].damageWeight;
        _damageTypes.RemoveAt(damageTypeIndex);

        int mainDamageTypesCount = 0;
        for (int i = 0; i < _damageTypes.Count; i++) {
            if (_damageTypes[i].isMainDamageType)
                mainDamageTypesCount += 1;
        }

        modifyMainTypesBy /= mainDamageTypesCount;
        for (int i = 0; i < _damageTypes.Count; i++) {
            if (_damageTypes[i].isMainDamageType)
                _damageTypes[i].damageWeight += modifyMainTypesBy;
        }
    }


    public void ModifyAbsoluteCritBenefit(CriticalStrikeBenefitType cbt, float value, float replace, bool remove) {
        for (int i = 0; i < BenefitFromCriticalStrike.Length; i++) {
            if (BenefitFromCriticalStrike[i].CriticalStrikeBenefitType == cbt) {
                if (remove) {
                    BenefitFromCriticalStrike[i].CriticalStrikeBenefit.RemoveAbsoluteModifier(value);

                } else {
                    BenefitFromCriticalStrike[i].CriticalStrikeBenefit.AddAbsoluteModifier(value, replace);
                }

                break;
            }
        }
    }

    public void ModifyRelativeCritBenefit(CriticalStrikeBenefitType cbt, float value, float replace, bool remove) {
        for (int i = 0; i < BenefitFromCriticalStrike.Length; i++) {
            if (BenefitFromCriticalStrike[i].CriticalStrikeBenefitType == cbt) {
                if (remove) {
                    BenefitFromCriticalStrike[i].CriticalStrikeBenefit.RemoveRelativeModifier(value);

                } else {
                    BenefitFromCriticalStrike[i].CriticalStrikeBenefit.AddRelativeModifier(value, replace);
                }

                break;
            }
        }
    }

    public void ModifyAbsolutePenetrationBenefit(StatType penetrationType, float value, float replace, bool remove) {
        if (!penetrationType.IsPenetration()) {
            Debug.LogError("You just tried to modify penetration, but sent enum does not describe penetration.");
            return;
        }

        for (int i = 0; i < BenefitFromPenetration.Length; i++) {
            if (BenefitFromPenetration[i].PenetrationBenefitType == penetrationType) {
                if (remove) {
                    BenefitFromPenetration[i].PenetrationBenefit.RemoveAbsoluteModifier(value);
                } else {
                    BenefitFromPenetration[i].PenetrationBenefit.AddAbsoluteModifier(value, replace);
                }

                break;
            }
        }
    }

    public void ModifyRelativePenetrationBenefit(StatType penetrationType, float value, float replace, bool remove) {
        if (!penetrationType.IsPenetration()) {
            Debug.LogError("You just tried to modify penetration, but sent enum does not describe penetration.");
            return;
        }

        for (int i = 0; i < BenefitFromPenetration.Length; i++) {
            if (BenefitFromPenetration[i].PenetrationBenefitType == penetrationType) {
                if (remove) {
                    BenefitFromPenetration[i].PenetrationBenefit.RemoveRelativeModifier(value);
                } else {
                    BenefitFromPenetration[i].PenetrationBenefit.AddRelativeModifier(value, replace);
                }

                break;
            }
        }
    }


    protected virtual void BuildTooltipText() {
        isDirtyTooltip = false;
    }

    public StringBuilder GetTooltip() {
        if (isDirtyTooltip) BuildTooltipText();

        return abilityTooltip;
    }

    public void SetTooltipIsDirty() {
        isDirtyTooltip = true;
        OnTooltipDirtied?.Invoke(this);
    }
}
