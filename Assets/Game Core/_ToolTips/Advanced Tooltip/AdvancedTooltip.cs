using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MEC;

public class AdvancedTooltip : MonoBehaviour
{
    public static AdvancedTooltip Instance { get; private set; }

    void Awake() {
        if (Instance == null) Instance = this;
    }

    [SerializeField] private Canvas tooltipCanvas;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform tooltipRectTransform;
    [SerializeField] private float padding;

    private StringBuilder currentStringBuilder;

    [SerializeField] CanvasGroup canvasGroup;

    private CoroutineHandle fadeTooltipCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = Input.mousePosition;
        pos.z = 0f;
        float posYOffset = (tooltipRectTransform.rect.height * tooltipCanvas.scaleFactor) * tooltipRectTransform.pivot.y;

        float rightEdgeToScreenEdgeDistance = Screen.width - (pos.x + tooltipRectTransform.rect.width * tooltipCanvas.scaleFactor * 0.5f) - padding;
        if (rightEdgeToScreenEdgeDistance < 0) {
            pos.x += rightEdgeToScreenEdgeDistance;
        }

        float leftEdgeToScreenEdgeDistance = 0 - (pos.x - tooltipRectTransform.rect.width * tooltipCanvas.scaleFactor  * 0.5f) + padding;
        if (leftEdgeToScreenEdgeDistance > 0) {
            pos.x += leftEdgeToScreenEdgeDistance;
        }

        float topEdgeToScreenEdgeDistance = Screen.height - (pos.y - posYOffset + tooltipRectTransform.rect.height * tooltipCanvas.scaleFactor) - padding * 2;
        if (topEdgeToScreenEdgeDistance < 0) {
            pos.y += topEdgeToScreenEdgeDistance;
        }

        transform.position = pos;
    }

    public void ShowTooltip(StringBuilder sb) {
        if (sb == null) return;
        
        if(currentStringBuilder != sb) currentStringBuilder = sb;

        tooltipText.text = currentStringBuilder.ToString();

        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);

        fadeTooltipCoroutine = Utils.FadeCanvasGroup(canvasGroup, true, fadeTooltipCoroutine, 3f);
    }

    public void ShowTooltip(string message) {
        if (message == null) return;

        tooltipText.text = message;

        gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
        //LayoutRebuilder.MarkLayoutForRebuild(tooltipRectTransform);

        fadeTooltipCoroutine = Utils.FadeCanvasGroup(canvasGroup, true, fadeTooltipCoroutine, 3f);
    }
    
    public void HideTooltip() {
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false);
        tooltipText.text = "";
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        currentStringBuilder = null;
    }

    public bool CompareShownTooltip(StringBuilder sb) {
        if (sb == null) return false;

        return sb == currentStringBuilder;
    }
}
