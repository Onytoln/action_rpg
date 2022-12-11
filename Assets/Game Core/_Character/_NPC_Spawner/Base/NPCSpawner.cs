using MEC;
using System.Collections.Generic;
using UnityEngine;

public sealed class NPCSpawner : MonoBehaviour, IProgress {

    #region Singleton

    public static NPCSpawner Instance { get; private set; }

    public bool ReportsProgress => true;
    public float Progress { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;

        GameSceneManager.PostSceneLoadPhase.ExecuteSync(GetMonsterSpawners, this, ExecuteAmount.Always);
        GameSceneManager.PostSceneLoadPhase.ExecuteCoroutineConcurrently(SpawnEnemiesCoroutine, this, ExecuteAmount.Always);
    }

    #endregion

    [SerializeField, Header("Collision")] private LayerMask enemyCollisionLayer;
    [SerializeField] private float enemyCollisionRadius = 0.5f;
    [SerializeField] private float searchGrowthRadius = 1.5f;

    [SerializeField, Header("Monsters spawning")] private int defaultEnemiesSpawnPerFrame = 10;
    [SerializeField] private int defaultWaitFramesForNextSpawn = 1;

    [SerializeField, Header("Scenes data")] private DefaultEnemyHolder[] _sceneEnemyData;

#if UNITY_EDITOR
    [SerializeField, Header("Current enemies data")]
#endif
    private CurrentSceneEnemies _currentSceneEnemies;

    [SerializeField, Header("Monster spawners")] private TransformLootTable[] _monsterSpawners;
    [SerializeField] private TransformLootTable[] _currentMonsterSpawners;

    private void Start() { }

    private void OnValidate() {
        if(_sceneEnemyData != null) {
            for (int i = 0; i < _sceneEnemyData.Length; i++) {
                if (!GameSceneManager.IsValidNonCoreScene(_sceneEnemyData[i].Scene)) {
                    Debug.LogError($"Scene enemy data on position {i} has invalid scene assigned. Current scene assinged: {_sceneEnemyData[i].Scene.ToString()}");
                }
            }
        }
    }

    private void GetMonsterSpawners() {
        GameObject[] monsterSpawnerGameObjects = GameObject.FindGameObjectsWithTag(DataStorage.EnemySpawnerTag);

        _monsterSpawners = new TransformLootTable[monsterSpawnerGameObjects.Length];
        for (int i = 0; i < monsterSpawnerGameObjects.Length; i++) {
            _monsterSpawners[i] = new TransformLootTable(monsterSpawnerGameObjects[i].transform, 200);
        }

        _currentMonsterSpawners = _monsterSpawners.ToCumulative();
    }

    private void SpawnMapBasedEnemies() {
        Timing.RunCoroutine(SpawnEnemiesCoroutine());
    }

    private IEnumerator<float> SpawnEnemiesCoroutine() {
        _currentSceneEnemies = new CurrentSceneEnemies(GetCurrentSceneEnemies());

        if(!_currentSceneEnemies.IsValid) {
            Debug.Log("No enemies spawnable on this map.");
            yield break;
        }

        Progress = 0f;

        List<EnemyLootTable> enemies = new List<EnemyLootTable>();

        int enemiesToSpawn = Random.Range(_currentSceneEnemies.EnemySpawnCountMin, _currentSceneEnemies.EnemySpawnCountMax + 1);
        int totalEnemiesToSpawn = enemiesToSpawn;

        int enemiesToSpawnPreSpawning = enemiesToSpawn;
        AddGuaranteedSpawn(_currentSceneEnemies.EnemyLootTable, enemies, ref enemiesToSpawnPreSpawning);
        RandomizeEnemies(enemies, enemiesToSpawnPreSpawning);
        enemiesToSpawnPreSpawning = 0;

        int averagePerSpawner = Mathf.CeilToInt(enemies.Count / _currentMonsterSpawners.Length);

        while (enemiesToSpawn > 0) {
            int enemiesPerSpawner = Random.Range(averagePerSpawner, averagePerSpawner + averagePerSpawner);
            enemiesToSpawn -= enemiesPerSpawner;

            Vector3 pos = LootTableWorker.RandomizeLootTypeSingle(ref _currentMonsterSpawners, out int index).Transform.position;

            LootTableWorker.DecreaseChance(ref _currentMonsterSpawners, index, 0.5f);

            yield return Timing.WaitUntilDone(Timing.RunCoroutine(SpawnNpcsOverTimeCoroutine(EnemyLootTableListToEnemyOfCount(enemies, enemiesPerSpawner), pos)));

            Progress = (totalEnemiesToSpawn - enemiesToSpawn) / totalEnemiesToSpawn;
        }
    }

