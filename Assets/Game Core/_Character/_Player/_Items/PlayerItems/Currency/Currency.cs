using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Currency", menuName = "Currency/Default Currency Type")]
public class Currency : Item, IConsumedOnPickup {
    public int minCurrencyDropStackSize;
    public int maxCurrencyDropStackSize;

    public override void Awake() {
        base.Awake();
        if (stackAmountMax == 1) {
            stackAmountMax = int.MaxValue;
        }

        stackSize = Random.Range(minCurrencyDropStackSize, maxCurrencyDropStackSize);
    }

    public override bool Use() {
        base.Use();
        CurrencyInventory.Instance.AddCurrency(this);
        return true;
    }
}
