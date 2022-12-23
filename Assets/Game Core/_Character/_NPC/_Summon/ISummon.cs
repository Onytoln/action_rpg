using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISummon {
    public StatInt SummonsCount { get; set; }
    public StatFloat SummonDuration { get; set; }
    public bool PermanentSummon { get; set; }
    public SummonStatBoosts[] SummonStatBoosts { get; set; }
    
}

[System.Serializable]
public class SummonStatBoosts {
    public StatType statType;
    [Tooltip("Boost value is relative"), Range(-1f, 4f)]
    public float boostValue;
}
