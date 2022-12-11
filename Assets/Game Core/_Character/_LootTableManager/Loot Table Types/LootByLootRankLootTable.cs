using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootByLootRankLootTable : LootTableTemplate {

    [field: SerializeField] public LootType LootType { get; private set; }

    public LootByLootRankLootTable(int dropWeight) : base(dropWeight) {
    }

    public LootByLootRankLootTable(LootType lootType, int dropWeight) : base(dropWeight) {
        LootType = lootType;
    }

    public LootByLootRankLootTable(LootByLootRankLootTable lootByLootRankLootTable) : base(lootByLootRankLootTable) {
        LootType = lootByLootRankLootTable.LootType;
    }

    public LootByLootRankLootTable(LootByLootRankLootTable lootByLootRankLootTable1, LootByLootRankLootTable lootByLootRankLootTable2)
        : base(lootByLootRankLootTable1, lootByLootRankLootTable2) {

        LootType = lootByLootRankLootTable1.LootType;
    }
}
