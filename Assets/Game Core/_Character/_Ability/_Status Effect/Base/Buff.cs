using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff : StatusEffect {

    [SerializeField] private BuffType[] buffTypes;
    public BuffType[] BuffTypes { get => buffTypes; set => buffTypes = value; }
}
