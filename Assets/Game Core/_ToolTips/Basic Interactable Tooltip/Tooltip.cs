using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tooltip : MonoBehaviour
{
    [SerializeField]
    private Canvas tooltipCanvas;
    private Text tooltipText;
    private RectTransform backgroundRectTransform;
    [SerializeField] private float padding;

    public static Tooltip Instance { get; private set; }

    private void Awake(){
        if(Instance == null) {
            Instance = this;
        }

        backgroundRectTransform = transform.Find("Background").GetComponent<RectTransform>();
        tooltipText = transform.Find("Text").GetComponent<Text>();
    }

    private void Start(){
        gameObject.SetActive(false);
    }

    private void Update(){
        Vector3 pos = Input.mousePosition;
        pos.z = 0f;

        float rightEdgeToScreenEdgeDistance = Screen.width - (pos.x + backgroundRectTransform.rect.width * tooltipCanvas.scaleFactor / 2) - padding;
        if(rightEdgeToScreenEdgeDistance < 0) {
            pos.x += rightEdgeToScreenEdgeDistance;
        }

        float leftEdgeToScreenEdgeDistance = 0 - (pos.x - backgroundRectTransform.rect.width * tooltipCanvas.scaleFactor / 2) + padding;
        if (leftEdgeToScreenEdgeDistance > 0) {
            pos.x += leftEdgeToScreenEdgeDistance;
        }

        float topEdgeToScreenEdgeDistance = Screen.height - (pos.y + backgroundRectTransform.rect.height * tooltipCanvas.scaleFactor) - padding;
        if (topEdgeToScreenEdgeDistance < 0) {
            pos.y += topEdgeToScreenEdgeDistance;
        }

        transform.position = pos;

    }

    public void ShowTooltip(string tooltipString, int stackSize = 0, Color32? color = null){
       
        if (stackSize > 1) {
            tooltipText.text = tooltipString + " x" + stackSize;
        } else {
            tooltipText.text = tooltipString;
        }

        tooltipText.color = (Color32)(color == null ? Color.white : color);

        Vector2 backgroundSize = new Vector2(tooltipText.preferredWidth + 10f, tooltipText.preferredHeight + 10f);
        backgroundRectTransform.sizeDelta = backgroundSize;

        gameObject.SetActive(true);
    }

    public void HideTooltip(){
        gameObject.SetActive(false);
        tooltipText.text = "";
    }
}
