using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enchantment : ScriptableObject {
    public bool IsCopy { get; private set; } = false;
    //absolute result that is added to the stat
    private float absoluteBaseValue;
    
    private float enchantmentPercantageValue;

    [SerializeField]
    private float enchantmentMin;
    [SerializeField]
    private float enchantmentMax;

    public Enchantment GetCopy() {
        Enchantment copy = Instantiate(this);
        copy.IsCopy = true;
        return copy;
    }

    public virtual void SetBaseValue(float value) {
        absoluteBaseValue = value;
    }

    public void SetPercentageValue(float value) {
        enchantmentPercantageValue = value;
    }

    public float GetBaseValue() {
        return absoluteBaseValue;
    }

    public float GetPercentageValue() {
        return enchantmentPercantageValue;
    }

    public float GetMin() {
        return enchantmentMin;
    }

    public float GetMax() {
        return enchantmentMax;
    }
}
