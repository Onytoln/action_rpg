using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScalableStat : CharacterStat, IScalableStatReadonly {

    public override StatClassType StatClassType => StatClassType.CharacterScalable;

    [field: SerializeField, Header("Scalable stat properties")] public float DefaultScaleValue { get; private set; }
    [field: SerializeField] public float MinScaleValue { get; private set; } 
    [field: SerializeField] public float MaxScaleValue { get; private set; }
    public float ScaleValue { get; private set; }

    #region Scale Uncap Modifiers

    [NonSerialized] private ModifierHandler<float> _scaleUncapModifiers;
    protected ModifierHandler<float> ScaleUncapModifiers => _scaleUncapModifiers ??= new ModifierHandler<float>(StatDefinitions.floatSumHandler); //lazy loading
    public float UncappedScaleValue => MaxScaleValue + ScaleUncapModifiers.SumVal;

    #endregion

    [SerializeField, Header("Total scaled value")] private float _totalScaledValue;

    public int ValueForMaxScaledValue => (int)(ScaleValue * UncappedScaleValue);

    public override float Value {
        get { 
            _ = base.Value;
            return _totalScaledValue;
        }
    }

    public ScalableStat(float defaultScaleValue, float minScaleValue, float maxScaleValue,
        CharacterStatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(statType, primaryValue, minStatValue, maxStatValue) {

        DefaultScaleValue = defaultScaleValue;
        MinScaleValue = minScaleValue;
        MaxScaleValue = maxScaleValue;
    }

    public ScalableStat(float defaultScaleValue, float minScaleValue, float maxScaleValue, string statName,
        CharacterStatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(statName, statType, primaryValue, minStatValue, maxStatValue) {

        DefaultScaleValue = defaultScaleValue;
        MinScaleValue = minScaleValue;
        MaxScaleValue = maxScaleValue;
    }

    protected override void Initialize() {
        SetMinMax(float.NegativeInfinity, float.PositiveInfinity);
        ScaleValue = DefaultScaleValue;
        base.Initialize();
    }
    protected override float CalculateValue() {
        float baseCharStatValue = base.CalculateValue();
        _totalScaledValue = Mathf.Clamp(baseCharStatValue / ScaleValue, MinScaleValue, UncappedScaleValue);
        return baseCharStatValue;
    }

    public void SetScaleValue(float scaleValue) {
        ScaleValue = scaleValue;
        StatChanged();
    }

    public override void UncapValue(float uncapValue, float replace = 0f) {
        AddModifierInternal(ScaleUncapModifiers, uncapValue, replace);
    }

    public override void RemoveUncapValue(float uncapValue) {
        RemoveModifierInternal(ScaleUncapModifiers, uncapValue);
    }

    [Obsolete("Use ValueForMaxScaledValue property.")]
    public int GetValueNeededForMaxScaledValue() {
        return ValueForMaxScaledValue;
    }
}
