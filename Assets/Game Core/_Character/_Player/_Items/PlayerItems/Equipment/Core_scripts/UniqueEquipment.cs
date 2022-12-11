using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unique Equipment", menuName = "Inventory/Unique Equipment")]
public class UniqueEquipment : Equipment
{
   public string uniqueValue;

    public override void Awake() {
        base.Awake();
        itemRarity = ItemRarity.Unique;
    }
}
