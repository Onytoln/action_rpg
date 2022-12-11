using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerBarElementHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public event Action<bool> OnPointerEnterEvent;

    public void OnPointerEnter(PointerEventData eventData) {
        OnPointerEnterEvent?.Invoke(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        OnPointerEnterEvent?.Invoke(false);
    }
}
