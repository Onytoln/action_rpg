using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class EnchantmentExtension 
{
    public static int RarityToEnchantmentCount(this ItemRarity itemRarity) {
        switch (itemRarity) {
            case ItemRarity.Common:
                return 1;
            case ItemRarity.Uncommon:
                return 2;
            case ItemRarity.Rare:
                return 3;
            case ItemRarity.Legendary:
                return 4;
            case ItemRarity.Unique:
                return 2;
            case ItemRarity.Mythical:
                return 3;
            default:
                return 0;
        }
    }
}
