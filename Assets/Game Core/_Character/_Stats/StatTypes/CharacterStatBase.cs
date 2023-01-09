using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterStat : CharacterStatBase<ICharacterStatReadonly>, ICharacterStat, ICharacterStatReadonly {
    public CharacterStat(CharacterStatType statType, float primaryValue, float minStatValue, float maxStatValue) 
        : base(statType, primaryValue, minStatValue, maxStatValue) { }

    public CharacterStat(string statName, CharacterStatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(statName, statType, primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class CharacterStatBase<CallbackReturnType> : StatFloatBase<CallbackReturnType>
    where CallbackReturnType : IStatBaseReadonly<float> {

    public override StatClassType StatClassType => StatClassType.Character;

    [field: SerializeField, TextArea] public string StatDescription { get; private set; }
    [field: SerializeField] public CharacterStatType StatType { get; private set; }

    #region Collective Modifiers

    [NonSerialized] private ModifierHandler<float> _collectiveModifiers;
    protected ModifierHandler<float> CollectiveModifiers => _collectiveModifiers ??= new ModifierHandler<float>(StatDefinitions.floatSumHandler); //lazy loading
    public float TotalCollectiveModifiers => CollectiveModifiers.SumVal;

    #endregion

    #region UncapModifiers

    [NonSerialized] private ModifierHandler<float> _uncapModifiers;
    protected ModifierHandler<float> UncapModifiers => _uncapModifiers ??= new ModifierHandler<float>(StatDefinitions.floatSumHandler);
    public float UncappedValue => MaxValue + UncapModifiers.SumVal;

    #endregion

    public CharacterStatBase(CharacterStatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(primaryValue, minStatValue, maxStatValue) {

        StatType = statType;
    }
    public CharacterStatBase(string statName, CharacterStatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(statName, primaryValue, minStatValue, maxStatValue) {

        StatType = statType;
    }

    protected override float CalculateValue() {
        float temp = PrimaryValue;
        temp += AbsoluteModifiers.SumVal;
        temp *= 1f + RelativeModifiers.SumVal;
        temp *= 1f + CollectiveModifiers.SumVal;
        return Mathf.Clamp(temp, MinValue, UncappedValue);
    }

    public virtual void UncapValue(float uncapValue, float replace = 0f) {
        AddModifierInternal(UncapModifiers, uncapValue, replace);
    }

    public virtual void RemoveUncapValue(float uncapValue) {
        RemoveModifierInternal(UncapModifiers, uncapValue);
    }

    public void AddCollectiveModifier(float collectiveModifier, float replace) {
        AddModifierInternal(CollectiveModifiers, collectiveModifier, replace);
    }

    public void RemoveCollectiveModifier(float collectiveModifier) {
        RemoveModifierInternal(CollectiveModifiers, collectiveModifier);
    }
}
