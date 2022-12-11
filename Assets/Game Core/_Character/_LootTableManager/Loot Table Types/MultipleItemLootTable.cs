using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MultipleItemLootTable : MultipleLootTableTemplate {

    [field: SerializeField] public ItemLootTable[] ItemLootTable { get; set; }

    public MultipleItemLootTable(int dropChance) : base(dropChance) {
    }

    public MultipleItemLootTable(ItemLootTable[] itemLootTable, int dropChance) : base(dropChance) {
        ItemLootTable = itemLootTable;
    }

    public MultipleItemLootTable(MultipleItemLootTable multipleItemLootTable) : base(multipleItemLootTable) {
        ItemLootTable = multipleItemLootTable.ItemLootTable;
    }
}
