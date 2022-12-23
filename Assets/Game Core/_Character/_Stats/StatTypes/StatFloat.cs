using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatFloat : StatBase {
    [field: System.NonSerialized] public Action SetTooltipDirtyMethod { get; set; }

    [SerializeField, Header("Core values")] private float primaryValue;
    [SerializeField] private float minStatValue;
    [SerializeField] private float maxStatValue;
    [NonSerialized] private List<float> absoluteModifiers = new List<float>();
    [NonSerialized] private List<float> relativeModifiers = new List<float>();
    [SerializeField, Header("Total value")]
    private float totalValue;

    [SerializeField, HideInInspector] private bool isDirty = true;

    [NonSerialized] private bool initialized = false;

    public StatFloat(float primaryValue, float minStatValue, float maxStatValue) {
        this.primaryValue = primaryValue;
        this.minStatValue = minStatValue;
        this.maxStatValue = maxStatValue;
    }

    private void CalculateValue() {
        totalValue = primaryValue;
        float totalRelativeMods = 0f;
        absoluteModifiers.ForEach(x => totalValue += x);
        relativeModifiers.ForEach(x => totalRelativeMods += x);
        totalValue *= 1 + totalRelativeMods;
        totalValue = Mathf.Clamp(totalValue, minStatValue, maxStatValue);
    }

    public float GetValue() {
        Initialize();
        if (isDirty) {
            isDirty = false;
            CalculateValue();
        }
        return totalValue;
    }

    public void AddAbsoluteModifier(float absoluteModifier, float replace = 0f) {
        if (absoluteModifier == 0f && replace == 0f) { return; }
        Initialize();

        if (replace != 0f) {
            int index = absoluteModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                absoluteModifiers[index] = absoluteModifier;
            }
        } else {
            absoluteModifiers.Add(absoluteModifier);
        }

        SetTooltipDirty();
        StatChanged();
    }

    public void RemoveAbsoluteModifier(float absoluteModifier) {
        if (absoluteModifier == 0f) { return; }
        Initialize();

        absoluteModifiers.Remove(absoluteModifier);

        SetTooltipDirty();
        StatChanged();
    }

    public void AddRelativeModifier(float relativeModifier, float replace = 0f) {
        if (relativeModifier == 0f && replace == 0f) { return; }
        Initialize();

        if (replace != 0f) {
            int index = relativeModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                relativeModifiers[index] = relativeModifier;
            }
        } else {
            relativeModifiers.Add(relativeModifier);
        }

        SetTooltipDirty();
        StatChanged();
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        if (relativeModifier == 0f) { return; }
        Initialize();

        relativeModifiers.Remove(relativeModifier);

        SetTooltipDirty();
        StatChanged();
    }

    public void SetPrimaryValue(float value) {
        if (value == 0f) { return; }
        Initialize();

        primaryValue = value;

        SetTooltipDirty();
        StatChanged();
    }

    public float GetPrimaryValue() {
        return primaryValue;
    }

    public float GetMinValue() {
        return minStatValue;
    }

    public float GetMaxValue() {
        return maxStatValue;
    }

    public void SetMinMax(float min, float max) {
        Initialize();

        minStatValue = min;
        maxStatValue = max;

        SetTooltipDirty();
        StatChanged();
    }

    public void SetTooltipDirty() {
        isDirty = true;
        SetTooltipDirtyMethod?.Invoke();
    }

    private void Initialize() {
        if (initialized) return;
        initialized = true;

        absoluteModifiers ??= new List<float>();
        relativeModifiers ??= new List<float>();

        isDirty = true;
        _ = GetValue();
    }
}
