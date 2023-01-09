using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelStatMultipliers {
    [field: SerializeField] public CharacterStatType Stat { get; set; }
    [field: SerializeField] public float[] Multiplier { get; set; }
}
