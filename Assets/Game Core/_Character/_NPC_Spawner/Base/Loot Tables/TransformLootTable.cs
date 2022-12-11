using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TransformLootTable : LootTableTemplate {
    [field: SerializeField] public Transform Transform { get; private set; }

    public TransformLootTable(int dropWeight) : base(dropWeight) {
    }

    public TransformLootTable(Transform transform, int dropWeight) : base(dropWeight) {
        Transform = transform;
    }

    public TransformLootTable(TransformLootTable transformLootTable) : base(transformLootTable) {
        Transform = transformLootTable.Transform;
    }

    public TransformLootTable(TransformLootTable transformLootTable1, TransformLootTable transformLootTable2) : base(transformLootTable1, transformLootTable2) {
        Transform = transformLootTable1.Transform;
    }
}
