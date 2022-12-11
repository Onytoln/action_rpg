using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DamageTypeWeight
{
    public DamageType damageType;
    public float damageWeight;
    public bool isMainDamageType = false;

    public DamageTypeWeight (DamageType _damageType, float _damageWeight, bool _isMainDamage ) {
        damageType = _damageType;
        damageWeight = _damageWeight;
        isMainDamageType = _isMainDamage;
    }

}
