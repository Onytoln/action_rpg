using System;
using UnityEngine;

public class ActionBar : MonoBehaviour {

    #region Singleton

    public static ActionBar Instance { get; private set; }

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }

    #endregion

    public Action<ActionSlot, SlotStateChanged> onActionSlotStateChanged;

    public GameObject actionBarParent { get; private set; }
    public ActionSlot[] actionSlots { get; private set; }

    private Canvas mainCanvas;

    private void Start() {
        LoadHandler.NotifyOnLoad(TargetManager.Instance, (loadable) => Initialize());
    }

    private void Initialize() {
        actionBarParent = Utils.FindChildObjectInTaggedParentByName("MainCanvas", "ActionBar");
        actionSlots = actionBarParent.GetComponentsInChildren<ActionSlot>();

        mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();

        PlayerController playerController = TargetManager.Player.GetComponent<PlayerController>();
        StatusEffectsManager statusEffectsManager = TargetManager.Player.GetComponent<StatusEffectsManager>();

        for (int i = 0; i < actionSlots.Length; i++) {
            actionSlots[i].ActionBarObjectDrag.parentCanvas = mainCanvas;
            actionSlots[i].playerController = playerController;
            actionSlots[i].statusEffectsManager = statusEffectsManager;
        }

        onActionSlotStateChanged += HandleSlotChanges;
        EquipmentManager.Instance.OnEquipmentChanged += HandleEquipmentChanges;
    }

    private void HandleSlotChanges(ActionSlot aSlot, SlotStateChanged slotState) {
        if (slotState != SlotStateChanged.ObjectAdded) return;

        Item item = aSlot.SlottedObject as Item;

        if (item == null) return;

        Action handlerItem = null;
        handlerItem = () => { 
            aSlot.ClearSlot(item);
            item.OnItemRemoval -= handlerItem;
        };
        item.OnItemRemoval += handlerItem;

        Action<ActionSlot, SlotStateChanged> handlerSlot = null;
        handlerSlot = (slot, slotStateChange) => {
            if (slot == aSlot && slotStateChange == SlotStateChanged.ObjectRemoved) {
                item.OnItemRemoval -= handlerItem;
                onActionSlotStateChanged -= handlerSlot;
            }
        };
        onActionSlotStateChanged += handlerSlot;
    }

    private void HandleEquipmentChanges(Equipment newItem, Equipment oldItem) {
        if (oldItem == null) return;
        Equipment comparableEquipment;
        for (int i = 0; i < actionSlots.Length; i++) {
            comparableEquipment = actionSlots[i].SlottedObject as Equipment;
            if (comparableEquipment != null && comparableEquipment == newItem) {
                actionSlots[i].FillSlot(oldItem, oldItem.icon);
                oldItem.ApplyCooldown(0.3f);
            }
        }
    }
}
