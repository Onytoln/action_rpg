using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IExperience {

    public int DefaultExperienceGain { get; }
    public bool CanGiveExperience { get; }

    public int GetCurrentLevel();
}
