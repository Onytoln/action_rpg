using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Loot Holder", menuName = "Loot/Default Loot Holder")]
public class DefaultLootHolder : ScriptableObject {
    [SerializeField] private Scenes scene;
    public Scenes Scene { get => scene; }

    [SerializeField] private ItemLootTable[] currencyLoot;
    public ItemLootTable[] CurrencyLoot { get => currencyLoot; }

    [SerializeField] private ItemLootTable[] regularEquipmentLoot;
    public ItemLootTable[] RegularEquipmentLoot { get => regularEquipmentLoot; }

    [SerializeField] private ItemLootTable[] uniqueEquipmentLoot;
    public ItemLootTable[] UniqueEquipmentLoot { get => uniqueEquipmentLoot; }

    [SerializeField] private ItemLootTable[] consumablesLoot;
    public ItemLootTable[] ConsumablesLoot { get => consumablesLoot; }

    [SerializeField] private ItemLootTable[] otherLoot;
    public ItemLootTable[] OtherLoot { get => otherLoot; }

}