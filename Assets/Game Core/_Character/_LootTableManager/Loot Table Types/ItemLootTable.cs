using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemLootTable : LootTableTemplate {

    [field: SerializeField] public Item Item { get; private set; }

    public ItemLootTable(int dropWeight) : base(dropWeight) { 
    }

    public ItemLootTable(Item item, int dropWeight) : base(dropWeight){
        Item = item;
    }

    public ItemLootTable(ItemLootTable itemLootTable) : base(itemLootTable) {
        Item = itemLootTable.Item;
    }

    public ItemLootTable(ItemLootTable itemLootTable1, ItemLootTable itemLootTable2) : base(itemLootTable1, itemLootTable2) {
        Item = itemLootTable1.Item;
    }
}
