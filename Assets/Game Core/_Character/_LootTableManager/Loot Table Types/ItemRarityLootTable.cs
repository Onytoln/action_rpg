using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemRarityLootTable : LootTableTemplate
{
    [field: SerializeField] public ItemRarity ItemRarity { get; private set; }

    public ItemRarityLootTable(int dropWeight) : base(dropWeight) {
    }

    public ItemRarityLootTable(ItemRarity itemRarity, int dropWeight) : base(dropWeight) {
        ItemRarity = itemRarity;
    }

    public ItemRarityLootTable(ItemRarityLootTable itemRarityLootTable) : base(itemRarityLootTable) {
        ItemRarity = itemRarityLootTable.ItemRarity;
    }

    public ItemRarityLootTable(ItemRarityLootTable itemRarityLootTable1, ItemRarityLootTable itemRarityLootTable2) : base(itemRarityLootTable1, itemRarityLootTable2) {
        ItemRarity = itemRarityLootTable1.ItemRarity;
    }
}
