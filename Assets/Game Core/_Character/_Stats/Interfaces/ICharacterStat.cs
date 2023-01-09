using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterStatReadonly : IStatBaseReadonly<float> {
    public CharacterStatType StatType { get; }
    public string StatDescription { get; }

    public float TotalCollectiveModifiers { get; }

    public float UncappedValue { get; }
}

public interface ICharacterStat : ICharacterStatReadonly, IStatBase<ICharacterStatReadonly, float> {
    public void UncapValue(float uncapValue, float replace = 0f);
    public void RemoveUncapValue(float uncapValue);
    public void AddCollectiveModifier(float collectiveModifier, float replace);
    public void RemoveCollectiveModifier(float collectiveModifier);
}
