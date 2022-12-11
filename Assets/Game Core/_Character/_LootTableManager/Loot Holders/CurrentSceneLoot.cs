using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CurrentSceneLoot
{
    [SerializeField] private ItemLootTable[] currencyLoot;
    public ItemLootTable[] CurrencyLoot { get => currencyLoot; set => currencyLoot = value; }

    [SerializeField] private ItemLootTable[] regularEquipmentLoot;
    public ItemLootTable[] RegularEquipmentLoot { get => regularEquipmentLoot; set => regularEquipmentLoot = value; }

    [SerializeField] private ItemLootTable[] uniqueEquipmentLoot;
    public ItemLootTable[] UniqueEquipmentLoot { get => uniqueEquipmentLoot; set => uniqueEquipmentLoot = value; }

    [SerializeField] private ItemLootTable[] consumablesLoot;
    public ItemLootTable[] ConsumablesLoot { get => consumablesLoot; set => consumablesLoot = value; }

    [SerializeField] private ItemLootTable[] otherLoot;
    public ItemLootTable[] OtherLoot { get => otherLoot; set => otherLoot = value; }


    public CurrentSceneLoot(DefaultLootHolder globalLoot, DefaultLootHolder sceneLoot) {
        currencyLoot = Utils.MergeArrays(globalLoot.CurrencyLoot, sceneLoot != null ? sceneLoot.CurrencyLoot ?? null : null).ToCumulative();
        regularEquipmentLoot = Utils.MergeArrays(globalLoot.RegularEquipmentLoot, sceneLoot != null ? sceneLoot.RegularEquipmentLoot ?? null : null).ToCumulative();
        uniqueEquipmentLoot = Utils.MergeArrays(globalLoot.UniqueEquipmentLoot, sceneLoot != null ? sceneLoot.UniqueEquipmentLoot ?? null : null).ToCumulative();
        consumablesLoot = Utils.MergeArrays(globalLoot.ConsumablesLoot, sceneLoot != null ? sceneLoot.ConsumablesLoot ?? null : null).ToCumulative();
        otherLoot = Utils.MergeArrays(globalLoot.OtherLoot, sceneLoot != null ? sceneLoot.OtherLoot ?? null : null).ToCumulative();
    }
}
