using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[System.Serializable]
public class StatInt : StatIntBase<StatInt> {
    public StatInt(int primaryValue, int minStatValue, int maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class StatIntBase<T> : StatBase<T, int> where T : StatIntBase<T> {

    public override StatClassType StatClassType => StatClassType.Int;

    public StatIntBase(int primaryValue, int minStatValue, int maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }

    protected override void CalculateValue() {
        int temp = PrimaryValue;
        temp += absoluteModifiers.Sum();
        temp = (int)(temp * (1f + relativeModifiers.Sum()));
        Value = Mathf.Clamp(temp, MinStatValue, MaxStatValue);
    }

    public void AddAbsoluteModifier(int absoluteModifier, int replace = 0) {
        AddModifierInternal(absoluteModifiers, absoluteModifier, replace);
    }

    public void RemoveAbsoluteModifier(int absoluteModifier) {
        RemoveModifierInternal(absoluteModifiers, absoluteModifier);
    }

    public void AddRelativeModifier(float relativeModifier, float replace = 0f) {
        AddModifierInternal(relativeModifiers, relativeModifier, replace);
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        RemoveModifierInternal(relativeModifiers, relativeModifier);
    }

    public override void SetPrimaryValue(int value) {
        SetPrimaryValueInternal(value);
    }

    public override void SetMinMax(int min, int max) {
        SetMinMaxInternal(min, max);
    }
}

