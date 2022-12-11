using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyLootTable : LootTableTemplate {
    [field: SerializeField] public Enemy Enemy { get; private set; }
    [field: SerializeField] public int GuaranteedSpawnCount { get; set; }

    public EnemyLootTable(int dropWeight) : base(dropWeight) {
    }

    public EnemyLootTable(Enemy item, int guaranteedSpawnCount, int dropWeight) : base(dropWeight) {
        Enemy = item;
        GuaranteedSpawnCount = guaranteedSpawnCount;
    }

    public EnemyLootTable(EnemyLootTable enemyLootTable) : base(enemyLootTable) {
        Enemy = enemyLootTable.Enemy;
        GuaranteedSpawnCount = enemyLootTable.GuaranteedSpawnCount;
    }

    public EnemyLootTable(EnemyLootTable enemyLootTable1, EnemyLootTable enemyLootTable2) : base(enemyLootTable1, enemyLootTable2) {
        Enemy = enemyLootTable1.Enemy;
        GuaranteedSpawnCount = enemyLootTable1.GuaranteedSpawnCount;
    }
}
