using System.Collections.Generic;
using UnityEngine;


public class ScalableStat : ChracterStat {
    [Header("Scalable stat properties")]
    [SerializeField] private int defaultScaleValue = 100;
    private int finalScaleValue;
    [SerializeField] private float minScalableStatValue = -0.2f;
    [SerializeField] private float maxScalableStatValue = 0.8f;
    private List<float> maxScaledStatValueModifiers = new List<float>();
    private float finalMaxScaledStatValue;
    private bool scaledCapIsDirty = true;

    [SerializeField] private float totalScaledValue;

    [SerializeField, HideInInspector] private bool scalableIsDirty = true;

    public override void Initialize() {
        if (minStatValue == 0) minStatValue = float.NegativeInfinity;
        if (maxStatValue == 0) maxStatValue = float.PositiveInfinity;
      
        scalableIsDirty = true;
        scaledCapIsDirty = true;
        finalScaleValue = defaultScaleValue;

        base.Initialize();
    }

    public void CalculateScalableValue() {
        if (scaledCapIsDirty) {
            scaledCapIsDirty = false;

            float capMods = 0f;
            maxScaledStatValueModifiers.ForEach(x => capMods += x);

            finalMaxScaledStatValue = maxScalableStatValue + capMods;
        }

        totalScaledValue = Mathf.Clamp(base.GetValue() / finalScaleValue, minScalableStatValue, finalMaxScaledStatValue);
    }

    public override float GetValue() {
        if (scalableIsDirty) {
            scalableIsDirty = false;
            CalculateScalableValue();
        }

        return totalScaledValue;
    }

    public override void SetScaleValue(int valueMultiplier) {
        Mathf.Clamp(finalScaleValue = defaultScaleValue * valueMultiplier, minScalableStatValue, 0.99f);
        SetIsDirty();
    }

    public override void UncapScalableStatValue(float uncapValue) {
        if (uncapValue == 0) return;

        maxScaledStatValueModifiers.Add(uncapValue);

        scaledCapIsDirty = true;
        SetIsDirty();
    }

    public override void CapScalableStatValue(float capValue) {
        if (capValue == 0) return;

        int index = maxScaledStatValueModifiers.FindIndex(x => x == capValue);
        if (index == -1) return;

        maxScaledStatValueModifiers.RemoveAt(index);

        scaledCapIsDirty = true;
        SetIsDirty();
    }

    public override void SetIsDirty() {
        base.SetIsDirty();
        scalableIsDirty = true;
    }

    public override float GetMinPossibleValue() {
        return minScalableStatValue;
    }

    public override float GetMaxPossibleValue() {
        return finalMaxScaledStatValue;
    }

    public int GetValueNeededForMaxScaledValue() {
        return (int)(finalMaxScaledStatValue * finalScaleValue);
    }
}
