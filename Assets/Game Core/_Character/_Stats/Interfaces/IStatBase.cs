using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IStatBaseReadonly<NumberType> {
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

    public void SubscribeToOnChange<T>(Action<T> actionDelegate) where T : Delegate;
    public void UnsubscribeFromOnChange<T>(Action<T> actionDelegate) where T : Delegate;
}

public interface IStatBase<CallbackReturnType, NumberType> : IStatBaseReadonly<NumberType>, IOnChange<CallbackReturnType> {
    public void AddAbsoluteModifier(NumberType absoluteModifier, NumberType replace);
    public void RemoveAbsoluteModifier(NumberType absoluteModifier);
    public void AddRelativeModifier(float relativeModifier, float replace);
    public void RemoveRelativeModifier(float relativeModifier);
    public void SetPrimaryValue(NumberType value);
    public void SetMinMax(NumberType min, NumberType max);
}
