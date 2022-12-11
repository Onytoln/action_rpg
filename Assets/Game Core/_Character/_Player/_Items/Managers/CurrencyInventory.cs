using System;
using UnityEngine;

public class CurrencyInventory : MonoBehaviour
{
    #region Singleton
    public static CurrencyInventory Instance { get; private set; }

    private void Awake() {
        if(Instance == null) {
            Instance = this;
        }
    }
    #endregion

    [SerializeField]
    private Currency[] currencies;
    public Currency[] Currencies {
        get => currencies;
    }

    public Action<int> onCurrencyChanged;

    private void Start() {
        for (int i = 0; i < currencies.Length; i++) {
            currencies[i] = (Currency)currencies[i].GetCopy();
        }
    }

    public void AddCurrency(Currency currency) {
        for (int i = 0; i < currencies.Length; i++) {
            if(currencies[i].itemID == currency.itemID) {
                currencies[i].StackSize += currency.StackSize;

                onCurrencyChanged?.Invoke(i);
            }
        }
    }
}
