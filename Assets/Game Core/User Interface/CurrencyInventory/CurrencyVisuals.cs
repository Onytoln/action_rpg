using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CurrencyVisuals : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Currency currency;
    [SerializeField]
    private Image currencyImage;
    [SerializeField]
    private Text currencyText;

    [SerializeField]
    private Tooltip tooltip;

    public void SetCurrency(Currency _currency) {
        currency = _currency;
        currencyImage.sprite = _currency.icon;
        currencyText.text = _currency.StackSize.ToString();
        tooltip = Tooltip.Instance;
        //tooltip = StaticUtils.FindChildObjectInParentByTag(GameObject.FindGameObjectWithTag("UserInterface"), "SecondaryCanvas").GetComponentInChildren<Tooltip>();
    }

    public void UpdateCurrency() {
        currencyText.text = currency.StackSize.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        tooltip.ShowTooltip(currency.name, currency.StackSize);
    }

    public void OnPointerExit(PointerEventData eventData) {
        tooltip.HideTooltip();
    }
}
