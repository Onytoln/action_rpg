using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuffImmunityContainer {
    [field: SerializeField] public BuffType BuffType { get; set; }
    [field: SerializeField] public bool Removable { get; set; } = true;
}
