using System.Text;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MEC;
using UnityEngine.UI;

public class StatusEffectsTooltip : MonoBehaviour
{
    public static StatusEffectsTooltip Instance { get; private set; }

    void Awake() {
        if (Instance == null) Instance = this;
    }

    [SerializeField] private Canvas tooltipCanvas;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private TextMeshProUGUI durationText;
    [SerializeField] private RectTransform tooltipRectTransform;
    [SerializeField] private float padding;

    private StringBuilder currentStringBuilder;

    [SerializeField] CanvasGroup canvasGroup;

    private CoroutineHandle fadeTooltipCoroutine;

    // Start is called before the first frame update
    void Start() {
        gameObject.SetActive(false);
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    // Update is called once per frame
    void Update() {
        Vector3 pos = Input.mousePosition;
        pos.z = 0f;
        float posYOffset = (tooltipRectTransform.rect.height * tooltipCanvas.scaleFactor) * tooltipRectTransform.pivot.y;

        float rightEdgeToScreenEdgeDistance = Screen.width - (pos.x + tooltipRectTransform.rect.width * tooltipCanvas.scaleFactor / 2) - padding;
        if (rightEdgeToScreenEdgeDistance < 0) {
            pos.x += rightEdgeToScreenEdgeDistance;
        }

        float leftEdgeToScreenEdgeDistance = 0 - (pos.x - tooltipRectTransform.rect.width * tooltipCanvas.scaleFactor / 2) + padding;
        if (leftEdgeToScreenEdgeDistance > 0) {
            pos.x += leftEdgeToScreenEdgeDistance;
        }

        float topEdgeToScreenEdgeDistance = Screen.height - (pos.y - posYOffset + tooltipRectTransform.rect.height * tooltipCanvas.scaleFactor) - padding * 2;
        if (topEdgeToScreenEdgeDistance < 0) {
            pos.y += topEdgeToScreenEdgeDistance;
        }

        transform.position = pos;
    }

    public void ShowTooltip(StringBuilder sb, float duration) {
        if (sb == null) return;

        if (currentStringBuilder != sb) currentStringBuilder = sb;

        tooltipText.text = currentStringBuilder.ToString();
        durationText.text = Utils.TimeToStringTextDetailed(duration);

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
        gameObject.SetActive(true);

        fadeTooltipCoroutine = Utils.FadeCanvasGroup(canvasGroup, true, fadeTooltipCoroutine, 3f);
    }

    public void ShowTooltip(string message) {
        if (message == null) return;

        tooltipText.text = message;

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
        gameObject.SetActive(true);

        fadeTooltipCoroutine = Utils.FadeCanvasGroup(canvasGroup, true, fadeTooltipCoroutine, 3f);
    }

    public void UpdateTooltip(StringBuilder sb) {
        if (sb == null) return;

        if (currentStringBuilder == sb) return;

        tooltipText.text = sb.ToString();

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
    }

    public void UpdateDuration(float duration) {
        durationText.text = Utils.TimeToStringTextDetailed(duration);
    }

    public void UpdateDuration(string text) {
        durationText.text = text;
    }

    public void HideTooltip() {
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false);
        tooltipText.text = "";
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public bool CompareShownTooltip(StringBuilder sb) {
        if (sb == currentStringBuilder) return true;
        return false;
    }
}
