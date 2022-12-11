using UnityEngine;
using UnityEngine.EventSystems;

public class OnClickScript : MonoBehaviour, IPointerClickHandler {
    InventorySlot slot;

    float lastLeftClick = 0f;
    float lastRightClick = 0f;
    float interval = 0.35f;

    void Start() {
        slot = GetComponentInParent<InventorySlot>();
    }

    public void OnPointerClick(PointerEventData eventData) {
        if (slot.Item == null) return;

        if (eventData.button == PointerEventData.InputButton.Left) {

            if ((lastLeftClick + interval) > Time.time) {
                slot.Item.OnItemClickOperation(ItemClickOperation.LeftDoubleClick);
            } else {
                lastLeftClick = Time.time;
                slot.Item.OnItemClickOperation(ItemClickOperation.LeftSingleClick);
            }

        } else if (eventData.button == PointerEventData.InputButton.Right) {

            if ((lastRightClick + interval) > Time.time) {
                slot.Item.OnItemClickOperation(ItemClickOperation.RightDoubleClick);
            } else {
                lastLeftClick = Time.time;
                slot.Item.OnItemClickOperation(ItemClickOperation.RightSingleClick);
            }

        }
    }
}
