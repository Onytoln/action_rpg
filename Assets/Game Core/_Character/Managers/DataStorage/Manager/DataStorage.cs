using System;
using System.Collections.Generic;
using UnityEngine;

public class DataStorage : MonoBehaviour {
    public static DataStorage Instance { get; private set; }

    private void Awake() {
        if (Instance == null) Instance = this;

        InitUtils();
        InitBaseStatDict();
        InitLevelStatMultiplierDict();
    }

    public const float AOBJ_RELEASE_Y_DEFAULT_OFFSET = 1.25f;

    [SerializeField] private ObstacleCarveDisabler obstacleCarveDisabler;
    [SerializeField] private ChargeHandler chargeHandler;

    [SerializeField] private GameObject releasePointObject;
    public static GameObject ReleasePointObject => Instance.releasePointObject;

    [SerializeField]
    private BaseStatsValues[] baseStatsDefaultValues;

    private readonly Dictionary<StatType, MinMax> baseStatsDefaultValuesDict = new Dictionary<StatType, MinMax>();

    [SerializeField]
    private BaseStatMultiplierEnchantment[] baseStatMultiplierEnchantments;
    [SerializeField]
    private BaseStatEnchantment[] baseStatEnchantments;

    [SerializeField]
    private LevelStatMultipliers[] levelStatMultipliers;

    private readonly Dictionary<StatType, float[]> levelStatMultipliersDict = new Dictionary<StatType, float[]>();

    [SerializeField]
    private ItemIconSetup[] itemIconSetup;

    //layer
    
    public static string GroundLayerName { get; private set; } = "Ground";
    public static int GroundLayerIndex { get; private set; } = 6;
    public static LayerMask GroundLayerBitMask { get; private set; } = 1 << GroundLayerIndex;

    public static string InteractableLayerName { get; private set; } = "Interactable";
    public static int InteractableLayerIndex { get; private set; } = 7;
    public static LayerMask InteractableLayerBitMask { get; private set; } = 1 << InteractableLayerIndex;

    public static string TerrainLayerName { get; private set; } = "Terrain";
    public static int TerrainLayerIndex { get; private set; } = 8;
    public static LayerMask TerrainLayerBitMask { get; private set; } = 1 << TerrainLayerIndex;

    public static string PlayerLayerName { get; private set; } = "Player";
    public static int PlayerLayerIndex { get; private set; } = 10;
    public static LayerMask PlayerLayerBitMask { get; private set; } = 1 << PlayerLayerIndex;

    public static string EnemyLayerName { get; private set; } = "Enemy";
    public static int EnemyLayerIndex { get; private set; } = 11;
    public static LayerMask EnemyLayerBitMask { get; private set; } = 1 << EnemyLayerIndex;

    public static string CollideWithEnemyLayerName { get; private set; } = "CollideWithEnemy";
    public static int CollideWithEnemyLayerIndex { get; private set; } = 12;
    public static LayerMask CollideWithEnemyLayerBitMask { get; private set; } = 1 << CollideWithEnemyLayerIndex;

    public static string CollideWithPlayerLayerName { get; private set; } = "CollideWithyPlayer";
    public static int CollideWithPlayerLayerIndex { get; private set; } = 13;
    public static LayerMask CollideWithPlayerLayerBitMask { get; private set; } = 1 << CollideWithPlayerLayerIndex;

    public static string CollideWithAllNPCLayerName { get; private set; } = "CollideWithAllNPC";
    public static int CollideWithAllNPCLayerIndex { get; private set; } = 14;
    public static LayerMask CollideWithAllNPCLayerBitMask { get; private set; } = 1 << CollideWithAllNPCLayerIndex;

    public static LayerMask GroundTerrainLayerBitMask => GroundLayerBitMask | TerrainLayerBitMask;

