using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : Item
{
    protected Combat characterCombat;

    /*public override void Awake() {
        try {
            characterCombat = GameObject.FindGameObjectWithTag("Player").GetComponent<Combat>();
        } catch (Exception exc) {
            Debug.Log(exc);
        }
    }*/

    public override void OnItemClickOperation(ItemClickOperation itemClickOperation) {
        if(itemClickOperation == ItemClickOperation.RightSingleClick) {
            _ = Use();
        }
    }
}
