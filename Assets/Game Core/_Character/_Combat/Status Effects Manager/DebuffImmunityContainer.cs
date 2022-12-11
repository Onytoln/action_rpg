using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DebuffImmunityContainer {
    [field: SerializeField] public DebuffType DebuffType { get; set; }
    [field: SerializeField] public bool Removable { get; set; } = true;
}
