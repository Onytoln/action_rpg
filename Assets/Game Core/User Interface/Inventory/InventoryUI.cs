using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour, IUiWindow {

    [field: SerializeField] public KeyCode OpenWindowKey { get; private set; }

    public Transform itemsParent;

    public GameObject inventoryUI;
    public RectTransform inventoryUiTransform;

    /*
    public Vector2 inventoryHiddenVector;
    public Vector2 inventoryVisibleVector;*/

    public GameObject equipmentUI;

    public CanvasGroup canvasGroup;
    public GraphicRaycaster raycaster;
    /*public RectTransform equipmentUiTransform;
    public Vector2 equipmentHiddenVector;
    public Vector2 equipmentVisibleVector;*/

    Inventory inventory;
    InventorySlot[] slots;
    private bool isEnabled;

    CoroutineHandle inventoryFade;

    private InputManager inputManager;

    void Start() {
        inventory = Inventory.Instance;
        inventory.OnItemChangedCallback += UpdateUISlotAtIndex;
        slots = itemsParent.GetComponentsInChildren<InventorySlot>();

        for (int i = 0; i < slots.Length; i++) {
            slots[i].slotIndex = i;
        }

        isEnabled = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        raycaster.enabled = false;

        inputManager = InputManager.Instance;
        inputManager.OnKeyDown += OpenCloseInventory;
    }

    private void OpenCloseInventory(KeyCode key) {
        if (key != OpenWindowKey) return;

        if (isEnabled) {
            isEnabled = false;
            raycaster.enabled = false;
            inventoryFade = Utils.FadeCanvasGroup(canvasGroup, false, inventoryFade);
        } else {
            isEnabled = true;
            raycaster.enabled = true;
            inventoryFade = Utils.FadeCanvasGroup(canvasGroup, true, inventoryFade);
        }
    }

    private void UpdateUI() {
        for (int i = 0; i < slots.Length; i++) {
            if (i < inventory.Items.Length) {
                if (inventory.Items[i] == null) {
                    slots[i].ClearSlot();
                } else {
                    slots[i].AddItem(inventory.Items[i]);
                }
            } else {
                slots[i].ClearSlot();
            }
        }
    }

    private void UpdateUISlotAtIndex(int index) {
        if (index < 0) return;
        slots[index].AddItem(inventory.Items[index]);
    }

}
