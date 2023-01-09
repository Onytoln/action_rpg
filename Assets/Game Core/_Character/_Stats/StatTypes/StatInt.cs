using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class StatInt : StatIntBase<IStatIntReadonly>, IStatIntReadonly {
    public StatInt(int primaryValue, int minStatValue, int maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }

    public StatInt(string statName, int primaryValue, int minStatValue, int maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class StatIntBase<CallbackReturnType> : StatBase<CallbackReturnType, int> 
    where CallbackReturnType : IStatBaseReadonly<int> {

    public override StatClassType StatClassType => StatClassType.Int;

    protected override Func<IEnumerable<int>, int> AbsoluteModsSumHandler => StatDefinitions.intSumHandler;
    protected override Func<IEnumerable<float>, float> RelativeModsSumHandler => StatDefinitions.floatSumHandler;

    public StatIntBase(int primaryValue, int minStatValue, int maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }
    public StatIntBase(string statName, int primaryValue, int minStatValue, int maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }

    protected override int CalculateValue() {
        int temp = PrimaryValue;
        temp += AbsoluteModifiers.SumVal;
        temp = (int)(temp * (1f + RelativeModifiers.SumVal));
        return Mathf.Clamp(temp, MinValue, MaxValue);
    }

    public override void AddAbsoluteModifier(int absoluteModifier, int replace = 0) {
        AddModifierInternal(AbsoluteModifiers, absoluteModifier, replace);
    }

    public override void RemoveAbsoluteModifier(int absoluteModifier) {
        RemoveModifierInternal(AbsoluteModifiers, absoluteModifier);
    }

    public override void AddRelativeModifier(float relativeModifier, float replace = 0f) {
        AddModifierInternal(RelativeModifiers, relativeModifier, replace);
    }

    public override void RemoveRelativeModifier(float relativeModifier) {
        RemoveModifierInternal(RelativeModifiers, relativeModifier);
    }

    public override void SetPrimaryValue(int value) {
        SetPrimaryValueInternal(value);
    }

    public override void SetMinMax(int min, int max) {
        SetMinMaxInternal(min, max);
    }
}

