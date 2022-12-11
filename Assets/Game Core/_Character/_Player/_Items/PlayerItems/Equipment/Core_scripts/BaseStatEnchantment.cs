using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enchantment", menuName = "Enchantments/Base Stat Enchantment")]
public class BaseStatEnchantment : Enchantment {
    [SerializeField] private StatType statType;

    public StatType GetStatType() => statType;
}
