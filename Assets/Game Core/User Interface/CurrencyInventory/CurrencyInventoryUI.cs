using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyInventoryUI : MonoBehaviour
{
    [SerializeField]
    private GameObject currencyPrefab;
    [SerializeField]
    private Transform currenciesParent;

    private CurrencyInventory currencyInventory;

    private CurrencyVisuals[] currencyVisuals;

    void Start()
    {
        currencyInventory = CurrencyInventory.Instance;
        currencyVisuals = new CurrencyVisuals[currencyInventory.Currencies.Length];
        for (int i = 0; i < currencyInventory.Currencies.Length; i++) {
            GameObject currencyObj = Instantiate(currencyPrefab, currenciesParent);
            currencyVisuals[i] = currencyObj.GetComponent<CurrencyVisuals>();
            currencyVisuals[i].SetCurrency(currencyInventory.Currencies[i]);
        }
        currencyInventory.onCurrencyChanged += UpdateCurrency;
    }

    public void UpdateCurrency(int index) {
        currencyVisuals[index].UpdateCurrency();
    }
}
