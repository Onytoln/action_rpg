using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoEObject : AbilityObject
{
    private IAoEValues mainAoeObjectProperties;
    [SerializeField, Header("AoE Object")] private Transform mainAoeObject;
    private Vector3 mainAoeObjectDefaultScale;

    public override void Awake() {
        base.Awake();
        mainAoeObjectDefaultScale = mainAoeObject.localScale;
    }

    public override void OnEnable() {
        base.OnEnable();
        if (CoreAbilityData == null) return;
        mainAoeObjectProperties = CoreAbilityData.AbilityPropertiesValuesContainer.TryGetAoEPropertiesValues();

        Utils.ScaleTransform(mainAoeObject, mainAoeObjectDefaultScale,
            mainAoeObjectProperties.ScaleValues.Value, mainAoeObjectProperties.ScaleValues.PrimaryValue);
    }

    public override void OnDisable() {
        base.OnDisable();
        mainAoeObjectProperties = null;
    }

    protected virtual void OnTriggerEnter(Collider other) { }
}
