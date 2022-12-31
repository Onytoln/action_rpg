using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class StatInt : StatIntBase<StatInt> {
    public StatInt(int primaryValue, int minStatValue, int maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }

    public StatInt(string statName, int primaryValue, int minStatValue, int maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class StatIntBase<CallbackReturnType> : StatBase<CallbackReturnType, int> 
    where CallbackReturnType : StatIntBase<CallbackReturnType> {

    public override StatClassType StatClassType => StatClassType.Int;

    protected override Func<IEnumerable<int>, int> AbsoluteModsSumHandler => StatDefinitions.intSumHandler;
    protected override Func<IEnumerable<float>, float> RelativeModsSumHandler => StatDefinitions.floatSumHandler;

    public StatIntBase(int primaryValue, int minStatValue, int maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }
    public StatIntBase(string statName, int primaryValue, int minStatValue, int maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }

    protected override void CalculateValue() {
        int temp = PrimaryValue;
        temp += _absoluteModifiers.SumVal;
        temp = (int)(temp * (1f + _relativeModifiers.SumVal));
        Value = Mathf.Clamp(temp, MinValue, MaxValue);
    }

    public void AddAbsoluteModifier(int absoluteModifier, int replace = 0) {
        AddModifierInternal(_absoluteModifiers, absoluteModifier, replace);
    }

    public void RemoveAbsoluteModifier(int absoluteModifier) {
        RemoveModifierInternal(_absoluteModifiers, absoluteModifier);
    }

    public void AddRelativeModifier(float relativeModifier, float replace = 0f) {
        AddModifierInternal(_relativeModifiers, relativeModifier, replace);
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        RemoveModifierInternal(_relativeModifiers, relativeModifier);
    }

    public override void SetPrimaryValue(int value) {
        SetPrimaryValueInternal(value);
    }

    public override void SetMinMax(int min, int max) {
        SetMinMaxInternal(min, max);
    }
}

