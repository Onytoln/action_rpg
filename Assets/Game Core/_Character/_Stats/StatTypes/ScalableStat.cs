using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ScalableStat : ScalableStatBase<ScalableStat> {
    public ScalableStat(float defaultScaleValue, float minScaleValue, float maxScaleValue,
        StatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(defaultScaleValue, minScaleValue, maxScaleValue, statType, primaryValue, minStatValue, maxStatValue) { }

    public ScalableStat(float defaultScaleValue, float minScaleValue, float maxScaleValue, string statName,
        StatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(defaultScaleValue, minScaleValue, maxScaleValue, statName, statType, primaryValue, minStatValue, maxStatValue) { }
}

[System.Serializable]
public class ScalableStatBase<CallBackReturnType> : CharacterStatBase<CallBackReturnType> 
    where CallBackReturnType : ScalableStatBase<CallBackReturnType> {

    public override StatClassType StatClassType => StatClassType.CharacterScalable;

    [field: SerializeField, Header("Scalable stat properties")] public float DefaultScaleValue { get; private set; }
    [field: SerializeField] public float MinScaleValue { get; private set; } 
    [field: SerializeField] public float MaxScaleValue { get; private set; }

    public float ScaleValue { get; private set; }

    [NonSerialized] private ModifierHandler<float> _scaleUncapModifiers;
    protected ModifierHandler<float> ScaleUncapModifiers => _scaleUncapModifiers ??= new ModifierHandler<float>(StatDefinitions.floatSumHandler); //lazy loading
    public float UncappedScaleValue => MaxScaleValue + ScaleUncapModifiers.SumVal;

    [SerializeField, Header("Total scaled value")] private float _totalScaledValue;

    public int ValueForMaxScaledValue => (int)(ScaleValue * UncappedScaleValue);

    public override float Value {
        get { 
            _ = base.Value;
            return _totalScaledValue;
        }
    }

    public ScalableStatBase(float defaultScaleValue, float minScaleValue, float maxScaleValue,
        StatType statType, float primaryValue, float minStatValue, float maxStatValue)
        : base(statType, primaryValue, minStatValue, maxStatValue) {

        DefaultScaleValue = defaultScaleValue;
        MinScaleValue = minScaleValue;
        MaxScaleValue = maxScaleValue;
    }

    public ScalableStatBase(float defaultScaleValue, float minScaleValue, float maxScaleValue, string statName,
        StatType statType, float primaryValue, float minStatValue, float maxStatValue)
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
    protected override void CalculateValue() {
        _totalScaledValue = Mathf.Clamp(base.Value / ScaleValue, MinScaleValue, UncappedScaleValue);
    }

    public void SetScaleValue(float scaleValue) {
        ScaleValue = scaleValue;
        StatChanged();
    }

    public void UncapScalableValue(float uncapValue) {
        if (uncapValue == 0) return;

        ScaleUncapModifiers.Add(uncapValue);

        StatChanged();
    }

    public void RemoveScalableUncapValue(float uncapValue) {
        if (uncapValue == 0) return;

        ScaleUncapModifiers.Remove(uncapValue);

        StatChanged();
    }

    [Obsolete("Use ValueForMaxScaledValue property.")]
    public int GetValueNeededForMaxScaledValue() {
        return ValueForMaxScaledValue;
    }
}