    public void SpawnEnemiesOverTimeExternal(List<EnemyLootTable> enemyLootTables, Transform[] positions) {

    }

    public void SpawnEnemiesOverTimeExternal(Enemy[] enemyArr, Transform[] positions) {

    }

    private IEnumerator<float> SpawnNpcsOverTimeCoroutine(Enemy[] enemiesToSpawn, Vector3 pos, int enemiesPerFrame = 0, int waitFramesBetweenSpawns = 0) {
        if (enemiesToSpawn == null || enemiesToSpawn.Length == 0) {
            Debug.LogError("Enemy LootTable is null or empty in SpawnNpcsOverTimeCoroutine");
            yield break;
        }

        enemiesPerFrame = enemiesPerFrame <= 0 ? defaultEnemiesSpawnPerFrame : enemiesPerFrame;
        waitFramesBetweenSpawns = waitFramesBetweenSpawns <= 0 ? defaultWaitFramesForNextSpawn : waitFramesBetweenSpawns;

        for (int i = 0; i < enemiesToSpawn.Length; i++) {

            _ = Utils.GetObjectSpawnPos(pos, out Vector3 spawnPos, enemyCollisionLayer, enemyCollisionRadius, searchGrowthRadius);

            _ = Instantiate(enemiesToSpawn[i], spawnPos, Utils.GetRandomAxisRotation(Axis.Y));

            if ((i + 1) % enemiesPerFrame == 0) {
                yield return Timing.WaitUntilDone(Utils.WaitFrames(waitFramesBetweenSpawns));
            }
        }

        yield return Timing.WaitUntilDone(Utils.WaitFrames(waitFramesBetweenSpawns));
        Utils.ClearObjectSpawnSearchBuffer(pos, enemyCollisionLayer);
    }


    #region Helpers

    public void RandomizeEnemies(List<EnemyLootTable> enemies, int count) {
        var temp = _currentSceneEnemies.EnemyLootTable;
        LootTableWorker.RandomizeLootTypeSingle(ref temp, count, enemies);
        _currentSceneEnemies.EnemyLootTable = temp;
    }

    public DefaultEnemyHolder GetCurrentSceneEnemies() {
        Scenes currentScene = GameSceneManager.Instance.CurrentActiveScene;

        for (int i = 0; i < _sceneEnemyData.Length; i++) {
            if (_sceneEnemyData[i].Scene == currentScene) {
                return _sceneEnemyData[i];
            }
        }

        return null;
    }

    public Enemy[] EnemyLootTableListToEnemyArr(List<EnemyLootTable> enemyLootTable) {
        if (enemyLootTable == null || enemyLootTable.Count == 0) {
            Debug.LogError("Enemy Loot Table array is null in EnemyLootTableToEnemyArr");
        }

        Enemy[] enemyArr = new Enemy[enemyLootTable.Count];

        for (int i = 0; i < enemyLootTable.Count; i++) {
            enemyArr[i] = enemyLootTable[i].Enemy;
        }

        return enemyArr;
    }

    public Enemy[] EnemyLootTableListToEnemyOfCount(List<EnemyLootTable> enemyLootTable, int count) {
        if (enemyLootTable == null || enemyLootTable.Count == 0 || count == 0) {
            Debug.LogError("Enemy Loot Table array is null in EnemyLootTableListToEnemyOfCount, or count is 0.");
        }

        if (count > enemyLootTable.Count) count = enemyLootTable.Count;

        Enemy[] enemyArr = new Enemy[count];

        int index = 0;
        for (int i = enemyLootTable.Count; i-- > 0 && index < enemyArr.Length; index++) {
            enemyArr[index] = enemyLootTable[i].Enemy;
            enemyLootTable.RemoveAt(i);
        }

        return enemyArr;
    }

    public void AddGuaranteedSpawn(EnemyLootTable[] enemyLootTable, List<EnemyLootTable> enemies, ref int enemiesToSpawn) {
        if (enemies == null) return;

        for (int i = 0; i < enemyLootTable.Length; i++) {
            for (int j = 0; j < enemyLootTable[i].GuaranteedSpawnCount; j++) {
                enemies.Add(enemyLootTable[i]);
                enemiesToSpawn--;
            }
        }
    }

    #endregion
}
