using System;
using System.Collections.Generic;
using UnityEngine;


public class CharacterStat : CharacterStatBase<CharacterStat> {
    public CharacterStat(float primaryValue, float minStatValue, float maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }
    public CharacterStat(string statName, float primaryValue, float minStatValue, float maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }
}


public class CharacterStatBase<CallBackReturnType> : StatFloatBase<CallBackReturnType>, ICoreCharacterStat 
    where CallBackReturnType : CharacterStatBase<CallBackReturnType>  {

    [field: SerializeField, TextArea] public string StatDescription { get; private set; }
    [field: SerializeField] public StatType StatType { get; private set; }

    private ModifierHandler<float> _collectiveModifiers;
    public float TotalCollectiveModifiers => _collectiveModifiers.SumVal;

    private ModifierHandler<float> _uncapModifiers;
    [System.NonSerialized] private bool _capIsDirty = true;

    public CharacterStatBase(float primaryValue, float minStatValue, float maxStatValue) : base(primaryValue, minStatValue, maxStatValue) { }
    public CharacterStatBase(string statName, float primaryValue, float minStatValue, float maxStatValue) : base(statName, primaryValue, minStatValue, maxStatValue) { }

    protected override void Initialize() {
        _collectiveModifiers ??= new ModifierHandler<float>(StatDefinitions.floatSumHandler);

        _uncapModifiers ??= new ModifierHandler<float>(StatDefinitions.floatSumHandler);
        _capIsDirty = true;

        base.Initialize();
    }

    protected override void CalculateValue() {
        totalAbsoluteMods = primaryValue;
        totalRelativeMods = 0f;
        totalTotalMods = 0f;

        absoluteModifiers.ForEach(x => totalAbsoluteMods += x);
        relativeModifiers.ForEach(x => totalRelativeMods += x);
        totalModifiers.ForEach(x => totalTotalMods += x);

        totalValue = totalAbsoluteMods;
        totalValue *= 1f + totalRelativeMods;
        totalValue *= 1f + totalTotalMods;

        if (_capIsDirty) {
            _capIsDirty = false;

            float capMods = 0f;
            maxStatValueModifiers.ForEach(x => capMods += x);

            finalMaxStatValue = maxStatValue + capMods;
        }

        totalValue = Mathf.Clamp(totalValue, minStatValue, finalMaxStatValue);
    }

    public void UncapStatValue(float uncapValue) {
        if (uncapValue == 0) return;

        maxStatValueModifiers.Add(uncapValue);

        _capIsDirty = true;
        SetIsDirty();
    }

    public void CapStatValue(float capValue) {
        if (capValue == 0) return;

        int index = maxStatValueModifiers.FindIndex(x => x == capValue);
        if (index == -1) return;

        maxStatValueModifiers.RemoveAt(index);

        _capIsDirty = true;
        SetIsDirty();
    }

    public virtual void SetIsDirty() {
        isStatDirty = true;
    }

    public void SetPrimaryValue(float primaryValueModifier) {
        this.primaryValue = defaultPrimaryValue * primaryValueModifier;

        SetIsDirty();
    }

    public float GetPrimaryValue() {
        return primaryValue;
    }

    public void AddAbsoluteModifier(float absoluteModifier, float replace) {
        if (absoluteModifier == 0f && replace == 0f) { return; }

        if (replace != 0f) {
            int index = absoluteModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                absoluteModifiers[index] = absoluteModifier;
            }
        } else {
            absoluteModifiers.Add(absoluteModifier);
        }

        SetIsDirty();
    }

    public void RemoveAbsoluteModifier(float absoluteModifier) {
        if (absoluteModifier == 0) { return; }
        absoluteModifiers.Remove(absoluteModifier);

        SetIsDirty();
    }

    public void AddRelativeModifier(float relativeModifier, float replace) {
        if (relativeModifier == 0f && replace == 0f) { return; }

        if (replace != 0f) {
            int index = relativeModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                relativeModifiers[index] = relativeModifier;
            }
        } else {
            relativeModifiers.Add(relativeModifier);
        }

        SetIsDirty();
    }

    public void RemoveRelativeModifier(float relativeModifier) {
        if (relativeModifier == 0f) { return; }
        relativeModifiers.Remove(relativeModifier);

        SetIsDirty();
    }

    public void AddCollectiveModifier(float totalModifier, float replace) {
        if (totalModifier == 0f && replace == 0f) { return; }

        if (replace != 0f) {
            int index = totalModifiers.FindIndex(x => x == replace);
            if (index != -1) {
                totalModifiers[index] = totalModifier;
            }
        } else {
            totalModifiers.Add(totalModifier);
        }

        SetIsDirty();
    }

    public void RemoveCollectiveModifier(float totalModifier) {
        if (totalModifier == 0f) { return; }
        totalModifiers.Remove(totalModifier);

        SetIsDirty();
    }

}
