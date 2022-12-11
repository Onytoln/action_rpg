using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootByLootRankData {

    [SerializeField] private CoreLootRankDataContainer[] _container;
    public CoreLootRankDataContainer[] LootByLootRankCoreDataContainer { get => _container; }

    public LootByLootRankData(LootByLootRank[] lootByLootRank, List<LootType> exludeLootTypes) {
        _container = new CoreLootRankDataContainer[lootByLootRank.Length];

        for (int i = 0; i < lootByLootRank.Length; i++) {
            _container[i] = new CoreLootRankDataContainer(lootByLootRank[i].LootRank, lootByLootRank[i].MinItemsToDropDefault,
                lootByLootRank[i].ChanceToDropLoot, lootByLootRank[i].LootByLootRankLootTable, lootByLootRank[i].ItemRarityLootTable, exludeLootTypes);
        }
    }

    public CoreLootRankDataContainer GetLootByLootRank(int lootRank) {
        for (int i = 0; i < _container.Length; i++) {
            if (_container[i].LootRank == lootRank) return _container[i];
        }

        return null;
    }

    public void UpdateContainerData(int lootRank, LootByLootRankLootTable[] lootByLootRankLootTable) {
        for (int i = 0; i < _container.Length; i++) {
            if (_container[i].LootRank == lootRank) {
                _container[i].LootByLootRankLootTable = lootByLootRankLootTable;
            }
        }
    }
}

[System.Serializable]
public class CoreLootRankDataContainer {
    [field: SerializeField] public int LootRank { get; private set; }

    [field: SerializeField] public int MinItemsToDropDefault { get; private set; }

    [field: SerializeField] public float ChanceToDropLoot { get; private set; }

    [field: SerializeField, Header("Loot Type Drop Chances")]
    public LootByLootRankLootTable[] LootByLootRankLootTable { get; set; }

    [field: SerializeField, Header("Item rarity drop chances")]
    public ItemRarityLootTable[] ItemRarityLootTable { get; set; }

    public CoreLootRankDataContainer(int lootRank, int minItemsToDrop, float chanceToDrop, LootByLootRankLootTable[] lootByLootRankLootTables,
        ItemRarityLootTable[] itemRarityLootTable, List<LootType> exludeLootTypes) {
        LootRank = lootRank;
        MinItemsToDropDefault = minItemsToDrop;
        ChanceToDropLoot = chanceToDrop;

        ItemRarityLootTable = itemRarityLootTable.ToCumulative();

        List<LootByLootRankLootTable> withExcludes = new List<LootByLootRankLootTable>();
        for (int i = 0; i < lootByLootRankLootTables.Length; i++) {
            if(!exludeLootTypes.Exists(x => x == lootByLootRankLootTables[i].LootType)) {
                withExcludes.Add(lootByLootRankLootTables[i]);
            }
        }

        LootByLootRankLootTable = withExcludes.ToArray().ToCumulative();
    }
}
