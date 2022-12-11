using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SkillStatInt {
    public event Action<SkillStatInt> OnStatChanged;
    public string description;
    [System.NonSerialized] public Action setTooltipDirtyMethod;

    [SerializeField, Header("Core values")] private int primaryValue;
    [SerializeField] private int minStatValue;
    [SerializeField] private int maxStatValue;
    [NonSerialized] private List<int> absoluteModifiers = new List<int>();
    [NonSerialized] private List<float> relativeModifiers = new List<float>();
    [SerializeField, Header("Total value")] private int totalValue;

    [SerializeField, HideInInspector] private bool isDirty = true;

    [NonSerialized] private bool initialized = false;

    public SkillStatInt(string description, int primaryValue, int minStatValue, int maxStatValue) {
        this.description = description;
        this.primaryValue = primaryValue;
        this.minStatValue = minStatValue;
        this.maxStatValue = maxStatValue;
    }

    private void CalculateValue() {
        totalValue = primaryValue;
        float totalRelativeMods = 0;
        absoluteModifiers.ForEach(x => totalValue += x);
        relativeModifiers.ForEach(x => totalRelativeMods += x);
        totalValue = (int)(totalValue * (1 + totalRelativeMods));
        totalValue = Mathf.Clamp(totalValue, minStatValue, maxStatValue);
    }

    public int GetValue() {
        Initialize();
        if (isDirty) {
            isDirty = false;
            CalculateValue();
        }
        return totalValue;
    }

    public void AddAbsoluteModifier(int absoluteModifier, int replace = 0) {
        if (absoluteModifier == 0 && replace == 0) { return; }
        Initialize();

        if (replace != 0) {
            int index = absoluteModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                absoluteModifiers[index] = absoluteModifier;
            }
        } else {
            absoluteModifiers.Add(absoluteModifier);
        }

        SetTooltipDirty();
        OnStatChanged?.Invoke(this);
    }

    public void RemoveAbsoluteModifier(int absoluteModifier) {
        if (absoluteModifier == 0) { return; }
        Initialize();

        absoluteModifiers.Remove(absoluteModifier);

        SetTooltipDirty();
        OnStatChanged?.Invoke(this);
    }

    public void AddRelativeModifier(float relativeModifier, float replace = 0) {
        if (relativeModifier == 0f && replace == 0f) return;
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
        OnStatChanged?.Invoke(this);
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        if (relativeModifier == 0f) return;
        Initialize();

        relativeModifiers.Remove(relativeModifier);

        SetTooltipDirty();
        OnStatChanged?.Invoke(this);
    }

    public void SetPrimaryValue(int value) {
        if (value == 0) return;
        Initialize();

        primaryValue = value;

        SetTooltipDirty();
        OnStatChanged?.Invoke(this);
    }

    public void SetMinMax(int min, int max) {
        Initialize();

        minStatValue = min;
        maxStatValue = max;

        SetTooltipDirty();
        OnStatChanged?.Invoke(this);
    }

    public int GetPrimaryValue() {
        return primaryValue;
    }

    public void SetTooltipDirty() {
        isDirty = true;
        setTooltipDirtyMethod?.Invoke();
    }

    public void Initialize() {
        if (initialized) return;
        initialized = true;

        absoluteModifiers ??= new List<int>();
        relativeModifiers ??= new List<float>();

        isDirty = true;
        _ = GetValue();
    }
}

