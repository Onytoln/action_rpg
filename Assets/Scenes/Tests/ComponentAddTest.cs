using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentAddTest : MonoBehaviour
{
    #region Singleton
    public static ComponentAddTest instance;

    private void Awake() {
        if (instance != null) {
            return;
        }
        instance = this;
    }
    #endregion

    public Charge charge;
}
