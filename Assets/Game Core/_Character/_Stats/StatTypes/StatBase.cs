using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum StatClassType { Float, Int, Character, CharacterScalable, CharacterRange, CharacterOverridablePrimary }

[System.Serializable]
public abstract class StatBase<CallbackReturnType, NumberType> : IOnChange<CallbackReturnType>, ISerializationCallbackReceiver 
    where CallbackReturnType : StatBase<CallbackReturnType, NumberType>
    where NumberType : struct, IComparable, IComparable<NumberType>, IConvertible, IEquatable<NumberType>, IFormattable {

    public event Action<CallbackReturnType> OnChanged;

    [field: SerializeField] public string StatName { get; private set; } //only for naming array elements
    [field: SerializeField, Header("Core values")] public NumberType PrimaryValue { get; protected set; }
    public NumberType DefaultPrimaryValue { get; protected set; }

    [field: SerializeField] public NumberType MinValue { get; protected set; }
    [field: SerializeField] public NumberType MaxValue { get; protected set; }

    [NonSerialized] protected ModifierHandler<NumberType> _absoluteModifiers;
    protected abstract Func<IEnumerable<NumberType>, NumberType> AbsoluteModsSumHandler { get; }
    public NumberType TotalAbsoluteMods => _absoluteModifiers.SumVal;

    [NonSerialized] protected ModifierHandler<float> _relativeModifiers;
    protected abstract Func<IEnumerable<float>, float> RelativeModsSumHandler { get; }
    public float TotalRelativeMods => _relativeModifiers.SumVal;

    [SerializeField, Header("Total value")] private NumberType _value;
    public NumberType Value {
        get {
            if (_isDirty) {
                _isDirty = false;
                CalculateValue();
            }

            return _value;
        }
        protected set => _value = value;
    }

    public abstract StatClassType StatClassType { get; }

    [NonSerialized] private bool _initialized;
    [NonSerialized] private bool _isDirty;

    public StatBase(NumberType primaryValue, NumberType minStatValue, NumberType maxStatValue) {
        PrimaryValue = primaryValue;
        MinValue = minStatValue;
        MaxValue = maxStatValue;
    }

    public StatBase(string statName, NumberType primaryValue, NumberType minStatValue, NumberType maxStatValue) {
        StatName = statName;
        PrimaryValue = primaryValue;
        MinValue = minStatValue;
        MaxValue = maxStatValue;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() {
        TryInitialize();
    }

    protected abstract void CalculateValue();

    [Obsolete("Use Value property.")]
    public NumberType GetValue() => Value;

    [Obsolete("Use PrimaryValue property.")]
    public NumberType GetPrimaryValue() => PrimaryValue;

    [Obsolete("Use MinValue property.")]
    public NumberType GetMinValue() => MinValue;

    [Obsolete("Use MaxValue property.")]
    public NumberType GetMaxValue() => MaxValue;

    protected void TryInitialize() {
        if (_initialized) return;
        _initialized = true;

        Initialize();
    }

    protected virtual void Initialize() {
        DefaultPrimaryValue = PrimaryValue;

        _absoluteModifiers ??= new ModifierHandler<NumberType>(AbsoluteModsSumHandler);
        _relativeModifiers ??= new ModifierHandler<float>(RelativeModsSumHandler);

        _isDirty = true;
        _ = Value;
    }

    protected void StatChanged() {
        _isDirty = true;
        OnChanged?.Invoke((CallbackReturnType)this);
    }

    #region Modifiers operations

    protected void AddModifierInternal(ModifierHandler<int> modifiers, int modifier, int replace) {
        if (modifier == 0) return;

        if (replace != 0) {
            ReplaceModifierInternal(modifiers, modifier, replace);
        } else {
            modifiers.Add(modifier);
        }

        StatChanged();
    }

    protected void AddModifierInternal(ModifierHandler<float> modifiers, float modifier, float replace) {
        if (modifier == 0f) return;

        if (replace != 0f) {
            ReplaceModifierInternal(modifiers, modifier, replace);
        } else {
            modifiers.Add(modifier);
        }

        StatChanged();
    }

    private void ReplaceModifierInternal<ModifierType>(ModifierHandler<ModifierType> modifiers, ModifierType modifier, ModifierType replaceWith) 
        where ModifierType : struct, IComparable, IComparable<ModifierType>, IConvertible, IEquatable<ModifierType>, IFormattable {

        _ = modifiers.Replace(modifier, replaceWith);
    }

    protected void RemoveModifierInternal(ModifierHandler<int> modifiers, int modifier) {
        if (modifier == 0) return;
        _ = modifiers.Remove(modifier);
        StatChanged();
    }

    protected void RemoveModifierInternal(ModifierHandler<float> modifiers, float modifier) {
        if (modifier == 0f) return;
        _ = modifiers.Remove(modifier);
        StatChanged();
    }

    public abstract void SetPrimaryValue(NumberType value);
    public abstract void SetMinMax(NumberType min, NumberType max);

    protected void SetPrimaryValueInternal(NumberType value) {
        PrimaryValue = value;
        StatChanged();
    }

    protected void SetMinMaxInternal(NumberType min, NumberType max) {
        MinValue = min;
        MaxValue = max;
        StatChanged();
    }

    #endregion
}
