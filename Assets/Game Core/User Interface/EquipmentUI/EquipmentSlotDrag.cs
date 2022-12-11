using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MEC;
using System.Collections.Generic;

public class EquipmentSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {

    private EquipmentGearSlot parentSlot;

    private CoroutineHandle slotScaleHandle;

    private Canvas parentCanvas;
    private RectTransform draggedCopy;

    void Awake() {
        parentSlot = gameObject.GetComponentInParent<EquipmentGearSlot>();
        parentCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas").GetComponent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (parentSlot.Item == null) return;

        AdvancedTooltip.Instance.HideTooltip();

        slotScaleHandle = Utils.ScaleCanvasRectPreservedAspect(parentSlot.transform, 0.3f, 0.8f, slotScaleHandle);

        draggedCopy = Instantiate(transform.GetChild(0) as RectTransform, parentCanvas.transform);
        Image img = draggedCopy.GetComponent<Image>();
        //img.sprite = parentSlot.icon.sprite;
        img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        img.rectTransform.sizeDelta = new Vector2(62, 62);
        img.raycastTarget = false;
        //Destroy(draggedCopy.GetChild(0).gameObject);
        draggedCopy.position = transform.position;
        eventData.selectedObject = null;

        UItrigger.Instance.EnterUI();
    }

    public void OnDrag(PointerEventData eventData) {
        if (draggedCopy != null) {
            draggedCopy.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
            UItrigger.Instance.EnterUI();
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (draggedCopy == null) return;

        slotScaleHandle = Utils.ScaleCanvasRectPreservedAspect(parentSlot.transform, 0.2f, 1f, slotScaleHandle);

        var results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        if(results.Count > 0) {
            foreach(RaycastResult result in results) {
                InventorySlot slot = result.gameObject.GetComponentInParent<InventorySlot>();
                if (slot != null) {
                    Equipment equipment = slot.Item as Equipment;
                    if(equipment != null && equipment.equipmentSlot == parentSlot.slotType) {
                        if (EquipmentManager.Instance.Equip(equipment, parentSlot.slotIndex)) equipment.RemoveFromInventoryOrDestack();
                        break;
                    } else if(slot.Item == null) {
                        EquipmentManager.Instance.Unequip(parentSlot.slotIndex, slot.slotIndex);
                        break;
                    }
                }

                EquipmentGearSlot equipmentSlot = result.gameObject.GetComponentInParent<EquipmentGearSlot>();
                if(equipmentSlot != null) {
                    EquipmentManager.Instance.Swap(parentSlot.slotIndex, equipmentSlot.slotIndex);
                    break;
                }
            }
        }

        Destroy(draggedCopy.gameObject);
        draggedCopy = null;

        UItrigger.Instance.ExitUI();
    }
}
