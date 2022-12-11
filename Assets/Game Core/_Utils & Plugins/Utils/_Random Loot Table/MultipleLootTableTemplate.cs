using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MultipleLootTableTemplate {

    [field: SerializeField, Range(0f, 1f)] public float DropChance { get; set; }

    public MultipleLootTableTemplate(int dropChance) {
        DropChance = dropChance;
    }

    public MultipleLootTableTemplate(MultipleLootTableTemplate multipleLootTableTemplate) {
        DropChance = multipleLootTableTemplate.DropChance;
    }
}
