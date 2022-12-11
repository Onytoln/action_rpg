using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

[CreateAssetMenu(fileName = "Frost Debuff", menuName = "Status Effects/Debuffs/Frost Debuff")]
public class FrostDebuff : Debuff {

    private FrostDebuffProperties frostDebuffProperties;

    [SerializeField] private Material frostedMaterial;
    static readonly byte minMaterialAlpha = 35;
    static readonly byte maxMaterialAlpha = 80;
    [SerializeField] float materialLerpSpeed = 0.8f;
    private Material[] materials;
    private CoroutineHandle? materialsLerpCoroutine;

    [SerializeField] private AudioSource audioPrefab;
    private AudioSource audio;

    float addedAttackSpeedSlow = 0f;
    public float AddedAttackSpeedSlow { get => addedAttackSpeedSlow; }

    float addedMovementSpeedSlow = 0f;
    public float AddedMovementSpeedSlow { get => addedMovementSpeedSlow; }

    float addedHealingEffectivityReduction = 0f;
    public float AddedHealingEffectivityReduction { get => addedHealingEffectivityReduction; }

    public override void Awake() {
        base.Awake();

        if (DebuffTypes == null || DebuffTypes.Length == 0) {
            DebuffTypes = new DebuffType[2] { DebuffType.Frost, DebuffType.Slow };
        }
    }

    public override void Apply(int stacksCount, HitOutput hitOutput) {
        base.Apply(stacksCount, hitOutput);

        frostDebuffProperties = statusEffectProperties as FrostDebuffProperties;

        AppliedToStatusEffectsManager.OnIsDeadChanged += AppliedToDied;

        UpdateDebuffStrengthModifiers(ApplierStatsContainer);

        AppliedToStatusEffectsManager.SetIsSlowed(true);

        HandleDebuffApplication();
    }

    public override void Refresh(CoreStatsValuesContainer _applierStatsContainer, int stacksCount, HitOutput hitOutput) {
        base.Refresh(_applierStatsContainer, stacksCount, hitOutput);

        UpdateDebuffStrengthModifiers(_applierStatsContainer);

        HandleDebuffApplication();

        HandleMaterialLerp();
    }

    private void HandleDebuffApplication() {
        float attackSpeedSlowValue = -(frostDebuffProperties.attackSpeedSlowAmount.GetValue() * CurrentStacks) *
            (DebuffStrenghtModifier - (frostDebuffProperties.iceResistanceProtectionModifier.GetValue() * AppliedToStats.IceResistanceValue));

        if (attackSpeedSlowValue > 0f) attackSpeedSlowValue = 0f;

        float movementSpeedSlowValue = -(frostDebuffProperties.movementSpeedSlowAmount.GetValue() * CurrentStacks) *
            (DebuffStrenghtModifier - (frostDebuffProperties.iceResistanceProtectionModifier.GetValue() * AppliedToStats.IceResistanceValue));

        if (movementSpeedSlowValue > 0f) movementSpeedSlowValue = 0f;

        float healingEffectivityReduction = -(frostDebuffProperties.healingEffectivityDecrease.GetValue() * CurrentStacks) * DebuffStrenghtModifier;

        if (healingEffectivityReduction > 0f) healingEffectivityReduction = 0f;

        AppliedToCharacterComponent.CharacterStats.AddRelativeStat(StatType.AttackSpeed, attackSpeedSlowValue, addedAttackSpeedSlow);
        AppliedToCharacterComponent.CharacterStats.AddRelativeStat(StatType.MovementSpeed, movementSpeedSlowValue, addedMovementSpeedSlow);
        AppliedToCharacterComponent.CharacterStats.AddAbsoluteStat(StatType.HealingEffectivity, healingEffectivityReduction, addedHealingEffectivityReduction);

        addedAttackSpeedSlow = attackSpeedSlowValue;
        addedMovementSpeedSlow = movementSpeedSlowValue;
        addedHealingEffectivityReduction = healingEffectivityReduction;
    }

