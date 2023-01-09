using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
public static class StatValuesExtensions {
    public static StatValues SetIsCopy(this StatValues statValues, bool isCopy) {
        statValues.IsCopy = isCopy;
        return statValues;
    }
}

[CreateAssetMenu(fileName = "New NPC Stats", menuName = "NPC/Stats")]
public class StatValues : ScriptableObject, ISerializationCallbackReceiver {
    public bool IsCopy { get; set; } = false;

    //!!!!!!!!!!!!!!NEVER RENAME THIS VARIABLE!!!!!!!!!!!!!!! - if it happened, set back to previous name immediately, otherwise set back to previous and restore values
    //from save file of scriptable object, if that's not possible then this is fucked lmao
    [field: SerializeReference] private ICharacterStat[] _stats;
    private Dictionary<CharacterStatType, ICharacterStat> _statsDict;

    private Action<ICharacterStatReadonly> _onStatChangedCallback;

    public StatValues GetCopy() {
        if (IsCopy) {
            return this;
        } else {
            return Instantiate(this).SetIsCopy(true);
        }
    }

    public StatValues SetOnStatChangeCallback(Action<ICharacterStatReadonly> onStatChangedCallback) {
        _onStatChangedCallback = onStatChangedCallback;
        return this;
    }

    public StatValues RaiseStatChangedCallbackForEveryStat() {
        if (_onStatChangedCallback == null)
            throw new Exception($"You must first use {nameof(SetOnStatChangeCallback)} to set callback before calling it.");

        for (int i = 0; i < _stats.Length; i++) {
            OnStatChange(_stats[i]);
        }
        return this;
    }

    public CharStatsValContainer GetCurrentStatsValuesCopy() {
        return new CharStatsValContainer(_stats);
    }

    private void OnDisable() {
        //Debug.Log("SO disabled");
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize() {
        if (_stats == null) return;

        _statsDict = new Dictionary<CharacterStatType, ICharacterStat>();

        for (int i = 0; i < _stats.Length; i++) {
            _statsDict.Add(_stats[i].StatType, _stats[i]);
            _stats[i].OnChanged += OnStatChange;
        }
    }

    private void OnStatChange(ICharacterStatReadonly characterStatReadonly) {
        _onStatChangedCallback?.Invoke(characterStatReadonly);
    }

    #region Add/Remove operations

    public ICharacterStatReadonly GetStat(CharacterStatType statType) {
        Validate(statType);
        return _statsDict[statType];
    }

    public void SetPrimaryValue(CharacterStatType statType, float value) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.SetPrimaryValue(value);
    }

    public void SetScaleValue(ScalableCharacterStatType statType, float value) {
        Validate(statType);

        IScalableStat stat = (IScalableStat)_statsDict[statType.ToCharacterStatType()];
        stat.SetScaleValue(value);
    }

