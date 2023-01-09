using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitInput {
    public CoreAbilityData CoreAbilityData { get; private set; } = null;
    //Reference to combat of oponent that we want to hit
    public Combat TargetCombatComponent { get; private set; }
    //% value of skills damage
    public float AbilityDamageValue { get; private set; }
    public string CurrentHitInfoId { get; private set; }
    //passed damage types - if no damage types are passed, the default damage type from ability properties is used
    private readonly List<DamageTypeWeight> preferedDamageTypes;
    //damage types
    public List<DamageTypeWeight> CurrentDamageTypes { get; private set; }
    //total damage of the hit
    [SerializeField]
    private HitStat totalDamage;
    private bool totalDamageLoaded = false;
    //total crit chance of the hit
    [SerializeField]
    private HitStat totalCritChance;
    //total crit damage of the hit
    [SerializeField]
    private HitStat totalCritDamage;
    //total penetrations of the hit
    [SerializeField]
    private HitStat[] totalPenetration;

    private readonly ICoreAbilityPropertiesProvider coreAbilityPropertiesProvider;

    #region Static Hit/Damage Enemy methods

    public static HitOutput HitEnemy(Collider other, float damageValue, string currentHitInfoId, Character attackerCharacterComponent,
       AbilityProperties abilityProperties, AbilityPropertiesValuesContainer abilityPropertiesValuesContainer, CharStatsValContainer coreStats,
       AbilityObject abilityObject, SkillTemplate skillLogic, Vector3 castPoint, GameObject target, List<DamageTypeWeight> damageTypes = null) {

        HitInput hitInput = new HitInput(
            new CoreAbilityData(attackerCharacterComponent,
            abilityProperties,
            abilityPropertiesValuesContainer,
            coreStats,
            attackerCharacterComponent.CharacterCombat,
            abilityObject,
            skillLogic,
            castPoint,
            target),

            other.GetComponent<Combat>(),
            damageValue,
            currentHitInfoId,
            damageTypes
        );

        if (hitInput.TargetCombatComponent == null)
            return null;

        return attackerCharacterComponent.CharacterCombat.HitEnemy(hitInput);
    }

    public static HitOutput HitEnemy(Combat targetCombatComponent, float damageValue, string currentHitInfoId, Character attackerCharacterComponent,
        AbilityProperties abilityProperties, AbilityPropertiesValuesContainer abilityPropertiesValuesContainer, CharStatsValContainer coreStats,
        AbilityObject abilityObject, SkillTemplate skillLogic, Vector3 castPoint, GameObject target, List<DamageTypeWeight> preferedDamageTypes = null) {

        HitInput hitInput = new HitInput(
            new CoreAbilityData(attackerCharacterComponent,
            abilityProperties,
            abilityPropertiesValuesContainer,
            coreStats,
            attackerCharacterComponent.CharacterCombat,
            abilityObject,
            skillLogic,
            castPoint,
            target),

            targetCombatComponent,
            damageValue,
            currentHitInfoId,
            preferedDamageTypes
        );

        if (hitInput.TargetCombatComponent == null)
            return null;

        return attackerCharacterComponent.CharacterCombat.HitEnemy(hitInput);
    }

    public static HitOutput HitEnemyExactDamage(Collider other, float exactDamage, string currentHitInfoId, Character attackerCharacterComponent,
       AbilityProperties abilityProperties, AbilityPropertiesValuesContainer abilityPropertiesValuesContainer, CharStatsValContainer coreStats,
       AbilityObject abilityObject, SkillTemplate skillLogic, Vector3 castPoint, GameObject target, List<DamageTypeWeight> damageTypes = null) {

        HitInput hitInput = new HitInput(
            new CoreAbilityData(attackerCharacterComponent,
            abilityProperties,
            abilityPropertiesValuesContainer,
            coreStats,
            attackerCharacterComponent.CharacterCombat,
            abilityObject,
            skillLogic,
            castPoint,
            target),

            exactDamage,
            other.GetComponent<Combat>(),
            currentHitInfoId,
            damageTypes
        );

        if (hitInput.TargetCombatComponent == null)
            return null;

        return attackerCharacterComponent.CharacterCombat.DamageEnemyViaNonHit(hitInput);
    }

    public static HitOutput HitEnemyExactDamage(Combat targetCombatComponent, float exactDamage, string currentHitInfoId, Character attackerCharacterComponent,
        AbilityProperties abilityProperties, AbilityPropertiesValuesContainer abilityPropertiesValuesContainer, CharStatsValContainer coreStats,
        AbilityObject abilityObject, SkillTemplate skillLogic, Vector3 castPoint, GameObject target, List<DamageTypeWeight> preferedDamageTypes = null) {

        HitInput hitInput = new HitInput(
            new CoreAbilityData(attackerCharacterComponent,
            abilityProperties,
            abilityPropertiesValuesContainer,
            coreStats,
            attackerCharacterComponent.CharacterCombat,
            abilityObject,
            skillLogic,
            castPoint,
            target),

            exactDamage,
            targetCombatComponent,
            currentHitInfoId,
            preferedDamageTypes
        );

        if (hitInput.TargetCombatComponent == null)
            return null;

        return attackerCharacterComponent.CharacterCombat.DamageEnemyViaNonHit(hitInput);
    }

    public static HitOutput HitEnemy(Combat targetCombatComponent, CoreAbilityData coreAbilityData, float damageValue, string currentHitInfoId,
        List<DamageTypeWeight> preferedDamageTypes = null) {

        HitInput hitInput = new HitInput(
           coreAbilityData,
           targetCombatComponent,
           damageValue,
           currentHitInfoId,
           preferedDamageTypes
        );

        if (hitInput.TargetCombatComponent == null)
            return null;

        return coreAbilityData.CharacterComponent.CharacterCombat.HitEnemy(hitInput);
    }

    public static HitOutput HitEnemy(Collider other, CoreAbilityData coreAbilityData, float damageValue, string currentHitInfoId,
       List<DamageTypeWeight> preferedDamageTypes = null) {

        HitInput hitInput = new HitInput(
           coreAbilityData,
           other.GetComponent<Combat>(),
           damageValue,
           currentHitInfoId,
           preferedDamageTypes
        );

        if (hitInput.TargetCombatComponent == null)
            return null;

        return coreAbilityData.CharacterComponent.CharacterCombat.HitEnemy(hitInput);
    }

    public static HitOutput HitEnemyExactDamage(Combat targetCombatComponent, CoreAbilityData coreAbilityData, float exactDamage, string currentHitInfoId,
      List<DamageTypeWeight> preferedDamageTypes = null) {

        HitInput hitInput = new HitInput(
           coreAbilityData,
           exactDamage,
           targetCombatComponent,
           currentHitInfoId,
           preferedDamageTypes
        );

        if (hitInput.TargetCombatComponent == null)
            return null;

        return coreAbilityData.CharacterComponent.CharacterCombat.DamageEnemyViaNonHit(hitInput);
    }

    public static HitOutput HitEnemyExactDamage(Collider other, CoreAbilityData coreAbilityData, float exactDamage, string currentHitInfoId,
       List<DamageTypeWeight> preferedDamageTypes = null) {

        HitInput hitInput = new HitInput(
           coreAbilityData,
           exactDamage,
           other.GetComponent<Combat>(),
           currentHitInfoId,
           preferedDamageTypes
        );

        if (hitInput.TargetCombatComponent == null)
            return null;

        return coreAbilityData.CharacterComponent.CharacterCombat.DamageEnemyViaNonHit(hitInput);
    }

    #endregion

    private HitInput(CoreAbilityData coreAbilityData, Combat targetCombat, float abilityDamageValue, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        CoreAbilityData = coreAbilityData;

        if (coreAbilityData.CoreStatsValues == null) {
            coreCharacterStatsProvider = coreAbilityData.CharacterComponent.CharacterStats.CoreStats;
        } else {
            coreCharacterStatsProvider = coreAbilityData.CoreStatsValues;
        }

        if (coreAbilityData.AbilityPropertiesValuesContainer == null) {
            coreAbilityPropertiesProvider = coreAbilityData.AbilityProperties;
        } else {
            coreAbilityPropertiesProvider = coreAbilityData.AbilityPropertiesValuesContainer;
        }

        TargetCombatComponent = targetCombat;
        AbilityDamageValue = abilityDamageValue;
        CurrentHitInfoId = currentHitInfoId;
        this.preferedDamageTypes = preferedDamageTypes;

        CalculateHitData();
    }

    private HitInput(CoreAbilityData coreAbilityData, float exactDamageToDeal, Combat targetCombat, string currentHitInfoId, List<DamageTypeWeight> preferedDamageTypes = null) {
        CoreAbilityData = coreAbilityData;

        if (coreAbilityData.CoreStatsValues == null) {
            coreCharacterStatsProvider = coreAbilityData.CharacterComponent.CharacterStats.CoreStats;
        } else {
            coreCharacterStatsProvider = coreAbilityData.CoreStatsValues;
        }

        if (coreAbilityData.AbilityPropertiesValuesContainer == null) {
            coreAbilityPropertiesProvider = coreAbilityData.AbilityProperties;
        } else {
            coreAbilityPropertiesProvider = coreAbilityData.AbilityPropertiesValuesContainer;
        }

        TargetCombatComponent = targetCombat;
        CurrentHitInfoId = currentHitInfoId;
        this.preferedDamageTypes = preferedDamageTypes;
        LoadExactDamageToDeal(exactDamageToDeal);

        CalculateHitData();
    }

    /// <summary>
    /// Calculates pre-hit final values
    /// </summary>
    private void CalculateHitData() {

        ProcessDamageTypes();
        ProcessDamage();
        ProcessCritChance();
        ProcessCritDamage();
        ProcessPenetration();

        //RecalculateResults();
    }

    private void ProcessDamageTypes() {
        if (preferedDamageTypes == null) {
            CurrentDamageTypes = new List<DamageTypeWeight>(coreAbilityPropertiesProvider.DamageTypes.Count);

            for (int i = 0; i < coreAbilityPropertiesProvider.DamageTypes.Count; i++) {
                CurrentDamageTypes.Add(new DamageTypeWeight(
                    coreAbilityPropertiesProvider.DamageTypes[i].damageType,
                    coreAbilityPropertiesProvider.DamageTypes[i].damageWeight,
                    coreAbilityPropertiesProvider.DamageTypes[i].isMainDamageType));
            }
        } else {
            CurrentDamageTypes = new List<DamageTypeWeight>(preferedDamageTypes.Count);

            for (int i = 0; i < preferedDamageTypes.Count; i++) {
                CurrentDamageTypes.Add(new DamageTypeWeight(
                    preferedDamageTypes[i].damageType,
                    preferedDamageTypes[i].damageWeight,
                    preferedDamageTypes[i].isMainDamageType));
            }
        }
    }

    private void LoadExactDamageToDeal(float exactDamageToDeal) {
        if (exactDamageToDeal == 0f) return;

        totalDamage = new HitStat(exactDamageToDeal, 0f, float.MaxValue);
        totalDamageLoaded = true;
    }

    private void ProcessDamage() {
        if (totalDamageLoaded) return;

        totalDamage = new HitStat(AbilityDamageValue * coreCharacterStatsProvider.DamageStat.Value, 0f, float.MaxValue);
    }

    private void ProcessCritChance() {
        totalCritChance = new HitStat(coreCharacterStatsProvider.CriticalStrikeChanceStat.Value, 0f, 1f);

        totalCritChance.AddAbsoluteModifier(coreAbilityPropertiesProvider.AbsoluteCritChanceBenefit);
        totalCritChance.AddRelativeModifier(coreAbilityPropertiesProvider.RelativeCritChanceBenefit);
    }

    private void ProcessCritDamage() {
        totalCritDamage = new HitStat(coreCharacterStatsProvider.CriticalDamageStat.Value, 0f, 7f);

        totalCritDamage.AddAbsoluteModifier(coreAbilityPropertiesProvider.AbsoluteCritDamageBenefit);
        totalCritDamage.AddRelativeModifier(coreAbilityPropertiesProvider.RelativeCritDamageBenefit);
    }

    private void ProcessPenetration() {
        totalPenetration = new HitStat[CurrentDamageTypes.Count];

        for (int i = 0; i < totalPenetration.Length; i++) {
            totalPenetration[i] = new HitStat(coreCharacterStatsProvider.GetPenetrationValueByDamageType(CurrentDamageTypes[i].damageType), 0f, 1f);
            totalPenetration[i].AddAbsoluteModifier(coreAbilityPropertiesProvider.GetPenetrationBenefitValueByDamageType(CurrentDamageTypes[i].damageType));
        }
    }

    public void AddDamageType(DamageType modifyTo, float value) {
        if (CurrentDamageTypes.FindIndex(x => x.damageType == modifyTo && x.isMainDamageType) != -1) return;

        float modifyOthersBy = value / CurrentDamageTypes.Count;
        for (int i = 0; i < CurrentDamageTypes.Count; i++) {
            CurrentDamageTypes[i].damageWeight -= modifyOthersBy;
        }
        CurrentDamageTypes.Add(new DamageTypeWeight(modifyTo, value, false));
        ProcessPenetration();
    }

    public float RemoveDamageType(DamageType typeToRemove) {
        if (CurrentDamageTypes.Count < 1) { return 0; }
        if (CurrentDamageTypes.FindIndex(x => x.damageType == typeToRemove && x.isMainDamageType) != -1) return 0;

        int damageTypeIndex = CurrentDamageTypes.FindIndex(x => x.damageType == typeToRemove);
        if (damageTypeIndex == -1) { return 0; }

        float damageToRemove = -(totalDamage.GetTotalAbsoluteValue() * CurrentDamageTypes[damageTypeIndex].damageWeight);
        totalDamage.AddAbsoluteModifier(damageToRemove);
        CurrentDamageTypes.RemoveAt(damageTypeIndex);
        ProcessPenetration();

        return damageToRemove;
    }

    public void AddTotalDamageRelativeModifier(float relativeModifier) {
        totalDamage.AddRelativeModifier(relativeModifier);
        //totalDamage.CalculateValue();
    }

    public void RemoveTotalDamageRelativeModifier(float relativeModifier) {
        totalDamage.RemoveRelativeModifier(relativeModifier);
        //totalDamage.CalculateValue();
    }

    public float GetTotalDamage() {
        return totalDamage.GetValue();
    }

    public float GetTotalCritChance() {
        return totalCritChance.GetValue();
    }

    public float GetTotalCritDamage() {
        return totalCritDamage.GetValue();
    }

    public float GetTotalPenetration(int index) {
        return totalPenetration[index].GetValue();
    }

    public void RecalculateResults() {
        totalDamage.CalculateValue();
        totalCritChance.CalculateValue();
        totalCritDamage.CalculateValue();
        for (int i = 0; i < totalPenetration.Length; i++) {
            totalPenetration[i].CalculateValue();
        }
    }

    public bool CheckForNulls() {
        if (CoreAbilityData == null || CoreAbilityData.AbilityProperties == null || TargetCombatComponent == null || AbilityDamageValue == 0) {
            return true;
        }
        return false;
    }
}
