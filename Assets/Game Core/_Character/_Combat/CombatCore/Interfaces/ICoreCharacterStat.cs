using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICoreCharacterStat {
    public StatType StatType { get; }

    public float Value { get;  }

    public float MinValue { get;  }
}