    public void UncapStatValue(CharacterStatType statType, float uncapValue, float replace = 0f) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.UncapValue(uncapValue);
    }

    public void RemoveStatUncap(CharacterStatType statType, float capValue) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.RemoveUncapValue(capValue);
    }

    public void AddStatModifier(CharacterStatType statType, float modifier, StatValueType statValueType, float replace = 0f) {
        Validate(statValueType);

        switch (statValueType) {
            case StatValueType.Absolute:
                AddAbsoluteStatInternal(statType, modifier, replace);
                break;
            case StatValueType.Relative:
                AddRelativeStatInternal(statType, modifier, replace);
                break;
            case StatValueType.Collective:
                AddCollectiveStatInternal(statType, modifier, replace);
                break;
        }
    }

    public void RemoveStatModifier(CharacterStatType statType, float modifier, StatValueType statValueType) {
        Validate(statValueType);

        switch (statValueType) {
            case StatValueType.Absolute:
                RemoveAbsoluteStatInternal(statType, modifier);
                break;
            case StatValueType.Relative:
                RemoveRelativeStatInternal(statType, modifier);
                break;
            case StatValueType.Collective:
                RemoveCollectiveStatInternal(statType, modifier);
                break;
        }
    }


    private void AddAbsoluteStatInternal(CharacterStatType statType, float absoluteModifier, float replace = 0f) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.AddAbsoluteModifier(absoluteModifier, replace);
    }

    private void RemoveAbsoluteStatInternal(CharacterStatType statType, float absoluteModifier) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.RemoveAbsoluteModifier(absoluteModifier);
    }

    private void AddRelativeStatInternal(CharacterStatType statType, float relativeModifier, float replace = 0f) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.AddRelativeModifier(relativeModifier, replace);
    }

    private void RemoveRelativeStatInternal(CharacterStatType statType, float relativeModifier) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.RemoveRelativeModifier(relativeModifier);
    }

    private void AddCollectiveStatInternal(CharacterStatType statType, float collectiveModifier, float replace = 0f) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.AddCollectiveModifier(collectiveModifier, replace);
    }

    private void RemoveCollectiveStatInternal(CharacterStatType statType, float totalModifier) {
        Validate(statType);

        ICharacterStat stat = _statsDict[statType];
        stat.RemoveCollectiveModifier(totalModifier);
    }

    #endregion

    #region Validations and exceptions

    private void Validate(CharacterStatType characterStatType, [CallerMemberName] string methodName = "") {
        if (characterStatType == CharacterStatType.None)
            throw new InvalidOperationException($"Cannot send {nameof(CharacterStatType)} of type: {CharacterStatType.None} into {methodName} method.");
    }

    private void Validate(ScalableCharacterStatType characterStatType, [CallerMemberName] string methodName = "") {
        if (characterStatType == ScalableCharacterStatType.None)
            throw new InvalidOperationException($"Cannot send {nameof(ScalableCharacterStatType)} of type: {ScalableCharacterStatType.None} into {methodName} method.");
    }

    private void Validate(StatValueType statValueType, [CallerMemberName] string methodName = "") {
        throw new Exception($"{nameof(StatValueType)} cannot send value of {statValueType} into {methodName}.");
    }

    #endregion

    #region Helpers

    public float GetPenetrationValueByDamageType(DamageType damageType) => damageType switch {
        DamageType.Physical => PhysicalPenetrationValue,
        DamageType.Fire => FirePenetrationValue,
        DamageType.Ice => IcePenetrationValue,
        DamageType.Lightning => LightningPenetrationValue,
        DamageType.Poison => PoisonPenetrationValue,
        DamageType.Magical => PhysicalPenetrationValue,
        _ => 0f,
    };

    public (float reduction, float min) GetResistanceValueByDamageType(DamageType damageType) => damageType switch {
        DamageType.Physical => (PhysicalResistanceValue, _stats[13].MinValue),
        DamageType.Fire => (FireResistanceValue, _stats[14].MinValue),
        DamageType.Ice => (IceResistanceValue, _stats[15].MinValue),
        DamageType.Lightning => (LightningResistanceValue, _stats[16].MinValue),
        DamageType.Poison => (PoisonResistanceValue, _stats[17].MinValue),
        DamageType.Magical => (PhysicalResistanceValue * 0.5f, _stats[13].MinValue),
        _ => (0f, 0f),
    };

    #endregion

    #region Easy value access

    public float DamageValue => _stats[0].GetValue();
    public float AttackSpeedValue => _stats[1].GetValue();
    public float CriticalStrikeChanceValue => _stats[2].GetValue();
    public float CriticalDamageValue => _stats[3].GetValue();
    public float DebuffStrenghtValue => _stats[4].GetValue();
    public float MovementSpeedValue => _stats[5].GetValue();
    public float ManaValue => _stats[6].GetValue();
    public float ManaRegenerationValue => _stats[7].GetValue();
    public float HealthValue => _stats[8].GetValue();
    public float HealthRegenerationValue => _stats[9].GetValue();
    public float BlockChanceValue => _stats[10].GetValue();
    public float BlockStrenghtValue => _stats[11].GetValue();
    public float EvasionChanceValue => _stats[12].GetValue();
    public float PhysicalResistanceValue => _stats[13].GetValue();
    public float FireResistanceValue => _stats[14].GetValue();
    public float IceResistanceValue => _stats[15].GetValue();
    public float LightningResistanceValue => _stats[16].GetValue();
    public float PoisonResistanceValue => _stats[17].GetValue();
    public float DebuffProtectionValue => _stats[18].GetValue();
    public float PhysicalPenetrationValue => _stats[19].GetValue();
    public float FirePenetrationValue => _stats[20].GetValue();
    public float IcePenetrationValue => _stats[21].GetValue();
    public float LightningPenetrationValue => _stats[22].GetValue();
    public float PoisonPenetrationValue => _stats[23].GetValue();
    public float LifeStealValue => _stats[24].GetValue();
    public float HealingEffectivityValue => _stats[25].GetValue();

    #endregion
}
