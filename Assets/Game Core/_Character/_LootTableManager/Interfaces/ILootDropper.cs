using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILootDropper {
    int MinItemsToDrop { get; }
    int GuaranteedCurrencyLoot { get; }
    int GuaranteedRegularEquipmentLoot { get; }
    int GuaranteedUniqueEquipmentLoot { get; }
    int GuaranteedConsumablesLoot { get; }
    int GuaranteedOtherLoot { get; }

    MultipleItemLootTable[] PersonalLoot { get; }

    float ChanceToDropLoot { get; }
    bool CanDropLoot { get; }

    int GetLootCategoryRank();

    Vector3 GetDropPosition();
}
