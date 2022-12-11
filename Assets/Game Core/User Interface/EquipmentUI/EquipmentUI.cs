using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentUI : MonoBehaviour, IUiWindow {

    [field: SerializeField] public KeyCode OpenWindowKey { get; private set; }

    public Transform gearSlotsParent;
    public EquipmentManager equipmentManager;
    public EquipmentGearSlot[] equipmentGearSlots;

    void Start()
    {
        equipmentManager = EquipmentManager.Instance;
        equipmentGearSlots = gearSlotsParent.GetComponentsInChildren<EquipmentGearSlot>();
        for (int i = 0; i < equipmentGearSlots.Length; i++) {
            equipmentGearSlots[i].slotIndex = i;
        }
        equipmentManager.OnEquipmentChanged += UpdateGearUI;
    }

    void UpdateGearUI(Equipment newItem, Equipment oldItem) {
        for (int i = 0; i < equipmentManager.currentEquipment.Length; i++) {
            if(equipmentManager.currentEquipment[i].slottedItem != null) {
                equipmentGearSlots[i].AddItem(equipmentManager.currentEquipment[i].slottedItem);
            } else {
                equipmentGearSlots[i].ClearSlot();
            }
        }
    }
}
