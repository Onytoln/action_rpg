using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Stat", menuName = "NPC/Stat")]
public class ChracterStat : ICoreCharacterStat {
    public string statName;

    [TextArea] public string statDescription;
    public StatType statType;
  
    [SerializeField] protected float primaryValue;
    [SerializeField] protected float minStatValue;
    [SerializeField] protected float maxStatValue;
    private List<float> maxStatValueModifiers = new List<float>();
    [System.NonSerialized] private float finalMaxStatValue;
    [System.NonSerialized] private bool capIsDirty = true;

    private List<float> absoluteModifiers = new List<float>();
    [System.NonSerialized] private float totalAbsoluteMods;
    public float TotalAbsoluteMods { get => totalAbsoluteMods; }

    private List<float> relativeModifiers = new List<float>();
    [System.NonSerialized] private float totalRelativeMods;
    public float TotalRelativeMods { get => totalRelativeMods; }

    private List<float> totalModifiers = new List<float>();
    [System.NonSerialized] private float totalTotalMods;
    public float TotalTotalMods { get => totalTotalMods; }

    [SerializeField] private float totalValue;

    public float TotalUnscalableValue {
        get {
            if (isStatDirty) {
                isStatDirty = false;
                CalculateValue();
            }

            return totalValue;
        }
    }

    [SerializeField, HideInInspector] private bool isStatDirty = true;

    #region ICoreCharacterStat

    public StatType StatType => statType;
    public float Value => GetValue();
    public float MinValue => GetMinPossibleValue();

    #endregion

    public ChracterStat GetCopy() {
        BeforeInstantiate();
        return Instantiate(this);
    }

    public T GetCopy<T>() {
        BeforeInstantiate();
        return (T)(object)Instantiate(this);
    }

    protected virtual void BeforeInstantiate() {
        _ = GetValue();
    }

    public virtual void Initialize() {
        isStatDirty = true;
        capIsDirty = true;
        defaultPrimaryValue = primaryValue;
        _ = GetValue();
    }

    private void CalculateValue() {
        totalAbsoluteMods = primaryValue;
        totalRelativeMods = 0f;
        totalTotalMods = 0f;

        absoluteModifiers.ForEach(x => totalAbsoluteMods += x);
        relativeModifiers.ForEach(x => totalRelativeMods += x);
        totalModifiers.ForEach(x => totalTotalMods += x);

        totalValue = totalAbsoluteMods;
        totalValue *= 1f + totalRelativeMods;
        totalValue *= 1f + totalTotalMods;

        if (capIsDirty) {
            capIsDirty = false;

            float capMods = 0f;
            maxStatValueModifiers.ForEach(x => capMods += x);

            finalMaxStatValue = maxStatValue + capMods;
        }

        totalValue = Mathf.Clamp(totalValue, minStatValue, finalMaxStatValue);
    }

    public virtual float GetValue() {
        if (isStatDirty) {
            isStatDirty = false;
            CalculateValue();
        }
        return totalValue;
    }

    public virtual float GetMinPossibleValue() {
        return minStatValue;
    }

    public virtual float GetMaxPossibleValue() {
        return finalMaxStatValue;
    }

    public void UncapStatValue(float uncapValue) {
        if (uncapValue == 0) return;

        maxStatValueModifiers.Add(uncapValue);

        capIsDirty = true;
        SetIsDirty();
    }

    public void CapStatValue(float capValue) {
        if (capValue == 0) return;

        int index = maxStatValueModifiers.FindIndex(x => x == capValue);
        if (index == -1) return;

        maxStatValueModifiers.RemoveAt(index);

        capIsDirty = true;
        SetIsDirty();
    }

    public virtual void SetScaleValue(int value) { }
    public virtual void UncapScalableStatValue(float uncapValue) { }
    public virtual void CapScalableStatValue(float uncapValue) { }

    public virtual void SetIsDirty() {
        isStatDirty = true;
    }

    public void SetPrimaryValue(float primaryValueModifier) {
        this.primaryValue = defaultPrimaryValue * primaryValueModifier;

        SetIsDirty();
    }

    public float GetPrimaryValue() {
        return primaryValue;
    }

    public void AddAbsoluteModifier(float absoluteModifier, float replace) {
        if (absoluteModifier == 0f && replace == 0f) { return; }

        if (replace != 0f) {
            int index = absoluteModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                absoluteModifiers[index] = absoluteModifier;
            }
        } else {
            absoluteModifiers.Add(absoluteModifier);
        }

        SetIsDirty();
    }

    public void RemoveAbsoluteModifier(float absoluteModifier) {
        if (absoluteModifier == 0) { return; }
        absoluteModifiers.Remove(absoluteModifier);

        SetIsDirty();
    }

    public void AddRelativeModifier(float relativeModifier, float replace) {
        if (relativeModifier == 0f && replace == 0f) { return; }

        if (replace != 0f) {
            int index = relativeModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                relativeModifiers[index] = relativeModifier;
            }
        } else {
            relativeModifiers.Add(relativeModifier);
        }

        SetIsDirty();
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        if (relativeModifier == 0f) { return; }
        relativeModifiers.Remove(relativeModifier);

        SetIsDirty();
    }

    public void AddTotalModifier(float totalModifier, float replace) {
        if (totalModifier == 0f && replace == 0f) { return; }

        if (replace != 0f) {
            int index = totalModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                totalModifiers[index] = totalModifier;
            }
        } else {
            totalModifiers.Add(totalModifier);
        }

        SetIsDirty();
    }

    public void RemoveTotalModifier(float totalModifier) {
        if (totalModifier == 0f) { return; }
        totalModifiers.Remove(totalModifier);

        SetIsDirty();
    }

}
