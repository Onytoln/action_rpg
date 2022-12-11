using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AbilityProperties), true)]
public class PreloadDefaultAbilityPropertiesValues : Editor
{
    public override void OnInspectorGUI() {
        if (GUILayout.Button("Preload defaults to this ability properties")) {
            PreloadDefaults();
        }

        base.OnInspectorGUI();
    }

    public void PreloadDefaults() {
        AbilityProperties abilityProperties = target as AbilityProperties;

        if(abilityProperties == null) {
            Debug.Log("Error occured, please try again.");
            return;
        }

        if (abilityProperties.abilityDamage == null || abilityProperties.abilityDamage.GetMaxValue() == 0) {
            abilityProperties.abilityDamage = new SkillStat(0, 0, 10);
        }

        if(abilityProperties.DamageTypes == null || abilityProperties.DamageTypes.Count == 0) {
            abilityProperties.DamageTypes.Add(new DamageTypeWeight(DamageType.Physical, 1, true));
        }

        if (abilityProperties.BenefitFromCriticalStrike == null || abilityProperties.BenefitFromCriticalStrike.Length == 0) {
            abilityProperties.BenefitFromCriticalStrike = new CritBenefit[4];
            abilityProperties.BenefitFromCriticalStrike[0] = new CritBenefit(CriticalStrikeBenefitType.CritChanceAbsolute, new SkillStat(0f, 0f, 1f));
            abilityProperties.BenefitFromCriticalStrike[1] = new CritBenefit(CriticalStrikeBenefitType.CritChanceRelative, new SkillStat(0f, 0f, 1f));
            abilityProperties.BenefitFromCriticalStrike[2] = new CritBenefit(CriticalStrikeBenefitType.CritDamageAbsolute, new SkillStat(0f, 0f, 3f));
            abilityProperties.BenefitFromCriticalStrike[3] = new CritBenefit(CriticalStrikeBenefitType.CritDamageRelative, new SkillStat(0f, 0f, 1f));
        }

        if (abilityProperties.BenefitFromPenetration == null || abilityProperties.BenefitFromPenetration.Length == 0) {
            abilityProperties.BenefitFromPenetration = new PeneBenefit[5];
            abilityProperties.BenefitFromPenetration[0] = new PeneBenefit(StatType.PhysicalPenetration, new SkillStat(0, 0, 1f));
            abilityProperties.BenefitFromPenetration[1] = new PeneBenefit(StatType.FirePenetration, new SkillStat(0, 0, 1f));
            abilityProperties.BenefitFromPenetration[2] = new PeneBenefit(StatType.IcePenetration, new SkillStat(0, 0, 1f));
            abilityProperties.BenefitFromPenetration[3] = new PeneBenefit(StatType.LightningPenetration, new SkillStat(0, 0, 1f));
            abilityProperties.BenefitFromPenetration[4] = new PeneBenefit(StatType.PoisonPenetration, new SkillStat(0, 0, 1f));
        }



        EditorUtility.SetDirty(target);
    }
}
