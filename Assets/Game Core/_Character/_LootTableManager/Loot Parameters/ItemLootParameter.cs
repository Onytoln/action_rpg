using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLootParameter : LootParameter {
    public ItemRarity ItemRarity { get; private set; }

    public ItemLootParameter(ItemRarity itemRarity) {
        ItemRarity = itemRarity;   
    }
}


