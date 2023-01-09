using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum StatClassType { Float, Int, Character, CharacterScalable, CharacterRange, CharacterOverridablePrimary }

[System.Serializable]
public abstract class StatBase<CallbackReturnType, NumberType> : IStatBase<CallbackReturnType, NumberType>, ISerializationCallbackReceiver 
    where CallbackReturnType : IStatBaseReadonly<NumberType>
    where NumberType : struct, IComparable, IComparable<NumberType>, IConvertible, IEquatable<NumberType>, IFormattable {

    public event Action<CallbackReturnType> OnChanged;

    [field: SerializeField] public string StatName { get; private set; } //only for naming array elements
    [field: SerializeField, Header("Core values")] public NumberType PrimaryValue { get; private set; }
    public NumberType DefaultPrimaryValue { get; private set; }

    [field: SerializeField] public NumberType MinValue { get; private set; }
    [field: SerializeField] public NumberType MaxValue { get; private set; }

    #region Absolute Modifiers

    [NonSerialized] private ModifierHandler<NumberType> _absoluteModifiers;
    protected ModifierHandler<NumberType> AbsoluteModifiers => _absoluteModifiers ??= new ModifierHandler<NumberType>(AbsoluteModsSumHandler); //lazy loading
    protected abstract Func<IEnumerable<NumberType>, NumberType> AbsoluteModsSumHandler { get; }
    public NumberType TotalAbsoluteMods => AbsoluteModifiers.SumVal;

    #endregion

    #region Relative Modifiers

    [NonSerialized] private ModifierHandler<float> _relativeModifiers;
    protected ModifierHandler<float> RelativeModifiers => _relativeModifiers ??= new ModifierHandler<float>(RelativeModsSumHandler); //lazy loading
    protected abstract Func<IEnumerable<float>, float> RelativeModsSumHandler { get; }
    public float TotalRelativeMods => RelativeModifiers.SumVal;

    #endregion

    [SerializeField, Header("Total value")] private NumberType _value;
    public virtual NumberType Value {
        get {
            if (_isDirty) {
                _isDirty = false;
                _value = CalculateValue();
            }

            return _value;
        }
    }

    public virtual NumberType UnmodifiedValue {
        get {
            _ = Value;
            return _value;
        }
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

    #region Subscription to OnChanged

    public void SubscribeToOnChange<T>(Action<T> actionDelegate) where T : Delegate {
        if(actionDelegate.GetType() == typeof(Action<CallbackReturnType>)) {
            OnChanged += (Action<CallbackReturnType>)(object)actionDelegate;
        } else {
            HandleSubscribeError();
        }
    }

    public void UnsubscribeFromOnChange<T>(Action<T> actionDelegate) where T : Delegate {
        if (actionDelegate.GetType() == typeof(Action<CallbackReturnType>)) {
            OnChanged -= (Action<CallbackReturnType>)(object)actionDelegate;
        } else {
            HandleSubscribeError();
        }
    }

    private void HandleSubscribeError([CallerMemberName] string methodName = "") {
        Debug.LogError($"Incorrect delegate type sent to {nameof(SubscribeToOnChange)} method. Expected delegate type of {typeof(Action<CallbackReturnType>)}.");
    }

    #endregion
    protected abstract NumberType CalculateValue();

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
        _isDirty = true;
    }

    protected virtual void StatChanged() {
        _isDirty = true;
        OnChanged?.Invoke((CallbackReturnType)(object)this);
    }

    #region Base modifiers add/remove

    public abstract void AddAbsoluteModifier(NumberType absoluteModifier, NumberType replace);
    public abstract void RemoveAbsoluteModifier(NumberType absoluteModifier);
    public abstract void AddRelativeModifier(float relativeModifier, float replace);
    public abstract void RemoveRelativeModifier(float relativeModifier);
    public abstract void SetPrimaryValue(NumberType value);
    public abstract void SetMinMax(NumberType min, NumberType max);

    #endregion

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
