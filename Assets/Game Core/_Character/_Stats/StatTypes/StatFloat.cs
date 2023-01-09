using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class StatFloat : StatFloatBase<IStatFloatReadonly>, IStatFloatReadonly {
    public StatFloat(float primaryValue, float minStatValue, float maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }

    public StatFloat(string statName, float primaryValue, float minStatValue, float maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class StatFloatBase<CallbackReturnType> : StatBase<CallbackReturnType, float> 
    where CallbackReturnType : IStatBaseReadonly<float> {

    public override StatClassType StatClassType => StatClassType.Float;

    protected override Func<IEnumerable<float>, float> AbsoluteModsSumHandler => StatDefinitions.floatSumHandler;
    protected override Func<IEnumerable<float>, float> RelativeModsSumHandler => StatDefinitions.floatSumHandler;

    public StatFloatBase(float primaryValue, float minStatValue, float maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }
    public StatFloatBase(string statName, float primaryValue, float minStatValue, float maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }

    protected override float CalculateValue() {
        float temp = PrimaryValue;
        temp += AbsoluteModifiers.SumVal;
        temp *= 1f + RelativeModifiers.SumVal;
        return Mathf.Clamp(temp, MinValue, MaxValue);
    }

    public override void AddAbsoluteModifier(float absoluteModifier, float replace = 0f) {
        AddModifierInternal(AbsoluteModifiers, absoluteModifier, replace);
    }

    public override void RemoveAbsoluteModifier(float absoluteModifier) {
        RemoveModifierInternal(AbsoluteModifiers, absoluteModifier);
    }

    public override void AddRelativeModifier(float relativeModifier, float replace = 0f) {
        AddModifierInternal(RelativeModifiers, relativeModifier, replace);
    }

    public override void RemoveRelativeModifier(float relativeModifier) {
        RemoveModifierInternal(RelativeModifiers, relativeModifier);
    }

    public override void SetPrimaryValue(float value) {
        SetPrimaryValueInternal(value);
    }

    public override void SetMinMax(float min, float max) {
        SetMinMaxInternal(min, max);
    }
}
