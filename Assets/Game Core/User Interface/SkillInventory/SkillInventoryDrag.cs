using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillInventoryDrag : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private SkillInventorySlot parentSlot;
    [SerializeField] private RectTransform skillDraggableTransform;

    private RectTransform draggedCopy;

    public Canvas parentCanvas;

    public void OnBeginDrag(PointerEventData eventData) {
        AdvancedTooltip.Instance.HideTooltip();

        draggedCopy = Instantiate(skillDraggableTransform, parentCanvas.transform);
        Image img = draggedCopy.GetComponent<Image>();
        img.sprite = parentSlot.SkillIcon.sprite;
        img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        img.rectTransform.sizeDelta = new Vector2(70, 70);
        img.raycastTarget = false;
        draggedCopy.position = skillDraggableTransform.position;
        DraggedObjectReference draggedObjectReference = draggedCopy.gameObject.AddComponent<DraggedObjectReference>();
        draggedObjectReference.draggedObject = parentSlot.assignedSkill;
        draggedObjectReference.draggedObjectImage = parentSlot.SkillIcon;
        eventData.selectedObject = draggedCopy.gameObject;
        UItrigger.Instance.EnterUI();
    }

    public void OnDrag(PointerEventData eventData) {
        if (draggedCopy != null) {
            draggedCopy.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
            UItrigger.Instance.EnterUI();
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        Destroy(draggedCopy.gameObject);
        draggedCopy = null;
        UItrigger.Instance.ExitUI();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        AdvancedTooltip.Instance.ShowTooltip(parentSlot.assignedSkill.GetTooltip());
    }

    public void OnPointerExit(PointerEventData eventData) {
        AdvancedTooltip.Instance.HideTooltip();
    }
}
