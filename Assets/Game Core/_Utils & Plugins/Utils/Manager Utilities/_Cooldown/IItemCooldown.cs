using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemCooldown : ICooldown { 

    public ItemCooldownCategory ItemCooldownCategory { get; }
}

public enum ItemCooldownCategory { None, HealingPotion, ManaPotion, HealingPotionOverTime, ManaPotionOverTime }