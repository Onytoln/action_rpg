using System;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CharacterStat : CharacterStatBase<CharacterStat> {
    public CharacterStat(StatType statType, float primaryValue, float minStatValue, float maxStatValue) 
        : base(statType, primaryValue, minStatValue, maxStatValue) { }

    public CharacterStat(string statName, StatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(statName, statType, primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class CharacterStatBase<CallBackReturnType> : StatFloatBase<CallBackReturnType>, ICoreCharacterStat 
    where CallBackReturnType : CharacterStatBase<CallBackReturnType>  {

    public override StatClassType StatClassType => StatClassType.Character;

    [field: SerializeField, TextArea] public string StatDescription { get; private set; }
    [field: SerializeField] public StatType StatType { get; private set; }

    [NonSerialized] private ModifierHandler<float> _collectiveModifiers;
    protected ModifierHandler<float> CollectiveModifiers => _collectiveModifiers ??= new ModifierHandler<float>(StatDefinitions.floatSumHandler); //lazy loading
    public float TotalCollectiveModifiers => CollectiveModifiers.SumVal;

    [NonSerialized] private ModifierHandler<float> _uncapModifiers;
    protected ModifierHandler<float> UncapModifiers => _uncapModifiers ??= new ModifierHandler<float>(StatDefinitions.floatSumHandler);
    public float UncappedValue => MaxValue + UncapModifiers.SumVal;

    public CharacterStatBase(StatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(primaryValue, minStatValue, maxStatValue) {

        StatType = statType;
    }
    public CharacterStatBase(string statName, StatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(statName, primaryValue, minStatValue, maxStatValue) {

        StatType = statType;
    }

    protected override void CalculateValue() {
        float temp = PrimaryValue;
        temp += AbsoluteModifiers.SumVal;
        temp *= 1f + RelativeModifiers.SumVal;
        temp *= 1f + CollectiveModifiers.SumVal;
        Value = Mathf.Clamp(temp, MinValue, UncappedValue);
    }

    public void UncapValue(float uncapValue) {
        if (uncapValue == 0f) return;

        UncapModifiers.Add(uncapValue);

        StatChanged();
    }

    public void RemoveUncapValue(float uncapValue) {
        if (uncapValue == 0) return;

        UncapModifiers.Remove(uncapValue);

        StatChanged();
    }

    public void AddCollectiveModifier(float collectiveModifier, float replace) {
        AddModifierInternal(CollectiveModifiers, collectiveModifier, replace);
    }

    public void RemoveCollectiveModifier(float collectiveModifier) {
        RemoveModifierInternal(CollectiveModifiers, collectiveModifier);
    }
}
