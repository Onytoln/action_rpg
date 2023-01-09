using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IStatBaseReadonly<out CallbackReturnType, NumberType> : IOnChange<CallbackReturnType> {
    public StatClassType StatClassType { get; }

    public NumberType PrimaryValue { get; }
    public NumberType DefaultPrimaryValue { get; }

    public NumberType MinValue { get; }
    public NumberType MaxValue { get; }

    public NumberType TotalAbsoluteMods { get; }
    public float TotalRelativeMods { get; }

    public  NumberType Value { get; }
    public  NumberType UnmodifiedValue { get; }

    [Obsolete("Use Value property.")]
    public NumberType GetValue();

    [Obsolete("Use PrimaryValue property.")]
    public NumberType GetPrimaryValue();

    [Obsolete("Use MinValue property.")]
    public NumberType GetMinValue();

    [Obsolete("Use MaxValue property.")]
    public NumberType GetMaxValue();
}
