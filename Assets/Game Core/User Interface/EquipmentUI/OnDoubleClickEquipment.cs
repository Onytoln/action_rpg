using UnityEngine;
using UnityEngine.EventSystems;

public class OnDoubleClickEquipment : MonoBehaviour, IPointerClickHandler {
    public EquipmentGearSlot slot;

    float lastClick = 0f;
    float interval = 0.4f;

    void Start() {
        slot = GetComponentInParent<EquipmentGearSlot>();
    }

    public void OnPointerClick(PointerEventData clickData) {
        if ((lastClick + interval) > Time.time) {
            slot.Remove();
        } else {
            lastClick = Time.time;
        }
    }
}