    //tags
    public static string PlayerAllyTag { get; private set; } = "PlayerAlly";
    public static string EnemyTag { get; private set; } = "Enemy";
    public static string ExcludeFromLayerSet { get; private set; } = "ExcludeFromLayerSet";
    public static string AbilityObjectTag { get; private set; } = "AbilityObject";
    public static string EnemySpawnerTag { get; private set; } = "EnemySpawnerTransform";

    //damage type colors
    public static Color32 PhysicalDamageColor { get; private set; } = new Color32(255, 255, 255, 255);
    public static Color32 FireDamageColor { get; private set; } = new Color32(255, 0, 0, 255);
    public static Color32 IceDamageColor { get; private set; } = new Color32(135, 206, 250, 255);
    public static Color32 LightningDamageColor { get; private set; } = new Color32(255, 248, 0, 255);
    public static Color32 PoisonDamageColor { get; private set; } = new Color32(16, 116, 0, 255);
    public static Color32 MagicalDamageColor { get; private set; } = new Color32(0, 181, 215, 255);
    public static Color32 VoidDamageColor { get; private set; } = new Color32(46, 0, 146, 255);
    public static Color32 DefaultDamageColor { get; private set; } = new Color32(255, 255, 255, 255);
    public static Color32 MultipleDamageTypesColor1 { get; private set; } = new Color32(85, 0, 0, 255);
    public static Color32 MultipleDamageTypesColor2 { get; private set; } = new Color32(142, 142, 142, 255);

    //resistance colors
    public static Color32 FireResistanceColor { get; private set; } = new Color32(255, 0, 0, 255);
    public static Color32 IceResistanceColor { get; private set; } = new Color32(135, 206, 250, 255);
    public static Color32 LightningResistanceColor { get; private set; } = new Color32(255, 248, 0, 255);
    public static Color32 PoisonResistanceColor { get; private set; } = new Color32(16, 116, 0, 255);
    public static Color32 DefaultResistanceColor { get; private set; } = new Color32(255, 255, 255, 255);

    //other combat colors
    public static Color32 DefaultBlockColor { get; private set; } = new Color32(0, 74, 150, 255);
    public static Color32 DefaultStatusColor { get; private set; } = new Color32(0, 0, 255, 255);
    public static Color32 DefaultHealColor { get; private set; } = new Color32(50, 205, 50, 255);
    public static Color32 DefaultManaRegenColor { get; private set; } = new Color32(0, 34, 255, 255);

    //item rarity colors
    public static Color32 ItemCommonRarityColor { get; private set; } = new Color32(255, 255, 255, 255);
    public static Color32 ItemUncommonRarityColor { get; private set; } = new Color32(144, 238, 144, 255);
    public static Color32 ItemRareRarityColor { get; private set; } = new Color32(0, 0, 255, 255);
    public static Color32 ItemLegendaryRarityColor { get; private set; } = new Color32(255, 140, 0, 255);
    public static Color32 ItemUniqueRarityColor { get; private set; } = new Color32(255, 215, 0, 255);
    public static Color32 ItemMythicalRarityColor { get; private set; } = new Color32(139, 0, 0, 255);

    //outline
    public static Color32 DefaultEnemyOutlineColor { get; private set; } = new Color32(255, 0, 0, 180);
    public static int DefaultEnemyOutlineWidth = 5;
    public static Color32 DefaultLootHolderOutlineColor { get; private set; } = new Color32(204, 253, 77, 180);
    public static int DefaultLootHolderOutlineWidth = 3;

    public static int DefaultSkillNameFontSize = 35;
    public static int DefaultSkillDescriptionFontSize = 20;
    public static int DefaultStatusEffectNameFontSize = 20;
    public static int DefaultStatusEffectDescriptionFontSize = 16;

    public const float MIN_STOPPING_DISTANCE = 1.2f;
    public const float SKILL_DISTANCE_OFFSET = 0.2f;

    void Start() {
        
    }

    private void InitUtils() {
        Utils.InitUtils(GroundLayerBitMask, CollideWithAllNPCLayerBitMask, obstacleCarveDisabler, chargeHandler, AOBJ_RELEASE_Y_DEFAULT_OFFSET);
    }

