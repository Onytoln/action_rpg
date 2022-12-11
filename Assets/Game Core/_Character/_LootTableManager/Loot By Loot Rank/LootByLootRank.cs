using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Loot By Loot Rank", menuName = "Loot/Loot By Loot Rank")]
public class LootByLootRank : ScriptableObject {
    [field: SerializeField] public int LootRank { get; private set; }

    [field: SerializeField] public int MinItemsToDropDefault { get; private set; } = 1;

    [field: SerializeField, Range(0.1f, 1f)] public float ChanceToDropLoot { get; private set; } = 0f;

    [field: SerializeField, Header("Loot Type Drop Chances")]
    public LootByLootRankLootTable[] LootByLootRankLootTable { get; private set; }

    [field: SerializeField, Header("Equipment Rarity Drop Chances")]
    public ItemRarityLootTable[] ItemRarityLootTable { get; private set; }

    private void OnValidate() {
        if(LootByLootRankLootTable == null || LootByLootRankLootTable.Length == 0) {
            LootByLootRankLootTable = new LootByLootRankLootTable[5];
            LootByLootRankLootTable[0] = new LootByLootRankLootTable(LootType.Currency, 0);
            LootByLootRankLootTable[1] = new LootByLootRankLootTable(LootType.RegularEquipment, 0);
            LootByLootRankLootTable[2] = new LootByLootRankLootTable(LootType.UniqueEquipment, 0);
            LootByLootRankLootTable[3] = new LootByLootRankLootTable(LootType.Consumables, 0);
            LootByLootRankLootTable[4] = new LootByLootRankLootTable(LootType.Other, 0);
        }

        if(ItemRarityLootTable == null || ItemRarityLootTable.Length == 0) {
            ItemRarityLootTable = new ItemRarityLootTable[4];
            ItemRarityLootTable[0] = new ItemRarityLootTable(ItemRarity.Common, 0);
            ItemRarityLootTable[1] = new ItemRarityLootTable(ItemRarity.Uncommon, 0);
            ItemRarityLootTable[2] = new ItemRarityLootTable(ItemRarity.Rare, 0);
            ItemRarityLootTable[3] = new ItemRarityLootTable(ItemRarity.Legendary, 0);
        }
    }
}
