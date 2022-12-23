using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatClassType { Base, Float, Int, Character, CharacterScalable}

public class StatBase
{
    public event Action<StatBase> OnStatChanged;

    public virtual StatClassType StatClassType => StatClassType.Base;

    protected void StatChanged() {
        OnStatChanged?.Invoke(this);
    }
}
