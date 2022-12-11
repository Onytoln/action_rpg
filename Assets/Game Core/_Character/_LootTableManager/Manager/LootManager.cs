using MEC;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;

public sealed class LootManager : MonoBehaviour {

    #region Singleton
    public static LootManager Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        GameSceneManager.LatePostSceneLoadPhase.ExecuteTaskConcurrently(InitializeAsync, null, ExecuteAmount.Always);
    }
    #endregion

    [SerializeField, Header("Collision")] private LayerMask lootCollisionLayer;
    [SerializeField] private float lootCollisionRadius = 0.4f;
    [SerializeField] private float searchRadiusGrowth = 1.2f;

    [SerializeField, Header("Default container")] private ItemPickUp defaultLootContainer;

    [SerializeField, Header("Coroutine parameters")] private int maxDropsPerFrame = 3;
    [SerializeField] private int waitFramesBetweenDrops = 2;

    [SerializeField, Header("Pre-pool")] private GameObject[] lootHoldersToPrePool;

    [SerializeField, Header("Loot data")] private DefaultLootHolder[] scenesLootData;

    [SerializeField] private DefaultLootHolder globalLoot;

    [SerializeField, Header("Loot By Loot Rank Drops")] private LootByLootRank[] lootByLootRank;

    [SerializeField, ContextMenuItem("TestLootDrop", "TestLootDropCall")] private CurrentSceneLoot currentLoot;

    [SerializeField] private LootByLootRankData currentLootByLootRank;

    private Task InitializeAsync() {
        return InitializeLoot();
    }

    private async Task InitializeLoot() {
        //sync
        ObjectPoolManager objPM = ObjectPoolManager.Instance;
        for (int i = 0; i < lootHoldersToPrePool.Length; i++) {
            objPM.PrePoolObjects(lootHoldersToPrePool[i].name, lootHoldersToPrePool[i], 50, 100);
        }

        currentLoot = new CurrentSceneLoot(globalLoot, GetCurrentSceneLoot());

        //async
        await Task.Run(() => {
            List<LootType> excludeTypes = new List<LootType>();

            if (currentLoot.CurrencyLoot == null || currentLoot.CurrencyLoot.Length == 0) excludeTypes.Add(LootType.Currency);
            if (currentLoot.RegularEquipmentLoot == null || currentLoot.RegularEquipmentLoot.Length == 0) excludeTypes.Add(LootType.RegularEquipment);
            if (currentLoot.UniqueEquipmentLoot == null || currentLoot.UniqueEquipmentLoot.Length == 0) excludeTypes.Add(LootType.UniqueEquipment);
            if (currentLoot.ConsumablesLoot == null || currentLoot.ConsumablesLoot.Length == 0) excludeTypes.Add(LootType.Consumables);
            if (currentLoot.OtherLoot == null || currentLoot.OtherLoot.Length == 0) excludeTypes.Add(LootType.Other);

            currentLootByLootRank = new LootByLootRankData(lootByLootRank, excludeTypes);
        }).ConfigureAwait(true);

        EventManager.Instance.OnLootDropperTriggered += DropLoot;
    }

    #region Loot Drop Test

    /*private void Update() {
      if (Input.GetKeyDown(KeyCode.Keypad2)) {
          TestLootDrop(2);
      }
    }*/

    [ContextMenu("Loot Drop Test")]
    void TestLootDropCall() => TestLootDrop(1);

    private void TestLootDrop(int a = 1) {
        List<ItemLootTable> listTemp = new List<ItemLootTable>();

        ItemLootTable[] temp = currentLoot.ConsumablesLoot;
        LootTableWorker.RandomizeLootTypeSingle(ref temp, 20, listTemp);
        temp = currentLoot.RegularEquipmentLoot;
        LootTableWorker.RandomizeLootTypeSingle(ref temp, 10, listTemp);
        temp = currentLoot.CurrencyLoot;
        LootTableWorker.RandomizeLootTypeSingle(ref temp, 10, listTemp);

        List<ParameterizedLoot<ItemLootTable>> loot = new List<ParameterizedLoot<ItemLootTable>>();

        for (int i = 0; i < listTemp.Count; i++) {
            loot.Add(new ParameterizedLoot<ItemLootTable>(listTemp[i]));
        }

        DropLootOverTime(loot, a == 1 ? TargetManager.Player.transform.position : TargetManager.CurrentPlayersTargetPoint);
    }

    #endregion


    public void DropLoot(int lootRank) {
       
    }

    public void DropLoot(ILootDropper lootDropper) {
        if (lootDropper == null || !lootDropper.CanDropLoot) return;

        int lootRank = lootDropper.GetLootCategoryRank();
        if (lootRank < 0 || lootRank > 3) return;

        var currentLootData = currentLootByLootRank.GetLootByLootRank(lootRank);
        var currentLootArray = currentLootData.LootByLootRankLootTable;

        if (Random.value > (lootDropper.ChanceToDropLoot <= 0f ? currentLootData.ChanceToDropLoot : lootDropper.ChanceToDropLoot)) return;

        LootTypesToDrop lootTypesToDrop = MergeWithGuaranteedLootDrops(lootDropper);

        int minItemsToDrop = lootDropper.MinItemsToDrop <= 0 ? currentLootData.MinItemsToDropDefault : lootDropper.MinItemsToDrop;

        for (int i = 0; i < minItemsToDrop; i++) {
            LootTypeToLootTypesToDrop(LootTableWorker.RandomizeLootTypeSingle(ref currentLootArray).LootType, lootTypesToDrop);
            if (!LootTableWorker.IsCumulativeQuickCheck(currentLootData.LootByLootRankLootTable)) {
                currentLootByLootRank.UpdateContainerData(lootRank, currentLootArray);
            }
        }

        LootTableContainer<ItemLootTable> dropList = null;

        if (lootDropper.PersonalLoot != null || lootDropper.PersonalLoot.Length != 0) {
            List<MultipleItemLootTable> droppedItem = LootTableWorker.RandomizeLootTypeMultiple(lootDropper.PersonalLoot);

            if (droppedItem.Count != 0) {
                dropList = new LootTableContainer<ItemLootTable>();

                for (int i = 0; i < droppedItem.Count; i++) {
                    var temp = droppedItem[i].ItemLootTable;
                    dropList.Add(LootTableWorker.RandomizeLootTypeSingle(ref temp));
                    droppedItem[i].ItemLootTable = temp;
                }
            }
        }

        DropLoot(lootTypesToDrop, lootDropper.GetDropPosition(), currentLootArray, dropList, currentLootData);
    }

    public void DropLoot(LootTypesToDrop lootTypesToDrop, Vector3 dropPosition, LootByLootRankLootTable[] lootByLootRankLootTable = null,
        LootTableContainer<ItemLootTable> dropList = null, CoreLootRankDataContainer lootRankData = null) {

        LootTableContainer<ItemLootTable> itemsDropped = dropList ?? new LootTableContainer<ItemLootTable>();

        #region Re-randomize In Case item type has no drop on map

        int count = 0;

        if (lootTypesToDrop.CurrencyItemsToDrop > 0 && currentLoot.CurrencyLoot.NullOrEmpty()) {
            count += lootTypesToDrop.CurrencyItemsToDrop;
            lootTypesToDrop.CurrencyItemsToDrop = 0;
        }

        if (lootTypesToDrop.RegularEquipmentItemsToDrop > 0 && currentLoot.RegularEquipmentLoot.NullOrEmpty()) {
            count += lootTypesToDrop.RegularEquipmentItemsToDrop;
            lootTypesToDrop.RegularEquipmentItemsToDrop = 0;
        }

        if (lootTypesToDrop.UniqueEquipmentItemsToDrop > 0 && currentLoot.UniqueEquipmentLoot.NullOrEmpty()) {
            count += lootTypesToDrop.UniqueEquipmentItemsToDrop;
            lootTypesToDrop.UniqueEquipmentItemsToDrop = 0;
        }

        if (lootTypesToDrop.ConsumableItemsToDrop > 0 && currentLoot.ConsumablesLoot.NullOrEmpty()) {
            count += lootTypesToDrop.ConsumableItemsToDrop;
            lootTypesToDrop.ConsumableItemsToDrop = 0;
        }

        if (lootTypesToDrop.OtherItemsToDrop > 0 && currentLoot.OtherLoot.NullOrEmpty()) { 
            count += lootTypesToDrop.OtherItemsToDrop;
            lootTypesToDrop.OtherItemsToDrop = 0;
        }

        ReRandomize(lootTypesToDrop, lootByLootRankLootTable, count);

        #endregion

        if (lootTypesToDrop.CurrencyItemsToDrop > 0) {
            RandomizeCurrencyLoot(itemsDropped, lootTypesToDrop.CurrencyItemsToDrop, lootRankData);
        }

        if (lootTypesToDrop.RegularEquipmentItemsToDrop > 0) {
            RandomizeRegularEquipmentLoot(itemsDropped, lootTypesToDrop.RegularEquipmentItemsToDrop, lootRankData);
        }

        if (lootTypesToDrop.UniqueEquipmentItemsToDrop > 0) {
            RandomizeUniqueEquipmentLoot(itemsDropped, lootTypesToDrop.UniqueEquipmentItemsToDrop, lootRankData);
        }

        if (lootTypesToDrop.ConsumableItemsToDrop > 0) {
            RandomizeConsumablesLoot(itemsDropped, lootTypesToDrop.ConsumableItemsToDrop, lootRankData);
        }

        if (lootTypesToDrop.OtherItemsToDrop > 0) {
            RandomizeOtherLoot(itemsDropped, lootTypesToDrop.OtherItemsToDrop, lootRankData);
        }

        DropLootOverTime(itemsDropped.ParameterizedLoot, dropPosition);
    }

    public void RandomizeCurrencyLoot(LootTableContainer<ItemLootTable> dropList, int count, CoreLootRankDataContainer lootRankData = null, int lootRank = 0) {
        var temp = currentLoot.CurrencyLoot;
        RandomizeLoot(dropList, ref temp, count);
        currentLoot.CurrencyLoot = temp;
    }

    public void RandomizeRegularEquipmentLoot(LootTableContainer<ItemLootTable> dropList, int count, CoreLootRankDataContainer lootRankData = null, int lootRank = 0) {
        var temp = currentLoot.RegularEquipmentLoot;

        CoreLootRankDataContainer currentLootRankData = lootRankData ?? this.currentLootByLootRank.GetLootByLootRank(lootRank);
        ItemRarityLootTable[] itemRarityLootTable = currentLootRankData.ItemRarityLootTable;

        for (int i = 0; i < count; i++) {
            dropList.Add(LootTableWorker.RandomizeLootTypeSingle(ref temp), new ItemLootParameter(LootTableWorker.RandomizeLootTypeSingle(ref itemRarityLootTable).ItemRarity));
        }

        currentLootRankData.ItemRarityLootTable = itemRarityLootTable;
        currentLoot.RegularEquipmentLoot = temp;
    }

    public void RandomizeUniqueEquipmentLoot(LootTableContainer<ItemLootTable> dropList, int count, CoreLootRankDataContainer lootRankData = null, int lootRank = 0) {
        var temp = currentLoot.UniqueEquipmentLoot;
        RandomizeLoot(dropList, ref temp, count);
        currentLoot.UniqueEquipmentLoot = temp;
    }

    public void RandomizeConsumablesLoot(LootTableContainer<ItemLootTable> dropList, int count, CoreLootRankDataContainer lootRankData = null, int lootRank = 0) {
        var temp = currentLoot.ConsumablesLoot;
        RandomizeLoot(dropList, ref temp, count);
        currentLoot.ConsumablesLoot = temp;
    }

    public void RandomizeOtherLoot(LootTableContainer<ItemLootTable> dropList, int count, CoreLootRankDataContainer lootRankData = null, int lootRank = 0) {
        var temp = currentLoot.OtherLoot;
        RandomizeLoot(dropList, ref temp, count);
        currentLoot.OtherLoot = temp;
    }

    #region Drop Into World

    public void DropLootOverTime(List<ParameterizedLoot<ItemLootTable>> lootList, Vector3 dropPosition) {
        if (lootList == null) return;

        Timing.RunCoroutine(DropLootOverTimeCoroutine(lootList, dropPosition));
    }

    private IEnumerator<float> DropLootOverTimeCoroutine(List<ParameterizedLoot<ItemLootTable>> lootList, Vector3 dropPosition) {
        ObjectPoolManager objectPoolManager = ObjectPoolManager.Instance;

        for (int i = 0; i < lootList.Count; i++) {

            _ = Utils.GetObjectSpawnPos(dropPosition, out Vector3 spawnPos, lootCollisionLayer, lootCollisionRadius, searchRadiusGrowth);

            ItemPickUp lootHolder;
            Item item = lootList[i].LootTable.Item;

            bool hasParams = lootList[i].LootParameters?.Count > 0;

            if (item.LootHolder != null) {
                lootHolder = objectPoolManager.GetPooledObject(item.name, item.LootHolder.gameObject, spawnPos).GetComponent<ItemPickUp>();
            } else {
                lootHolder = objectPoolManager.GetPooledObject(defaultLootContainer.name, defaultLootContainer.gameObject, spawnPos).GetComponent<ItemPickUp>();
            }

            Item itemCopy = item.GetCopy();
            lootHolder.LoadItem(itemCopy);

            if(hasParams) {
                itemCopy.LootTableManagerInit(lootList[i].LootParameters);
            }

            lootHolder.gameObject.SetActive(true);

            if ((i + 1) % maxDropsPerFrame == 0) {
                yield return Timing.WaitUntilDone(Utils.WaitFrames(waitFramesBetweenDrops));
            }
        }

        Utils.ClearObjectSpawnSearchBuffer(dropPosition, lootCollisionLayer);
    }

    #endregion

    #region Helpers

    public DefaultLootHolder GetCurrentSceneLoot() {
        Scenes currentScene = GameSceneManager.Instance.CurrentActiveScene;

        for (int i = 0; i < scenesLootData.Length; i++) {
            if (scenesLootData[i].Scene == currentScene) {
                return scenesLootData[i];
            }
        }

        return null;
    }

    public List<Item> ItemLootTableListToItemList(List<ItemLootTable> itemLootTableList) {
        List<Item> lootList = new List<Item>(capacity: itemLootTableList.Count);

        for (int i = 0; i < itemLootTableList.Count; i++) {
            lootList.Add(itemLootTableList[i].Item);
        }

        return lootList;
    }

    public LootTypesToDrop MergeWithGuaranteedLootDrops(ILootDropper lootDropper) {
        return new LootTypesToDrop() {
            CurrencyItemsToDrop = lootDropper.GuaranteedCurrencyLoot,
            RegularEquipmentItemsToDrop = lootDropper.GuaranteedRegularEquipmentLoot,
            UniqueEquipmentItemsToDrop = lootDropper.GuaranteedUniqueEquipmentLoot,
            ConsumableItemsToDrop = lootDropper.GuaranteedConsumablesLoot,
            OtherItemsToDrop = lootDropper.GuaranteedOtherLoot
        };
    }

    public void LootTypeToLootTypesToDrop(LootType lootType, LootTypesToDrop lootTypesToDrop) {
        switch (lootType) {
            case LootType.Currency:
                lootTypesToDrop.CurrencyItemsToDrop++;
                break;
            case LootType.RegularEquipment:
                lootTypesToDrop.RegularEquipmentItemsToDrop++;
                break;
            case LootType.UniqueEquipment:
                lootTypesToDrop.UniqueEquipmentItemsToDrop++;
                break;
            case LootType.Consumables:
                lootTypesToDrop.ConsumableItemsToDrop++;
                break;
            case LootType.Other:
                lootTypesToDrop.OtherItemsToDrop++;
                break;
        }
    }

    public void DebugLootTypesToDrop(LootTypesToDrop lootTypesToDrop) {
        Debug.Log(
            "Currency count: " + lootTypesToDrop.CurrencyItemsToDrop
            + ", Regular Equipment count: " + lootTypesToDrop.RegularEquipmentItemsToDrop
            + ", Unique Equipment count: " + lootTypesToDrop.UniqueEquipmentItemsToDrop
            + ", Consumables count: " + lootTypesToDrop.ConsumableItemsToDrop
            + ", Other count: " + lootTypesToDrop.OtherItemsToDrop
        );
    }


    public void ReRandomize(LootTypesToDrop lootTypesToDrop, LootByLootRankLootTable[] reRandomizeFrom, int count) {
        if (count <= 0) return;

        if (reRandomizeFrom == null) {
            lootTypesToDrop.CurrencyItemsToDrop += count;
            return;
        }

        for (int i = 0; i < count; i++) {
            LootTypeToLootTypesToDrop(LootTableWorker.RandomizeLootTypeSingle(ref reRandomizeFrom).LootType, lootTypesToDrop);
        }
    }

    public void RandomizeLoot(LootTableContainer<ItemLootTable> assignTo, ref ItemLootTable[] dropFrom, int count) {
        LootTableWorker.RandomizeLootTypeSingle(ref dropFrom, count, assignTo);
    }

    #endregion
}
