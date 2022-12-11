using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterActivator : MonoBehaviour, IDisableUntilGameLoaded
{
    Collider activatorCollider;

    void OnEnable() {
        activatorCollider = GetComponent<Collider>();
        activatorCollider.enabled = false;
        activatorCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (!enabled) return;

        if (!other.TryGetComponent<Character>(out Character characterComponent)) return;

        characterComponent.ActivateTarget();
    }

    private void OnTriggerExit(Collider other) {
        //other.GetComponent<EnemyBehaviorTemplate>().enabled = false;
    }
}
