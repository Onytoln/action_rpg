using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enchantment", menuName = "Enchantments/Base Stat Multiplier Enchantment")]
public class BaseStatMultiplierEnchantment : BaseStatEnchantment
{
    private float enchantmentBoostValue;

    public override void SetBaseValue(float val) => enchantmentBoostValue = val;
    public float GetEnchantmentBoostValue() => enchantmentBoostValue;

    public void CalculateAbsoluteBase(float statVal) {
        base.SetBaseValue(statVal * enchantmentBoostValue);
    }
}
