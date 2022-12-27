using System;
using System.Collections.Generic;
using UnityEngine;

public enum StatClassType { Float, Int, Character, CharacterScalable, CharacterRange, CharacterOverridablePrimary }

[System.Serializable]
public abstract class StatBase<T, U> : IOnChange<T>, ISerializationCallbackReceiver 
    where T : StatBase<T, U>
    where U : struct, IComparable, IComparable<U>, IConvertible, IEquatable<U>, IFormattable {

    public event Action<T> OnChanged;

    [field: SerializeField, Header("Core values")] public U PrimaryValue { get; protected set; }
    public U DefaultPrimaryValue { get; protected set; }

    [field: SerializeField] public U MinStatValue { get; protected set; }
    [field: SerializeField] public U MaxStatValue { get; protected set; }

    [NonSerialized] protected List<U> absoluteModifiers = new List<U>();
    [NonSerialized] protected List<float> relativeModifiers = new List<float>();

    [SerializeField, Header("Total value")] private U _value;
    public U Value {
        get {
            if (isDirty) {
                isDirty = false;
                CalculateValue();
            }

            return _value;
        }
        protected set => _value = value;
    }

    public abstract StatClassType StatClassType { get; }

    [NonSerialized] private bool initialized = false;
    [NonSerialized] private bool isDirty;

    public StatBase(U primaryValue, U minStatValue, U maxStatValue) {
        PrimaryValue = primaryValue;
        MinStatValue = minStatValue;
        MaxStatValue = maxStatValue;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() {
        TryInitialize();
    }

    protected abstract void CalculateValue();

    protected void TryInitialize() {
        if (initialized) return;
        initialized = true;

        Initialize();
    }

    protected virtual void Initialize() {
        DefaultPrimaryValue = PrimaryValue;

        absoluteModifiers ??= new List<U>();
        relativeModifiers ??= new List<float>();

        isDirty = true;
        _ = Value;
    }

    protected void StatChanged() {
        isDirty = true;
        OnChanged?.Invoke((T)this);
    }

    #region Modifiers operations

    protected void AddModifierInternal(List<int> modifiers, int modifier, int replace) {
        if (modifier == 0) return;

        if (replace != 0) {
            ReplaceModifierInternal(modifiers, modifier, replace);
        } else {
            modifiers.Add(modifier);
        }

        StatChanged();
    }

    protected void AddModifierInternal(List<float> modifiers, float modifier, float replace) {
        if (modifier == 0f) return;

        if (replace != 0f) {
            ReplaceModifierInternal(modifiers, modifier, replace);
        } else {
            modifiers.Add(modifier);
        }

        StatChanged();
    }

    private void ReplaceModifierInternal<ModifierType>(List<ModifierType> modifiers, ModifierType modifier, ModifierType replace) {
        int index = modifiers.FindIndex(x => x.Equals(replace));

        if (index != -1) {
            modifiers[index] = modifier;
        }
    }

    protected void RemoveModifierInternal(List<int> modifiers, int modifier) {
        if (modifier == 0) return;
        modifiers.Remove(modifier);
        StatChanged();
    }

    protected void RemoveModifierInternal(List<float> modifiers, float modifier) {
        if (modifier == 0f) return;
        modifiers.Remove(modifier);
        StatChanged();
    }

    public abstract void SetPrimaryValue(U value);
    public abstract void SetMinMax(U min, U max);

    protected void SetPrimaryValueInternal(U value) {
        PrimaryValue = value;
        StatChanged();
    }

    protected void SetMinMaxInternal(U min, U max) {
        MinStatValue = min;
        MaxStatValue = max;
        StatChanged();
    }

    #endregion
}
