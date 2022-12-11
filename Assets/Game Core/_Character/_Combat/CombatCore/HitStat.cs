using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HitStat
{
    [SerializeField]
    private float baseValue;
    [SerializeField]
    private float minStatValue;
    [SerializeField]
    private float maxStatValue;
    [SerializeField]
    private List<float> absoluteModifiers = new List<float>();
    [SerializeField]
    private List<float> relativeModifiers = new List<float>();
    [SerializeField]
    private float totalValue;

    private bool isDirty = true;

    public HitStat(float baseVal, float minVal, float maxVal) {
        baseValue = baseVal;
        minStatValue = minVal;
        maxStatValue = maxVal;
        SetDirty();
    }

    public void CalculateValue() {
        totalValue = baseValue;
        float totalRelativeMods = 0f;
        absoluteModifiers.ForEach(x => totalValue += x);
        relativeModifiers.ForEach(x => totalRelativeMods += x);
        totalValue = totalValue * (1f + totalRelativeMods);
        totalValue = Mathf.Clamp(totalValue, minStatValue, maxStatValue);
    }

    public float GetValue() {
        if (isDirty) {
            isDirty = false;
            CalculateValue();
        }
        return totalValue;
    }

    public void AddAbsoluteModifier(float absoluteModifier) {
        if (absoluteModifier != 0)
            absoluteModifiers.Add(absoluteModifier);

        SetDirty();
    }

    public void RemoveAbsoluteModifier(float absoluteModifier) {
        if (absoluteModifier != 0)
            absoluteModifiers.Remove(absoluteModifier);

        SetDirty();
    }

    public void AddRelativeModifier(float relativeModifier) {
        if (relativeModifier != 0)
            relativeModifiers.Add(relativeModifier);

        SetDirty();
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        if (relativeModifier != 0)
            relativeModifiers.Remove(relativeModifier);

        SetDirty();
    }

    public void SetMinVal(float minVal) {
        minStatValue = minVal;
        SetDirty();
    }
    public void SetMaxVal(float maxVal) {
        maxStatValue = maxVal;
        SetDirty();
    }

    public void SetBaseValue(float value) {
        baseValue = value;
        SetDirty();
    }

    public float GetTotalAbsoluteValue() {
        float dummy = baseValue;
        absoluteModifiers.ForEach(x => dummy += x);
        return dummy;
    }

    private void SetDirty() => isDirty = true;
}
