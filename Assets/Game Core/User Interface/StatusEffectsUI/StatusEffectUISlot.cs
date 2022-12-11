using MEC;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatusEffectUISlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] Image icon;
    [SerializeField] Image fill;
    [SerializeField] Text stackCount;
    [SerializeField] TextMeshProUGUI duration;

    [SerializeField] CanvasGroup canvasGroup;

    private StatusEffect currentStatusEffect;

    private LTDescr stackCountTween;
    private Vector3 defaultStackTextScale;
    private CoroutineHandle fadeHandle;

    private void Start() {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void FillSlot(StatusEffect statusEffect, bool newStatusEffect = false) {
        if (statusEffect == null || statusEffect == currentStatusEffect) return;

        currentStatusEffect = statusEffect;
        currentStatusEffect.OnStacksChanged += OnCurrentStacksChanged;
        icon.sprite = statusEffect.StatusEffectProperties.icon;
        UpdateSlot();
        UpdateStackCount();

        if (currentStatusEffect.Permanent) duration.text = string.Empty;

        if (newStatusEffect) fadeHandle = Utils.FadeCanvasGroup(canvasGroup, true, fadeHandle, setProperties: false);
        canvasGroup.blocksRaycasts = true;
    }

    public void ClearSlot() {
        if (currentStatusEffect == null) return;

        currentStatusEffect.OnStacksChanged -= OnCurrentStacksChanged;
        StopRefreshDurationTooltipTextCoroutine();

        currentStatusEffect = null;

        fadeHandle = Utils.FadeCanvasGroup(canvasGroup, false, fadeHandle, setProperties: false);
        canvasGroup.blocksRaycasts = false;
    }

    public void UpdateSlot() {
        if (currentStatusEffect == null) return;

        UpdateFill();
        UpdateDuration();
    }

    private void OnCurrentStacksChanged(int currentStacks) {
        UpdateStackCount();

        if (stackCountTween != null) {
            LeanTween.cancel(stackCountTween.id);
            stackCount.transform.localScale = defaultStackTextScale;
            stackCountTween = null;
        }

        defaultStackTextScale = stackCount.transform.localScale;

        stackCountTween = LeanTween.scale(stackCount.gameObject, defaultStackTextScale * 1.5f, 0.2f)
            .setOnComplete(() => LeanTween.scale(stackCount.gameObject, defaultStackTextScale, 0.2f));
    }

    private void UpdateFill() {
        if (!currentStatusEffect.Permanent) {
            fill.fillAmount = currentStatusEffect.CurrentDuration / currentStatusEffect.StartingDuration;
        } else {
            fill.fillAmount = 0f;
        }
    }

    private void UpdateStackCount() {
        if (currentStatusEffect.StatusEffectProperties.maxStacks.GetValue() > 1) {
            stackCount.text = currentStatusEffect.CurrentStacks.ToString();
        } else {
            stackCount.text = string.Empty;
        }
    }

    private void UpdateDuration() {
        if (!currentStatusEffect.Permanent) {
            duration.text = Utils.TimeToStringText(currentStatusEffect.CurrentDuration);
        } else {
            duration.text = string.Empty;
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        StatusEffectsTooltip.Instance.ShowTooltip(currentStatusEffect.GetCombatTooltip(), currentStatusEffect.CurrentDuration);

        Timing.RunCoroutine(RefreshDurationTooltipText(), "RefreshDurationTooltipTextStatusEffect");
    }

    public void OnPointerExit(PointerEventData eventData) {
        StatusEffectsTooltip.Instance.HideTooltip();

        StopRefreshDurationTooltipTextCoroutine();
    }

    private void StopRefreshDurationTooltipTextCoroutine() {
        Timing.KillCoroutines("RefreshDurationTooltipTextStatusEffect");
    }

    private IEnumerator<float> RefreshDurationTooltipText() {
        StatusEffectsTooltip statusEffectsTooltip = StatusEffectsTooltip.Instance;
        while (true) {
            yield return Timing.WaitForOneFrame;
            if (currentStatusEffect.Permanent) {
                statusEffectsTooltip.UpdateDuration("Permanent");
            } else {
                statusEffectsTooltip.UpdateDuration(currentStatusEffect.CurrentDuration);
            }

            statusEffectsTooltip.UpdateTooltip(currentStatusEffect.GetCombatTooltip());
        }
    }
}
