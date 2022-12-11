using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OutputByDamageType {
    public DamageType damageType = DamageType.Fire;
    public float reductionModifier = 0;
    public float damageByThisType = 0;
    public float percentOutOfMaxDamage = 0;
}
