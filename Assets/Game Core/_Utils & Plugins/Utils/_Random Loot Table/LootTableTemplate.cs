using UnityEngine;

/// <summary>
/// Never enter -999 into DropWeight value
/// </summary>
[System.Serializable]
public class LootTableTemplate {
    [field: SerializeField] public int DropWeight { get; internal set; }

    public LootTableTemplate(int dropWeight) {
        DropWeight = dropWeight;
    }

    public LootTableTemplate(LootTableTemplate lootTableTemplate) {
        DropWeight = lootTableTemplate.DropWeight;
    }

    public LootTableTemplate(LootTableTemplate lootTableTemplate1, LootTableTemplate lootTableTemplate2) {
        DropWeight = lootTableTemplate1.DropWeight + lootTableTemplate2.DropWeight;
    }
}
