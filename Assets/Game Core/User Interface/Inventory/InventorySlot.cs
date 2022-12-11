using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler {

    public int slotIndex;

    public Image icon;
    public Button itemButton;
    public Image border;
    public Image background;
    public GameObject identified;

    //public TMPro.TextMeshProUGUI itemStackSize;
    public Text itemStackSize;

    private Item item;
    public Item Item { get => item; }

    private ICooldown slottedItemCooldown;

    //cooldown
    [SerializeField]
    private GameObject cooldownVisualObj;
    [SerializeField]
    private Image cooldownFill;
    [SerializeField]
    private Text cooldownText;

    private void Awake() {
        itemButton.enabled = false;
        border.enabled = false;
        background.enabled = false;
        identified.SetActive(false);
    }

    public void AddItem(Item newItem) {
        ClearSlot();

        if (newItem == null) return;

        item = newItem;
        if (item.icon != null) {
            icon.sprite = item.icon;
        }
        icon.enabled = true;

        ItemIconSetup ic = DataStorage.GetItemIconsByRarity(item.itemRarity);
        if(ic.border != null) {
            border.sprite = ic.border;
            border.enabled = true;
        }

        if(ic.background != null) {
            background.sprite = ic.background;
            background.enabled = true;
        }

        if (!item.IsIdentified) {
            identified.SetActive(true);
        } else {
            identified.SetActive(false);
        }

        item.onIdentified += ItemIdentified;

        slottedItemCooldown = item;

        slottedItemCooldown.OnCooldownStart += OnCooldownStart;
        slottedItemCooldown.OnCooldownChanged += OnCooldownChanged;
        slottedItemCooldown.OnCooldownEnd += OnCooldownEnd;

        itemButton.enabled = true;
        if (newItem.StackSize < 2 || newItem.stackAmountMax < 2) {
            itemStackSize.gameObject.SetActive(false);
        } else {
            itemStackSize.gameObject.SetActive(true);
            itemStackSize.text = newItem.StackSize.ToString();
        }
    }

    public void ClearSlot() {
        if (item == null) { return; }

        slottedItemCooldown.OnCooldownStart -= OnCooldownStart;
        slottedItemCooldown.OnCooldownChanged -= OnCooldownChanged;
        slottedItemCooldown.OnCooldownEnd -= OnCooldownEnd;

        OnCooldownEnd(null);

        item.onIdentified -= ItemIdentified;

        item = null;
        slottedItemCooldown = null;
        icon.enabled = false;
        icon.sprite = null;
        
        border.enabled = false;
        border.sprite = null;
        background.enabled = false;
        background.sprite = null;
      
        identified.SetActive(false);

        itemButton.enabled = false;
        itemStackSize.gameObject.SetActive(false);
    }

    private void ItemIdentified() {
        if (!item.IsIdentified) {
            identified.SetActive(true);
        } else {
            identified.SetActive(false);
        }

        item.onIdentified -= ItemIdentified;
    }

    public void OnRemoveButton() {
        //item.DropFromInventory();
    }

    /*public void UseItem() {
        if (item != null) {
            item.Use();
            tooltip.HideTooltip();
        }
    }*/

    public void OnPointerEnter(PointerEventData data) {
        if (item != null) {
            AdvancedTooltip.Instance.ShowTooltip(item.GetTooltip());
        }
    }

    public void OnPointerExit(PointerEventData data) {
        if (item != null) {
            AdvancedTooltip.Instance.HideTooltip();
        }
    }

    public void OnDrop(PointerEventData data) {
        if (data.selectedObject == null) return;
        DraggedItem draggedItem = data.selectedObject.GetComponent<DraggedItem>();

        if(draggedItem != null && draggedItem.draggedFrom != slotIndex) {
            Inventory.Instance.Swap(draggedItem.draggedFrom, slotIndex);
        }
    }

    public void OnCooldownStart(ICooldown iCd) {
        cooldownVisualObj.SetActive(true);
        cooldownText.text = slottedItemCooldown.CurrentCooldown.ToString("N1");
    }

    public void OnCooldownChanged(ICooldown iCd) {
        if (!cooldownVisualObj.activeSelf) cooldownVisualObj.SetActive(true);
        cooldownFill.fillAmount = slottedItemCooldown.CurrentCooldown / slottedItemCooldown.CurrentStartingCooldown;
        cooldownText.text = slottedItemCooldown.CurrentCooldown.ToString("N1");
    }
    public void OnCooldownEnd(ICooldown iCd) {
        cooldownVisualObj.SetActive(false);
        cooldownFill.fillAmount = 1;
    }
}
