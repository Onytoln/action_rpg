using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IsResistanceExtension
{
    public static bool IsResistanceEnchantment(this Enchantment enchantment) {
        if (enchantment is BaseStatMultiplierEnchantment bs) {
            switch (bs.GetStatType()) {
                case CharacterStatType.FireResistance:
                    return true;
                case CharacterStatType.IceResistance:
                    return true;
                case CharacterStatType.LightningResistance:
                    return true;
                case CharacterStatType.PoisonResistance:
                    return true;
                default:
                    return false;
            }
        }
        return false;
    }
}
