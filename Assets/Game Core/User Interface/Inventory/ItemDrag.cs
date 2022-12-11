using UnityEngine;
using UnityEngine.EventSystems;
using MEC;
using UnityEngine.UI;

public class ItemDrag : MonoBehaviour , IBeginDragHandler, IEndDragHandler, IDragHandler {

    private InventorySlot parentSlot;

    private CoroutineHandle slotScaleHandle;

    [SerializeField] private Canvas draggedObjectParentCanvas;
    private RectTransform draggedCopy;

    void Awake() {
        parentSlot = gameObject.GetComponentInParent<InventorySlot>();

        if(draggedObjectParentCanvas == null) draggedObjectParentCanvas = GameObject.FindGameObjectWithTag("InventoryCanvas").GetComponent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData) {
        if (parentSlot.Item == null) return;

        AdvancedTooltip.Instance.HideTooltip();

        slotScaleHandle = Utils.ScaleCanvasRectPreservedAspect(parentSlot.transform, 0.3f, 0.8f, slotScaleHandle);

        draggedCopy = Instantiate(transform.GetChild(0) as RectTransform, draggedObjectParentCanvas.transform);
        Image img = draggedCopy.GetComponent<Image>();
        //img.sprite = parentSlot.icon.sprite;
        img.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        img.rectTransform.sizeDelta = new Vector2(62, 62);
        img.raycastTarget = false;
        //Destroy(draggedCopy.GetChild(0).gameObject);
        draggedCopy.position = transform.position;
        DraggedItem draggedItem = draggedCopy.gameObject.AddComponent<DraggedItem>();
        draggedItem.draggedObjectImage = parentSlot.icon;
        draggedItem.draggedObject = parentSlot.Item;
        draggedItem.draggedFrom = parentSlot.slotIndex;
        eventData.selectedObject = draggedCopy.gameObject;

        UItrigger.Instance.EnterUI();
    }

    public void OnDrag(PointerEventData eventData) {
        if (draggedCopy != null) {
            draggedCopy.anchoredPosition += eventData.delta / draggedObjectParentCanvas.scaleFactor;
            UItrigger.Instance.EnterUI();
        }
    }

    public void OnEndDrag(PointerEventData eventData) {
        if (draggedCopy == null) return;

        slotScaleHandle = Utils.ScaleCanvasRectPreservedAspect(parentSlot.transform, 0.2f, 1f, slotScaleHandle);

        Destroy(draggedCopy.gameObject);
        draggedCopy = null;

        UItrigger.Instance.ExitUI();
    }    
}
