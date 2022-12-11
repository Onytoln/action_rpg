using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Default Enemy Holder", menuName = "Npc Spawner/Default Enemy Holder")]
public class DefaultEnemyHolder : ScriptableObject {
    [field: SerializeField] public Scenes Scene { get; private set; }
    [field: SerializeField] public int EnemySpawnCountMin { get; private set; }
    [field: SerializeField] public int EnemySpawnCountMax { get; private set; }
    [field: SerializeField] public EnemyLootTable[] EnemyLootTable { get; private set; }
}
