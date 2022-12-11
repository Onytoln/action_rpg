using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IsResistanceExtension
{
    public static bool IsResistanceEnchantment(this Enchantment enchantment) {
        if (enchantment is BaseStatMultiplierEnchantment bs) {
            switch (bs.GetStatType()) {
                case StatType.FireResistance:
                    return true;
                case StatType.IceResistance:
                    return true;
                case StatType.LightningResistance:
                    return true;
                case StatType.PoisonResistance:
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }
}
