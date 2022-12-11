using UnityEngine;

[System.Serializable]
public class EquipmentInventorySlot {
    public EquipmentSlot slotType;
    public Equipment slottedItem;
}

public class EquipmentManager : MonoBehaviour {
    #region
    public static EquipmentManager Instance { get; private set; }

    private void Awake() {
        Instance = this;
    }
    #endregion

    public delegate void OnEquipmentChangedDelegate(Equipment newItem, Equipment oldItem);
    public event OnEquipmentChangedDelegate OnEquipmentChanged;

    Inventory inventory;
    public EquipmentInventorySlot[] currentEquipment;

    private Equipment oldItem;
    private int oldItemInventoryIndex = -1;
    private Equipment newItem;

    private void Start() {
        inventory = Inventory.Instance;
        oldItemInventoryIndex = -1;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.U)) {
            UnequipAll();
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            Unequip(newItem);
        }
    }

    public bool Equip(Equipment newItem) {
        if (!newItem.IsIdentified) return false;

        this.newItem = newItem;
        oldItem = null;

        if (newItem == null) return false;

        for (int i = 0; i < currentEquipment.Length; i++) {
            if (currentEquipment[i].slotType != newItem.equipmentSlot) continue;

            if (currentEquipment[i].slottedItem != null) {
                newItem.OnItemRemoval += AddOldItemToInventory;
                oldItem = currentEquipment[i].slottedItem;
                oldItemInventoryIndex = inventory.GetItemInventoryIndex(newItem);
            }

            if (newItem.equipmentSlot == EquipmentSlot.Ring) {
                HandleRings();
            } else {
                currentEquipment[i].slottedItem = newItem;
            }

            OnEquipmentChanged?.Invoke(newItem, oldItem);

            break;
        }

        return true;
    }

    public bool Equip(Equipment newItem, int index) {
        bool equipped = false;
        if (newItem == null) return equipped;

        if (!newItem.IsIdentified) return equipped;

        if (index < 0 || index > currentEquipment.Length - 1) return equipped;

        EquipmentInventorySlot slot = currentEquipment[index];

        if (newItem.equipmentSlot == slot.slotType) {

            this.newItem = newItem;

            if (slot.slottedItem != null) {
                newItem.OnItemRemoval += AddOldItemToInventory;
                oldItem = slot.slottedItem;
                oldItemInventoryIndex = inventory.GetItemInventoryIndex(newItem);
            }

            slot.slottedItem = newItem;

            OnEquipmentChanged?.Invoke(newItem, oldItem);

            equipped = true;
        }

        return equipped;
    }

    public void Swap(int indexFrom, int indexTo) {
        if (indexFrom == indexTo) return;

        if(currentEquipment[indexFrom].slotType == currentEquipment[indexTo].slotType) {
            Equipment from = currentEquipment[indexFrom].slottedItem;
            Equipment to = currentEquipment[indexTo].slottedItem;

            currentEquipment[indexFrom].slottedItem = null;
            currentEquipment[indexTo].slottedItem = null;

            Equip(from, indexTo);
            Equip(to, indexFrom);

            AdvancedTooltip.Instance.ShowTooltip(from.GetTooltip());
            OnEquipmentChanged?.Invoke(null, from);
            OnEquipmentChanged?.Invoke(null, to);
        }
    }

    void HandleRings() {
        int ringSlotFull = 0;
        for (int i = 0; i < 3; i++) {
            if (ringSlotFull < 2) {
                if (currentEquipment[i + 8].slottedItem == null) {
                    currentEquipment[i + 8].slottedItem = newItem;
                    oldItem = null;
                    break;
                } else {
                    ringSlotFull += 1;
                }
            } else {
                oldItem = currentEquipment[8].slottedItem;
                currentEquipment[8].slottedItem = newItem;
                break;
            }
        }
    }


    public void Unequip(Equipment item) {
        if (!inventory.IsFull()) {
            if (item != null) {
                for (int i = 0; i < currentEquipment.Length; i++) {
                    if (currentEquipment[i].slottedItem == item) {
                        if (inventory.Add(item)) {
                            currentEquipment[i].slottedItem = null;
                            newItem = null;
                            oldItem = null;
                            break;
                        }
                    }
                }
                OnEquipmentChanged?.Invoke(null, item);
            }
        }
    }

    /// <summary>
    /// Unequips item into empty inventory slot
    /// </summary>
    /// <param name="equipmentInventoryIndex"></param>
    /// <param name="inventoryIndex"></param>
    public void Unequip(int equipmentInventoryIndex, int inventoryIndex) {

        if (currentEquipment[equipmentInventoryIndex].slottedItem == null) return;

        Equipment equipment = currentEquipment[equipmentInventoryIndex].slottedItem;
        if (Inventory.Instance.Add(currentEquipment[equipmentInventoryIndex].slottedItem, inventoryIndex)) {
            currentEquipment[equipmentInventoryIndex].slottedItem = null;
            OnEquipmentChanged?.Invoke(null, equipment);
        }
    }

    public void UnequipAll() {
        for (int i = 0; i < currentEquipment.Length; i++) {
            if (currentEquipment[i].slottedItem != null) {
                Unequip(currentEquipment[i].slottedItem);
            }
        }
    }

    private void AddOldItemToInventory() {
        if (oldItem != null) {
            if (oldItemInventoryIndex == -1) inventory.Add(oldItem); else inventory.Add(oldItem, oldItemInventoryIndex);

            oldItem.OnItemRemoval -= AddOldItemToInventory;
            oldItem = null;
            oldItemInventoryIndex = -1;
        }

        if (newItem != null) {
            newItem.OnItemRemoval -= AddOldItemToInventory;
            newItem = null;
        }
    }
}
/*
 private void Start() {
    int numSlots = System.Enum.GetNames(typeof(EquipmentSlot)).Length;

    inventory = Inventory.instance;

    currentEquipment = new Equipment[numSlots];
}

private void Update() {
    if (Input.GetKeyDown(KeyCode.U)) {
        UnequipAll();
    }
}

public void Equip(Equipment newItem) {
    int slotIndex = (int)newItem.equipmentSlot;

    this.newItem = newItem;
    oldItem = null;

    if (currentEquipment[slotIndex] != null) {
        oldItem = currentEquipment[slotIndex];
        newItem.onItemRemoval.AddListener(AddOldItemToInventory);
    }

    if (onEquipmentChanged != null) {
        onEquipmentChanged.Invoke(newItem, oldItem);
        Debug.Log(onEquipmentChanged);
    }

    currentEquipment[slotIndex] = newItem;
}

public void Unequip(int slotIndex) {
    if (currentEquipment[slotIndex] != null) {
        Equipment oldItem = currentEquipment[slotIndex];
        inventory.Add(oldItem);

        currentEquipment[slotIndex] = null;

        if (onEquipmentChanged != null) {
            onEquipmentChanged.Invoke(null, oldItem);
            Debug.Log(onEquipmentChanged);
        }
    }
}

public void UnequipAll() {
    for (int i = 0; i < currentEquipment.Length; i++) {
        Unequip(i);
    }
}

public void AddOldItemToInventory() {
    if(oldItem != null) {
        inventory.Add(oldItem);
        newItem.onItemRemoval.RemoveListener(AddOldItemToInventory);
    }
}*/
