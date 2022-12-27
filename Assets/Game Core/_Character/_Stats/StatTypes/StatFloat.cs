using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StatFloat : StatFloatBase<StatFloat> {
    public StatFloat(float primaryValue, float minStatValue, float maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class StatFloatBase<T> : StatBase<T, float> where T : StatFloatBase<T> {

    public override StatClassType StatClassType => StatClassType.Float;

    public StatFloatBase(float primaryValue, float minStatValue, float maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }

    protected override void CalculateValue() {
        float temp = PrimaryValue;
        temp += absoluteModifiers.Sum();
        temp *= 1f + relativeModifiers.Sum();
        Value = Mathf.Clamp(temp, MinStatValue, MaxStatValue);
    }

    public void AddAbsoluteModifier(float absoluteModifier, float replace = 0f) {
        AddModifierInternal(absoluteModifiers, absoluteModifier, replace);
    }

    public void RemoveAbsoluteModifier(float absoluteModifier) {
        RemoveModifierInternal(absoluteModifiers, absoluteModifier);
    }

    public void AddRelativeModifier(float relativeModifier, float replace = 0f) {
        AddModifierInternal(relativeModifiers, relativeModifier, replace);
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        RemoveModifierInternal(relativeModifiers, relativeModifier);
    }

    public override void SetPrimaryValue(float value) {
        SetPrimaryValueInternal(value);
    }

    public override void SetMinMax(float min, float max) {
        SetMinMaxInternal(min, max);
    }
}
