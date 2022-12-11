using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public sealed class CoreAbilityData {

    public Character CharacterComponent { get; private set; }
    public IHitLayers CharacterHitLayers { get; private set; }
    public AbilityProperties AbilityProperties { get; private set; }
    public AbilityPropertiesValuesContainer AbilityPropertiesValuesContainer { get; private set; }
    public CoreStatsValuesContainer CoreStatsValues { get; private set; }
    public Combat Combat { get; private set; }
    public AbilityObject AbilityObject { get; private set; }
    public SkillTemplate SkillLogic { get; private set; }
    public Vector3 CastPoint { get; private set; }
    public GameObject Target { get; private set; }

    public CoreAbilityData(Character character, AbilityProperties abilityProperties, CoreStatsValuesContainer coreStatsValuesContainer, Combat combat,
        AbilityObject abilityObject, SkillTemplate skillLogic, Vector3 castPoint, GameObject target) {
        CharacterComponent = character;
        CharacterHitLayers = character;
        AbilityProperties = abilityProperties;
        CoreStatsValues = coreStatsValuesContainer;
        Combat = combat;
        AbilityObject = abilityObject;
        SkillLogic = skillLogic;
        CastPoint = castPoint;
        Target = target;
    }

    public CoreAbilityData(Character character, AbilityProperties abilityProperties, AbilityPropertiesValuesContainer abilityPropertiesValuesContainer,
        CoreStatsValuesContainer coreStatsValuesContainer, Combat combat, AbilityObject abilityObject, SkillTemplate skillLogic, Vector3 castPoint, GameObject target) {
        CharacterComponent = character;
        CharacterHitLayers = character;
        AbilityProperties = abilityProperties;
        AbilityPropertiesValuesContainer = abilityPropertiesValuesContainer;
        CoreStatsValues = coreStatsValuesContainer;
        Combat = combat;
        AbilityObject = abilityObject;
        SkillLogic = skillLogic;
        CastPoint = castPoint;
        Target = target;
    }

    public CoreAbilityData(CoreAbilityData coreAbilityData, bool nullAO = false) {
        CharacterComponent = coreAbilityData.CharacterComponent;
        CharacterHitLayers = coreAbilityData.CharacterHitLayers;
        AbilityProperties = coreAbilityData.AbilityProperties;
        AbilityPropertiesValuesContainer = coreAbilityData.AbilityPropertiesValuesContainer;
        CoreStatsValues = coreAbilityData.CoreStatsValues;
        Combat = coreAbilityData.Combat;
        AbilityObject = nullAO ? null : coreAbilityData.AbilityObject;
        SkillLogic = coreAbilityData.SkillLogic;
        CastPoint = coreAbilityData.CastPoint;
        Target = coreAbilityData.Target;
    }

    public CoreAbilityData GetCopy() {
        return new CoreAbilityData(this);
    }

    public CoreAbilityData GetCopyWithNullAbilityObject() {
        return new CoreAbilityData(this, true);
    }
    
    public void SetAbilityObject(AbilityObject abilityObject) {
        AbilityObject = abilityObject;
    }
}