    public override void End() {
        base.End();

        AppliedToCharacterComponent.CharacterStats.RemoveRelativeStat(StatType.AttackSpeed, addedAttackSpeedSlow);
        AppliedToCharacterComponent.CharacterStats.RemoveRelativeStat(StatType.MovementSpeed, addedMovementSpeedSlow);
        AppliedToCharacterComponent.CharacterStats.RemoveAbsoluteStat(StatType.HealingEffectivity, addedHealingEffectivityReduction);

        AppliedToStatusEffectsManager.SetIsSlowed(false);
    }

    public override void OnStartVfx() {
        Utils.AddMaterials(AppliedToCharacterComponent.AllCharacterRenderers, Utils.CreateNewMaterialInstance(frostedMaterial));

        materials = Utils.GetMaterialsByName(AppliedToCharacterComponent.AllCharacterRenderers, frostedMaterial.name);

        materialsLerpCoroutine = Utils.LerpMaterials(materialsLerpCoroutine, materials, minMaterialAlpha, materialLerpSpeed);
    }

    public override void OnEndVfx() {
        _ = Timing.KillCoroutines((CoroutineHandle)materialsLerpCoroutine);

        materials ??= Utils.GetMaterialsByName(AppliedToCharacterComponent.AllCharacterRenderers, frostedMaterial.name);

        materialsLerpCoroutine = Utils.LerpMaterials(materialsLerpCoroutine, materials, 0, materialLerpSpeed, MaterialLerpFinalEnd);

        Timing.RunCoroutine(UnsubscribeOnLerpEnd());
    }

    private void HandleMaterialLerp() {
        materialsLerpCoroutine = Utils.LerpMaterials(
            materialsLerpCoroutine,
            materials,
            (byte)(maxMaterialAlpha - ((maxMaterialAlpha - minMaterialAlpha) * (1f - (CurrentStacks / statusEffectProperties.maxStacks.GetValue())))),
            materialLerpSpeed);
    }

    private void MaterialLerpFinalEnd() {
        Utils.RemoveMaterialsByName(AppliedToCharacterComponent.AllCharacterRenderers, frostedMaterial.name);
    }

    private void AppliedToDied(bool state) {
        Timing.KillCoroutines((CoroutineHandle)materialsLerpCoroutine);
        AppliedToStatusEffectsManager.OnIsDeadChanged -= AppliedToDied;
    }

    private IEnumerator<float> UnsubscribeOnLerpEnd() {
        yield return Timing.WaitUntilDone((CoroutineHandle)materialsLerpCoroutine);
        AppliedToStatusEffectsManager.OnIsDeadChanged -= AppliedToDied;
    }

    /*private IEnumerator<float> MaterialsLerpCoroutine() {
        time = 0f;
        Color color;

        materials ??= StaticUtils.GetMaterials(AppliedToCharacterComponent.AllCharacterRenderers, frostedMaterial.name);

        float startValLerpTo = StaticUtils.Color32ValTo01(minMaterialAlpha);
       
        while (time < 1f && !interruptFirstStageLerp) {
            Lerp(0f, startValLerpTo, materials);
            yield return Timing.WaitForOneFrame;
        }
       
        while (true) {
            if (time < 1f) {
                if (lerpFrom == -1f) lerpFrom = materials[0].color.a;
                Lerp(lerpFrom, lerpTo, materials);
            }

            yield return Timing.WaitForOneFrame;
        }

        void Lerp(float from, float to, Material[] materials) {
            for (int i = 0; i < materials.Length; i++) {
                color = materials[i].color;
                color.a = Mathf.Lerp(from, to, time);
                materials[i].color = color;
            }

            time += Time.deltaTime * materialLerpSpeed;
        }
    }

    private IEnumerator<float> MaterialsEndingLerpCoroutine() {
        time = 0f;
        Color color;

        materials ??= StaticUtils.GetMaterials(AppliedToCharacterComponent.AllCharacterRenderers, frostedMaterial.name);

        while (time < 1f && !appliedToDied) {
            for (int i = 0; i < materials.Length; i++) {
                color = materials[i].color;
                color.a = Mathf.Lerp(materials[0].color.a, 0f, time);
                materials[i].color = color;
            }

            time += Time.deltaTime * materialLerpSpeed;

            yield return Timing.WaitForOneFrame;
        }

        StaticUtils.RemoveMaterial(AppliedToCharacterComponent.AllCharacterRenderers, frostedMaterial);
    }*/

}

