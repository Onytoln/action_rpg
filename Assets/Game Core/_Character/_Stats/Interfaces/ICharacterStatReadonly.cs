using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterStatReadonly : IStatBaseReadonly<ICharacterStatReadonly ,float> {
    public CharacterStatType StatType { get; }
    public string StatDescription { get; }

    public float TotalCollectiveModifiers { get; }

    public float UncappedValue { get; }
}
