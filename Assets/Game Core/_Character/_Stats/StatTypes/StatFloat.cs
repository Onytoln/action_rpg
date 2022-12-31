using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StatFloat : StatFloatBase<StatFloat> {
    public StatFloat(float primaryValue, float minStatValue, float maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }

    public StatFloat(string statName, float primaryValue, float minStatValue, float maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class StatFloatBase<CallBackReturnType> : StatBase<CallBackReturnType, float> 
    where CallBackReturnType : StatFloatBase<CallBackReturnType> {

    public override StatClassType StatClassType => StatClassType.Float;

    protected override Func<IEnumerable<float>, float> AbsoluteModsSumHandler => StatDefinitions.floatSumHandler;
    protected override Func<IEnumerable<float>, float> RelativeModsSumHandler => StatDefinitions.floatSumHandler;

    public StatFloatBase(float primaryValue, float minStatValue, float maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }
    public StatFloatBase(string statName, float primaryValue, float minStatValue, float maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }

    protected override void CalculateValue() {
        float temp = PrimaryValue;
        temp += _absoluteModifiers.SumVal;
        temp *= 1f + _relativeModifiers.SumVal;
        Value = Mathf.Clamp(temp, MinValue, MaxValue);
    }

    public void AddAbsoluteModifier(float absoluteModifier, float replace = 0f) {
        AddModifierInternal(_absoluteModifiers, absoluteModifier, replace);
    }

    public void RemoveAbsoluteModifier(float absoluteModifier) {
        RemoveModifierInternal(_absoluteModifiers, absoluteModifier);
    }

    public void AddRelativeModifier(float relativeModifier, float replace = 0f) {
        AddModifierInternal(_relativeModifiers, relativeModifier, replace);
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        RemoveModifierInternal(_relativeModifiers, relativeModifier);
    }

    public override void SetPrimaryValue(float value) {
        SetPrimaryValueInternal(value);
    }

    public override void SetMinMax(float min, float max) {
        SetMinMaxInternal(min, max);
    }
}
