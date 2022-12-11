using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class EquipmentGearSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {

    public int slotIndex;

    public EquipmentSlot slotType;
    public Image icon;
    public Sprite defaultIcon;
    public Button button;

    public Image border;
    public Image background;

    public Equipment Item { get; private set; }
    private EquipmentManager equipmentManager;

    void Start() {
        ClearSlot();
        equipmentManager = EquipmentManager.Instance;
    }

    public void AddItem(Equipment newItem) {
        if (newItem == null) return;

        icon.sprite = newItem.icon;
        button.enabled = true;

        ItemIconSetup ic = DataStorage.GetItemIconsByRarity(newItem.itemRarity);
        
        if(ic.border != null) {
            border.sprite = ic.border;
            border.enabled = true;
        }

        if(ic.background != null) {
            background.sprite = ic.background;
            background.enabled = true;
        }

        Item = newItem;

        Color tmp = icon.color;
        tmp.a = 1f;
        icon.color = tmp;
    }

    public void ClearSlot() {
        icon.sprite = defaultIcon;
        Item = null;

        button.enabled = false;
        border.enabled = false;
        border.sprite = null;
        background.enabled = false;
        background.sprite = null;
        background.enabled = false;

        Color tmp = icon.color;
        tmp.a = 0.5f;
        icon.color = tmp;
    }

    public void Remove() {
        equipmentManager.Unequip(Item);
        AdvancedTooltip.Instance.HideTooltip();
    }

    public void OnDrop(PointerEventData data) {
        if (data.selectedObject == null) return;

        DraggedItem draggedItem = data.selectedObject.GetComponent<DraggedItem>();
        if (draggedItem == null) return;

        Equipment equipment = draggedItem.draggedObject as Equipment;
        if (equipment == null) return;

        if (EquipmentManager.Instance.Equip(equipment, slotIndex)) equipment.RemoveFromInventoryOrDestack();

        if (Item != null) {
            AdvancedTooltip.Instance.ShowTooltip(Item.GetTooltip());
        }
    }

    public void OnPointerEnter(PointerEventData data) {
        if (Item != null) {
            AdvancedTooltip.Instance.ShowTooltip(Item.GetTooltip());
        }
    }

    public void OnPointerExit(PointerEventData data) {
        if (Item != null) {
            AdvancedTooltip.Instance.HideTooltip();
        }
    }
}
