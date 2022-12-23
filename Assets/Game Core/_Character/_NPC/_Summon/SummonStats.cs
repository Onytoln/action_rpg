using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonStats : CharacterStats {

   public ISummon SummonProperties { get; private set; }

   public void SetSummonStats(StatValues stats, ISummon summonProperties) {
        CoreStats = stats;
        base.Awake();
        for (int i = 0; i < summonProperties.SummonStatBoosts.Length; i++) {
            AddRelativeStat(summonProperties.SummonStatBoosts[i].statType, summonProperties.SummonStatBoosts[i].boostValue);
        }
        currentHealth = CoreStats.HealthValue;
        currentMana = CoreStats.ManaValue;
    }
}
