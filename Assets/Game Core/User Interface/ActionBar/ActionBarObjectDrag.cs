using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActionBarObjectDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
    [SerializeField]
    private ActionSlot parentActionSlot;
    [SerializeField]
    private RectTransform rect;

    public Canvas parentCanvas;

    private Vector2 startingPos;

    public void OnBeginDrag(PointerEventData eventData) {
        AdvancedTooltip.Instance.HideTooltip();
        startingPos = rect.anchoredPosition;
        rect.SetParent(parentCanvas.transform, true);
        UItrigger.Instance.EnterUI();
    }

    public void OnDrag(PointerEventData eventData) {
        rect.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;

        var results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        bool foundActionSlot = false;
        if (results.Count > 0) {
            foreach (RaycastResult result in results) {
                ActionSlot aSlot = result.gameObject.GetComponent<ActionSlot>();
                
                if (aSlot != null) {
                    foundActionSlot = true;
                } 
            }
        }

        Image img;
        img = parentActionSlot.SlottedImage;
        if (foundActionSlot) {
            var temp = img.color;
            temp.a = 1f;
            img.color = temp;
        } else {
            var temp = img.color;
            temp.a = 0.5f;
            img.color = temp;
        }
        UItrigger.Instance.EnterUI();
    }

    public void OnEndDrag(PointerEventData eventData) {
        var results = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0) {
            foreach (RaycastResult result in results) {
                ActionSlot aSlot = result.gameObject.GetComponent<ActionSlot>();
                if (aSlot != null) {
                    IActionBarSlottable swap = aSlot.SlottedObject;
                    IActionBarSlottable newSlottable = parentActionSlot.SlottedObject;

                    Sprite swapIcon = aSlot.SlottedImage.sprite;
                    Sprite newIcon = parentActionSlot.SlottedImage.sprite;

                    parentActionSlot.ClearSlot();

                    aSlot.FillSlot(newSlottable, newIcon); 
                    parentActionSlot.FillSlot(swap, swapIcon);    

                    FinalizeDrag();
                    return;
                }
            }
            parentActionSlot.ClearSlot();
            FinalizeDrag();
        }
    }

    private void FinalizeDrag() {
        rect.SetParent(parentActionSlot.transform, true);
        rect.SetAsFirstSibling();
        rect.anchoredPosition = startingPos;

        //set opacity back to 1f if destroyed
        Image img;
        img = parentActionSlot.SlottedImage;
        var temp = img.color;
        temp.a = 1f;
        img.color = temp;

        UItrigger.Instance.ExitUI();
    }
}
