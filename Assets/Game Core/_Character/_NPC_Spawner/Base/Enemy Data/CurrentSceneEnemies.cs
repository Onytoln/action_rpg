using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CurrentSceneEnemies {
#if UNITY_EDITOR
    [SerializeField]
#endif
    private EnemyLootTable[] enemyLootTable;
    public EnemyLootTable[] EnemyLootTable { get => enemyLootTable; set => enemyLootTable = value; }
    [field: SerializeField] public int EnemySpawnCountMin { get; private set; }
    [field: SerializeField] public int EnemySpawnCountMax { get; private set; }

    public bool IsValid { get; private set; } = true;

    public CurrentSceneEnemies(DefaultEnemyHolder defaultEnemyHolder) {
        if(defaultEnemyHolder == null) {
            IsValid = false;
            return;
        }

        EnemySpawnCountMin = defaultEnemyHolder.EnemySpawnCountMin;
        EnemySpawnCountMax = defaultEnemyHolder.EnemySpawnCountMax;

        if(EnemySpawnCountMin > EnemySpawnCountMax) {
            int temp = EnemySpawnCountMin;
            EnemySpawnCountMin = EnemySpawnCountMax;
            EnemySpawnCountMax = temp;
        }

        enemyLootTable = defaultEnemyHolder.EnemyLootTable.ToCumulative();
    }
}
