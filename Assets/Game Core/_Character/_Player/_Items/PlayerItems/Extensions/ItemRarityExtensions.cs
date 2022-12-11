using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ItemRarityExtensions {
   public static Color ItemRarityToColor32(this ItemRarity ir) {
        return ir switch {
            ItemRarity.Common => DataStorage.ItemCommonRarityColor,
            ItemRarity.Uncommon => DataStorage.ItemUncommonRarityColor,
            ItemRarity.Rare => DataStorage.ItemRareRarityColor,
            ItemRarity.Legendary => DataStorage.ItemLegendaryRarityColor,
            ItemRarity.Unique => DataStorage.ItemUniqueRarityColor,
            ItemRarity.Mythical => DataStorage.ItemMythicalRarityColor,
            _ => DataStorage.ItemCommonRarityColor
        };
    }

    public static string ItemRarityToColorRGB(this ItemRarity ir) {
        return ir switch {
            ItemRarity.Common => ColorUtility.ToHtmlStringRGB(DataStorage.ItemCommonRarityColor),
            ItemRarity.Uncommon => ColorUtility.ToHtmlStringRGB(DataStorage.ItemUncommonRarityColor),
            ItemRarity.Rare => ColorUtility.ToHtmlStringRGB(DataStorage.ItemRareRarityColor),
            ItemRarity.Legendary => ColorUtility.ToHtmlStringRGB(DataStorage.ItemLegendaryRarityColor),
            ItemRarity.Unique => ColorUtility.ToHtmlStringRGB(DataStorage.ItemUniqueRarityColor),
            ItemRarity.Mythical => ColorUtility.ToHtmlStringRGB(DataStorage.ItemMythicalRarityColor),
            _ => ColorUtility.ToHtmlStringRGB(DataStorage.ItemCommonRarityColor)
        };
    }
}
