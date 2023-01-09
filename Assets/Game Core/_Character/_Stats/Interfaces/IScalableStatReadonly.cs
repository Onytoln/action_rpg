using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScalableStatReadonly : ICharacterStatReadonly {
    public float DefaultScaleValue { get; }
    public float MinScaleValue { get; }
    public float MaxScaleValue { get; }
    public float ScaleValue { get; }

    public float UncappedScaleValue { get; }

    public int ValueForMaxScaledValue { get; }

    [Obsolete("Use ValueForMaxScaledValue property.")]
    public int GetValueNeededForMaxScaledValue();
}
