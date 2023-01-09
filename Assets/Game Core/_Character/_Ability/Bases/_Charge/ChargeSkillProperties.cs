using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeSkillProperties : AoESkillProperties {
    [field: SerializeField] public StatFloat ChargeDuration { get; set; }
    [field: SerializeField] public StatFloat ChargeSpeed { get; set; }
    [field: SerializeField] public StatInt MaxPierceCount { get; set; }
    [field: SerializeField] public BoolControlComplex PiercesAllTargets { get; set; }

    public override AbilityPropertiesValuesContainer GetValuesCopy() {
        return new ChargeSkillPropertiesValuesContainer(this);
    }
}