    private void InitBaseStatDict() {
        for (int i = 0; i < baseStatsDefaultValues.Length; i++) { 
            baseStatsDefaultValuesDict.Add(baseStatsDefaultValues[i].statType, new MinMax(baseStatsDefaultValues[i].min, baseStatsDefaultValues[i].max));
        }
    }

    private void InitLevelStatMultiplierDict() {
        for (int i = 0; i < levelStatMultipliers.Length; i++) {
            levelStatMultipliersDict.Add(levelStatMultipliers[i].Stat, levelStatMultipliers[i].Multiplier);
        }
    }

    public float GetLevelStatMultiplier(StatType stat, int currentLevel) {
        if(levelStatMultipliersDict.TryGetValue(stat, out float[] val)) {
            return val[currentLevel - 1];
        }

        return 1;
    }

    public static MinMax GetBaseStatMinMaxValues(StatType statType) { 

        DataStorage dataStorage = Instance;

        if (dataStorage.baseStatsDefaultValuesDict.Count == 0) dataStorage.InitBaseStatDict();

        if (dataStorage.baseStatsDefaultValuesDict.TryGetValue(statType, out MinMax result)) {
            return result;
        }

        return default;
    }

    /// <summary>
    /// Does not return copies.
    /// </summary>
    /// <param name="itemBaseStats"></param>
    /// <returns></returns>
    public static List<Enchantment> GetRollableEnchantments(List<ItemBaseStat> itemBaseStats) {
        if (itemBaseStats == null || itemBaseStats.Count == 0) return null;

        DataStorage dataStorage = Instance;

        List<Enchantment> result = new List<Enchantment>();

        for (int i = 0; i < dataStorage.baseStatEnchantments.Length; i++) {
            result.Add(dataStorage.baseStatEnchantments[i]);
        }

        for (int i = 0; i < itemBaseStats.Count; i++) {
            for (int j = 0; j < dataStorage.baseStatMultiplierEnchantments.Length; j++) {
                if (itemBaseStats[i].GetStatType() == dataStorage.baseStatMultiplierEnchantments[j].GetStatType()) {
                    result.Add(dataStorage.baseStatMultiplierEnchantments[j]);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Does not return copies.
    /// </summary>
    /// <param name="itemBaseStats"></param>
    /// <param name="activeEnchantments"></param>
    /// <returns></returns>
    public static List<Enchantment> GetRollableEnchantments(List<ItemBaseStat> itemBaseStats, List<Enchantment> activeEnchantments) {
        if (itemBaseStats == null || itemBaseStats.Count == 0) return null;

        DataStorage dataStorage = Instance;

        List<Enchantment> result = new List<Enchantment>();

        for (int i = 0; i < dataStorage.baseStatEnchantments.Length; i++) {
            result.Add(dataStorage.baseStatEnchantments[i]);
        }

        for (int i = 0; i < itemBaseStats.Count; i++) {
            for (int j = 0; j < dataStorage.baseStatMultiplierEnchantments.Length; j++) {
                if (itemBaseStats[i].GetStatType() == dataStorage.baseStatMultiplierEnchantments[j].GetStatType()
                    && !activeEnchantments.Exists(x => x.IsResistanceEnchantment()
                    && (x as BaseStatMultiplierEnchantment).GetStatType() == dataStorage.baseStatMultiplierEnchantments[j].GetStatType())) {

                    result.Add(dataStorage.baseStatMultiplierEnchantments[j]);
                }
            }
        }

        return result;
    }

    public static ItemIconSetup GetItemIconsByRarity(ItemRarity ir) {
        ItemIconSetup[] ic = Instance.itemIconSetup;

        for (int i = 0; i < ic.Length; i++) {
            if (ic[i].itemRarity == ir) {
                return ic[i];
            }
        }

        return default;
    }

}
